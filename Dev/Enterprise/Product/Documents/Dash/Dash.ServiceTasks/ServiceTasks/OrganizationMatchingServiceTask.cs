using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Integration.Services;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.Dash.ServiceTasks.ServiceTasks;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.OrganizationMatching,
	"Document Ingestion Organization Matching Service Task",
	"DOC",
	typeof(OrganizationMatchingServiceTask),
	AllowsMultipleInstances = false,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]
[assembly: HostedServiceBusinessObjectBinding(
	WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.OrganizationMatching,
	EDIMessageSchema.Constants.TableName,
	[
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DashDocumentDataProcessing,
		EDIMessageSchema.Constants.EM_MessageType     + "=" + WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.OrganizationMatching,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal,
	], "DashOrganizationMatching Queue")]
namespace Enterprise.Dash.ServiceTasks.ServiceTasks
{
	public class OrganizationMatchingServiceTask : DashMessageProcessingTask
	{
		public OrganizationMatchingServiceTask()
		{
		}

		public OrganizationMatchingServiceTask(OrganizationMatchingMessageProcessor dashMessageProcessor)
		{
			this.dashMessageProcessor = dashMessageProcessor;
		}

		DashMessageProcessor dashMessageProcessor;

		protected override DashMessageProcessor DashMessageProcessor
		{
			get
			{
				dashMessageProcessor ??= new OrganizationMatchingMessageProcessor(
					new BusinessObjectFactory(),
					ServiceLogger,
					ObjectFactory.Get<IDashGlowService>(),
					ObjectFactory.Get<IShipamaxService>(),
					ObjectFactory.Get<IDashErrorReporter>());
				return dashMessageProcessor;
			}
		}
	}
}
