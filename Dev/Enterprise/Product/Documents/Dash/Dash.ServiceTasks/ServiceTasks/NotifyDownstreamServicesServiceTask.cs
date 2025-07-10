using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.Dash.ServiceTasks.ServiceTasks;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.NotifyDownstreamServices,
	"Document Ingestion Notify Downstream Services Service Task",
	"DOC",
	typeof(NotifyDownstreamServicesServiceTask),
	AllowsMultipleInstances = false,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]
[assembly: HostedServiceBusinessObjectBinding(
	WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.NotifyDownstreamServices,
	EDIMessageSchema.Constants.TableName,
	[
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DashDocumentDataProcessing,
		EDIMessageSchema.Constants.EM_MessageType     + "=" + WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.NotifyDownstreamServices,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal,
	], "DashNotifyDownstreamServicesQueue")]
namespace Enterprise.Dash.ServiceTasks.ServiceTasks
{
	public class NotifyDownstreamServicesServiceTask : DashMessageProcessingTask
	{
		public NotifyDownstreamServicesServiceTask()
		{
		}

		public NotifyDownstreamServicesServiceTask(NotifyDownstreamServicesMessageProcessor dashMessageProcessor)
		{
			this.dashMessageProcessor = dashMessageProcessor;
		}

		DashMessageProcessor dashMessageProcessor;

		protected override DashMessageProcessor DashMessageProcessor
		{
			get
			{
				var shipamaxService = ObjectFactory.Get<IShipamaxService>();
				var dashErrorReporter = ObjectFactory.Get<IDashErrorReporter>();
				dashMessageProcessor ??= new NotifyDownstreamServicesMessageProcessor(
					new BusinessObjectFactory(),
					ServiceLogger,
					shipamaxService,
					dashErrorReporter,
					new DashCompletionService(shipamaxService, new DashPostingService()));
				return dashMessageProcessor;
			}
		}
	}
}
