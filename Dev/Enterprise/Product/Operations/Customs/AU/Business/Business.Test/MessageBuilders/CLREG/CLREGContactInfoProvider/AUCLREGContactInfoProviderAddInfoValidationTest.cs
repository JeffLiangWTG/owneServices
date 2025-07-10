using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCLREGContactInfoProviderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckContactPhones()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_IsOrg = true;

			var contactDataProvider = dataProvider.CLREGContactInfoProvider;
			contactDataProvider.AddInfoValidation.ValidateZA_ContAH();
			contactDataProvider.AddInfoValidation.ValidateZA_ContEmail();
			contactDataProvider.AddInfoValidation.ValidateZA_ContFax();
			contactDataProvider.AddInfoValidation.ValidateZA_ContMob();
			contactDataProvider.AddInfoValidation.ValidateZA_ContPh();
			AssertHasMessageError(contactDataProvider.ZA_ContAHInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertHasMessageError(contactDataProvider.ZA_ContEmailInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertHasMessageError(contactDataProvider.ZA_ContFaxInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertHasMessageError(contactDataProvider.ZA_ContMobInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertHasMessageError(contactDataProvider.ZA_ContPhInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);

			var errorString = string.Format(AUCLREGContactInfoProviderAddInfoValidation.PhPrefixRequired, "Phone");
			contactDataProvider.ZA_ContPh = "123456";
			AssertHasMessageError(contactDataProvider.ZA_ContPhPrefInfo, errorString);

			contactDataProvider.ZA_ContPhPref = "04";
			AssertNoMessageError(contactDataProvider.ZA_ContPhPrefInfo, errorString);

			errorString = string.Format(AUCLREGContactInfoProviderAddInfoValidation.PhPrefixRequired, "Fax");
			contactDataProvider.ZA_ContFax = "123456";
			AssertHasMessageError(contactDataProvider.ZA_ContFaxPrefInfo, errorString);

			contactDataProvider.ZA_ContFaxPref = "04";
			AssertNoMessageError(contactDataProvider.ZA_ContFaxPrefInfo, errorString);

			errorString = string.Format(AUCLREGContactInfoProviderAddInfoValidation.PhPrefixRequired, "After Hours Phone");
			contactDataProvider.ZA_ContAH = "123456";
			AssertHasMessageError(contactDataProvider.ZA_ContAHPrefInfo, errorString);

			contactDataProvider.ZA_ContAHPref = "04";
			AssertNoMessageError(contactDataProvider.ZA_ContAHPrefInfo, errorString);

			contactDataProvider.ZA_ContEmail = "test@test.com";
			contactDataProvider.AddInfoValidation.ValidateZA_ContAH();
			AssertNoMessageError(contactDataProvider.ZA_ContAHInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertNoMessageError(contactDataProvider.ZA_ContEmailInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertNoMessageError(contactDataProvider.ZA_ContFaxInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertNoMessageError(contactDataProvider.ZA_ContMobInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
			AssertNoMessageError(contactDataProvider.ZA_ContPhInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequired);
		}

		public void TestCheckContactAddressProperties()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var contactDataProvider = wrapper.CLREGInfoProvider.CLREGContactInfoProvider;

			contactDataProvider.ZA_ContPort = "!";
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPortInfo, ListValidation.InvalidCodeMessageError);

			contactDataProvider.ZA_ContState = "!";
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContStateInfo, ListValidation.InvalidCodeMessageError);

			contactDataProvider.ZA_ContPort = "AUSYD";
			contactDataProvider.ZA_ContState = "NSW";
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPortInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContStateInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckContactPostalAddressProperties()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var contactDataProvider = wrapper.CLREGInfoProvider.CLREGContactInfoProvider;
			contactDataProvider.ZA_ContPostPort = "!";
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPostPortInfo, ListValidation.InvalidCodeMessageError);

			contactDataProvider.ZA_ContPostState = "!";
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPostStateInfo, ListValidation.InvalidCodeMessageError);

			wrapper.CLREGInfoProvider.ZA_IsIndiv = true;
			contactDataProvider.ZA_ContPostPort = "AUSYD";
			contactDataProvider.ZA_ContPostState = "NSW";
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPostPortInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPostStateInfo, ListValidation.InvalidCodeMessageError);

			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPost1Info, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPostCityInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPostPortInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPostStateInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertNoMessageErrorContaining(contactDataProvider.ZA_ContPostPostCodeInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);

			contactDataProvider.ZA_ContPostPort = "";
			contactDataProvider.ZA_ContPostState = "";
			contactDataProvider.ZA_ContPost1 = "";
			contactDataProvider.ZA_ContPostCity = "";
			contactDataProvider.ZA_ContPostPostCode = "";

			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPost1Info, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPostCityInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPostPortInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPostStateInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
			AssertHasMessageErrorContaining(contactDataProvider.ZA_ContPostPostCodeInfo, AUCLREGContactInfoProviderAddInfoValidation.ContactDataRequiredForIndiv);
		}
	}
}
