using System.Threading;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"XES",
	"Standard XML Event Service",
	"ESV",
	typeof(StandardXMLEventServiceTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding("XES",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.XMS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_MessageType     + "=" + EDIMessageTypeList.Codes.XMS,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + EDIMessageSubTypeList.Codes.Events
	},
	"XML Event Receiver")]

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class StandardXMLEventServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			new StandardXMLEventProcessor().Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}
	}
}
