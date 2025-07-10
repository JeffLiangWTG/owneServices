using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ScanAccessRestrictionManagerTest : TestCaseWithFactory
	{
		public void TestLockMutex_NullUser()
		{
			var scanObj = GetScanMasterBill();

			using (var manager2 = GetNewScanForOutturnManager(scanObj))
			{
				using (var manager1 = GetNewScanForOutturnManager(scanObj))
				{
					manager1.SelectedUnderbond = manager1.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
					manager1.ValidateSelectedUnderbond();

					manager1.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;
					manager1.SetSelectedShipment();

					AssertEquals("lock scan should succeed", null, manager1.TryLockScan());

					manager2.SelectedUnderbond = manager2.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
					manager2.ValidateSelectedUnderbond();

					manager2.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;
					manager2.SetSelectedShipment();

					var lockInfo = MutexIDs.ScanningForOutturn.Name;
					var emptyGuid = Guid.Empty;
					var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";

					TestConnection.ExecuteNonQuery(sql);

					AssertNoExceptionThrown(() => manager2.TryLockScan());
				}
			}
		}

		ScanMasterBill GetScanMasterBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();

			var oceanBill1 = Factory.New<CusSCAOceanBill>();
			oceanBill1.CB_ParentId = consol.PK;
			oceanBill1.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var hb1 = oceanBill1.HouseBills.AddNew();
			hb1.CA_JS = shipment1.PK;
			hb1.CA_ShipmentStatus = "HLD";

			var container1 = oceanBill1.Containers.AddNew();
			container1.CN_ContainerNumber = "CN1";
			container1.Pivots.AddNew().CV_CA = hb1.PK;

			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			container1.Underbonds.Add(underbond);
			Factory.Save();

			return new ScanCusSCAOceanBill(oceanBill1);
		}

		ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj)
		{
			return new SeaScanForOutturnManager((ScanCusSCAOceanBill)scanObj);
		}
	}
}
