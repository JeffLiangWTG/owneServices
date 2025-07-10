using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateE2_Email()
	{
		const string expectedMessageError = "Email Address is not valid";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			locationAddress = ((CusGoodsLocation)declaration.GoodsLocation).Address;
			CombineAssertions("UCC6+EXP", () =>
			{
				locationAddress.E2_Email = "hello";
				AssertHasMessageError("Invalid email address", locationAddress.E2_EmailInfo, expectedMessageError);

				locationAddress.E2_Email = "abc@email.com";
				AssertNoMessageErrorContaining("Valid email address", locationAddress.E2_EmailInfo, expectedMessageError);
			});

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			locationAddress = ((CusGoodsLocation)declaration.GoodsLocation).Address;
			AssertBaseBehavior("UCC6+IMP");
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			locationAddress = ((CusGoodsLocation)declaration.GoodsLocation).Address;
			AssertBaseBehavior("Non-UCC6+EXP");

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			locationAddress = ((CusGoodsLocation)declaration.GoodsLocation).Address;
			AssertBaseBehavior("Non-UCC6+IMP");
		}

		void AssertBaseBehavior(string groupAssertionMessage)
		{
			CombineAssertions(groupAssertionMessage, () =>
			{
				locationAddress.E2_Email = "hello";
				AssertHasErrorContaining("Invalid email address", locationAddress.E2_EmailInfo, expectedMessageError);

				locationAddress.E2_Email = "abc@email.com";
				AssertNoErrorContaining("Valid email address", locationAddress.E2_EmailInfo, expectedMessageError);
			});
		}
	}

	public void TestValidateE2_Address1AndE2_Address2()
	{
		const string expectedError = "You have not entered a Location: Street + Number";
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				var locationAddress = goodsLocation.Address;
				locationAddress.E2_Address1AndE2_Address2 = "ABC";
				locationAddress.Validation.ValidateE2_Address1AndE2_Address2();
				AssertNoMessageErrorContaining("Should not trigger validation if address is filled", locationAddress.E2_Address1AndE2_Address2Info, expectedError);

				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				locationAddress.Validation.ValidateE2_Address1AndE2_Address2();
				AssertHasMessageErrorContaining("Should trigger validation if address is empty", locationAddress.E2_Address1AndE2_Address2Info, expectedError);
			});
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				var locationAddress = goodsLocation.Address;
				locationAddress.E2_Address1AndE2_Address2 = "ABC";
				locationAddress.Validation.ValidateE2_Address1AndE2_Address2();
				AssertNoMessageErrorContaining("Should not trigger validation if address is filled", locationAddress.E2_Address1AndE2_Address2Info, expectedError);

				locationAddress.E2_Address1AndE2_Address2 = ZString.Empty;
				locationAddress.Validation.ValidateE2_Address1AndE2_Address2();
				AssertNoMessageErrorContaining("Should not trigger validation if address is empty", locationAddress.E2_Address1AndE2_Address2Info, expectedError);
			});
		}
	}

	public void TestAddressFieldsForCGL_QualifierZAndUcc6Export()
	{
		const string expectedPartialMessage = "You have not entered";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertAllAddressFieldsForMandatoryError("UCC6, Export, Qualifier=Z");

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			AssertAllAddressFieldsForNoMandatoryError("UCC6, Export, Qualifier=Y");

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertAllAddressFieldsForNoMandatoryError("UCC6, Import, Qualifier=Z");
		}

		void AssertAllAddressFieldsForMandatoryError(string assertionGroupMessage)
		{
			validation.ValidateE2_City();
			validation.ValidateE2_RN_NKCountryCode();

			CombineAssertions(assertionGroupMessage, () =>
			{
				AssertHasMessageErrorContaining("E2_City", locationAddress.E2_CityInfo, expectedPartialMessage);
				AssertHasMessageErrorContaining("E2_RN_NKCountryCode", locationAddress.E2_RN_NKCountryCodeInfo, expectedPartialMessage);
			});
		}

		void AssertAllAddressFieldsForNoMandatoryError(string assertionGroupMessage)
		{
			validation.ValidateE2_City();
			validation.ValidateE2_RN_NKCountryCode();

			CombineAssertions(assertionGroupMessage, () =>
			{
				AssertNoMessageErrorContaining("E2_City", locationAddress.E2_CityInfo, expectedPartialMessage);
				AssertNoMessageErrorContaining("E2_RN_NKCountryCode", locationAddress.E2_RN_NKCountryCodeInfo, expectedPartialMessage);
			});
		}
	}

	public void TestE2_RN_NKCountryCode_InvalidCode()
	{
		const string expectedErrorMessage = "The code you have selected is not in the list";
		locationAddress.E2_RN_NKCountryCode = "XX";
		AssertHasMessageErrorContaining("Invalid Country Code", locationAddress.E2_RN_NKCountryCodeInfo, expectedErrorMessage);

		locationAddress.E2_RN_NKCountryCode = "IT";
		AssertNoMessageErrorContaining("Invalid Country Code", locationAddress.E2_RN_NKCountryCodeInfo, expectedErrorMessage);
	}

	public void TestE2_Contact_RequiredForUCC6ExportPhoneIsFilledAndQualifierIsZ()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = "0123333333";
			validation.ValidateE2_Contact();
			AssertHasMessageErrorContaining("UCC6,EXP, CGL_Qualifier=Z, Contact is empty and Phone is filled", locationAddress.E2_ContactInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = "Company";
			locationAddress.E2_Phone = ZString.Empty;
			validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining("UCC6,EXP, CGL_Qualifier=Z, Contact is filled and Phone is empty", locationAddress.E2_ContactInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = "Company";
			locationAddress.E2_Phone = "0123333333";
			validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = ZString.Empty;
			validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = "0123333333";
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, nameAndPhoneAreRequiredErrorMessage);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = "0123333333";
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			validation.ValidateE2_Contact();
			AssertNoMessageErrorContaining(locationAddress.E2_ContactInfo, nameAndPhoneAreRequiredErrorMessage);
		}
	}

	public void TestE2_Phone_RequiredForUCC6ExportContactIsFilledAndQualifierIsZ()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;

			locationAddress.E2_Contact = "Company";
			locationAddress.E2_Phone = ZString.Empty;
			validation.ValidateE2_Phone();
			AssertHasMessageErrorContaining("UCC6,EXP, CGL_Qualifier=Z, Contact is filled and Phone is empty", locationAddress.E2_PhoneInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = "0123333333";
			validation.ValidateE2_Phone();
			AssertNoMessageErrorContaining("UCC6,EXP, CGL_Qualifier=Z, Contact is empty and Phone is filled", locationAddress.E2_PhoneInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = "Company";
			locationAddress.E2_Phone = "0123333333";
			validation.ValidateE2_Phone();
			AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = ZString.Empty;
			validation.ValidateE2_Phone();
			AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, nameAndPhoneAreRequiredErrorMessage);

			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = "0123333333";
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			validation.ValidateE2_Phone();
			AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, nameAndPhoneAreRequiredErrorMessage);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			locationAddress.E2_Contact = ZString.Empty;
			locationAddress.E2_Phone = "0123333333";
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			validation.ValidateE2_Phone();
			AssertNoMessageErrorContaining(locationAddress.E2_PhoneInfo, nameAndPhoneAreRequiredErrorMessage);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		locationAddress = goodsLocation.Address;
		locationAddress.E2_AddressOverride = ZBool.True;
		validation = new CusGoodsLocationAddressValidation(locationAddress);
	}

	JobDeclaration declaration;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationAddress locationAddress;
	CusGoodsLocationAddressValidation validation;

	const string nameAndPhoneAreRequiredErrorMessage = "Name and Phone Number must be both filled or both empty";
}
