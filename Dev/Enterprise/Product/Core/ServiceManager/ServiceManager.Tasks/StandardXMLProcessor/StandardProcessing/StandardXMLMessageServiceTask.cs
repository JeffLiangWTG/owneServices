using System.Threading;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"XMS",
	"Standard XML Message Service",
	"ESV",
	typeof(StandardXMLMessageServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes")
]
[assembly: HostedServiceBusinessObjectBinding("XMS", EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.XMS, EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.XMS, EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive, EDIMessageSchema.Constants.EM_MessageSubType + "!=" + EDIMessageSubTypeList.Codes.Events },
	"XML Message Events")]
namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class StandardXMLMessageServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			new StandardXMLMessageProcessor().Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
