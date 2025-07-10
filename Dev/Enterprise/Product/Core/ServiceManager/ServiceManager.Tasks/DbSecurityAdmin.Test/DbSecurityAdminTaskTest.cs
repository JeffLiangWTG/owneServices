using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ServiceManager.Tasks.DbSecurityAdmin;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.LoginSyncServiceTask.Testing
{
	[TestedType(typeof(DbSecurityAdminTask))]
	sealed class DbSecurityAdminTaskTest : ServiceTaskTestCase<DbSecurityAdminTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}
	}
}
