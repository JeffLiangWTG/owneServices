using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmScheduleTaskCopyRecipientCollection))]
	sealed class StmScheduleTaskCopyRecipientCollectionTest : ActiveBusinessObjectCollectionTestCase<StmScheduleTaskCopyRecipientCollection>
	{
		public override void TestAdd()
		{
			base.TestAdd();
			Assert("CC", Collection.All(c => c.SCR_RecipientType == Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient));
		}

		public void TestUpdatedEventIsFired()
		{
			// Arrange
			bool isUpdatedFired = false;
			Collection.Updated += (sender, args) => isUpdatedFired = true;
			var scheduleTaskCopyRecipient = scheduleTaskRecipient.BlindCarbonCopyRecipients.AddNew();
			scheduleTaskCopyRecipient.SCR_RecipientType = Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient;
			scheduleTaskCopyRecipient.SCR_S6 = scheduleTaskRecipient.PK;
			scheduleTaskCopyRecipient.SCR_EmailAddress = "test@test.com";
			Factory.Save();
			// Act
			Collection.Add(scheduleTaskCopyRecipient);
			// Assert
			Assert("Updated event should be fired when adding an element to the collection", isUpdatedFired);

			// Arrange
			isUpdatedFired = false;
			// Act
			Collection.Delete(scheduleTaskCopyRecipient);
			// Assert
			Assert("Updated event should be fired when deleting an element from the collection", isUpdatedFired);
		}

		public void TestValue()
		{
			// Arrange
			AssertEquals("Precondition - The initial value should be empty.", ZString.Empty, Collection.Value);
			// Act
			Collection.Value = "test1@test.com, test2@test.com";
			// Assert
			AssertEquals("There should be two copy recipients created.", 2, Collection.Count);
			AssertEquals("test1@test.com", Collection[0].SCR_EmailAddress);
			AssertEquals("test2@test.com", Collection[1].SCR_EmailAddress);
		}

		#region Implementations

		protected override StmScheduleTaskCopyRecipientCollection GetCollectionToTest()
		{
			scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTaskRecipient = scheduleTask.Recipients.AddNew();
			scheduleTaskRecipient.S6_S5 = scheduleTask.PK;
			Factory.Save();
			return new StmScheduleTaskCopyRecipientCollection(scheduleTaskRecipient, Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient);
		}

		StmScheduleTask scheduleTask;
		StmScheduleTaskRecipient scheduleTaskRecipient;

		#endregion
	}
}
