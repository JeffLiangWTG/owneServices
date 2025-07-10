using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(GetOrgAccreditationAttempts))]
	internal sealed class GetOrgAccreditationAttemptsTest : DbCreateScriptTest
	{
		public void TestPrimaryContactShouldGetAttemptsFromPrimaryOrganisation()
		{
			var accreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();
			var person3PK = Guid.NewGuid();
			var person4PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var org2PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var contact2APK = Guid.NewGuid();
			var contact2BPK = Guid.NewGuid();
			var contact3APK = Guid.NewGuid();
			var contact3BPK = Guid.NewGuid();
			var contact4PK = Guid.NewGuid();
			var staffPK = Guid.NewGuid();
			var person1Attempt1PK = Guid.NewGuid();
			var person1Attempt2PK = Guid.NewGuid();
			var person2AttemptPK = Guid.NewGuid();
			var person3AttemptPK = Guid.NewGuid();
			var person4AttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person3PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person4PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org2PK}', 'DEFGHIJ')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact1PK}', 'assdgasd', '{org1PK}', '{person1PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact2APK}', 'asddasd', '{org1PK}', '{person2PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact2BPK}', 'asaaasd', '{org2PK}', '{person2PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact3APK}', 'aassdsd', '{org2PK}', '{person3PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact3BPK}', 'aaseedr', '{org1PK}', '{person3PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact4PK}', 'awadadsd', '{org1PK}', '{person4PK}')

INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{staffPK}', 'asdasd', 'ABA', '{person4PK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person2PK}', '{contact2APK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person3PK}', '{contact3APK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person4PK}', '{staffPK}', 'GS')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1Attempt1PK}', '{accreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1Attempt2PK}', '{accreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person2AttemptPK}', '{accreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3AttemptPK}', '{accreditationPK}', '{person3PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person4AttemptPK}', '{accreditationPK}', '{person4PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact1PK);
			AssertEquals("Should return attempts whose primary workplace matches the querying contact", 3, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert("Should return its own attempts", attemptPKs.Contains(person1Attempt1PK));
			Assert("Should return its own attempts", attemptPKs.Contains(person1Attempt2PK));
			Assert("Should return person 2's attempts since its primary workplace is org1", attemptPKs.Contains(person2AttemptPK));
			Assert("Should not return person 3's attempts since its primary workplace is in org2", !attemptPKs.Contains(person3AttemptPK));
			Assert("Should not return person 4's attempts since its primary workplace is a staff member", !attemptPKs.Contains(person4AttemptPK));
		}

		public void TestPrimaryStaffShouldGetAttemptsFromOtherPrimaryStaff()
		{
			var accreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();
			var person3PK = Guid.NewGuid();
			var person4PK = Guid.NewGuid();
			var person5PK = Guid.NewGuid();
			var person6PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var org2PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var contact2APK = Guid.NewGuid();
			var contact2BPK = Guid.NewGuid();
			var contact3APK = Guid.NewGuid();
			var contact3BPK = Guid.NewGuid();
			var contact4PK = Guid.NewGuid();
			var contact5PK = Guid.NewGuid();
			var contact6PK = Guid.NewGuid();
			var staff1PK = Guid.NewGuid();
			var staff2PK = Guid.NewGuid();
			var staff3PK = Guid.NewGuid();
			var person2StaffPK = Guid.NewGuid();
			var person1Attempt1PK = Guid.NewGuid();
			var person1Attempt2PK = Guid.NewGuid();
			var person2AttemptPK = Guid.NewGuid();
			var person3AttemptPK = Guid.NewGuid();
			var person4AttemptPK = Guid.NewGuid();
			var person5AttemptPK = Guid.NewGuid();
			var person6AttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person3PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person4PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person5PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person6PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org2PK}', 'DEFGHIJ')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact1PK}', 'assdgasd', '{org1PK}', '{person1PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact2APK}', 'asddasd', '{org1PK}', '{person2PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact2BPK}', 'asaaasd', '{org2PK}', '{person2PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact3APK}', 'aassdsd', '{org2PK}', '{person3PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact3BPK}', 'aaseedr', '{org1PK}', '{person3PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact4PK}', 'awadadsd', '{org1PK}', '{person4PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact5PK}', 'aasaadsd', '{org2PK}', '{person5PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact6PK}', 'aasaadsd', '{org1PK}', '{person6PK}')

INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{staff1PK}', 'asdasd', 'ABA', '{person4PK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{staff2PK}', 'sasdss', 'NWA', '{person5PK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{staff3PK}', 'asaasd', 'ABN', '{person6PK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{person2StaffPK}', 's', 'WAG', '{person2PK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person2PK}', '{contact2APK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person3PK}', '{contact3APK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person4PK}', '{staff1PK}', 'GS')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person5PK}', '{staff2PK}', 'GS')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person6PK}', '{staff3PK}', 'GS')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1Attempt1PK}', '{accreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1Attempt2PK}', '{accreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person2AttemptPK}', '{accreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person3AttemptPK}', '{accreditationPK}', '{person3PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person4AttemptPK}', '{accreditationPK}', '{person4PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person5AttemptPK}', '{accreditationPK}', '{person5PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person6AttemptPK}', '{accreditationPK}', '{person6PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact4PK);
			AssertEquals("Should return attempts who have a contact in org1 and whose primary workplace matches the querying contact's staff", 2, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert("Should not return person 1's attempts since it is not connected to any staff", !attemptPKs.Contains(person1Attempt1PK));
			Assert("Should not return person 1's attempts since it is not connected to any staff", !attemptPKs.Contains(person1Attempt2PK));
			Assert("Should not return person 2's attempts since its primary workplace is a contact", !attemptPKs.Contains(person2AttemptPK));
			Assert("Should not return person 3's attempts since it is not connected to any staff", !attemptPKs.Contains(person3AttemptPK));
			Assert("Should return its own attempts", attemptPKs.Contains(person4AttemptPK));
			Assert("Should not return person 5's attempts since its only contact is in org 2", !attemptPKs.Contains(person5AttemptPK));
			Assert("Should return person 6's attempts since its primary workplace is in org 1 and its primary workplace is a staff member", attemptPKs.Contains(person6AttemptPK));
		}

		public void TestShouldOnlyGetWebPublishedAttemptsWhenNoStaffLinked()
		{
			var accreditation1PK = Guid.NewGuid();
			var accreditation2PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var accreditation1AttemptPK = Guid.NewGuid();
			var accreditation2AttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditation1PK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditation2PK}', 'CCS', 'CargoWise Certified Specialist', 'SCC', 0)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact1PK}', 'assdgasd', '{org1PK}', '{person1PK}')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{accreditation1AttemptPK}', '{accreditation1PK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{accreditation2AttemptPK}', '{accreditation2PK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact1PK);
			AssertEquals("Should only return attempts for web published accreditations", 1, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert("accreditation 1 is web published", attemptPKs.Contains(accreditation1AttemptPK));
			Assert("accreditation 2 is not web published", !attemptPKs.Contains(accreditation2AttemptPK));
		}

		public void TestWhenStaffInactiveShouldOnlyGetWebPublishedAttempts()
		{
			var accreditation1PK = Guid.NewGuid();
			var accreditation2PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var staffPK = Guid.NewGuid();
			var accreditation1AttemptPK = Guid.NewGuid();
			var accreditation2AttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditation1PK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditation2PK}', 'CCS', 'CargoWise Certified Specialist', 'SCC', 0)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact1PK}', 'assdgasd', '{org1PK}', '{person1PK}')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{staffPK}', 'asdasd', 'ABA', '{person1PK}', 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{accreditation1AttemptPK}', '{accreditation1PK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{accreditation2AttemptPK}', '{accreditation2PK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact1PK);
			AssertEquals("Should only return attempts for web published accreditations since linked staff is inactive", 1, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert("accreditation 1 is web published", attemptPKs.Contains(accreditation1AttemptPK));
			Assert("accreditation 2 is not web published", !attemptPKs.Contains(accreditation2AttemptPK));
		}

		public void TestWhenStaffActiveShouldGetNonWebPublishedAttempts()
		{
			var accreditation1PK = Guid.NewGuid();
			var accreditation2PK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var staffPK = Guid.NewGuid();
			var accreditation1AttemptPK = Guid.NewGuid();
			var accreditation2AttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditation1PK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditation2PK}', 'CCS', 'CargoWise Certified Specialist', 'SCC', 0)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact1PK}', 'assdgasd', '{org1PK}', '{person1PK}')
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_PER, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) values ('{staffPK}', 'asdasd', 'ABA', '{person1PK}', 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{accreditation1AttemptPK}', '{accreditation1PK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{accreditation2AttemptPK}', '{accreditation2PK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact1PK);
			AssertEquals("Should return all attempts (both web and non web published accreditations) since linked staff is active", 2, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert("accreditation 1 is web published", attemptPKs.Contains(accreditation1AttemptPK));
			Assert("accreditation 2 is not web published", attemptPKs.Contains(accreditation2AttemptPK));
		}

		public void TestShouldOnlyGetAttemptsForActiveContacts()
		{
			var accreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var person2PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var contact2PK = Guid.NewGuid();
			var person1AttemptPK = Guid.NewGuid();
			var person2AttemptPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person2PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) values ('{contact1PK}', 'assdgasd', '{org1PK}', '{person1PK}')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER, OC_IsActive) values ('{contact2PK}', 'assdgssd', '{org1PK}', '{person2PK}', 0)

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')
INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person2PK}', '{contact2PK}', 'OC')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person1AttemptPK}', '{accreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{person2AttemptPK}', '{accreditationPK}', '{person2PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact1PK);
			AssertEquals("Should only return attempts for active contact accreditations", 1, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert("person 1's contact is active", attemptPKs.Contains(person1AttemptPK));
			Assert("person 1's contact is inactive", !attemptPKs.Contains(person2AttemptPK));
		}

		public void TestShouldNotGetDuplicateAttempts()
		{
			//see if we can create a situation where it returns a duplicate without the group by then reinstate it (or test through ssms).
			var accreditationPK = Guid.NewGuid();
			var person1PK = Guid.NewGuid();
			var org1PK = Guid.NewGuid();
			var contact1PK = Guid.NewGuid();
			var contact2PK = Guid.NewGuid();
			var attempt1PK = Guid.NewGuid();

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode, HAC_IsWebPublished) VALUES ('{accreditationPK}', 'CCO', 'CargoWise Certified Operator', 'COC', 1)

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{person1PK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{org1PK}', 'ABCDEFG')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER, OC_WebAccessEnabled) values ('{contact1PK}', 'assdgasd (1)', '{org1PK}', '{person1PK}', 1)
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER, OC_WebAccessEnabled) values ('{contact2PK}', 'assdgssd (2)', '{org1PK}', '{person1PK}', 0)

INSERT INTO dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) values (NEWID(), '{person1PK}', '{contact1PK}', 'OC')

INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES ('{attempt1PK}', '{accreditationPK}', '{person1PK}', '2018-01-01', '2018-04-01', '2019-01-01', '2020-01-01')
");

			TestConnection.ExecuteNonQuery(commandText);

			var attempts = GetOrgAccreditationAttempts(org1PK, contact1PK);
			AssertEquals("Should not return duplicate attempt despite having 2 contacts", 1, attempts.Rows.Count);
			var attemptPKs = attempts.Select().Select(x => x[0]).Cast<Guid>().ToArray();
			Assert(attemptPKs.Contains(attempt1PK));
		}

		#region Implementation

		DataTable GetOrgAccreditationAttempts(Guid orgPK, Guid contactPK)
		{
			var command = TestConnection.Command(@"
SELECT HAA_PK
FROM GetOrgAccreditationAttempts(@OH_PK, @OC_PK)
");
			command.AddParameter("@OH_PK", SqlDbType.UniqueIdentifier, orgPK);
			command.AddParameter("@OC_PK", SqlDbType.UniqueIdentifier, contactPK);

			return DataUtils.GetDataTableFromCommand(command);
		}
		#endregion
	}
}

