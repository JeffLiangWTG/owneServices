using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(Report_AccreditationAttemptsWeightedScore))]
	class Report_AccreditationAttemptsWeightedScoreTest : DbCreateScriptTest
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
			AssertEquals("person1 with complete", "Completed", (string)result.Rows[0][9]);

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), status: "Expired");
			AssertEquals("person2 completed and expired, person 3 incomplete past due date", 2, result.Rows.Count);
			AssertEquals("person2/3", "Expired", (string)result.Rows[0][9]);
			AssertEquals("person2/3", "Expired", (string)result.Rows[1][9]);

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), status: "Commenced");
			AssertEquals("person3 with commenced", 1, result.Rows.Count);
			AssertEquals("person3 with commenced", "Commenced", (string)result.Rows[0][9]);

			#endregion

			#region Dates

			result = GetReportResultsWithDates(Enumerable.Empty<Guid>(), new[] { orgGuid }, Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-4), DateTime.UtcNow.Date);
			AssertEquals("Person2 match", 1, result.Rows.Count);
			AssertEquals("Person2 match", "heyoua", result.Rows[0][1]);

			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_AccreditationAttemptsWeightedScore(@PersonPks, 1, @OrganisationPks, 1, @AccreditationPks, 1,  @CountryPortCodes, 1, NULL, @CommencedDateFrom, @CommencedDateTo, NULL, NULL, NULL, NULL, NULL, NULL)"));
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

			#region Score

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), scoreLow: 1, scoreHigh: 99);
			AssertEquals("1 score", 1, result.Rows.Count);

			result = GetReportResults(Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), scoreLow: 80, scoreHigh: 80);
			AssertEquals("Score", (decimal)0.80, result.Rows[0][19]);
			#endregion
		}

		#region Scores

		public void TestScore()
		{
			var skill2Guid = Guid.NewGuid();
			var skill3Guid = Guid.NewGuid();
			var campaignForSkill2Guid = Guid.NewGuid();
			var campaignForSkill3Guid = Guid.NewGuid();
			var test2Guid = Guid.NewGuid();
			var test3Guid = Guid.NewGuid();
			var skillGroup2Guid = Guid.NewGuid();
			var tedCampaignItemForSkill2Guid = Guid.NewGuid();
			var tedCampaignItemForSkill3Guid = Guid.NewGuid();
			var examSetting1Guid = Guid.NewGuid();
			var examSetting2Guid = Guid.NewGuid();
			SetupReportData();

			var commandText = string.Format(@"
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'SKR', 'Some Skill2', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{1}', 'RRT', 'Some Skill3', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES	('{2}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001000', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{3}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001001', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{11}', 'somecode2', '{2}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{12}', 'somecode3', '{3}')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{4}', '{0}', 'Some form of Test', '{11}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{5}', '{1}', 'Some form of Test', '{12}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{6}', '{7}', 'HAC', 'Accred Group 2', 2)
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{6}', '{0}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{6}', '{1}')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_ClosedDateUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{8}', '{2}', 'HA', '{10}', DATEADD(DAY, -3, GETUTCDATE()), GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{9}', '{3}', 'HA', '{10}', DATEADD(DAY, -3, GETUTCDATE()), GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{8}', 50, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{9}', 60, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()))
", skill2Guid, skill3Guid, campaignForSkill2Guid, campaignForSkill3Guid, test2Guid, test3Guid, skillGroup2Guid, accreditationGuid, tedCampaignItemForSkill2Guid, tedCampaignItemForSkill3Guid, tedApplicantGuid, examSetting1Guid, examSetting2Guid);
			TestConnection.ExecuteNonQuery(commandText);

			var result = GetReportResults(new[] { tedPersonGuid }, Enumerable.Empty<Guid>(), Enumerable.Empty<Guid>(), Enumerable.Empty<string>(), scoreLow: (decimal)63.32, scoreHigh: (decimal)63.34);
			AssertEquals("1 score", 1, result.Rows.Count);
			AssertEquals("Weighted average score (weighted by group thresholds): (((40 + 60)/2)*2 + 80)/3", (decimal)0.6333, (decimal)result.Rows[0][19]);
		}

		#endregion

		#region Implementation

		void SetupReportData()
		{
			var commandText = string.Format(@"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{9}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{10}', 'heyoua', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{11}', 'mistua', 'AU')
INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('{4}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', '234', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'Some Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{17}', 'somecode1', '{4}')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{1}', '{0}', 'Some form of Test', '{17}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description, HAC_CertificateCode) VALUES ('{14}', 'NEW', 'New Accred', 'NID')
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES (NEWID(), '{14}', '{9}', DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, 1, GETUTCDATE()), DATEADD(DAY, 1, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES (NEWID(), '{14}', '{10}', DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate, HAA_ExpiryDate) VALUES (NEWID(), '{14}', '{11}', DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()), DATEADD(DAY, 2, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationAttempt (HAA_PK, HAA_HAC, HAA_PER, HAA_CommencementDate, HAA_CompletionDueDate) VALUES (NEWID(), '{14}', '{11}', DATEADD(DAY, -2, GETUTCDATE()), DATEADD(DAY, 1, GETUTCDATE()))
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description) VALUES ('{15}', '{14}', 'HAC', 'New Accred Group')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{15}', '{0}')

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{2}', 'ted@gmail.com', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{3}', 'madman@gmail.com', '{10}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{7}', 'samuel@gmail.com', '{11}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_ClosedDateUtc, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{12}', '{4}', 'HA', '{2}', DATEADD(DAY, -3, GETUTCDATE())	, GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{13}', '{4}', 'HA', '{3}', DATEADD(DAY, -2, GETUTCDATE())	, GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{8}' , '{4}', 'HA', '{7}', null							, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{12}', 80, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -3, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{13}', 81, DATEADD(DAY, -3, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{8}', 82, DATEADD(DAY, -2, GETUTCDATE()))

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{5}', 'TESTORG', 'Test Organisation', 'AUBNE')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{6}', 'TESTORG2', 'Test Organisation 2', '')

INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_RL_NKRelatedPortCode, OA_OH, OA_RN_NKCountryCode) VALUES ('{16}', '42 Wallaby Way', 'AUSYD', '{5}', 'AU')

INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH, OC_OA_OrgAddress) VALUES (NEWID(), '{9}', 'Tedd', 'ted@gmail.com', 1, '{5}', '{16}')
INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), '{10}', 'Mad Man', 'madman@gmail.com', 1, '{5}')
INSERT INTO dbo.OrgContact (OC_PK, OC_PER, OC_ContactName, OC_Email, OC_IsActive, OC_OH) VALUES (NEWID(), '{11}', 'Samuel', 'samuel@gmail.com', 1, '{6}')

