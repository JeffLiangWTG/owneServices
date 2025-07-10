using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmPrintJobCopyRecipientCollection))]
	sealed class StmPrintJobCopyRecipientCollectionTest : ActiveBusinessObjectCollectionTestCase<StmPrintJobCopyRecipientCollection>
	{
		public override void TestAdd()
		{
			base.TestAdd();
			Assert("CC", Collection.All(c => c.SPR_RecipientType == Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient));
		}

		public void TestUpdatedEventIsFired()
		{
			// Arrange
			bool isUpdatedFired = false;
			Collection.Updated += (sender, args) => isUpdatedFired = true;
			// Act
			var printJobCopyRecipient = Factory.NewWithValidTestData<StmPrintJobCopyRecipient>();
			printJobCopyRecipient.SPR_RecipientType = Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient;
			printJobCopyRecipient.SPR_SP = Collection.Owner.PK;
			printJobCopyRecipient.SPR_EmailAddress = "test@test.com";
			Factory.Save();
			Collection.Add(printJobCopyRecipient);
			// Assert
			Assert("Updated event should be fired when adding an element to the collection", isUpdatedFired);

			// Arrange
			isUpdatedFired = false;
			// Act
			Collection.Delete(printJobCopyRecipient);
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
			AssertEquals("test1@test.com", Collection[0].SPR_EmailAddress);
			AssertEquals("test2@test.com", Collection[1].SPR_EmailAddress);
		}

		#region Implementations

		protected override StmPrintJobCopyRecipientCollection GetCollectionToTest()
		{
			var printJob = Factory.NewWithValidTestData<StmPrintJob>();
			return new StmPrintJobCopyRecipientCollection(printJob, Core.Constants.CopyRecipientType.BlindCarbonCopyRecipient);
		}

		#endregion
	}
}
