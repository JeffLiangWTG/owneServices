using System.Threading;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MessageProcessorServiceTask.MessageProcessorServiceCode,
	MessageProcessorServiceTask.MessageProcessorServiceName,
	MessageProcessorServiceTask.MessageServiceTaskCategory,
	typeof(MessageProcessorServiceTask),
	MinimumPeriod = "10Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(MessageProcessorServiceTask.MessageProcessorServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAACI,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs ACI messages inbound")]

[assembly: HostedServiceBusinessObjectBinding(MessageProcessorServiceTask.MessageProcessorServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAEXP,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs EXP messages inbound")]

[assembly: HostedServiceBusinessObjectBinding(MessageProcessorServiceTask.MessageProcessorServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAIMP,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs IMP messages inbound")]

namespace Enterprise.Customs.CA.ServiceTasks
{
	/// <summary>
	/// Canadian Customs Message Processor, processes queued messages in EdiMessage
	/// </summary>
	public sealed class MessageProcessorServiceTask : ServiceProviderImplUnderDefaultBranch
	{
		public const string MessageServiceTaskCategory = "CAC";
		public const string MessageProcessorServiceCode = "CAM";
		public const string MessageProcessorServiceName = "Canadian Customs Message Processor";

		[HostedServiceRequirement]
		public static string CheckCompanyInCanadaOrAppliesAllCountries() => BatchProcessorUtilities.CheckCompanyInCanadaOrAppliesAllCountries();

		protected override void RunTaskMain(CancellationToken token)
		{
			using (var messageProcessor = new CAMessageProcessor())
			{
				messageProcessor.Logger.OnLogInfoAdded += (log, logType) => ServiceLogger.Log(logType, log);
				messageProcessor.ExecuteBatch(token);
			}
		}
	}
}
