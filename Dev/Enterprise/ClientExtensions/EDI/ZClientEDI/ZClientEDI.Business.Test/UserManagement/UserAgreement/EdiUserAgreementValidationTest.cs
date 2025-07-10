namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;

	public class EdiUserAgreementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateERA_TypeShouldBeEntered()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = string.Empty;
			agreement.Validation.ValidateERA_Type();
			AssertHasError("Empty should not be allowed", agreement.ERA_TypeInfo, "Please enter a value.");
		}

		public void TestValidateERA_TypeShouldBeFromLookupList()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			AssertNoErrors("Valid type", agreement.ERA_TypeInfo);

			agreement.ERA_Type = "EAA";
			AssertHasError("Invalid type", agreement.ERA_TypeInfo, "Enter a valid selection.");
		}

		public void TestValidateERA_TitleShouldBeEntered()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Precondition", string.Empty, agreement.ERA_Title);
			agreement.Validation.ValidateERA_Title();
			AssertHasError("Empty should not be allowed", agreement.ERA_TitleInfo, "Please enter a value.");

			agreement.ERA_Title = "Title";
			AssertNoErrors("Title has been entered", agreement.ERA_TitleInfo);
		}

		public void TestCheckERA_VersionNumberIsNotEmpty()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Precondition", 0, agreement.ERA_VersionNumber);
			agreement.Validation.ValidateERA_VersionNumber();
			AssertNoErrors("Empty should be allowed", agreement.ERA_VersionNumberInfo);
		}

		public void TestValidateERA_VersionNumber_TypeCountryCodeVersionNumberMinorVersionVariantCodeCombinationShouldBeUnique()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = "EUA";
			agreement1.ERA_RN_NKCountryCode = "AU";
			agreement1.ERA_VersionNumber = 1;
			Factory.Save();

			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_Type = "EUA";
			agreement2.ERA_RN_NKCountryCode = "NZ";
			agreement2.ERA_VersionNumber = 1;
			AssertNoErrors("Unique country code", agreement2.ERA_VersionNumberInfo);

			var agreement3 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement3.ERA_Type = "EUA";
			agreement3.ERA_RN_NKCountryCode = "AU";
			agreement3.ERA_VersionNumber = 2;
			AssertNoErrors("Unique version number", agreement3.ERA_VersionNumberInfo);

			var agreement4 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement4.ERA_Type = "EUB";
			agreement4.ERA_RN_NKCountryCode = "AU";
			agreement4.ERA_VersionNumber = 1;
			AssertNoErrors("Unique type", agreement4.ERA_VersionNumberInfo);

			var agreement5 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement5.ERA_Type = "EUA";
			agreement5.ERA_RN_NKCountryCode = "AU";
			agreement5.ERA_VersionNumber = 1;
			AssertHasError("Matches agreement 1", agreement5.ERA_VersionNumberInfo, "An existing Agreement of this Type, Variant and Country/Region Code already exists with this Version Number. Please try recreating this Agreement to regenerate a Version Number.");

			agreement5.ERA_MinorVersion = 10;
			AssertNoErrors("Unique Minor Version", agreement5.ERA_VersionNumberInfo);

			agreement5.ERA_MinorVersion = agreement1.ERA_MinorVersion;
			agreement5.ERA_RN_NKCountryCode = "US";
			AssertNoErrors("Unique Country Code", agreement5.ERA_VersionNumberInfo);

			agreement5.ERA_RN_NKCountryCode = "NZ";
			AssertHasError("Matches agreement 2", agreement5.ERA_VersionNumberInfo, "An existing Agreement of this Type, Variant and Country/Region Code already exists with this Version Number. Please try recreating this Agreement to regenerate a Version Number.");

			agreement5.ERA_VariantCode = "CA1";
			AssertNoErrors("New variant should not conflict with other same type data", agreement5.ERA_VersionNumberInfo);
		}

		public void TestValidateEffectiveTimeLocalOnlyOneTypeCountryCodeCombinationShouldBeCurrent()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = "EUA";
			agreement1.ERA_RN_NKCountryCode = "AU";
			agreement1.ERA_VersionNumber = 1;
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddHours(1);
			Factory.Save();

			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_Type = "EUA";
			agreement2.ERA_RN_NKCountryCode = "NZ";
			agreement2.ERA_VersionNumber = 1;
			agreement2.EffectiveTimeLocal = agreement1.EffectiveTimeLocal;
			AssertNoErrors("Different country code", agreement2.EffectiveTimeLocalInfo);
			AssertNoErrors("Different country code", agreement2.ERA_EffectiveTimeUtcInfo);

			var agreement3 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement3.ERA_Type = "EUB";
			agreement3.ERA_RN_NKCountryCode = "AU";
			agreement3.ERA_VersionNumber = 1;
			agreement3.EffectiveTimeLocal = agreement1.EffectiveTimeLocal;
			AssertNoErrors("Different type", agreement3.EffectiveTimeLocalInfo);
			AssertNoErrors("Different type", agreement3.ERA_EffectiveTimeUtcInfo);

			var agreement4 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement4.ERA_Type = "EUA";
			agreement4.ERA_RN_NKCountryCode = "AU";
			agreement4.ERA_VariantCode = agreement1.ERA_VariantCode;
			agreement4.ERA_VersionNumber = 2;
			agreement4.EffectiveTimeLocal = agreement1.EffectiveTimeLocal;

			var expectedErrorMessage = "There is another Agreement with this Type, Variant and Country/Region Code which has the same Effective Time. Only one Agreement can be current for each Type, Variant and Country/Region Code combination.";
			AssertHasError("Both this and agreement 1 share type and country code", agreement4.EffectiveTimeLocalInfo, expectedErrorMessage);
			AssertHasError("Both this and agreement 1 share type and country code", agreement4.ERA_EffectiveTimeUtcInfo, expectedErrorMessage);

			agreement4.ERA_RN_NKCountryCode = "US";
			AssertNoErrors("Unique Country Code", agreement4.EffectiveTimeLocalInfo);
			AssertNoErrors("Unique Country Code", agreement4.ERA_EffectiveTimeUtcInfo);

			agreement4.ERA_RN_NKCountryCode = "NZ";
			agreement4.ERA_VariantCode = agreement2.ERA_VariantCode;
			AssertHasError("Matches agreement 2", agreement4.EffectiveTimeLocalInfo, expectedErrorMessage);
			AssertHasError("Matches agreement 2", agreement4.ERA_EffectiveTimeUtcInfo, expectedErrorMessage);
		}

		public void TestValidateEffectiveTimeLocalShouldAllowDatesUpTo5MinutesInThePast()
		{
			var currentAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			currentAgreement.ERA_Type = "EUA";
			currentAgreement.ERA_RN_NKCountryCode = "AU";
			currentAgreement.ERA_VersionNumber = 1;
			currentAgreement.EffectiveTimeLocal = ZDateTime.Now;
			AssertNoErrors("Time now is valid", currentAgreement.EffectiveTimeLocalInfo);
			AssertNoErrors("Time now is valid", currentAgreement.ERA_EffectiveTimeUtcInfo);

			var futureAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			futureAgreement.ERA_Type = "EUR";
			futureAgreement.ERA_RN_NKCountryCode = "AU";
			futureAgreement.ERA_VersionNumber = 1;
			futureAgreement.EffectiveTimeLocal = ZDateTime.Now.AddDays(1);
			AssertNoErrors("Future time is valid", futureAgreement.EffectiveTimeLocalInfo);
			AssertNoErrors("Future time is valid", futureAgreement.ERA_EffectiveTimeUtcInfo);

			var slightlyPastAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			slightlyPastAgreement.ERA_Type = "EDA";
			slightlyPastAgreement.ERA_RN_NKCountryCode = "AU";
			slightlyPastAgreement.ERA_VersionNumber = 1;
			slightlyPastAgreement.EffectiveTimeLocal = ZDateTime.Now.AddMinutes(-4);
			AssertNoErrors("Time 4 minutes ago is valid", slightlyPastAgreement.EffectiveTimeLocalInfo);
			AssertNoErrors("Time 4 minutes ago is valid", slightlyPastAgreement.ERA_EffectiveTimeUtcInfo);

			var olderAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			olderAgreement.ERA_Type = "EDC";
			olderAgreement.ERA_RN_NKCountryCode = "AU";
			olderAgreement.ERA_VersionNumber = 1;
			olderAgreement.EffectiveTimeLocal = ZDateTime.Now.AddMinutes(-6);
			AssertHasError("Time 6 minutes ago is invalid", olderAgreement.EffectiveTimeLocalInfo, "Effective Time must be in the future.");
			AssertHasError("Time 6 minutes ago is invalid", olderAgreement.ERA_EffectiveTimeUtcInfo, "Effective Time must be in the future.");
		}

		public void TestValidateEffectiveTimeLocalShouldNotShowPastDateErrorForUnchangedAgreementsInDatabase()
		{
			var agreementInDB = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreementInDB.ERA_Type = "EUA";
			agreementInDB.ERA_RN_NKCountryCode = "AU";
			agreementInDB.ERA_VersionNumber = 1;
			agreementInDB.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			DisableEffectiveDateTriggers();
			Factory.Save();
			EnableEffectiveDateTriggers();

			agreementInDB.Validation.ValidateERA_EffectiveTimeUtc();
			AssertNoErrors("Time is valid since this effective time is in the database", agreementInDB.EffectiveTimeLocalInfo);
			AssertNoErrors("Time is valid since this effective time is in the database", agreementInDB.ERA_EffectiveTimeUtcInfo);

			agreementInDB.EffectiveTimeLocal = ZDateTime.Now.AddDays(-2);
			AssertHasError("Now that agreement has been changed an error should show", agreementInDB.EffectiveTimeLocalInfo, "Effective Time must be in the future.");
			AssertHasError("Now that agreement has been changed an error should show", agreementInDB.ERA_EffectiveTimeUtcInfo, "Effective Time must be in the future.");
		}

		public void TestValidateVariantDescription()
		{
			var agreementInDB = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreementInDB.ERA_Type = "EUA";
			agreementInDB.ERA_VariantCode = string.Empty;

			AssertNullOrEmpty(agreementInDB.ERA_VariantCode);
			AssertNoErrors("The description could be empty when code is empty", agreementInDB.ERA_VariantDescriptionInfo);
			Factory.Save();

			AssertNullOrEmpty(agreementInDB.ERA_VariantDescription);

			agreementInDB.ERA_VariantCode = "A";
			AssertHasErrors("The description can not be empty when code is not empty", agreementInDB.ERA_VariantDescriptionInfo);
		}

		public void TestValidateCountryCode()
		{
			var agreementInDB = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreementInDB.ERA_Type = string.Empty;

			agreementInDB.Validation.ValidateERA_RN_NKCountryCode();
			AssertNoErrors(agreementInDB.ERA_RN_NKCountryCodeInfo);

			agreementInDB.ERA_RN_NKCountryCode = "CN";
			agreementInDB.Validation.ValidateERA_RN_NKCountryCode();
			AssertNoErrors(agreementInDB.ERA_RN_NKCountryCodeInfo);

			agreementInDB.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			agreementInDB.ERA_RN_NKCountryCode = "CN";
			agreementInDB.Validation.ValidateERA_RN_NKCountryCode();
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(agreementInDB.ERA_Type));
			AssertNoErrors("Field Should not have error if the type disabled variant", agreementInDB.ERA_RN_NKCountryCodeInfo);

			agreementInDB.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreementInDB.ERA_RN_NKCountryCode = "CN";
			agreementInDB.Validation.ValidateERA_RN_NKCountryCode();
			AssertNotNullOrEmpty(agreementInDB.ERA_RN_NKCountryCode);
			Assert(EdiUserAgreementTypesMapper.IsVariantEnabled(agreementInDB.ERA_Type));
			AssertHasError("Field Should have error if the type enabled variant", agreementInDB.ERA_RN_NKCountryCodeInfo, "The country/region should not be set. Please try setting a variant code for the agreement.");

			agreementInDB.ERA_RN_NKCountryCode = string.Empty;
			agreementInDB.Validation.ValidateERA_RN_NKCountryCode();
			AssertNoErrors("Field Should not have error if field is empty", agreementInDB.ERA_RN_NKCountryCodeInfo);
		}

		#region Implementation

		void DisableEffectiveDateTriggers()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		void EnableEffectiveDateTriggers()
		{
			TestConnection.ExecuteNonQuery("ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; ENABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		#endregion
	}
}
