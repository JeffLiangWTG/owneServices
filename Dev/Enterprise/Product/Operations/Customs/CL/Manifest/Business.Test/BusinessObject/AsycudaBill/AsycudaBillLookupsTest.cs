using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGoods_LocationLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var bill = header.Bills.AddNew();
			AssertType(typeof(WarehouseClientCollection), bill.Lookups.GoodsLocations);
		}

		public void TestPackageTypeListCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();
			AssertPackageTypeList(bill.Lookups.PackageTypeList, true);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertPackageTypeList(bill.Lookups.PackageTypeList, false);
		}

		void AssertPackageTypeList(CodeDescriptionPairList list, ZBool shouldCodeBeContained)
		{
			foreach (var uq in new SeaPackageTypeList().GetAllCodes())
			{
				AssertEquals(shouldCodeBeContained, list.ContainsCode(uq));
			}
		}
	}
}
