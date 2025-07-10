using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsDischargePortList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Mexico, "PORT", "100", "Acapulco", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Mexico, "PORT", "500", "DF", yesterday, tomorrow);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;

			var list = header.Lookups.CustomsDischargePortList as BusinessObjectCollection;
			list.Load();

			AssertEquals(0, list.Count);

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_RL_NKPortOfDischarge = "MXACA";

			list = header.Lookups.CustomsDischargePortList as BusinessObjectCollection;
			list.Load();

			AssertEquals(2, list.Count);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "100"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "500"));
		}

		public void TestCustomsLoadPortList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Mexico, "PORT", "100", "Acapulco", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Mexico, "PORT", "500", "DF", yesterday, tomorrow);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
			list.Load();

			AssertEquals(0, list.Count);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_RL_NKPortOfLoading = "MXACA";

			list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
			list.Load();

			AssertEquals(2, list.Count);
			AssertType<ZZRefCusCodeListCombinedCollection>(list);
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "100"));
			Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "500"));
		}
	}
}
