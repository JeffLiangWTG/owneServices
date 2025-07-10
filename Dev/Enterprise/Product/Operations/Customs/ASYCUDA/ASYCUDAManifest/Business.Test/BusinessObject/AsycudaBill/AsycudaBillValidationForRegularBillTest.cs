using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSADOfficeCode()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "BD", "Bangladesh", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("BD", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JAS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.SADOfficeCode = "ZZ";
			AssertHasMessageError("Not in lookup list", bill.SADOfficeCodeInfo, ListValidation.InvalidCodeMessageError);

			bill.SADOfficeCode = "JAS";
			AssertNoErrors("valid value from lookup list", bill.SADOfficeCodeInfo);
		}

		public void TestCheckCustomsEntryNumberType_ShowExportGeneralManifestAndSADOfficeCodePopulated()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "BD", "Bangladesh", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("BD", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JAS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "BD";
			header.AMA_Nature = "EXP";
			var bill = header.Bills.AddNew();
			bill.SADOfficeCode = "JAS";
			bill.Header.AMA_RN_NKCountry = "BD";
			Factory.Save();

			bill.CustomsEntryNumbers.Reload(true);
			Assert("reload inside", bill.CustomsEntryNumbers.Count == 1);
			bill.Validation.ValidateCustomsEntryNumberType();
			AssertNoNotifications(bill.CustomsEntryNumberTypeInfo);
		}
	}
}