INSERT INTO dbo.GenRegCertAccredMaintList(XZ_PK, XZ_ParentID, XZ_ParentTableCode, XZ_Type, XZ_RefNumber, XZ_IssueDate) values (NEWID(), '{2}', 'HA', 'NID', 12345, DATEADD(DAY, -3, GETUTCDATE()))
", skillGuid, testGuid, tedApplicantGuid, madmanApplicantGuid, campaignGuid, orgGuid, org2Guid, samuelApplicantGuid, samuelCampaignItemGuid, tedPersonGuid, madmanPersonGuid, samuelPersonGuid, tedCampaignItemGuid,
				madmanCampaignItemGuid, accreditationGuid, jobSkillGroupGuid, orgAddressGuid, examSettingGuid);

			TestConnection.ExecuteNonQuery(commandText);
		}

		DataTable GetReportResultsWithDates(IEnumerable<Guid> personPks, IEnumerable<Guid> organisationPks, IEnumerable<Guid> accreditationPks, IEnumerable<string> countryPorts, DateTime commencedDateFrom, DateTime commencedDateTo, DateTime completionDateFrom, DateTime completionDateTo, DateTime completionDueDateFrom, DateTime completionDueDateTo)
		{
			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_AccreditationAttemptsWeightedScore(@PersonPks, @PersonPksIsEmpty, @OrganisationPks, @OrganisationPksIsEmpty, @AccreditationPks, @AccreditationPksIsEmpty, @WorkingCountryPortCodes, @WorkingCountryPortCodesIsEmpty, NULL, @CommencedDateFrom, @CommencedDateTo, @CompletionDateFrom, @CompletionDateTo, @CompletionDueDateFrom, @CompletionDueDateTo, NULL, NULL)"));
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

		DataTable GetReportResults(IEnumerable<Guid> personPks, IEnumerable<Guid> organisationPks, IEnumerable<Guid> accreditationPks, IEnumerable<string> countryPorts, string status = null, decimal scoreLow = 0, decimal scoreHigh = 100)
		{
			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM Report_AccreditationAttemptsWeightedScore(@PersonPks, @PersonPksIsEmpty, @OrganisationPks, @OrganisationPksIsEmpty, @AccreditationPks, @AccreditationPksIsEmpty, @WorkingCountryPortCodes, @WorkingCountryPortCodesIsEmpty, @Status, NULL, NULL, NULL, NULL, NULL, NULL, @ScoreLow, @ScoreHigh)"));
			command.AddTableValuedParameter("@PersonPks", "dbo.TVP_uniqueidentifier", personPks);
			command.AddTableValuedParameter("@OrganisationPks", "dbo.TVP_uniqueidentifier", organisationPks);
			command.AddTableValuedParameter("@AccreditationPks", "dbo.TVP_uniqueidentifier", accreditationPks);
			command.AddTableValuedParameter("@WorkingCountryPortCodes", "dbo.TVP_varchar_250", countryPorts);

			command.AddParameter("@PersonPksIsEmpty", SqlDbType.Bit, (personPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@OrganisationPksIsEmpty", SqlDbType.Bit, (organisationPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@AccreditationPksIsEmpty", SqlDbType.Bit, (accreditationPks ?? Enumerable.Empty<Guid>()).Any() ? 0 : 1);
			command.AddParameter("@WorkingCountryPortCodesIsEmpty", SqlDbType.Bit, (countryPorts ?? Enumerable.Empty<string>()).Any() ? 0 : 1);
			command.AddParameter("@Status", SqlDbType.Char, (object)status ?? DBNull.Value);
			command.AddParameter("@ScoreLow", SqlDbType.Decimal, scoreLow);
			command.AddParameter("@ScoreHigh", SqlDbType.Decimal, scoreHigh);

			return DataUtils.GetDataTableFromCommand(command);
		}

		readonly Guid orgGuid = Guid.NewGuid();
		readonly Guid org2Guid = Guid.NewGuid();
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
		readonly Guid examSettingGuid = Guid.NewGuid();
		#endregion
	}
}

