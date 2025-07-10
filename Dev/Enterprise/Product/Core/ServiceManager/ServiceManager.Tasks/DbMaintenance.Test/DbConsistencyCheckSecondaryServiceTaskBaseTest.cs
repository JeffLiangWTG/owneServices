using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(DbConsistencyCheckSecondaryServiceTask))]
	sealed class DbConsistencyCheckSecondaryServiceTaskBaseTest : ServiceTaskTestCase<DbConsistencyCheckSecondaryServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}
	}
}
