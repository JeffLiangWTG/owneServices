using NUnit.Framework;

namespace Enterprise.TimeEngineScheduler.ServiceTask.Test
{
	public class SchedulerActionFactoryTest : TestCase
	{
		public void TestActionFactory_ShouldLoadAtLeastOneAction()
		{
			var testAction = new SchedulerActionFactory().GetSchedulerAction(SchedulerTestAction.Code);
			AssertNotNull("Must load a test action", testAction);
		}
	}
}
