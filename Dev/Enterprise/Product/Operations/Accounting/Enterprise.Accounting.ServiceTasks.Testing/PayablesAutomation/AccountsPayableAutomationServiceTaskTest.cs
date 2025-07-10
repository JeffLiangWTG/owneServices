using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Accounting.ServiceTasks.PayablesAutomation;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(AccountsPayableAutomationServiceTask))]
	public class AccountsPayableAutomationServiceTaskTest : ServiceTaskTestCase<AccountsPayableAutomationServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals(nameof(HostedServiceAttribute.Code), "APA", hostedServiceAttribute.Code);
				AssertEquals(nameof(HostedServiceAttribute.Description), "Accounts Payable Automation Service Task", hostedServiceAttribute.Description);
				AssertEquals(nameof(HostedServiceAttribute.Category), "ACC", hostedServiceAttribute.Category);
				AssertEquals(nameof(HostedServiceAttribute.CanRunInAnyBranch), true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals(nameof(HostedServiceAttribute.AllowsMultipleInstances), false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals(nameof(HostedServiceAttribute.IsMandatory), true, hostedServiceAttribute.IsMandatory);
				AssertEquals(nameof(HostedServiceAttribute.MinimumPeriod), "10minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals(nameof(HostedServiceAttribute.DefaultScheduleRunEvery), "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
				AssertEquals(nameof(HostedServiceAttribute.ActiveByDefault), true, hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestRunTask()
		{
			var cancellationToken = new CancellationToken();
			var serviceLogger = new TestServiceLogger();
			var task = new AccountsPayableAutomationServiceTask { ServiceLogger = serviceLogger };

			task.RunTask(cancellationToken);

			AssertEquals(1, serviceLogger.Count);
			AssertContains("Nothing to process.", serviceLogger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"AP Automation Queue",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.AccountsPayableAutomation,
						EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.PIN,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal)
				};
			}
		}
	}
}
