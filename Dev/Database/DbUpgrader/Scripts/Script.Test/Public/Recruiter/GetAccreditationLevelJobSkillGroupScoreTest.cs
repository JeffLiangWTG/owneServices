using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(GetAccreditationLevelJobSkillGroupScore))]
	class GetAccreditationLevelJobSkillGroupScoreTest : DbCreateScriptTest
	{
		[TestDate(2018, 12, 1)]
		public void TestFunction()
		{
			var skillGuid = Guid.NewGuid();
			var skill2Guid = Guid.NewGuid();
			var skill3Guid = Guid.NewGuid();
			var campaignGuid = Guid.NewGuid();
			var campaign2Guid = Guid.NewGuid();
			var campaign3Guid = Guid.NewGuid();
			var campaign4Guid = Guid.NewGuid();
			var testGuid = Guid.NewGuid();
			var test2Guid = Guid.NewGuid();
			var test3Guid = Guid.NewGuid();
			var test4Guid = Guid.NewGuid();
			var tedPersonGuid = Guid.NewGuid();
			var madmanPersonGuid = Guid.NewGuid();
			var tedApplicantGuid = Guid.NewGuid();
			var madmanApplicantGuid = Guid.NewGuid();
			var tedCampaignItem1Guid = Guid.NewGuid();
			var tedCampaignItem2Guid = Guid.NewGuid();
			var tedCampaignItem3Guid = Guid.NewGuid();
			var tedCampaignItem4Guid = Guid.NewGuid();
			var madmanCampaignItem1Guid = Guid.NewGuid();
			var madmanCampaignItem2Guid = Guid.NewGuid();
			var accreditationGuid = Guid.NewGuid();
			var jobSkillGroup1Guid = Guid.NewGuid();
			var jobSkillGroup2Guid = Guid.NewGuid();
			var jobSkillSubGroup1Guid = Guid.NewGuid();
			var jobSkillSubGroup2Guid = Guid.NewGuid();
			var jobSkillSubGroup3Guid = Guid.NewGuid();
			var examSetting1Guid = Guid.NewGuid();
			var examSetting2Guid = Guid.NewGuid();
			var examSetting3Guid = Guid.NewGuid();
			var examSetting4Guid = Guid.NewGuid();

			string commandText = string.Format(@"
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'SSK', 'Some Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{1}', 'SOS', 'Some Other Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{2}', 'SAK', 'Some Alternative Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES	('{3}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001000', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{4}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001001', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{5}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001002', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{6}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001003', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{27}', 'somecode1', '{3}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{28}', 'somecode2', '{4}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{29}', 'somecode3', '{5}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{30}', 'somecode4', '{6}')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{7}', '{0}', 'Some form of Test', '{27}', 4, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{8}', '{0}', 'Some form of Test', '{28}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{9}', '{1}', 'Some form of Test', '{29}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{10}', '{2}', 'Some form of Test', '{30}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{11}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{12}', 'heyoua', 'NZ')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{13}', 'ted@gmail.com', '{11}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{14}', 'madman@gmail.com', '{12}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{15}', '{3}', 'HA', '{13}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{16}', '{4}', 'HA', '{13}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{17}', '{5}', 'HA', '{13}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{18}', '{6}', 'HA', '{13}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{19}', '{3}', 'HA', '{14}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{20}', '{4}', 'HA', '{14}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{15}', 80, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{16}', 50, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{17}', 60, DATEADD(DAY, -4, GETUTCDATE()), GETUTCDATE())
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{18}', 40, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{19}', 90, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{20}', 82, DATEADD(DAY, -4, GETUTCDATE()))

INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description) VALUES ('{21}', 'NEW', 'New Accred')
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{22}', '{21}', 'HAC', 'New Accred Group', 2)
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{23}', '{22}', 'HJG', 'New Accred Group', 2)
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{24}', '{21}', 'HAC', 'New Accred Group', 2)
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{25}', '{24}', 'HJG', 'New Accred Group', 2)
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{26}', '{24}', 'HJG', 'New Accred Group', 1)
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{22}', '{0}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{23}', '{1}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{23}', '{2}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{24}', '{0}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{25}', '{1}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{25}', '{2}')
INSERT INTO dbo.GlbAccreditationJobSkillPivot (HAJ_PK, HAJ_HJG, HAJ_HS) VALUES (NEWID(), '{26}', '{1}')
", skillGuid, skill2Guid, skill3Guid, campaignGuid, campaign2Guid, campaign3Guid, campaign4Guid, testGuid, test2Guid, test3Guid, test4Guid, tedPersonGuid,
				madmanPersonGuid, tedApplicantGuid, madmanApplicantGuid, tedCampaignItem1Guid, tedCampaignItem2Guid, tedCampaignItem3Guid, tedCampaignItem4Guid,
				madmanCampaignItem1Guid, madmanCampaignItem2Guid, accreditationGuid, jobSkillGroup1Guid, jobSkillSubGroup1Guid, jobSkillGroup2Guid, jobSkillSubGroup2Guid, jobSkillSubGroup3Guid,
				examSetting1Guid, examSetting2Guid, examSetting3Guid, examSetting4Guid);

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetFunctionResults(tedPersonGuid, jobSkillGroup1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddMinutes(5));
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Threshold", (short)2, (short)result.Rows[0][0]);
			AssertEquals("((80*0.8 + 50*0.2) + 2((60+40)/2))/3", (decimal)58, (decimal)result.Rows[0][1]);
			result = GetFunctionResults(tedPersonGuid, jobSkillGroup1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2).AddMinutes(5));
			AssertEquals("((80*0.8 + 50*0.2) + 2((0+40)/2))/3", (decimal)38, (decimal)result.Rows[0][1]);

			result = GetFunctionResults(madmanPersonGuid, jobSkillGroup1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
			AssertEquals("Only skill test 1 has been completed ((90*0.8 + 0) + 2((0+0)/2))/3", (decimal)24, (decimal)result.Rows[0][1]);

			var alterText = "UPDATE dbo.ExamAttempt SET EXA_Score = 50 WHERE EXA_Score = 40 OR EXA_Score = 60";
			TestConnection.ExecuteNonQuery(alterText);

			result = GetFunctionResults(tedPersonGuid, jobSkillGroup2Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddMinutes(5));
			AssertEquals("When subgroups have a score tie, those with higher thresholds should be over those with lower thresholds ((80*0.8 + 50*0.2) + 2((50+50)/2))/3", (decimal)58, (decimal)result.Rows[0][1]);
		}

		[TestDate(2018, 12, 1)]
		public void TestGroupWithNoSkillsOrSubGroups()
		{
			var tedPersonGuid = Guid.NewGuid();
			var accreditationGuid = Guid.NewGuid();
			var jobSkillGroupGuid = Guid.NewGuid();

			string commandText = string.Format(@"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{0}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbAccreditation (HAC_PK, HAC_Code, HAC_Description) VALUES ('{1}', 'NEW', 'New Accred')
INSERT INTO dbo.GlbAccreditationJobSkillGroup (HJG_PK, HJG_ParentID, HJG_ParentTableCode, HJG_Description, HJG_Threshold) VALUES ('{2}', '{1}', 'HAC', 'New Accred Group', 1)
", tedPersonGuid, accreditationGuid, jobSkillGroupGuid);

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetFunctionResults(tedPersonGuid, jobSkillGroupGuid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
			AssertEquals("Should evaluate to 0", (decimal)0, (decimal)result.Rows[0][1]);
		}

		DataTable GetFunctionResults(Guid personPk, Guid groupPk, DateTime fromDate, DateTime toDate)
		{
			var command = TestConnection.Command("SELECT * FROM dbo.GetAccreditationLevelJobSkillGroupScore(@PersonPk, @GroupPk, @FromDate, @ToDate)");

			command.AddParameter("@PersonPk", SqlDbType.UniqueIdentifier, personPk);
			command.AddParameter("@GroupPk", SqlDbType.UniqueIdentifier, groupPk);
			command.AddParameter("@FromDate", SqlDbType.DateTime, fromDate);
			command.AddParameter("@ToDate", SqlDbType.DateTime, toDate);

			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}

