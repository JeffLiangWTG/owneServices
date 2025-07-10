using System.Threading;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	InterchangeSenderServiceTask.InterchangeSenderServiceCode,
	InterchangeSenderServiceTask.InterchangeSenderServiceName,
	MessageProcessorServiceTask.MessageServiceTaskCategory,
	typeof(InterchangeSenderServiceTask),
	MinimumPeriod = "10Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(InterchangeSenderServiceTask.InterchangeSenderServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAACI,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs ACI messages outbound")]

[assembly: HostedServiceBusinessObjectBinding(InterchangeSenderServiceTask.InterchangeSenderServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAEXP,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs EXP messages outbound")]

[assembly: HostedServiceBusinessObjectBinding(InterchangeSenderServiceTask.InterchangeSenderServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CAIMP,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs IMP messages outbound")]

[assembly: HostedServiceBusinessObjectBinding(InterchangeSenderServiceTask.InterchangeSenderServiceCode,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.CACustoms,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs messages outbound")]

namespace Enterprise.Customs.CA.ServiceTasks
{
	/// <summary>
	/// Canadian Customs Interchange Maker, drops interchanges into out box
	/// </summary>
	public sealed class InterchangeSenderServiceTask : ServiceProviderImplUnderDefaultBranch
	{
		public const string InterchangeSenderServiceCode = "CAS";
		public const string InterchangeSenderServiceName = "Canadian Customs Interchange Maker";

		[HostedServiceRequirement]
		public static string CheckCompanyInCanadaOrAppliesAllCountries() => BatchProcessorUtilities.CheckCompanyInCanadaOrAppliesAllCountries();

		protected override void RunTaskMain(CancellationToken token)
		{
			using (var sender = new Sender())
			{
				sender.Logger.OnLogInfoAdded += (log, logType) => ServiceLogger.Log(logType, log);
				sender.ExecuteBatch(token);
			}
		}
	}
}
