using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateE2_Email()
	{
		const string expectedMessageError = "Email Address is not valid";

		CombineAssertions("NCTS5 + Departure", () =>
		{
			locationAddress.E2_Email = "hello";
			AssertHasMessageErrorContaining("Email Address is not valid", locationAddress.E2_EmailInfo, expectedMessageError);

			locationAddress.E2_Email = "abc@email.com";
			AssertNoMessageErrorContaining("Valid email address", locationAddress.E2_EmailInfo, expectedMessageError);
		});

		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		validation.ValidateE2_Email();
		CombineAssertions("NCTS5 + Arrival", () =>
		{
			locationAddress.E2_Email = "hello";
			AssertHasMessageErrorContaining("Email Address is not valid", locationAddress.E2_EmailInfo, expectedMessageError);

			locationAddress.E2_Email = "abc@email.com";
			AssertNoMessageErrorContaining("Valid email address", locationAddress.E2_EmailInfo, expectedMessageError);
		});
	}

	public void TestE2_RN_NKCountryCode_InvalidCode()
	{
		const string expectedErrorMessage = "The code you have selected is not in the list";
		locationAddress.E2_RN_NKCountryCode = "XX";
		AssertHasMessageErrorContaining("Invalid Country Code", locationAddress.E2_RN_NKCountryCodeInfo, expectedErrorMessage);

		locationAddress.E2_RN_NKCountryCode = "IT";
		AssertNoMessageErrorContaining("Invalid Country Code", locationAddress.E2_RN_NKCountryCodeInfo, expectedErrorMessage);
	}

	public void TestE2_Contact_RequiredForNCTS5PhoneIsFilledAndQualifierIsZ()
	{
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = "0123333333";

		validation.ValidateE2_Contact();
		AssertHasMessageErrorContaining("NCTS5-Departure, CGL_Qualifier=Z, Contact is empty and Phone is filled", locationAddress.E2_ContactInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = "Company";
		locationAddress.E2_Phone = ZString.Empty;
		validation.ValidateE2_Contact();
		AssertNoMessageErrorContaining("NCTS5-Departure, CGL_Qualifier=Z, Contact is filled and Phone is empty", locationAddress.E2_ContactInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = "Company";
		locationAddress.E2_Phone = "0123333333";
		validation.ValidateE2_Contact();
		AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = ZString.Empty;
		validation.ValidateE2_Contact();
		AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = "0123333333";
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		validation.ValidateE2_Contact();
		AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, NameAndPhoneAreRequiredErrorMessage);

		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = "0123333333";
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
		validation.ValidateE2_Contact();
		AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, NameAndPhoneAreRequiredErrorMessage);
	}

	public void TestE2_Phone_RequiredForNCTS5ContactIsFilledAndQualifierIsZ()
	{
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

		locationAddress.E2_Contact = "Company";
		locationAddress.E2_Phone = ZString.Empty;
		validation.ValidateE2_Phone();
		AssertHasMessageErrorContaining("NCTS5-Departure, CGL_Qualifier=Z, Contact is filled and Phone is empty", locationAddress.E2_PhoneInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = "0123333333";
		validation.ValidateE2_Phone();
		AssertNoMessageErrorContaining("NCTS5-Departure, CGL_Qualifier=Z, Contact is empty and Phone is filled", locationAddress.E2_PhoneInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = "Company";
		locationAddress.E2_Phone = "0123333333";
		validation.ValidateE2_Phone();
		AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = ZString.Empty;
		validation.ValidateE2_Phone();
		AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, NameAndPhoneAreRequiredErrorMessage);

		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = "0123333333";
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		validation.ValidateE2_Phone();
		AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, NameAndPhoneAreRequiredErrorMessage);

		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		locationAddress.E2_Contact = ZString.Empty;
		locationAddress.E2_Phone = "0123333333";
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
		validation.ValidateE2_Phone();
		AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, NameAndPhoneAreRequiredErrorMessage);
	}

	public void TestCheckRuleCN0394_AddressFieldsHaveErrorWhenEmpty()
	{
		goodsLocation.CGL_Qualifier = "Z";
		locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
		locationAddress.E2_City = ZString.Empty;
		locationAddress.E2_RN_NKCountryCode = ZString.Empty;

		CombineAssertions("Rule CN0394 error must be present", () =>
		{
			AssertHasMessageErrorContaining($"{nameof(locationAddress.E2_Address1AndE2_Address2)}", locationAddress.E2_Address1AndE2_Address2Info, "CN0394");
			AssertHasMessageErrorContaining($"{nameof(locationAddress.E2_CityInfo)}", locationAddress.E2_CityInfo, "CN0394");
			AssertHasMessageErrorContaining($"{nameof(locationAddress.E2_RN_NKCountryCodeInfo)}", locationAddress.E2_RN_NKCountryCodeInfo, "CN0394");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		locationAddress = goodsLocation.Address;
		locationAddress.E2_AddressOverride = ZBool.True;
		validation = new CusGoodsLocationAddressValidation(locationAddress);
	}

	NctsHeader nctsHeader;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationAddress locationAddress;
	CusGoodsLocationAddressValidation validation;

	const string NameAndPhoneAreRequiredErrorMessage = "Name and Phone Number must be both filled or both empty";
}
