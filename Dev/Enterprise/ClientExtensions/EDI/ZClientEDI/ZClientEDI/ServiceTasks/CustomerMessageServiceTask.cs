using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.ServiceTasks.CustomerMessageServiceTask.Code,
	"eRequest Service",
	"CSP",
	typeof(Enterprise.Client.EDI.ServiceTasks.CustomerMessageServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MaximumPeriod = "30minutes",
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "15minutes")
]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Client.EDI.ServiceTasks.CustomerMessageServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.CustomerServiceRequest
	},
	"Customer Service Request",
	ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class CustomerMessageServiceTask : ServiceProviderImpl
	{
		public const string Code = "ERQ";

		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				CreateProcessor().Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		StandardXmlProcessor CreateProcessor()
		{
			return new CustomerMessageProcessor();
		}
	}

	public class CustomerMessageProcessor : SystemXmlMessageProcessorBase
	{
		public CustomerMessageProcessor()
		{
			AddSupportedMessage(SystemMessageList.Codes.CustomerServiceRequest, typeof(SupportRequestMessageAction));
		}
	}
}
