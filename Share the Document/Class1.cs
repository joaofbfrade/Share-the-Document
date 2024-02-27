using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Share_the_Document
{
    public class Class1 : IPlugin
    {
        public IPluginExecutionContext _context = null;
        public IOrganizationServiceFactory _serviceFactory = null;
        public IOrganizationService service = null;
        public ITracingService trace = null;

        public void Execute(IServiceProvider serviceProvider)
        {
            _context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = _serviceFactory.CreateOrganizationService(_context.UserId);
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Entered the plugin.");

            // ... switch on the MessageName
            //     and call actions to handle the supported messages
            switch (_context.MessageName.ToUpperInvariant())
            {
                case "CREATE":
                    OnCreate();
                    break;
                //case "UPDATE":
                //    OnUpdate();
                //    break;
                //case "DELETE":
                //    OnDelete();
                //    break;
                default:
                    break;
            }
        }
        private void OnCreate()
        {
            trace.Trace("Create");

            if (_context.InputParameters.Contains("Target") && _context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)_context.InputParameters["Target"];

                if (entity.LogicalName == "arq_documentpartylog")
                {
                    // Check if the "arq_relateduser" field is populated
                    if (entity.Contains("arq_relateduser") && entity["arq_relateduser"] is EntityReference)
                    {
                        EntityReference relatedUserRef = (EntityReference)entity["arq_relateduser"];
                        Guid userIdToShareWith = relatedUserRef.Id;

                        trace.Trace(userIdToShareWith.ToString());


                        // Create the principal object for the user
                        PrincipalAccess principalAccess = new PrincipalAccess
                        {
                            Principal = new EntityReference("systemuser", userIdToShareWith),
                            AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.AppendAccess | AccessRights.AppendToAccess | AccessRights.ShareAccess
                        };

                        // Create the GrantAccessRequest object
                        GrantAccessRequest grantAccessRequest = new GrantAccessRequest
                        {
                            PrincipalAccess = principalAccess,
                            Target = new EntityReference(entity.LogicalName, entity.Id)
                        };

                        // Execute the GrantAccessRequest
                        service.Execute(grantAccessRequest);
                    }
                }
            }
        }

        
    }
}
