namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using System;
	using System.Linq;
	using CargoWise.Data;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.Billing.Business.Test;
	using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(EdiUserAgreement))]
	public class EdiUserAgreementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestERA_VersionNumberShouldBeReadOnly()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Should always be read only since we don't want it changed from GUI layer", true, agreement.ERA_VersionNumberInfo.ReadOnly);
		}

		public void TestERA_MinorVersionShouldBeReadOnly()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Should always be read only since we don't want it changed from GUI layer", true, agreement.ERA_MinorVersionInfo.ReadOnly);
		}

		public void TestReadOnlyFieldsWhenERA_EffectiveTimeUtcInPast()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Precondition", true, agreement.ERA_EffectiveTimeUtc > ZDateTime.UtcNow);
			AssertEquals("Precondition", false, agreement.ERA_TitleInfo.ReadOnly);

			Factory.Save();
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMinutes(-7);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_TitleInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_TypeInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_EffectiveTimeUtcInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_ContentInfo.ReadOnly);
			DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			EnableEffectiveDateTriggers(TestConnection);
			AssertEquals("Should no longer be editable", true, agreement.ERA_TitleInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_TypeInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_EffectiveTimeUtcInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_ContentInfo.ReadOnly);
		}

		public void TestReadOnlyFieldsWhenERA_EffectiveTimeUtcInPastButNotInDatabase()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			AssertEquals("Precondition", true, agreement.ERA_EffectiveTimeUtc > ZDateTime.UtcNow);
			AssertEquals("Precondition", false, agreement.ERA_TitleInfo.ReadOnly);

			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMinutes(-6);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_TitleInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_TypeInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_EffectiveTimeUtcInfo.ReadOnly);
			AssertEquals("Should still be editable until saved", false, agreement.ERA_ContentInfo.ReadOnly);
			DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			EnableEffectiveDateTriggers(TestConnection);
			AssertEquals("Should no longer be editable", true, agreement.ERA_TitleInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_TypeInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_EffectiveTimeUtcInfo.ReadOnly);
			AssertEquals("Should no longer be editable", true, agreement.ERA_ContentInfo.ReadOnly);
		}

		public void TestShouldNotAllowDeleteIfERA_EffectiveTimeUtcInPastAndInDatabase()
		{
			var agreementNotInDB = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreementNotInDB.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMinutes(-6);
			AssertEquals("Delete should be allowed", true, agreementNotInDB.CanDelete);
			agreementNotInDB.Delete();

			var agreementWithFutureEffectiveDateInDB = Factory.NewWithValidTestData<EdiUserAgreement>();
			var agreementWithPastEffectiveDateInDB = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreementWithPastEffectiveDateInDB.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMinutes(-6);
			agreementWithFutureEffectiveDateInDB.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(5);
			DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			EnableEffectiveDateTriggers(TestConnection);
			AssertEquals("Delete should be allowed", true, agreementWithFutureEffectiveDateInDB.CanDelete);
			AssertEquals("User Agreements should not be deleted once in the database and past their effective date.", false, agreementWithPastEffectiveDateInDB.CanDelete);
			AssertEquals("User Agreements cannot be deleted once they have taken effect.", agreementWithPastEffectiveDateInDB.ReasonForNotAbleToDelete);
		}

		public void TestVersionNumberShouldBeAssignedOnSaving()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = "EUA";
			agreement1.ERA_RN_NKCountryCode = "AU";
			agreement1.ERA_VersionNumber = 1;
			agreement1.ERA_VariantCode = "ABC";

			agreement2.ERA_Type = "EUA";
			agreement2.ERA_RN_NKCountryCode = "NZ";
			AssertEquals("Precondition", ZInt.Zero, agreement2.ERA_VersionNumber);
			Factory.Save();

			AssertEquals("Version number should not change for agreement1", 1, agreement1.ERA_VersionNumber);
			AssertEquals("Version number should stay as default for non-CWN agreements", 0, agreement1.ERA_MinorVersion);
			AssertEquals("Version number should be assigned for agreement2", 1, agreement2.ERA_VersionNumber);
			AssertEquals("Version number should stay as default for non-CWN agreements", 0, agreement2.ERA_MinorVersion);

			var agreement3 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement3.ERA_Type = "EUA";
			agreement3.ERA_RN_NKCountryCode = "AU";
			agreement3.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);
			agreement3.ERA_VariantCode = "ABC";
			Factory.Save();
			AssertEquals("Next highest version number should be assigned for agreement3", 2, agreement3.ERA_VersionNumber);
			AssertEquals("Version number should stay as default for non-CWN agreements", 0, agreement3.ERA_MinorVersion);

			agreement1.ERA_VersionNumber = 10;
			agreement1.ERA_MinorVersion = 5;
			var agreement4 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement4.ERA_Type = "EUA";
			agreement4.ERA_RN_NKCountryCode = "AU";
			agreement4.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(2);
			agreement4.ERA_VariantCode = "ABC";
			AssertEquals("Precondition", ZInt.Zero, agreement4.ERA_VersionNumber);
			Factory.Save();
			AssertEquals("Next highest version number should be assigned for agreement3", 11, agreement4.ERA_VersionNumber);
			AssertEquals("Minor Version should reset when major version is bumped for non-CWN agreements", 0, agreement4.ERA_MinorVersion);
		}

		public void TestCWNMinorVersionShouldBeAssignedOnSaving()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement1.ERA_VariantCode = "ABC";
			Factory.Save();

			AssertEquals("Major version should default to 1", 1, agreement1.ERA_VersionNumber);
			AssertEquals("Minor version should default to 0", 0, agreement1.ERA_MinorVersion);

			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement2.ERA_VariantCode = "DEF";
			Factory.Save();

			AssertEquals("Major Version should not change for agreement1", 1, agreement1.ERA_VersionNumber);
			AssertEquals("Minor Version should not change for agreement1", 0, agreement1.ERA_MinorVersion);
			AssertEquals("Major Version should be assigned for agreement2 - no clash since different variants", 1, agreement2.ERA_VersionNumber);
			AssertEquals("Minor Version should be assigned for agreement2 - no clash since different variants", 0, agreement2.ERA_MinorVersion);

			var agreement3 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement3.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement3.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);
			agreement3.ERA_VariantCode = "ABC";
			Factory.Save();
			AssertEquals("Major Version should remain the same for CWN agreements", 1, agreement3.ERA_VersionNumber);
			AssertEquals("Next highest Minor Version should be assigned for agreement3", 1, agreement3.ERA_MinorVersion);

			agreement1.ERA_VersionNumber = 10;
			var agreement4 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement4.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement4.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(2);
			agreement4.ERA_VariantCode = "ABC";
			Factory.Save();
			AssertEquals("Major Version should match highest existing", 10, agreement4.ERA_VersionNumber);
			AssertEquals("Minor Version should add to existing", 1, agreement4.ERA_MinorVersion);
		}

		public void TestVersionNumber_VariantCode()
		{
			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = "XXX";
			agreement1.ERA_VariantCode = "ABC";
			agreement1.ERA_VersionNumber = 1;
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			agreement2.ERA_Type = "XXX";
			agreement2.ERA_VariantCode = "123";
			agreement2.ERA_RN_NKCountryCode = agreement1.ERA_RN_NKCountryCode;
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(2);
			Factory.Save();
			AssertEquals(1, agreement1.ERA_VersionNumber);
			AssertEquals(1, agreement2.ERA_VersionNumber);

			agreement2.ERA_VariantCode = agreement1.ERA_VariantCode;
			Factory.Save();
			AssertEquals("Precondition", 2, agreement2.ERA_VersionNumber);
		}

		public void TestVariantCodeDescriptionReadOnly()
		{
			var agreement1 = Factory.New<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement1.ERA_VariantCode = "VA1";
			agreement1.ERA_VariantDescription = "Variant 1";
			agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			var agreement2 = Factory.New<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement2.ERA_VariantCode = "VA2";
			agreement2.ERA_VariantDescription = "Variant 2";
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();
			var newAgreement = Factory.New<EdiUserAgreement>();
			newAgreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			newAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);

			AssertEquals(newAgreement.Level, EdiUserAgreementLevelList.Codes.User);
			Assert(newAgreement.ERA_VariantCodeInfo.ReadOnly);
			Assert(newAgreement.ERA_VariantDescriptionInfo.ReadOnly);

			newAgreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			Assert(!newAgreement.ERA_VariantCodeInfo.ReadOnly);
			Assert(!newAgreement.ERA_VariantDescriptionInfo.ReadOnly);

			newAgreement.ERA_VariantCode = "VA1";
			Assert(newAgreement.Lookups.Variants.ContainsCode(newAgreement.ERA_VariantCode));
			Assert(!newAgreement.ERA_VariantCodeInfo.ReadOnly);
			Assert("The description should be read only when the code is from list", newAgreement.ERA_VariantDescriptionInfo.ReadOnly);

			newAgreement.ERA_VariantCode = "ZA1";
			Assert(!newAgreement.ERA_VariantCodeInfo.ReadOnly);
			Assert(!newAgreement.ERA_VariantDescriptionInfo.ReadOnly);
			Factory.Save();

			Assert(newAgreement.Lookups.Variants.ContainsCode("ZA1"));
			Assert("The code has added into the list, so the description should be read only", newAgreement.ERA_VariantDescriptionInfo.ReadOnly);
		}

		public void TestCountryCodeReadOnly()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			AssertNullOrEmpty(agreement.ERA_Type);
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(string.Empty));
			Assert(!agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);

			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			Assert(EdiUserAgreementTypesMapper.IsVariantEnabled(agreement.ERA_Type));
			Assert("CountryCode should be read only when the type enabled variant.", agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);

			agreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(agreement.ERA_Type));
			Assert("CountryCode should not be read only when the type disabled variant.", !agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestHumanReadableName()
		{
			var agreement = Factory.New<EdiUserAgreement>();

			agreement.ERA_Type = "EUA";
			AssertEquals("EUA", agreement.HumanReadableName);

			agreement.ERA_Title = "Enterprise User Agreement";
			AssertEquals("EUA - Enterprise User Agreement", agreement.HumanReadableName);

			agreement.ERA_RN_NKCountryCode = "NZ";
			AssertEquals("EUA NZ - Enterprise User Agreement", agreement.HumanReadableName);
		}

		public void TestHasFallbackEffectiveDateShouldPrecedeThisRecord()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();

			agreement.ERA_Type = "EUA";
			AssertEquals("No records in the db yet", false, agreement.HasFallback);
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(5);
			agreement.ERA_RN_NKCountryCode = string.Empty;
			Factory.Save();

			AssertEquals("Should find itself as its own fallback since it has no country code", true, agreement.HasFallback);

			var countrySpecificAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			countrySpecificAgreement.ERA_Type = "EUA";
			countrySpecificAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);
			AssertEquals("Should not find any fallbacks", false, countrySpecificAgreement.HasFallback);
			countrySpecificAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(5);
			AssertEquals("Should find the first agreement as its fallback", true, countrySpecificAgreement.HasFallback);
		}

		public void TestHasFallbackEffectiveDateInThePast()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();

			agreement.ERA_Type = "EUA";
			AssertEquals("No records in the db yet", false, agreement.HasFallback);
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			agreement.ERA_RN_NKCountryCode = string.Empty;

			DisableEffectiveDateTriggers(TestConnection);
			Factory.Save();
			EnableEffectiveDateTriggers(TestConnection);

			AssertEquals("Should find itself as its own fallback since it has no country code", true, agreement.HasFallback);

			var countrySpecificAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			countrySpecificAgreement.ERA_Type = "EUA";
			countrySpecificAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			AssertEquals("Should find first agreement as fallback since both records have effective dates in the past", true, countrySpecificAgreement.HasFallback);
		}

		public void TestOnSavingShouldUpdateERA_EffectiveTimeUtcIfUpTo6MinutesInThePast()
		{
			var agreementInDBPastPK = new Guid("614ec5c2-a063-40cb-a760-64ab6cc3b658");
			var agreementInDBFuturePK = new Guid("018f34f0-412d-4bf3-96ab-767bc215c5fd");
			var pastTimeString = ZDateTime.UtcNow.AddMinutes(-2).ToISO8601String();
			var futureTimeString = ZDateTime.UtcNow.AddMinutes(3).ToISO8601String();
			var sql = FormattableString.Invariant($@"INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc) VALUES ('{agreementInDBPastPK}', 'DCA', 'AU', 1, '{pastTimeString}')
INSERT INTO dbo.EdiUserAgreement (ERA_PK, ERA_Type, ERA_RN_NKCountryCode, ERA_VersionNumber, ERA_EffectiveTimeUtc) VALUES ('{agreementInDBFuturePK}', 'DCB', 'NZ', 1, '{futureTimeString}')
");
			DisableEffectiveDateTriggers(TestConnection);
			TestConnection.ExecuteNonQuery(sql);
			EnableEffectiveDateTriggers(TestConnection);

			var futureTime = ZDateTime.UtcNow.AddMinutes(7);
			var fourMinsAgo = ZDateTime.UtcNow.AddMinutes(-4);
			var fourMinsFuture = ZDateTime.UtcNow.AddMinutes(4);

			var futureAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			futureAgreement.ERA_Type = "EUR";
			futureAgreement.ERA_RN_NKCountryCode = "AU";
			futureAgreement.ERA_VersionNumber = 1;
			futureAgreement.ERA_EffectiveTimeUtc = futureTime;

			var slightlyPastAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			slightlyPastAgreement.ERA_Type = "EDA";
			slightlyPastAgreement.ERA_RN_NKCountryCode = "AU";
			slightlyPastAgreement.ERA_VersionNumber = 1;
			slightlyPastAgreement.ERA_EffectiveTimeUtc = fourMinsAgo;

			var slightlyFutureAgreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			slightlyFutureAgreement.ERA_Type = "EDC";
			slightlyFutureAgreement.ERA_RN_NKCountryCode = "AU";
			slightlyFutureAgreement.ERA_VersionNumber = 1;
			slightlyFutureAgreement.ERA_EffectiveTimeUtc = fourMinsFuture;

			var loadedAgreementInDBPast = Factory.Load<EdiUserAgreement>(agreementInDBPastPK);
			var loadedAgreementInDBFuture = Factory.Load<EdiUserAgreement>(agreementInDBFuturePK);
			AssertNotNull(loadedAgreementInDBPast);
			AssertNotNull(loadedAgreementInDBFuture);
			loadedAgreementInDBFuture.ERA_Content = "Blah blah";

			Factory.Save();
			AssertLessThan("DB agreement in past should not be updated", loadedAgreementInDBPast.ERA_EffectiveTimeUtc, ZDateTime.UtcNow);
			AssertGreaterThan("DB agreement in future should be updated to 5 minutes in future", loadedAgreementInDBFuture.ERA_EffectiveTimeUtc, fourMinsFuture);
			AssertEquals("Future agreement should remain unchanged as it's outside the range", futureAgreement.ERA_EffectiveTimeUtc, futureTime);
			AssertGreaterThan("Slightly past agreement should be updated to 5 minutes in future", slightlyPastAgreement.ERA_EffectiveTimeUtc, fourMinsFuture);
			AssertGreaterThan("Slightly future agreement should be updated to 5 minutes in future", slightlyFutureAgreement.ERA_EffectiveTimeUtc, fourMinsFuture);
		}

		public void TestChangesToIsActiveShouldBeLogged()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();

			agreement.ERA_Type = "EUA";
			AssertEquals("No records in the db yet", false, agreement.HasFallback);
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(1);
			agreement.ERA_RN_NKCountryCode = string.Empty;
			AssertEquals("Precondition", true, agreement.ERA_IsActive);
			agreement.ERA_IsActive = false;
			Factory.Save();

			agreement.ERA_IsActive = true;
			Factory.Save();

			agreement.ERA_IsActive = false;
			agreement.ERA_IsActive = true;
			Factory.Save();

			agreement.ERA_IsActive = false;
			Factory.Save();
			var logs = agreement.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.EditedARecordCode && x.SL_Reference.StartsWith(EdiUserAgreementSchema.Constants.ERA_IsActive)).OrderBy(x => x.SL_PostedTimeUtc).ToArray();
			AssertEquals("Should be a log created on each subsequent change of value (to false then to true) after first save", 2, logs.Length);
			AssertEquals("Created on the second Factory.Save()", "ERA_IsActive|OLD=N|NEW=Y", logs[0].SL_Reference);
			AssertEquals("Created on the last Factory.Save()", "ERA_IsActive|OLD=Y|NEW=N", logs[1].SL_Reference);
		}

		public void TestLevel()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.DeniedPartyScreening;
			agreement.ERA_RN_NKCountryCode = "AU";
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "Variant 1";
			AssertEquals(EdiUserAgreementLevelList.Codes.User, agreement.Level);

			agreement.ERA_Type = "";
			AssertEquals(EdiUserAgreementLevelList.Codes.User, agreement.Level);
			AssertNullOrEmpty(agreement.ERA_VariantCode);
			AssertNullOrEmpty(agreement.ERA_VariantDescription);
			AssertEquals("AU", agreement.ERA_RN_NKCountryCode);
			Assert(!agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);

			agreement.ERA_RN_NKCountryCode = "AU";
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "Variant 1";
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(EdiUserAgreementLevelList.Codes.Corporate, agreement.Level);
			AssertNullOrEmpty(agreement.ERA_RN_NKCountryCode);
			AssertEquals("VA1", agreement.ERA_VariantCode);
			AssertEquals("Variant 1", agreement.ERA_VariantDescription);
			Assert(agreement.ERA_RN_NKCountryCodeInfo.ReadOnly);
		}

		public void TestHasAcknowledgedCorporateAgreement()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;

			var newUser = Factory.New<EdiCustomerUserAccount>();
			newUser.EUA_LD = db.PK;
			newUser.EUA_UserID = "U001";
			newUser.EUA_FullName = "User One";
			newUser.EUA_Email = "user.one@test.com";

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(EdiUserAgreementLevelList.Codes.Corporate, agreement.Level);
			Factory.Save();
			AssertEquals(false, agreement.HasAcknowledgedCorporateAgreement(db));

			var agreementLog = Factory.NewWithValidTestData<EdiUserAgreementAcceptanceLog>();
			agreementLog.EUL_EUA = newUser.PK;
			agreementLog.EUL_ERA = agreement.PK;
			Factory.Save();
			AssertEquals(true, agreement.HasAcknowledgedCorporateAgreement(db));
		}

		public void TestHasAcknowledgedCorporateAgreementAcceptedByOrg()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			db.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			var newUser = Factory.New<EdiCustomerUserAccount>();
			newUser.EUA_LD = db.PK;
			newUser.EUA_UserID = "U001";
			newUser.EUA_FullName = "User One";
			newUser.EUA_Email = "user.one@test.com";
			newUser.EUA_OC_WebAccessContact = contact.PK;

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(EdiUserAgreementLevelList.Codes.Corporate, agreement.Level);
			Factory.Save();

			AssertEquals("Precondition", false, agreement.HasAcknowledgedCorporateAgreement(db));

			agreement.AcceptOnBehalfOfOrganisation(org);
			Factory.Save();

			AssertEquals(true, agreement.HasAcknowledgedCorporateAgreement(db));
		}

		public void TestHasAcknowledgedCorporateAgreementEnterpriseLevelAcceptance()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			db.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(EdiUserAgreementLevelList.Codes.Corporate, agreement.Level);
			Factory.Save();

			AssertEquals("Precondition", false, agreement.HasAcknowledgedCorporateAgreement(db));

			var acceptance = Factory.New<EdiUserAgreementAcceptanceLog>();
			acceptance.EUL_ERA = agreement.PK;
			acceptance.EUL_LE = db.LD_LE;
			acceptance.EUL_AcceptedByName = "John";
			acceptance.EUL_AcceptedByEmail = "John@work.com";
			acceptance.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(true, agreement.HasAcknowledgedCorporateAgreement(db));
		}

		public void TestHasAcknowledgedCorporateAgreementLicenceDatabaseLevelAcceptance()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			db2.LD_LE = db.LD_LE;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			db.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow;
			AssertEquals(EdiUserAgreementLevelList.Codes.Corporate, agreement.Level);
			Factory.Save();

			AssertEquals("Precondition", false, agreement.HasAcknowledgedCorporateAgreement(db));
			AssertEquals("Precondition", false, agreement.HasAcknowledgedCorporateAgreement(db2));

			var acceptance = Factory.New<EdiUserAgreementAcceptanceLog>();
			acceptance.EUL_ERA = agreement.PK;
			acceptance.EUL_LD = db.PK;
			acceptance.EUL_AcceptedByName = "John";
			acceptance.EUL_AcceptedByEmail = "John@work.com";
			acceptance.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(true, agreement.HasAcknowledgedCorporateAgreement(db));
			AssertEquals(false, agreement.HasAcknowledgedCorporateAgreement(db2));

			AssertEquals(db.LD_LE, db2.LD_LE);

			acceptance.EUL_LD = ZGuid.Empty;
			acceptance.EUL_LE = db.LD_LE;
			Factory.Save();
			AssertEquals("The database should acknowledge agreement when Enterprise acknowledged", true, agreement.HasAcknowledgedCorporateAgreement(db));
			AssertEquals("The database should acknowledge agreement when Enterprise acknowledged", true, agreement.HasAcknowledgedCorporateAgreement(db2));

			acceptance.EUL_LD = db2.PK;
			acceptance.EUL_LE = db.LD_LE;
			Factory.Save();
			AssertEquals("The database should not acknowledge agreement when the agreement was acknowledged by Enterprise's specific database.", false, agreement.HasAcknowledgedCorporateAgreement(db));
			AssertEquals("The database should acknowledge agreement when the agreement was acknowledged by Enterprise's specific database.", true, agreement.HasAcknowledgedCorporateAgreement(db2));
		}

		public void TestAcceptOnBehalfOfOrganisation()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;

			var org = lic.Company.Header;
			db.LD_OH_WebAccessOrg = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			var newUser1 = Factory.New<EdiCustomerUserAccount>();
			newUser1.EUA_LD = db.PK;
			newUser1.EUA_UserID = "U001";
			newUser1.EUA_FullName = "User One";
			newUser1.EUA_Email = "user.one@test.com";
			newUser1.EUA_OC_WebAccessContact = contact.PK;

			var newUser2 = Factory.New<EdiCustomerUserAccount>();
			newUser2.EUA_LD = db.PK;
			newUser2.EUA_UserID = "U002";
			newUser2.EUA_FullName = "User Two";
			newUser2.EUA_Email = "user.two@test.com";

			AssertNull(EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, string.Empty).Agreement);
			AssertNull(EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, "AU").Agreement);

			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			agreement2.ERA_RN_NKCountryCode = "AU";

			var logUser = Factory.New<EdiUserAgreementAcceptanceLog>();
			logUser.EUL_EUA = newUser2.PK;
			logUser.EUL_ERA = agreement1.PK;
			logUser.EUL_LD = db.PK;
			logUser.EUL_LE = db.LD_LE;
			logUser.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(false, newUser1.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(false, newUser1.HasAcknowledgedUserAgreement(agreement2));
			AssertEquals(false, agreement1.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement2.HasBeenAcceptedByOrganisation(org));

			var log1 = agreement1.AcceptOnBehalfOfOrganisation(org);
			Factory.Save();

			AssertEquals(true, newUser1.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(false, newUser1.HasAcknowledgedUserAgreement(agreement2));
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, log1.EUL_GS);
			AssertGreaterThan(log1.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(true, agreement1.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement2.HasBeenAcceptedByOrganisation(org));

			var log2 = agreement2.AcceptOnBehalfOfOrganisation(org);
			Factory.Save();

			AssertEquals(true, newUser1.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(true, newUser1.HasAcknowledgedUserAgreement(agreement2));
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, log2.EUL_GS);
			AssertGreaterThan(log2.EUL_AcceptanceTimeUtc, ZDateTime.Today.AddDays(-1));
			AssertEquals(true, agreement1.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement1.HasBeenAcceptedByOrganisation(null));
			AssertEquals(true, agreement2.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement2.HasBeenAcceptedByOrganisation(null));
		}

		public void TestHasBeenAcceptedByEnterprise()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;
			var enterprise = db.LicEnterprise;

			var org = lic.Company.Header;
			db.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";
			var newUser = Factory.New<EdiCustomerUserAccount>();
			newUser.EUA_LD = db.PK;
			newUser.EUA_UserID = "U001";
			newUser.EUA_FullName = "User One";
			newUser.EUA_Email = "user.one@test.com";
			newUser.EUA_OC_WebAccessContact = contact.PK;

			AssertNull(EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, string.Empty).Agreement);
			AssertNull(EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, "AU").Agreement);

			var agreement1 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement1.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			agreement2.ERA_RN_NKCountryCode = "AU";

			var log1 = Factory.New<EdiUserAgreementAcceptanceLog>();
			log1.EUL_ERA = agreement1.PK;
			log1.EUL_LE = enterprise.PK;
			log1.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(true, newUser.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(false, newUser.HasAcknowledgedUserAgreement(agreement2));

			AssertEquals(true, agreement1.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement2.HasBeenAcceptedByOrganisation(org));
			AssertEquals(true, agreement1.HasBeenAcceptedByEnterprise(enterprise));
			AssertEquals(false, agreement2.HasBeenAcceptedByEnterprise(enterprise));

			var log2 = Factory.New<EdiUserAgreementAcceptanceLog>();
			log2.EUL_ERA = agreement2.PK;
			log2.EUL_LE = enterprise.PK;
			log2.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			AssertEquals(true, newUser.HasAcknowledgedUserAgreement(agreement1));
			AssertEquals(true, newUser.HasAcknowledgedUserAgreement(agreement2));

			AssertEquals(true, agreement1.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement1.HasBeenAcceptedByOrganisation(null));
			AssertEquals(true, agreement1.HasBeenAcceptedByEnterprise(enterprise));

			AssertEquals(true, agreement2.HasBeenAcceptedByOrganisation(org));
			AssertEquals(false, agreement2.HasBeenAcceptedByOrganisation(null));
			AssertEquals(true, agreement2.HasBeenAcceptedByEnterprise(enterprise));
		}

		public void TestAutoCompleteAgreementContent()
		{
			DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "Variant 1";
			agreement.ERA_Content = "AG1";
			agreement.ERA_IsActive = true;
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);

			var agreement2 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement2.ERA_VariantCode = "VA1";
			agreement2.ERA_VariantDescription = "Variant 1";
			agreement2.ERA_Content = "AG2";
			agreement2.ERA_IsActive = false;
			agreement2.ERA_VersionNumber = 2;
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-1);

			var agreement3 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement3.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement3.ERA_VariantCode = "VA2";
			agreement3.ERA_VariantDescription = "Variant 2";
			agreement3.ERA_Content = "BG1";
			agreement3.ERA_IsActive = true;
			agreement3.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-1);

			var agreement4 = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement4.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement4.ERA_VariantCode = "VA1";
			agreement4.ERA_VariantDescription = "Variant 1";
			agreement4.ERA_Content = "AG3";
			agreement4.ERA_IsActive = true;
			agreement4.ERA_VersionNumber = 3;
			agreement4.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(1);

			Factory.Save();

			EnableEffectiveDateTriggers(TestConnection);

			var newAgreement = (new BusinessObjectFactory() { RefreshEnabled = false }).New<EdiUserAgreement>();
			newAgreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;

			newAgreement.ERA_VariantCode = "VA1";
			AssertEquals(agreement.ERA_Content, newAgreement.ERA_Content);

			newAgreement.ERA_VariantCode = "VA2";
			AssertEquals(agreement3.ERA_Content, newAgreement.ERA_Content);
		}

		public void TestGetCurrentAgreement()
		{
			DisableEffectiveDateTriggers(TestConnection);
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;
			var enterprise = db.LicEnterprise;

			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseID = "E00121";
			enterprise2.LE_EnterpriseCode = "NN1";

			var org = lic.Company.Header;
			db.LD_OH_WebAccessOrg = org.PK;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "User One";
			contact.OC_Email = "user.one@test.com";

			var myaAgreement = Factory.New<EdiUserAgreement>();
			myaAgreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			myaAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			myaAgreement.ERA_IsActive = true;

			var cwnAgreement = Factory.New<EdiUserAgreement>();
			cwnAgreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			cwnAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			cwnAgreement.ERA_VariantCode = "VA1";
			cwnAgreement.ERA_VariantDescription = "Variant 1";

			Factory.Save();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.Parent = enterprise;
			assignment.EAE_AllowOnlineAcceptance = true;

			Factory.Save();

			var info1 = EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo, string.Empty);
			AssertEquals(0, EdiUserAgreementTypesMapper.GetBindingTableCodes(EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo).Count);
			AssertEquals(myaAgreement.PK, info1.Agreement.PK);
			Assert("The type that is not bound table should be allow online acceptance", info1.AllowOnlineAcceptance);

			var info2 = EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty);
			AssertNull(info2.Agreement);
			Assert("CWN agreement's AllowOnlineAcceptance should be false when parent is null", !info2.AllowOnlineAcceptance);

			var info3 = EdiUserAgreement.GetCurrentAgreement(Factory, enterprise, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty);
			AssertEquals(cwnAgreement.PK, info3.Agreement.PK);
			AssertEquals(assignment.PK, info3.Assignment.PK);
			Assert(info3.AllowOnlineAcceptance);

			var info4 = EdiUserAgreement.GetCurrentAgreement(Factory, enterprise2, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty);
			var query = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, LicenceEnterpriseSchema.Constants.Prefix)
				.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentID, enterprise2.PK);
			AssertNull("No Assignment", Factory.LoadTop1<EdiUserAgreementAssignment>(query));
			AssertNull("The current agreement should be null, because it is not be assigned to the enterprise", info4.Agreement);
			Assert("The default value of AllowOnlineAcceptance should be false for assignable agreement", !info4.AllowOnlineAcceptance);
		}

		public void TestGetCurrentAgreement_Database()
		{
			DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = database;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.EAE_OH_ClientAgreementOrg = org1.PK;
			Factory.Save();

			AssertEquals("Should get agreement since there's a match on assignment", agreement, EdiUserAgreement.GetCurrentAgreement(Factory, database, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty).Agreement);
			AssertEquals("Should not return an agreement since there's no assignment", null, EdiUserAgreement.GetCurrentAgreement(Factory, database2, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty).Agreement);

			assignment.Parent = database.LicEnterprise;
			AssertEquals("Should get agreement since there's a match on enterprise level assignment", agreement, EdiUserAgreement.GetCurrentAgreement(Factory, database, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty).Agreement);
		}

		public void TestHasAcknowledgedCorporateAgreement_DatabaseLevelAgreement()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			Factory.Save();

			AddDatabaseLevelAgreementAcceptanceLog(agreement, database);
			Factory.Save();

			AssertEquals("Should be true since agreement was acknowledged on database level", true, agreement.HasAcknowledgedCorporateAgreement(database));
		}

		EdiUserAgreementAcceptanceLog AddDatabaseLevelAgreementAcceptanceLog(EdiUserAgreement agreement, LicenceDatabase database)
		{
			var agreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();

			agreementLog.EUL_ERA = agreement.PK;
			agreementLog.EUL_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			agreementLog.EUL_AcceptedByEmail = "alex@contact.com";
			agreementLog.EUL_AcceptedByName = "Alex";
			agreementLog.EUL_AcceptedByIPAddress = "127.0.0.1";
			agreementLog.EUL_AcceptedByJobTitle = "Developer";

			agreementLog.EUL_MajorVersion = agreement.ERA_VersionNumber.ToString();
			agreementLog.EUL_MinorVersion = agreement.ERA_MinorVersion.ToString();

			agreementLog.EUL_LD = database.PK;
			return agreementLog;
		}

		#region Implementation

		public static void DisableEffectiveDateTriggers(DbConnection connection)
		{
			connection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		public static void EnableEffectiveDateTriggers(DbConnection connection)
		{
			connection.ExecuteNonQuery("ENABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; ENABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
		}

		#endregion
	}
}
