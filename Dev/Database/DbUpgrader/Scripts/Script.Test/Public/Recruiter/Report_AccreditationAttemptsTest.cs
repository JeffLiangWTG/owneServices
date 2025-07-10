using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(Report_AccreditationAttempts))]
	class Report_AccreditationAttemptsTest : DbCreateScriptTest
	{
		public void TestReport()
		{
			SetupReportData();

			#region Person and Organisation

			var result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>());
			AssertEquals("4 Accreditation Attempts exist", 4, result.Rows.Count);

			result = GetReportResults(new[] { tedPersonGuid }, new[] { orgGuid }, Enumerable.Empty<Guid>(), Enumerable.Empty<string>());
			AssertEquals("1 Accreditation Attempt exists for person1", 1, result.Rows.Count);

			#endregion

			#region Accreditations and Certificates

			result = GetReportResults(Enumerable.Empty<Guid>(), new[] { orgGuid }, new[] { accreditationGuid }, Enumerable.Empty<string>());
			AssertEquals("2 people in org with attempts for this code", 2, result.Rows.Count);
			AssertEquals("person1 should have a matching certificate", 1, result.Select("CertificateCode = 'NID'").Length);

			#endregion

			#region Location

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), new[] { "AUSYD" });
			AssertEquals("Person1 -> Contact -> OrgAddress -> OA_RL_NKRelatedPortCode", 1, result.Rows.Count);
			AssertEquals("Person1 -> Contact -> OrgAddress -> OA_RL_NKRelatedPortCode", "AUSYD", result.Rows[0][3]);

			result = GetReportResults(Enumerable.Empty<Guid>(), new[] { orgGuid }, Enumerable.Empty<Guid>(), new[] { "AUBNE", "AUSYD" });
			AssertEquals("Person2 -> Contact -> OrgHeader -> OH_RL_NKClosestPort", 2, result.Rows.Count);

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), new[] { "AU" });
			AssertEquals("Person1 -> Contact -> OrgHeader -> OH_RN_NKCountryCode, Person2 -> Contact -> OrgHeader -> OH_RL_NKClosestPort", 2, result.Rows.Count);

			#endregion

			#region Status

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), status: "Completed");
			AssertEquals("person1 with complete and not expired", 1, result.Rows.Count);
			AssertEquals("person1 with complete", "Completed", (string)result.Rows[0][8]);

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), status: "Expired");
			AssertEquals("person2 completed and expired, person 3 incomplete past due date", 2, result.Rows.Count);
			AssertEquals("person2/3", "Expired", (string)result.Rows[0][8]);
			AssertEquals("person2/3", "Expired", (string)result.Rows[1][8]);

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), status: "Commenced");
			AssertEquals("person3 with commenced", 1, result.Rows.Count);
			AssertEquals("person3 with commenced", "Commenced", (string)result.Rows[0][8]);

			#endregion

			#region Dates

			result = GetReportResultsWithDates(Enumerable.Empty<Guid>(), new[] { orgGuid }, Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date);
			AssertEquals("Person2 match", 1, result.Rows.Count);
			AssertEquals("Person2 match", "heyoua", result.Rows[0][1]);

			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_AccreditationAttempts(@PersonPks, 1, @OrganisationPks, 1, @AccreditationPks, 1,  @CountryPortCodes, 1, NULL, @CommencedDateFrom, @CommencedDateTo, NULL, NULL, NULL, NULL)"));
			command.AddTableValuedParameter("@PersonPks", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
			command.AddTableValuedParameter("@OrganisationPks", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
			command.AddTableValuedParameter("@AccreditationPks", "dbo.TVP_uniqueidentifier", Enumerable.Empty<Guid>());
			command.AddTableValuedParameter("@CountryPortCodes", "dbo.TVP_varchar_250", Enumerable.Empty<string>());
			command.AddParameter("@CommencedDateFrom", SqlDbType.DateTime, DateTime.UtcNow.Date.AddDays(-4));
			command.AddParameter("@CommencedDateTo", SqlDbType.DateTime, DateTime.UtcNow.Date.AddDays(-2));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(" Person1 and 2's attempts should be within this range", 2, result.Rows.Count);

			command.SetParameterValue("@CommencedDateTo", DateTime.UtcNow.Date.AddDays(-1));
			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("All attempts should be within this range", 4, result.Rows.Count);
			#endregion
		}

		#region Implementation

		void SetupReportData()
		{
			var commandText = $@"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{tedPersonGuid}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{madmanPersonGuid}', 'heyoua', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{samuelPersonGuid}', 'mistua', 'AU')
INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('{campaignGuid}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', '234', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{skillGuid}', 'Some Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{examSettingGuid}', 'somecode', '{campaignGuid}')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{testGuid}', '{skillGuid}', 'Some form of Test', '{examSettingGuid}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{accreditationGuid}', 'NEW', 'New Accred', 'NID')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES (NEWID(), '{accreditationGuid}', '{tedPersonGuid}', DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, 1, GETUTCDATE()), DATEADD(DAY, 1, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES (NEWID(), '{accreditationGuid}', '{madmanPersonGuid}', DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES (NEWID(), '{accreditationGuid}', '{samuelPersonGuid}', DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, 2, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES (NEWID(), '{accreditationGuid}', '{samuelPersonGuid}', DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, 1, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description) VALUES ('{jobSkillGroupGuid}', '{accreditationGuid}', 'HAC', 'New Accred Group')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{jobSkillGroupGuid}', '{skillGuid}')

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{tedApplicantGuid}', 'ted@gmail.com', '{tedPersonGuid}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{madmanApplicantGuid}', 'madman@gmail.com', '{madmanPersonGuid}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{samuelApplicantGuid}', 'samuel@gmail.com', '{samuelPersonGuid}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_ClosedDateUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{tedCampaignItemGuid}',    '{campaignGuid}', 'HA', '{tedApplicantGuid}',    DATEADD(DAY, -3, GETUTCDATE()),	GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{madmanCampaignItemGuid}', '{campaignGuid}', 'HA', '{madmanApplicantGuid}', DATEADD(DAY, -2, GETUTCDATE()),	GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{samuelCampaignItemGuid}', '{campaignGuid}', 'HA', '{samuelApplicantGuid}', null,								GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{tedCampaignItemGuid}', 80, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{madmanCampaignItemGuid}', 81, DATEADD(DAY, -3, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{samuelCampaignItemGuid}', 82, DATEADD(DAY, -2, GETUTCDATE()))

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{orgGuid}', 'TESTORG', 'Test Organisation', 'AUBNE')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org2Guid}', 'TESTORG2', 'Test Organisation 2', '')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org3Guid}', 'TESTORG3', 'Test Organisation 3', 'AUOOL')

INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_RL_NKRelatedPortCode, OA_OH, OA_RN_NKCountryCode) VALUES ('{orgAddressGuid}', '42 Wallaby Way', 'AUSYD', '{orgGuid}', 'AU')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_RL_NKRelatedPortCode, OA_OH, OA_RN_NKCountryCode) VALUES ('{orgAddress3Guid}', 'Arundel Tavern', 'AUOOL', '{org3Guid}', 'AU')

INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_OA_OrgAddress) VALUES ('{primaryContactPk}', '{tedPersonGuid}', 'Tedd', 'ted@gmail.com', 1, '{orgGuid}', '{orgAddressGuid}')
insert into dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) VALUES (newid(), '{tedPersonGuid}', '{primaryContactPk}', 'OC')
INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_OA_OrgAddress) VALUES (NEWID(), '{tedPersonGuid}', 'Tedd', 'ted@gmail.com', 1, '{org3Guid}', '{orgAddress3Guid}')
INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES ('{madmanContactPk}', '{madmanPersonGuid}', 'Mad Man', 'madman@gmail.com', 1, '{orgGuid}')
insert into dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) VALUES (newid(), '{madmanPersonGuid}', '{madmanContactPk}', 'OC')
INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES ('{samuelContactPk}', '{samuelPersonGuid}', 'Samuel', 'samuel@gmail.com', 1, '{org2Guid}')
insert into dbo.GlbPersonPrimaryRelationship (PPR_PK, PPR_PER, PPR_PrimaryId, PPR_PrimaryTableCode) VALUES (newid(), '{samuelPersonGuid}', '{samuelContactPk}', 'OC')

INSERT INTO dbo.GenRegCertAccredMaintList(XZ_PK, XZ_ParentID, XZ_ParentTableCode, XZ_Type, XZ_RefNumber, XZ_IssueDate) values (NEWID(), '{tedApplicantGuid}', 'HA', 'NID', 12345, DATEADD(DAY, -3, GETUTCDATE()))
";

			TestConnection.ExecuteNonQuery(commandText);
		}

		DataTable GetReportResultsWithDates(IEnumerable<Guid> personPks, IEnumerable<Guid> organisationPks, IEnumerable<Guid> accreditationPks, IEnumerable<string> countryPorts, DateTime commencedDateFrom, DateTime commencedDateTo, DateTime completionDateFrom, DateTime completionDateTo, DateTime completionDueDateFrom, DateTime completionDueDateTo)
		{
			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_AccreditationAttempts(@PersonPks, @PersonPksIsEmpty, @OrganisationPks, @OrganisationPksIsEmpty, @AccreditationPks, @AccreditationPksIsEmpty, @WorkingCountryPortCodes, @WorkingCountryPortCodesIsEmpty, NULL, @CommencedDateFrom, @CommencedDateTo, @CompletionDateFrom, @CompletionDateTo, @CompletionDueDateFrom, @CompletionDueDateTo)"));
			command.AddTableValuedParameter("@PersonPks", "dbo.TVP_uniqueidentifier", personPks);
			command.AddTableValuedParameter("@OrganisationPks", "dbo.TVP_uniqueidentifier", organisationPks);
			command.AddTableValuedParameter("@AccreditationPks", "dbo.TVP_uniqueidentifier", accreditationPks);
			command.AddTableValuedParameter("@WorkingCountryPortCodes", "dbo.TVP_varchar_250", countryPorts);

			command.AddParameter("@PersonPksIsEmpty", SqlDbType.Bit, (personPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@OrganisationPksIsEmpty", SqlDbType.Bit, (organisationPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@AccreditationPksIsEmpty", SqlDbType.Bit, (accreditationPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@WorkingCountryPortCodesIsEmpty", SqlDbType.Bit, (countryPorts ?? Enumerable.Empty<string>()).Any() ? 0 : 1);
			command.AddParameter("@CommencedDateFrom", SqlDbType.DateTime, commencedDateFrom);
			command.AddParameter("@CommencedDateTo", SqlDbType.DateTime, commencedDateTo);
			command.AddParameter("@CompletionDateFrom", SqlDbType.DateTime, completionDateFrom);
			command.AddParameter("@CompletionDateTo", SqlDbType.DateTime, completionDateTo);
			command.AddParameter("@CompletionDueDateFrom", SqlDbType.DateTime, completionDueDateFrom);
			command.AddParameter("@CompletionDueDateTo", SqlDbType.DateTime, completionDueDateTo);

			return DataUtils.GetDataTableFromCommand(command);
		}

		DataTable GetReportResults(IEnumerable<Guid> personPks, IEnumerable<Guid> organisationPks, IEnumerable<Guid> accreditationPks, IEnumerable<string> countryPorts, string status = null)
		{
			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_AccreditationAttempts(@PersonPks, @PersonPksIsEmpty, @OrganisationPks, @OrganisationPksIsEmpty, @AccreditationPks, @AccreditationPksIsEmpty, @WorkingCountryPortCodes, @WorkingCountryPortCodesIsEmpty, @Status, NULL, NULL, NULL, NULL, NULL, NULL)"));
			command.AddTableValuedParameter("@PersonPks", "dbo.TVP_uniqueidentifier", personPks);
			command.AddTableValuedParameter("@OrganisationPks", "dbo.TVP_uniqueidentifier", organisationPks);
			command.AddTableValuedParameter("@AccreditationPks", "dbo.TVP_uniqueidentifier", accreditationPks);
			command.AddTableValuedParameter("@WorkingCountryPortCodes", "dbo.TVP_varchar_250", countryPorts);

			command.AddParameter("@PersonPksIsEmpty", SqlDbType.Bit, (personPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@OrganisationPksIsEmpty", SqlDbType.Bit, (organisationPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@AccreditationPksIsEmpty", SqlDbType.Bit, (accreditationPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@WorkingCountryPortCodesIsEmpty", SqlDbType.Bit, (countryPorts ?? Enumerable.Empty<string>()).Any() ? 0 : 1);
			command.AddParameter("@Status", SqlDbType.Char, (object)status ?? DBNull.Value);

			return DataUtils.GetDataTableFromCommand(command);
		}

		readonly Guid orgGuid = Guid.NewGuid();
		readonly Guid org2Guid = Guid.NewGuid();
		readonly Guid org3Guid = Guid.NewGuid();
		readonly Guid skillGuid = Guid.NewGuid();
		readonly Guid testGuid = Guid.NewGuid();
		readonly Guid campaignGuid = Guid.NewGuid();
		readonly Guid tedApplicantGuid = Guid.NewGuid();
		readonly Guid madmanApplicantGuid = Guid.NewGuid();
		readonly Guid samuelApplicantGuid = Guid.NewGuid();
		readonly Guid samuelCampaignItemGuid = Guid.NewGuid();
		readonly Guid tedPersonGuid = Guid.NewGuid();
		readonly Guid madmanPersonGuid = Guid.NewGuid();
		readonly Guid samuelPersonGuid = Guid.NewGuid();
		readonly Guid tedCampaignItemGuid = Guid.NewGuid();
		readonly Guid madmanCampaignItemGuid = Guid.NewGuid();
		readonly Guid accreditationGuid = Guid.NewGuid();
		readonly Guid jobSkillGroupGuid = Guid.NewGuid();
		readonly Guid orgAddressGuid = Guid.NewGuid();
		readonly Guid orgAddress3Guid = Guid.NewGuid();
		readonly Guid examSettingGuid = Guid.NewGuid();
		readonly Guid primaryContactPk = Guid.NewGuid();
		readonly Guid madmanContactPk = Guid.NewGuid();
		readonly Guid samuelContactPk = Guid.NewGuid();

		#endregion
	}
}

