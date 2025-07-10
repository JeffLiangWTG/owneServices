using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AirScanForOutturnHeldShipmentManager))]
	sealed class AirScanForOutturnHeldShipmentManagerTest : ScanForOutturnHeldShipmentManagerTest<CusMAWB, CusHAWB>
	{
		protected override void AssertIsHeldAtOutturn(CusHAWB house)
		{
			Assert(house.CS_IsHeldAtOutturn);
		}

		protected override CusMAWB CreateMasterBill(string masterBillNo)
		{
			var result = Factory.New<CusMAWB>();
			result.CM_MAWB = masterBillNo;
			return result;
		}

		protected override CusHAWB CreateHouseBill(CusMAWB master, string billNo, int manifestQuantity, string customsStatus, bool isHeld)
		{
			var result = master.ChildBills.AddNew();
			result.CS_HAWB = billNo;
			result.CS_PiecesManifested = (short)manifestQuantity;
			result.CS_CustomsStatus = customsStatus;
			result.CS_IsHeldAtOutturn = isHeld;
			return result;
		}

		protected override ScanForOutturnHeldShipmentManager GetScanForOutturnHeldShipmentManager() => new AirScanForOutturnHeldShipmentManager(Factory);

		protected override ZArchitecture.Environment.CodePairRegistryItem CONClearReleaseStatusRegistry => AUCustomsDataRegistry.Instance.AirCargoOutturnScanningCONDCLEARReleaseStatus;
	}
}
