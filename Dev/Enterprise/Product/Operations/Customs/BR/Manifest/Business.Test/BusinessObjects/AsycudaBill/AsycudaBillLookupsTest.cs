using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BR.Manifest.Business.Test
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGoods_LocationLookups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DischargePortTerminalOperator, "Discharge Port Terminal Operator");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DischargePortTerminalOperator, "60", "MER 1", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DischargePortTerminalOperator, "62", "MER 2", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "01", "MURC 1", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "02", "MURC 2", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "03", "MURC 3", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = "MER";

			var bill = header.Bills.AddNew();
			var refCusCodeList = bill.Lookups.Locations;

			AssertEquals(2, refCusCodeList.Count);

			header.AMA_ManifestType = "MUCR";
			var bill2 = header.Bills.AddNew();
			var refCusCodeList2 = bill.Lookups.Locations;
			AssertEquals(3, refCusCodeList2.Count);
		}

		public void TestFRTModeLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = "MER";

			var bill = header.Bills.AddNew();
			var frtModeList = bill.Lookups.FRTModes;

			AssertEquals(4, frtModeList.Count);
		}
	}
}
