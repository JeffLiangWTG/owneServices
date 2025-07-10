using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Billing.ServiceTasks.Testing
{
	[TestedType(typeof(EdiBillingTestingEnvironmentSyncServiceTask))]
	public class EdiBillingTestingEnvironmentSyncServiceTaskTest : ServiceTaskTestCase<EdiBillingTestingEnvironmentSyncServiceTask>
	{
		public void TestRunTask()
		{
			var process = new EdiBillingTestingEnvironmentSyncServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			process.RunTask();
			var logs = $@"Information|Begin processing
Error|Please check the registry items under category: WiseTech Global Client Extensions/Licence Billing/External Servers/Billing Testing.
Information|End processing
";
			AssertEquals(logs, logger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
