using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSADOfficeCodeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "BD", "Bangladesh", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attpk = helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");

			helper.CreateNewOrGetExistingCusCodeList("BD", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JAS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "VU";
			header.AMA_RL_NKPortOfLoading = "VUSAN";
			var bill = header.Bills.AddNew();
			var headerLookups = header.Lookups;
			var billLookups = bill.Lookups;
			var customsOffices = (CodeDescriptionPairList)headerLookups.CustomsOffices;
			AssertEquals("CustomsOffices show customs office for ManifestCountry", true, customsOffices.ContainsCode("SAIR"));
			AssertEquals("Same list for CustomsOffices and SADOfficeCodeList", billLookups.SADOfficeCodeList, headerLookups.CustomsOffices);

			header.AMA_RN_NKCountry = "BD";
			AssertEquals("Same list for CustomsOffices and SADOfficeCodeList", billLookups.SADOfficeCodeList, headerLookups.CustomsOffices);
			AssertEquals("SADOfficeCodeList show customs office for BD", true, billLookups.SADOfficeCodeList.ContainsCode("JAS"));
		}
	}
}
