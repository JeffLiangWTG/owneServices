using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CodeType()
		{
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCode.OK_CustomsRegNo = "1234";
			var addr = organisation.Addresses.AddNew();
			cusCode.OK_OA_PremisesAddress = addr.PK;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCode2.OK_CustomsRegNo = "9876";
			var addr2 = organisation.Addresses.AddNew();
			cusCode2.OK_OA_PremisesAddress = addr2.PK;

			AssertNoMessageErrors("'TID' OrgCusCodes should allow multiple entries for each country", cusCode.OK_CodeTypeInfo);
			AssertNoMessageErrors("'TID' OrgCusCodes should allow multiple entries for each country", cusCode2.OK_CodeTypeInfo);
		}

		public void TestCheckOK_CustomsRegNo()
		{
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCode.OK_CustomsRegNo = "1234";
			var addr = organisation.Addresses.AddNew();
			cusCode.OK_OA_PremisesAddress = addr.PK;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			cusCode2.OK_CustomsRegNo = "4321";
			var addr2 = organisation.Addresses.AddNew();
			cusCode2.OK_OA_PremisesAddress = addr2.PK;

			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "Each 'TID' must have a unique code.");
			AssertNoMessageError(cusCode2.OK_CustomsRegNoInfo, "Each 'TID' must have a unique code.");

			cusCode.OK_CustomsRegNo = "4321";
			cusCode2.Validation.ValidateOK_CustomsRegNo();

			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Each 'TID' must have a unique code.");
			AssertHasMessageError(cusCode2.OK_CustomsRegNoInfo, "Each 'TID' must have a unique code.");

			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;

			cusCode.Validation.ValidateOK_CustomsRegNo();
			cusCode2.Validation.ValidateOK_CustomsRegNo();

			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "Each 'TID' must have a unique code.");
			AssertHasMessageError(cusCode2.OK_CustomsRegNoInfo, "Each 'TID' must have a unique code.");

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
			cusCode.OK_CustomsRegNo = "1234";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.");

			cusCode.OK_CustomsRegNo = "123456789012345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.");

			cusCode.OK_CustomsRegNo = "IM5221111111";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.");

			cusCode.OK_CustomsRegNo = "IM528a111111";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.");

			cusCode.OK_CustomsRegNo = "IM5281111111";
			AssertNoMessageError(cusCode.OK_CustomsRegNoInfo, "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit.");
		}

		public void TestCheckOK_CustomsRegNo_UniqueEORINumberNotRequired()
		{
			organisation.OH_Code = "ORG1";
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Austria;
			CreateGermanEORINumber_1234(cusCode);
			Factory.Save();

			CombineAssertions(() =>
			{
				var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
				organisation2.OH_Code = "ORG2";
				organisation2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;

				var cusCode2 = organisation2.CustomsCodes.AddNew();
				CreateGermanEORINumber_1234(cusCode2);
				AssertNoNotifications("Duplicate, but allowed for DE", cusCode2.OK_CustomsRegNoInfo);

				organisation2.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Austria;
				cusCode2.Validation.ValidateOK_CustomsRegNo();
				AssertNoNotifications("Duplicate, but allowed for AT", cusCode2.OK_CustomsRegNoInfo);
			});
		}

		public void TestCheckOK_CustomsRegNo_NoExceptionIfNoLinkedOrgHeader() => AssertNoExceptionThrown(() => CreateGermanEORINumber_1234(Factory.New<OrgCusCode>()));

		public void TestCheckOK_OA_PremisesAddress()
		{
			CombineAssertions(() =>
			{
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsOfficeForTransit;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				cusCode.OK_CustomsRegNo = "1234";
				cusCode.Validation.ValidateOK_OA_PremisesAddress();
				AssertNoMessageErrorContaining("Only one CTR orgCusCode exists, no messageError", cusCode.OK_OA_PremisesAddressInfo, MandatoryValidation.YouHaveNotEntered);

				var cusCode2 = organisation.CustomsCodes.AddNew();
				cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CustomsOfficeForTransit;
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				cusCode2.OK_CustomsRegNo = "2345";
				cusCode2.Validation.ValidateOK_OA_PremisesAddress();
				AssertHasMessageErrorContaining("Multiple CTR orgCusCode exists and current orgCusCode doesn't has OK_OA_PremisesAddress", cusCode2.OK_OA_PremisesAddressInfo, MandatoryValidation.YouHaveNotEntered);

				var addr = organisation.Addresses.AddNew();
				cusCode2.OK_OA_PremisesAddress = addr.PK;
				AssertNoMessageErrorContaining("Multiple CTR orgCusCode exists and current orgCusCode has OK_OA_PremisesAddress", cusCode2.OK_OA_PremisesAddressInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
		}
		OrgCusCode cusCode;
		OrgHeader organisation;

		void CreateGermanEORINumber_1234(OrgCusCode orgCusCode)
		{
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_CustomsRegNo = "1234";
		}
	}
}
