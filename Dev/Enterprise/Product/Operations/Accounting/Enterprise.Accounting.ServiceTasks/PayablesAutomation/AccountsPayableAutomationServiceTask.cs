using System.Threading;
using Enterprise.Accounting.APAutomation.AccountsPayableAutomationServiceTask;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.ServiceTasks.PayablesAutomation;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AccountsPayableAutomationServiceTask.Code,
	"Accounts Payable Automation Service Task",
	"ACC",
	typeof(AccountsPayableAutomationServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: HostedServiceBusinessObjectBinding(
	AccountsPayableAutomationServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	[
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.AccountsPayableAutomation,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.PIN,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal
	],
	"AP Automation Queue")
]

namespace Enterprise.Accounting.ServiceTasks.PayablesAutomation
{
	public class AccountsPayableAutomationServiceTask : ServiceProviderImpl
	{
		public const string Code = "APA";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var processor = new AccountsPayableAutomationProcessor(new APReconciliationPoster(), ServiceLogger);
			processor.Run(youMustReactToThisToken);
		}
	}
}
