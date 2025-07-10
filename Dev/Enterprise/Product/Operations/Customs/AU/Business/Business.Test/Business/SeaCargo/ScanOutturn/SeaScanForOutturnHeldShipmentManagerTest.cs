using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaScanForOutturnHeldShipmentManager))]
	sealed class SeaScanForOutturnHeldShipmentManagerTest : ScanForOutturnHeldShipmentManagerTest<CusSCAOceanBill, CusSCAHouse>
	{
		protected override ScanForOutturnHeldShipmentManager GetScanForOutturnHeldShipmentManager()
		{
			return new SeaScanForOutturnHeldShipmentManager(Factory);
		}

		protected override CusSCAHouse CreateHouseBill(CusSCAOceanBill master, string billNo, int manifestQuantity, string customsStatus, bool isHeld)
		{
			var result = master.HouseBills.AddNew();
			result.CA_HouseBill = billNo;
			var pivot = result.Pivot.AddNew();
			pivot.CV_CN = master.Containers[0].PK;
			pivot.CV_PackageCount = manifestQuantity;
			pivot.CV_CargoStatus = customsStatus;
			pivot.CV_IsHeldAtOutturn = isHeld;
			return result;
		}

		protected override CusSCAOceanBill CreateMasterBill(string masterBillNo)
		{
			var result = Factory.New<CusSCAOceanBill>();
			result.CB_OceanBill = masterBillNo;
			var container = result.Containers.AddNew();
			container.CN_ContainerNumber = "CN1234";
			return result;
		}

		protected override void AssertIsHeldAtOutturn(CusSCAHouse house)
		{
			Assert(house.Pivot[0].CV_IsHeldAtOutturn);
		}

		protected override ZArchitecture.Environment.CodePairRegistryItem CONClearReleaseStatusRegistry
		{
			get { return AUCustomsDataRegistry.Instance.SeaCargoOutturnScanningCONDCLEARReleaseStatus; }
		}
	}
}
