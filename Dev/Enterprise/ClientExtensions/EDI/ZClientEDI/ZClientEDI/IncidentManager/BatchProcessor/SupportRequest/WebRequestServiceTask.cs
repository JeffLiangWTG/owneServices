using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"WRQ",
	"Web Request Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.IncidentManager.BatchProcessor.WebRequestServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

// These bindings are just for GLOW. CW1 factory actually doesn't need nudging.
[assembly: HostedServiceBusinessObjectBinding("WRQ", IncidentRequestSchema.Constants.TableName,
	new string[] {
		IncidentRequestSchema.Constants.INC_Status + "=" + SupportIncidentLookups.LegacyStatusCodes.ApprovedAndSent,
	},
	null,
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("WRQ", IncidentRequestSchema.Constants.TableName,
	new string[] {
		IncidentRequestSchema.Constants.INC_Status + "=" + SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested,
	},
	null,
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("WRQ", IncidentRequestSchema.Constants.TableName,
	new string[] {
		IncidentRequestSchema.Constants.INC_Status + "=" + SupportIncidentLookups.LegacyStatusCodes.FormalQuotationAccepted,
	},
	null,
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("WRQ", IncidentRequestSchema.Constants.TableName,
	new string[] {
		IncidentRequestSchema.Constants.INC_Status + "=" + SupportIncidentLookups.LegacyStatusCodes.FormalQuotationDeclined,
	},
	null,
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("WRQ", IncidentRequestSchema.Constants.TableName,
	new string[] {
		IncidentRequestSchema.Constants.INC_Status + "=" + SupportIncidentLookups.LegacyStatusCodes.FormalQuotationRequested,
	},
	null,
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("WRQ", JobConversationMessageSchema.Constants.TableName,
	new string[] {
		JobConversationMessageSchema.Constants.JCM_IsInternal + "= 0",
	},
	null,
	ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("WRQ", EdiERequestDocumentQueueSchema.Constants.TableName,
	new string[] { }, null, ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	class WebRequestServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			var processor = new SupportRequestProcessor(ServiceLogger);
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				processor.ProcessAllWebUpdates();
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
