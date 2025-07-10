using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackedItemEntryNumLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsEntryNumberTypesShowsTypesForRelevantCountry()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "CustomsEntryNumberTypes");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "SAIR", "Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "AIR", "VUSAN");

			helper.CreateNewOrGetExistingCusCodeList("PG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "JAS", "Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "HIRH", "Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var country = GetPackedItem("VU");
			var entryNumber = country.CustomsEntryNumbers.AddNew();
			AssertEquals(true, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("SAIR"));
			AssertEquals(false, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("JAS"));
			country = GetPackedItem("PG");
			entryNumber = country.CustomsEntryNumbers.AddNew();
			AssertEquals(true, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("JAS"));
			AssertEquals(false, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("SAIR"));
			country = GetPackedItem("SB");
			entryNumber = country.CustomsEntryNumbers.AddNew();
			AssertEquals(true, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("HIRH"));
			AssertEquals(false, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("SAIR"));
		}

		AsycudaPackedItem GetPackedItem(ZString countryCode)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_RN_NKCountry = countryCode;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			return packedItem;
		}
	}
}
