using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class ScanForOutturnManagerTest : TestCaseWithFactory
	{
		public void TestIsOutturnCollectionCreatedAndHasMembers()
		{
			var manager = GetNewScanForOutturnManager(GetNewStandaloneScanMasterBill());
			Assert(!manager.IsOutturnCollectionCreatedAndHasMembers);
			Assert("Touch to create", manager.OutturnCollection.Count == 0);
			Assert(!manager.IsOutturnCollectionCreatedAndHasMembers);
			manager.OutturnCollection.Add(GetNewOutturnLine());
			Assert(manager.IsOutturnCollectionCreatedAndHasMembers);
		}

		[TestDate(2011, 4, 11, 11, 44, 55)]
		public void TestProperties()
		{
			var scanObj = GetNewStandaloneScanMasterBill();
			var manager = GetNewScanForOutturnManager(GetNewStandaloneScanMasterBill());
			AssertEquals("eManifest_" + scanObj.MasterBillNumber + "_201104111144.csv", manager.ExportFileName);

			var collection = manager.GetNewOutturnLineCollection();
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);

			Assert(manager.IsStandaloneShipment);

			AssertNotNull(manager.ScanWizardDataSource);
			AssertNull(manager.ScanWizardDataSource.ShipmentSelectorLineCollection);
			AssertNotNull(manager.ScanWizardDataSource.UnderbondSelectorLineCollection);

			SetMasterHouseBillNumber(scanObj.MasterBill);
			manager = GetNewScanForOutturnManager(scanObj);
			AssertEquals("eManifest_" + scanObj.MasterBillNumber + "_" + scanObj.MasterBill.MasterHouseBill + "_201104111144.csv", manager.ExportFileName);
		}

		#region Test ScanningForOutturnMutex

		public void TestScanningForOutturnMutex()
		{
			var scanObj = GetNewConsolScanMasterBill();

			string concurrencyErrorMessage = string.Empty;

			using (var manager2 = GetNewScanForOutturnManager(scanObj))
			{
				using (var manager1 = GetNewScanForOutturnManager(scanObj))
				{
					manager1.SelectedUnderbond = manager1.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
					manager1.ValidateSelectedUnderbond();
					AssertEquals(2, manager1.ScanWizardDataSource.ShipmentSelectorLineCollection.Count);

					manager1.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;
					manager1.ScanWizardDataSource.ShipmentSelectorLineCollection[1].IncludeInScan = true;
					manager1.SetSelectedShipment();

					AssertEquals("lock scan should succeed", null, manager1.TryLockScan());

					manager2.SelectedUnderbond = manager2.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
					manager2.ValidateSelectedUnderbond();
					AssertEquals(2, manager2.ScanWizardDataSource.ShipmentSelectorLineCollection.Count);

					manager2.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;
					manager2.ScanWizardDataSource.ShipmentSelectorLineCollection[1].IncludeInScan = true;
					manager2.SetSelectedShipment();

					concurrencyErrorMessage = manager2.TryLockScan();
					AssertNotEquals("lock scan should fail", null, concurrencyErrorMessage);
					string errorMessageHeader = @"(?s)Below user\(s\) are in the process of outturn scanning the same record\(s\) you are attempting to outturn scan\. Only one user may perform outturn scanning on the same record at the same time\. Please try again later\.";
					string patternToMatchLockingUser = @".*? is scanning .*? on .*? since .*?";
					string pattern = errorMessageHeader + patternToMatchLockingUser + patternToMatchLockingUser;
					Assert("Concurrency error message should match pattern", Regex.IsMatch(concurrencyErrorMessage, pattern));
				}

				AssertEquals("lock scan should succeed after manager1 unlock it", null, manager2.TryLockScan());
			}
		}

		#endregion

		protected abstract ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj);
		protected abstract ScanMasterBill GetNewConsolScanMasterBill();
		protected abstract ScanMasterBill GetNewStandaloneScanMasterBill();
		protected abstract OutturnLine GetNewOutturnLine();
		protected abstract void SetMasterHouseBillNumber(IScanMasterBillProvider masterBillProvider);
	}
}
