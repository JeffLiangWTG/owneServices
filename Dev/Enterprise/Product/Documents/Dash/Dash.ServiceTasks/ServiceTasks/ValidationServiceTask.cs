using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Business.Validation;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.Dash.ServiceTasks.ServiceTasks;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.Validation,
	"Validation Service Task",
	"DOC",
	typeof(ValidationServiceTask),
	AllowsMultipleInstances = false,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "15minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]
[assembly: HostedServiceBusinessObjectBinding(
	WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.Validation,
	EDIMessageSchema.Constants.TableName,
	[
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DashDocumentDataProcessing,
		EDIMessageSchema.Constants.EM_MessageType     + "=" + WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.Validation,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal,
	], "Dash Validation Queue")]
namespace Enterprise.Dash.ServiceTasks.ServiceTasks
{
	public class ValidationServiceTask : DashMessageProcessingTask
	{
		public ValidationServiceTask()
		{
		}

		public ValidationServiceTask(ValidationProcessor validationProcessor)
		{
			this.validationProcessor = validationProcessor;
		}

		ValidationProcessor validationProcessor;

		protected override DashMessageProcessor DashMessageProcessor
		{
			get
			{
				validationProcessor ??= new ValidationProcessor(
					new BusinessObjectFactory(),
					ServiceLogger,
					ObjectFactory.Get<IShipamaxService>(),
					ObjectFactory.Get<IDashErrorReporter>(),
					new DashCommercialInvoiceValidator());
				return validationProcessor;
			}
		}
	}
}
