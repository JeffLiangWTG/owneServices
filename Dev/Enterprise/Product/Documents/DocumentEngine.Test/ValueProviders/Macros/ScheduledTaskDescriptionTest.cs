using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ScheduledTaskDescription))]
	sealed class ScheduledTaskDescriptionTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Schedule TaskDescription>", !ValueProviderToTest.IsResponsibleForReplacing("<Schedule TaskDescription>", Passes.FirstPass));
			Assert("should match < scheduled Task          deScRiPtion       >", ValueProviderToTest.IsResponsibleForReplacing("< scheduled Task          deScRiPtion       >", Passes.FirstPass));
			Assert("should match <ScheduledTaskDescription>", ValueProviderToTest.IsResponsibleForReplacing("<ScheduledTaskDescription>", Passes.FirstPass));
		}

		public void TestReplacementNonScheduled()
		{
			PrepareRenderer();
			Assert("Should have returned new RowHider()", ValueProviderToTest.GetReplacement("<ScheduledTaskDescription>", Report) is RowHider);
		}

		public void TestReplacementScheduled()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "my scheduled task";
			Report.SetScheduleTask(scheduleTask);
			PrepareRenderer();
			AssertEquals("my scheduled task", ValueProviderToTest.GetReplacement("<ScheduledTaskDescription>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ScheduledTaskDescription();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "Address Profile Report";
			Report.SetScheduleTask(scheduleTask);
			PrepareRenderer();
		}
	}
}
