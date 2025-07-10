using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class ChangeLogTriggerGroupingTest : TestCase
	{
		public void TestEquality()
		{
			var self = new ChangeLogTriggerGrouping(null);
			var other = new ChangeLogTriggerGrouping(null);
			AssertEquals("Equals when both change log are null", self, other);
		}
	}
}
