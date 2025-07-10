using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"NMI",
	"Native Data Messaging Inbound",
	"ESV",
	typeof(Enterprise.DataTransfer.Native.ServiceTasks.ServiceTask),
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("NMI", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.NativeDataMessaging,  EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XDC }, "Native Data Messaging Inbound")]
namespace Enterprise.DataTransfer.Native.ServiceTasks
{
	public class ServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			ExecuteBatch(token);
		}

		void ExecuteBatch(CancellationToken token)
		{
			(processingManager ?? (processingManager = new NativeProcessingManager() { Logger = GetNewLogger() })).ExecuteBatch(token);
		}
		NativeProcessingManager processingManager;

		LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += Logger_OnLogInfoAdded;
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
	}
}
