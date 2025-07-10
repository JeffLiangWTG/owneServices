using CargoWise.BuildTools.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class DataUpgradeSetupControllerTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		public void TestUndoCheckOutWithoutCheckingAllTasksThrowsException()
		{
			DataUpgradeSetupControllerNoDbSaveOrLoadForTest fastController = new DataUpgradeSetupControllerNoDbSaveOrLoadForTest();

			try
			{
				fastController.CheckOut();
				fastController.TestTaskSetup.CheckOut();
				fastController.UndoCheckOut();
			}
			catch (ControllerHasUncheckedSetupTasksException)
			{
				Assert("Expected Exception", true);
			}
		}

		public void TestCheckOut()
		{
			var testController = new DataUpgradeSetupControllerForTest();

			AssertEquals("Is Controller Checked-out by me (1)", false, testController.IsCheckedOutByMe);
			AssertEquals("Major Version Before Check-out", 1, testController.MajorVersionInDataVersionFile);
			AssertEquals("Minor Version Before Check-out", 0, testController.MinorVersionInDataVersionFile);

			testController.CheckOut();

			AssertEquals("Is Controller Checked-out by me (2)", true, testController.IsCheckedOutByMe);

			if (ReleaseInfo.Instance.ReleaseRing == ReleaseRings.Codes.ALP)
			{
				AssertEquals("Major Version Before Check-out", 2, testController.MajorVersionInDataVersionFile);
				AssertEquals("Minor Version Before Check-out", 0, testController.MinorVersionInDataVersionFile);
			}
			else
			{
				AssertEquals("Major Version Before Check-out", 1, testController.MajorVersionInDataVersionFile);
				AssertEquals("Minor Version Before Check-out", 1, testController.MinorVersionInDataVersionFile);
			}
		}
	}
}
