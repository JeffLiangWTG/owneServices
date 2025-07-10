using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCLREGInfoProviderAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZA_ABN()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_ABN();
			AssertNoMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.ABNLength);
			AssertHasMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.MessagingModeShouldBeSelected);

			dataProvider.ZA_ABN = "123456";
			AssertHasMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.ABNLength);
			AssertNoMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.MessagingModeShouldBeSelected);

			dataProvider.ZA_ABN = "45632145252";
			AssertNoMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.ABNLength);
			AssertHasMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.RollRequired);

			dataProvider.Rolls.AddNew();
			AssertNoMessageError(dataProvider.ZA_ABNInfo, AUCLREGInfoProviderAddInfoValidation.RollRequired);
		}

		public void TestCheckZA_ABNInd()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_ABNInd = "@";
			AssertHasMessageError(dataProvider.ZA_ABNIndInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_ABNInd = dataProvider.AddInfoLookups.ABNNominatedClientTypeList[0].Code;
			AssertNoMessageError(dataProvider.ZA_ABNIndInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZA_CAC()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_CAC();
			AssertNoMessageError(dataProvider.ZA_CACInfo, AUCLREGInfoProviderAddInfoValidation.CACLength);
			AssertNoMessageError(dataProvider.ZA_CACInfo, AUCLREGInfoProviderAddInfoValidation.CACShouldBeBlank);

			dataProvider.ZA_CAC = "123";
			AssertHasMessageError(dataProvider.ZA_CACInfo, AUCLREGInfoProviderAddInfoValidation.CACShouldBeBlank);

			dataProvider.ZA_ABN = "123456789";
			dataProvider.AddInfoValidation.ValidateZA_CAC();
			AssertNoMessageError(dataProvider.ZA_CACInfo, AUCLREGInfoProviderAddInfoValidation.CACShouldBeBlank);
			AssertNoMessageError(dataProvider.ZA_CACInfo, AUCLREGInfoProviderAddInfoValidation.CACLength);

			dataProvider.ZA_CAC = "1";
			AssertHasMessageError(dataProvider.ZA_CACInfo, AUCLREGInfoProviderAddInfoValidation.CACLength);
		}

		public void TestCheckZA_CACType()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_CACType();
			AssertNoMessageError(dataProvider.ZA_CACTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(dataProvider.ZA_CACTypeInfo, AUCLREGInfoProviderAddInfoValidation.CACTypeShouldBeBlank);

			dataProvider.ZA_ABN = "45632578965";
			dataProvider.ZA_CACType = "12";
			AssertHasMessageError(dataProvider.ZA_CACTypeInfo, AUCLREGInfoProviderAddInfoValidation.CACTypeShouldBeBlank);

			dataProvider.ZA_CAC = "123";
			dataProvider.AddInfoValidation.ValidateZA_CACType();
			AssertNoMessageError(dataProvider.ZA_CACTypeInfo, AUCLREGInfoProviderAddInfoValidation.CACTypeShouldBeBlank);

			dataProvider.ZA_CACType = "1";
			AssertHasMessageError(dataProvider.ZA_CACTypeInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_CACType = "BA";
			AssertNoMessageError(dataProvider.ZA_CACTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckZA_IsIndividual()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_IsIndiv();
			AssertNoError(dataProvider.ZA_IsIndivInfo, AUCLREGInfoProviderAddInfoValidation.IsIndividualOrOrganization);

			dataProvider.ZA_ABN = "123456789";
			dataProvider.ZA_IsIndiv = true;
			AssertHasError(dataProvider.ZA_IsIndivInfo, AUCLREGInfoProviderAddInfoValidation.IsIndividualOrOrganization);

			dataProvider.ZA_ABN = "";
			dataProvider.ZA_IsOrg = true;
			dataProvider.AddInfoValidation.ValidateZA_IsIndiv();
			AssertHasError(dataProvider.ZA_IsIndivInfo, AUCLREGInfoProviderAddInfoValidation.IsIndividualOrOrganization);
		}

		public void TestCheckZA_IsOrganisation()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_IsOrg();
			AssertNoError(dataProvider.ZA_IsOrgInfo, AUCLREGInfoProviderAddInfoValidation.IsIndividualOrOrganization);

			dataProvider.ZA_ABN = "123456789";
			dataProvider.ZA_IsIndiv = true;
			dataProvider.ZA_IsOrg = true;
			AssertHasError(dataProvider.ZA_IsOrgInfo, AUCLREGInfoProviderAddInfoValidation.IsIndividualOrOrganization);

			dataProvider.ZA_IsIndiv = false;
			dataProvider.AddInfoValidation.ValidateZA_IsOrg();
			AssertHasError(dataProvider.ZA_IsOrgInfo, AUCLREGInfoProviderAddInfoValidation.IsIndividualOrOrganization);
		}

		public void TestCheckZA_BusinessName()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_BusinessName();
			AssertNoMessageError(dataProvider.ZA_BusinessNameInfo, AUCLREGInfoProviderAddInfoValidation.BusinessNameRequired);

			dataProvider.ZA_IsOrg = true;
			dataProvider.AddInfoValidation.ValidateZA_BusinessName();
			AssertHasMessageError(dataProvider.ZA_BusinessNameInfo, AUCLREGInfoProviderAddInfoValidation.BusinessNameRequired);

			dataProvider.ZA_BusinessName = "test";
			AssertNoMessageError(dataProvider.ZA_BusinessNameInfo, AUCLREGInfoProviderAddInfoValidation.BusinessNameRequired);
		}

		public void TestCheckZA_FamilyName()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_FamilyName();
			AssertNoMessageError(dataProvider.ZA_FamilyNameInfo, AUCLREGInfoProviderAddInfoValidation.FamilyNameRequired);

			dataProvider.ZA_IsIndiv = true;
			dataProvider.AddInfoValidation.ValidateZA_FamilyName();
			AssertHasMessageError(dataProvider.ZA_FamilyNameInfo, AUCLREGInfoProviderAddInfoValidation.FamilyNameRequired);
			dataProvider.ZA_FamilyName = "test";
			AssertNoMessageError(dataProvider.ZA_FamilyNameInfo, AUCLREGInfoProviderAddInfoValidation.FamilyNameRequired);
		}

		public void TestCheckZA_Gender()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_Gender = "@";
			AssertHasMessageError(dataProvider.ZA_GenderInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_Gender = CMRGenderCodes.Codes.NotSpecified;
			AssertNoMessageError(dataProvider.ZA_GenderInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(dataProvider.ZA_GenderInfo, MandatoryValidation.YouHaveNotEntered);

			dataProvider.ZA_IsIndiv = true;
			dataProvider.ZA_Gender = "";
			AssertHasMessageErrorContaining(dataProvider.ZA_GenderInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZA_IsExDocsUser()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			var roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.SeaCargoReporter;
			dataProvider.Rolls.Add(roll);

			dataProvider.ZA_IsExDocsUser = true;
			AssertHasMessageError(dataProvider.ZA_IsExDocsUserInfo, AUCLREGInfoProviderAddInfoValidation.ExDocsUser);

			roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.Exporter;
			dataProvider.Rolls.Add(roll);
			dataProvider.AddInfoValidation.ValidateZA_IsExDocsUser();
			AssertNoMessageError(dataProvider.ZA_IsExDocsUserInfo, AUCLREGInfoProviderAddInfoValidation.ExDocsUser);
		}

		public void TestCheckTitle()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.AddInfoValidation.ValidateZA_Title();
			AssertNoMessageError(dataProvider.ZA_TitleInfo, AUCLREGInfoProviderAddInfoValidation.TitleIsMandatory);

			dataProvider.ZA_IsIndiv = true;
			dataProvider.AddInfoValidation.ValidateZA_Title();
			AssertHasMessageError(dataProvider.ZA_TitleInfo, AUCLREGInfoProviderAddInfoValidation.TitleIsMandatory);

			dataProvider.ZA_Title = "Ms";
			AssertNoMessageError(dataProvider.ZA_TitleInfo, AUCLREGInfoProviderAddInfoValidation.TitleIsMandatory);
		}

		public void TestCheckBusinessAddressProperties()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_IsOrg = true;
			dataProvider.AddInfoValidation.ValidateZA_Bsn1();
			dataProvider.AddInfoValidation.ValidateZA_BsnCity();
			dataProvider.AddInfoValidation.ValidateZA_BsnPostCode();
			dataProvider.AddInfoValidation.ValidateZA_BsnPort();
			dataProvider.AddInfoValidation.ValidateZA_BsnState();

			AssertHasMessageErrorContaining(dataProvider.ZA_Bsn1Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(dataProvider.ZA_BsnCityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(dataProvider.ZA_BsnPostCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(dataProvider.ZA_BsnPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(dataProvider.ZA_BsnStateInfo, MandatoryValidation.YouHaveNotEntered);

			dataProvider.ZA_BsnPort = "!";
			AssertHasMessageErrorContaining(dataProvider.ZA_BsnPortInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_BsnState = "!";
			AssertHasMessageErrorContaining(dataProvider.ZA_BsnStateInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_Bsn1 = "test 1";
			dataProvider.ZA_BsnCity = "LONDON";
			dataProvider.ZA_BsnPostCode = "12345";
			dataProvider.ZA_BsnPort = "AUSYD";
			dataProvider.ZA_BsnState = "NSW";
			AssertNoMessageErrorContaining(dataProvider.ZA_Bsn1Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(dataProvider.ZA_BsnCityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(dataProvider.ZA_BsnPostCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(dataProvider.ZA_BsnPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(dataProvider.ZA_BsnStateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(dataProvider.ZA_BsnPortInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(dataProvider.ZA_BsnStateInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_IsOrg = false;
			dataProvider.ZA_ABN = "123456";
			dataProvider.ZA_Bsn2 = "test 2";
			dataProvider.AddInfoValidation.ValidateAll();
			AssertHasMessageError(dataProvider.ZA_Bsn1Info, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertHasMessageError(dataProvider.ZA_Bsn2Info, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertHasMessageError(dataProvider.ZA_BsnCityInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertHasMessageError(dataProvider.ZA_BsnPostCodeInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertHasMessageError(dataProvider.ZA_BsnPortInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertHasMessageError(dataProvider.ZA_BsnStateInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);

			dataProvider.ZA_Bsn1 = "";
			dataProvider.ZA_BsnCity = "";
			dataProvider.ZA_BsnPostCode = "";
			dataProvider.ZA_BsnPort = "";
			dataProvider.ZA_BsnState = "";
			dataProvider.ZA_Bsn2 = "";
			AssertNoMessageError(dataProvider.ZA_Bsn1Info, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertNoMessageError(dataProvider.ZA_Bsn2Info, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertNoMessageError(dataProvider.ZA_BsnCityInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertNoMessageError(dataProvider.ZA_BsnPostCodeInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertNoMessageError(dataProvider.ZA_BsnPortInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
			AssertNoMessageError(dataProvider.ZA_BsnStateInfo, AUCLREGInfoProviderAddInfoValidation.BusinessAddressNotRequired);
		}

		public void TestCheckPostalAddressProperties()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_PostPort = "!";
			AssertHasMessageErrorContaining(dataProvider.ZA_PostPortInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_PostState = "!";
			AssertHasMessageErrorContaining(dataProvider.ZA_PostStateInfo, ListValidation.InvalidCodeMessageError);

			dataProvider.ZA_PostPort = "AUSYD";
			dataProvider.ZA_PostState = "NSW";
			AssertNoMessageErrorContaining(dataProvider.ZA_PostPortInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(dataProvider.ZA_PostStateInfo, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2011, 06, 29)]
		public void TestZA_DateofBirth()
		{
			var wrapper = new OrgHeaderWrapper(Factory.New<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;

			dataProvider.ZA_DateofBirth = new ZDateTime(1909, 10, 28);
			AssertNoNotifications("No limitation in the past", dataProvider.ZA_DateofBirthInfo);

			dataProvider.ZA_DateofBirth = new ZDateTime(2011, 07, 01);
			AssertHasMessageError("No Future date for Date of Birth", dataProvider.ZA_DateofBirthInfo, AUCLREGInfoProviderAddInfoValidation.DOBFutureDate);

			dataProvider.ZA_DateofBirth = new ZDateTime(1959, 10, 28);
			AssertNoMessageError("No Future date for Date of Birth", dataProvider.ZA_DateofBirthInfo, AUCLREGInfoProviderAddInfoValidation.DOBFutureDate);
		}
	}
}
