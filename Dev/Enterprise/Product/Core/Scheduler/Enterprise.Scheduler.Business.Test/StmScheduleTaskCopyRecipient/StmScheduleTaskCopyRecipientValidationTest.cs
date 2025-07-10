using CargoWise.EntityFramework.Testing;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class StmScheduleTaskCopyRecipientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestODR_EmailAddress()
		{
			// Arrange
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			var scheduleTaskRecipient = scheduleTask.Recipients.AddNew();
			scheduleTaskRecipient.S6_S5 = scheduleTask.PK;
			Factory.Save();
			// Act
			var scheduleTaskCarbonCopyRecipient = scheduleTaskRecipient.CarbonCopyRecipients.AddNew();
			scheduleTaskCarbonCopyRecipient.SCR_EmailAddress = "test1@test.com";
			var scheduleTaskBlindCopyRecipient = scheduleTaskRecipient.BlindCarbonCopyRecipients.AddNew();
			scheduleTaskBlindCopyRecipient.SCR_EmailAddress = "123456";
			// Assert
			AssertNoErrors(scheduleTaskCarbonCopyRecipient.SCR_EmailAddressInfo);
			AssertHasErrors(scheduleTaskBlindCopyRecipient.SCR_EmailAddressInfo);
		}
	}
}
