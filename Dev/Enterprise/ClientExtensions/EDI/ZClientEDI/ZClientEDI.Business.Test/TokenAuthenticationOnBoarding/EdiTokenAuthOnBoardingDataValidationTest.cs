using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.Testing
{
	class EdiTokenAuthOnBoardingDataValidationTest : BusinessObjectValidationTestCase
	{
		const string InvalidCode = "123";

		public void TestValidateTOD_IDT()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			tenant.IDT_AuthorityUrl = "https://www.example.com";
			tenant.IDT_GraphClientId = ZGuid.NewZGuid().ToString();
			tenant.IDT_Name = "Test";
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_TenantId = ZGuid.NewZGuid().ToString();
			tenant2.IDT_AuthorityUrl = "https://www.example2.com";
			tenant2.IDT_GraphClientId = ZGuid.NewZGuid().ToString();
			tenant2.IDT_Name = "Test2";
			tenant2.IDT_Onboarding = true;
			Factory.Save();
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_IDT = ZGuid.Empty;
			Assert(ediTokenAuthOnBoardingData.TOD_IDTInfo.HasErrors());
			Assert(ediTokenAuthOnBoardingData.TOD_IDTInfo.GetErrors().Contains("Please enter a value."));
			ediTokenAuthOnBoardingData.TOD_IDT = tenant.PK;
			Assert(ediTokenAuthOnBoardingData.TOD_IDTInfo.HasErrors());
			Assert(ediTokenAuthOnBoardingData.TOD_IDTInfo.GetErrors().Contains("Only onboarding related tenants can be selected."));
			ediTokenAuthOnBoardingData.TOD_IDT = tenant2.PK;
			Assert(!ediTokenAuthOnBoardingData.TOD_IDTInfo.HasErrors());
		}

		public void TestValidateTOD_ValidTokenIssuerPrefix()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();

			ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefix = "";
			AssertMandatoryValidationError(ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefixInfo, isExpectingError: false);

			ediTokenAuthOnBoardingData.TOD_OIDCServer = "OKT";
			ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefix = "";
			AssertMandatoryValidationError(ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefixInfo, isExpectingError: true);

			ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefix = "https://ValidTokenIssuerPrefixInfo";
			AssertMandatoryValidationError(ediTokenAuthOnBoardingData.TOD_ValidTokenIssuerPrefixInfo, isExpectingError: false);
		}

		public void TestValidateClaimMappingIdentifier()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ValidateList(ediTokenAuthOnBoardingData.TOD_ClaimMappingIdentifierInfo,
				ediTokenAuthOnBoardingData.Lookups.ClaimMappingIdentifiersList, isMandatory: true);
		}

		public void TestValidateOIDCServer()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ValidateList(ediTokenAuthOnBoardingData.TOD_OIDCServerInfo,
				ediTokenAuthOnBoardingData.Lookups.OIDCServerTypesList, isMandatory: true);
		}

		public void TestValidateStatus()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ValidateList(ediTokenAuthOnBoardingData.TOD_StatusInfo,
				ediTokenAuthOnBoardingData.Lookups.OnBoardingStatusList, isMandatory: false);
		}

		public void TestValidateAll()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			var allProperties = ediTokenAuthOnBoardingData.ZPropertyInfoHash;
			// Sanity check: verify that the new (not auto) properties are present in the list
			AssertCollectionContains(ediTokenAuthOnBoardingData.VerificationResultInfo, allProperties);
			AssertCollectionContains(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo, allProperties);
			// Set a dummy validation for all properties
			foreach (ZPropertyInfo propertyInfo in allProperties)
			{
				propertyInfo.AdditionalValidation += () => propertyInfo.AddWarning(propertyInfo.Name);
			}
			// Call ValidateAll
			ediTokenAuthOnBoardingData.Validation.ValidateAll();
			// All properties should have been validated and contain the dummy validation
			CombineAssertions(() =>
			{
				foreach (ZPropertyInfo propertyInfo in allProperties)
				{
					if (propertyInfo.Name != ediTokenAuthOnBoardingData.EnvironmentInfo.Name
					&& propertyInfo.Name != EdiTokenAuthOnBoardingData.Schema.TOD_SystemCreateTimeUtc
					&& propertyInfo.Name != EdiTokenAuthOnBoardingData.Schema.TOD_SystemCreateUser
					&& propertyInfo.Name != EdiTokenAuthOnBoardingData.Schema.TOD_SystemLastEditTimeUtc
					&& propertyInfo.Name != EdiTokenAuthOnBoardingData.Schema.TOD_SystemLastEditUser)
					{
						AssertHasWarning(propertyInfo, propertyInfo.Name);
					}
				}
			});
		}

		public void TestValidateVerificationStatus()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.Validation.ValidateVerificationResultDetails();
			CombineAssertions(() =>
			{
				AssertNoErrors(ediTokenAuthOnBoardingData.VerificationResultInfo);
				AssertNoErrors(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo);
				AssertHasMessageError(ediTokenAuthOnBoardingData.VerificationResultInfo, "Verification has not been run yet");
				AssertHasMessageError(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo, "Verification has not been run yet");
			});

			ediTokenAuthOnBoardingData.VerificationResultDetails = InvalidCode;
			CombineAssertions(() =>
			{
				AssertNoErrors(ediTokenAuthOnBoardingData.VerificationResultInfo);
				AssertNoErrors(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo);
				AssertHasMessageError(ediTokenAuthOnBoardingData.VerificationResultInfo, InvalidCode);
				AssertHasMessageError(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo, InvalidCode);
			});

			ediTokenAuthOnBoardingData.VerificationResultDetails = null;
			CombineAssertions(() =>
			{
				AssertNoErrors(ediTokenAuthOnBoardingData.VerificationResultInfo);
				AssertNoErrors(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo);
				AssertNoMessageErrors(ediTokenAuthOnBoardingData.VerificationResultInfo);
				AssertNoMessageErrors(ediTokenAuthOnBoardingData.VerificationResultDetailsInfo);
			});
		}

		public void TestValidateTOD_LE()
		{
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			var licenceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			ediTokenAuthOnBoardingData.TOD_LE = licenceEnterprise.PK;
			Factory.Save();
			AssertNoErrors(ediTokenAuthOnBoardingData.TOD_LEInfo);

			var ediTokenAuthOnBoardingData2 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData2.TOD_LE = licenceEnterprise.PK;
			var errorMessage = AssertExceptionThrown<ZSaveException>("Factory save should fail because of the duplication check in trigger", () => Factory.Save()).Message;
			AssertContains($"Cannot insert duplicate key row in object 'dbo.EdiTokenAuthOnBoardingData' with unique index 'FK_UC__TOD_LE'. The duplicate key value is ({licenceEnterprise.PK}).", errorMessage);
			AssertHasError(ediTokenAuthOnBoardingData2.TOD_LEInfo, "A Token Authentication Onboarding form already exists for the selected customer. Please use the existing form for any changes. Only one Token Authentication Onboarding form can be created per customer.");
		}

		public void TestTOD_LEIsNotEmpty()
		{
			var ediTokenAuthOnBoardingData = Factory.New<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_LE = ZGuid.Empty;
			AssertHasError(ediTokenAuthOnBoardingData.TOD_LEInfo, "Please make sure the selected incident has an enterprise code attached to it. A Token Authentication Onboarding form can only be created if an enterprise code is assigned.");
		}

		static void ValidateList(ZPropertyInfo propertyInfo, CodeDescriptionPairList codeDescriptionPairList, bool isMandatory)
		{
			AssertCollectionNotContains($"Invalid test - need to change {nameof(InvalidCode)} value", InvalidCode,
				codeDescriptionPairList.GetAllCodes());
			AssertType<ZPropertyInfoString>(propertyInfo);
			var propertyInfoString = (ZPropertyInfoString)propertyInfo;

			propertyInfoString.Value = "";
			CombineAssertions(() =>
			{
				AssertMandatoryValidationError(propertyInfoString, isExpectingError: isMandatory);
				AssertListValidationInvalidCodeError(propertyInfoString, isExpectingError: false);
			});

			propertyInfoString.Value = InvalidCode;
			AssertListValidationInvalidCodeError(propertyInfoString, isExpectingError: true);

			propertyInfoString.Value = codeDescriptionPairList.GetAllCodes().Last();
			AssertListValidationInvalidCodeError(propertyInfoString, isExpectingError: false);
		}
	}
}
