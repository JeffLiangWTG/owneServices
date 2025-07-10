using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackedItemEntryNumValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryType()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "CustomsEntryNumberTypes");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "TNP", "TradeNet Permit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var packedItem = GetPackedItem(Core.Constants.CountryCodes.Singapore, "MGI");
			var entryNum = packedItem.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234";
			entryNum.Validation.ValidateCE_EntryType();
			AssertHasMessageErrorContaining(entryNum.CE_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);

			entryNum.CE_EntryType = "DK@";
			AssertNoMessageErrorContaining(entryNum.CE_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(entryNum.CE_EntryTypeInfo, ListValidation.InvalidCodeMessageError);

			entryNum.CE_EntryType = "TNP";
			AssertNoMessageErrors(entryNum.CE_EntryTypeInfo);

			packedItem = GetPackedItem(Core.Constants.CountryCodes.Vanuatu, "ASY");
			entryNum = packedItem.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryNum = "1234";
			entryNum.CE_EntryType = "DK@";
			AssertNoMessageErrors(entryNum.CE_EntryTypeInfo);
		}

		AsycudaPackedItem GetPackedItem(ZString countryCode, ZString manifestType)
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, countryCode, manifestType);
			header.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			return packedItem;
		}
	}
}
