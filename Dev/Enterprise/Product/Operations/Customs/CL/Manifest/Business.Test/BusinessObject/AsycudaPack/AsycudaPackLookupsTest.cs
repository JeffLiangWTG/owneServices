using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypeListCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var pack = header.Bills.AddNew().Packs.AddNew();
			AssertPackageTypeList(pack.Lookups.PackUQList, true);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertPackageTypeList(pack.Lookups.PackUQList, false);
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
