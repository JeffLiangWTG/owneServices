using System;
using System.Collections.Generic;
using Enterprise.Licensing.ServiceTasks;
using Enterprise.Licensing.ServiceTasks.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTask.EventLogs.Testing
{
	[TestedType(typeof(UserAccountReportingServiceTask))]
	public class UserAccountReportingServiceTaskTest : ServiceTaskTestCase<UserAccountReportingServiceTask>
	{
		[TestDate(2023, 8, 30, 12, 34, 56)]
		public void TestSendStaffAndBranchListToEdiProd()
		{
			var serviceTask = CreateAndRunTestService();
			AssertEquals(new DateTime(2023, 8, 30, 12, 34, 56), SystemDataRegistry.Instance.UserAccountLastReportTime.Value);
			AssertEquals("SendCurrentCalls", 1, serviceTask.TestSender.SendUserAccountCalls);
		}

		UserAccountReportingServiceTaskForTesting CreateAndRunTestService()
		{
			var task = new UserAccountReportingServiceTaskForTesting();
			task.ServiceLogger = new TestServiceLogger();
			task.RunTask();
			return task;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
