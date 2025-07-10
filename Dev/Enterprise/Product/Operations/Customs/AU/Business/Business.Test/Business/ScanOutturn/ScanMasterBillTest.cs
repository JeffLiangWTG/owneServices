using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ScanMasterBillTest : TestCaseWithFactory
	{
		public void TestGetMutexKeys_ConsolMasterBill()
		{
			var scanObj = GetNewConsolScanMasterBill();

			var manager = GetNewScanForOutturnManager(scanObj);
			manager.SelectedUnderbond = manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
			manager.ValidateSelectedUnderbond();
			AssertEquals(2, manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Count);

			manager.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection[1].IncludeInScan = true;
			manager.SetSelectedShipment();

			var mutexKeys = ((IScanMasterBill)scanObj).GetMutexKeys();
			AssertEquals("scanMutexArray.Length", 2, mutexKeys.Length);

			AssertEquals("scanMutex1.RecordIdentifier", "MasterBill:Master1 HouseBill:HOUSEBILL1", mutexKeys[0]);
			AssertEquals("scanMutex2.RecordIdentifier", "MasterBill:Master1", mutexKeys[1]);
		}

		protected abstract ScanMasterBill GetNewConsolScanMasterBill();
		protected abstract ScanMasterBill GetNewStandAloneScanMasterBill();
		protected abstract ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj);
	}
}
