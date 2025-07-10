using CargoWise.EntityFramework.Testing;

namespace Enterprise.UniversalCopy.Business.Testing
{
	class StmUniversalCopyScheduleTaskValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckS5_ScheduleDescription()
		{
			var task = Factory.New<StmUniversalCopyScheduleTask>();
			task.RunPreSaveValidation();
			AssertHasError(task.S5_ScheduleDescriptionInfo, "Please enter a Schedule Task Description.");
			task.S5_ScheduleDescription = "This is a test";
			AssertNoErrors(task.S5_ScheduleDescriptionInfo);
		}
	}
}
