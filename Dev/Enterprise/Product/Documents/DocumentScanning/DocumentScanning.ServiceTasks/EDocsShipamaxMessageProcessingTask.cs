using System.Threading;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EDocsShipamaxMessageProcessingTask.ServiceTaskCode,
	EDocsShipamaxMessageProcessingTask.ServiceTaskDescription,
	"DOC",
	typeof(EDocsShipamaxMessageProcessingTask),
	AllowsMultipleInstances = false,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "1day",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]
[assembly: HostedServiceBusinessObjectBinding(EDocsShipamaxMessageProcessingTask.ServiceTaskCode, EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ShipamaxIntegration,
		EDIMessageSchema.Constants.EM_Status +          "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive +        "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal,
	}, "EDocsShipamaxMessage Queue")]
namespace Enterprise.DocumentScanning.ServiceTasks
{
	public sealed class EDocsShipamaxMessageProcessingTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "DSP";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		public const string ServiceTaskDescription = "Document Ingestion Processing Task";

		public override void RunTask(CancellationToken token)
		{
			if (!EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				ServiceLogger.Error((NoResString)"Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types.");
				return;
			}

			if (string.IsNullOrEmpty(DocManagerRegistry.Instance.DocumentParserUrl.Value))
			{
				ServiceLogger.Error((NoResString)$"Failed to run service task. Please check if registry {DocManagerRegistry.Instance.DocumentParserUrl.GetLocationInEnglish()} has a valid url.");
				return;
			}

			Processor.ProcessMessages(token);
		}

		internal EDocsShipamaxMessageProcessor Processor
		{
			get => processor ??= new EDocsShipamaxMessageProcessor(ServiceLogger);
			set
			{
				processor = value;
			}
		}

		EDocsShipamaxMessageProcessor processor;
	}
}
