using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(GetHRJobSkillScore))]
	class GetHRJobSkillScoreTest : DbCreateScriptTest
	{
		[TestDate(2018, 12, 18)]
		public void TestFunction()
		{
			var skill1Guid = Guid.NewGuid();
			var skill2Guid = Guid.NewGuid();
			var campaignGuid = Guid.NewGuid();
			var campaign2Guid = Guid.NewGuid();
			var campaign3Guid = Guid.NewGuid();
			var test1Guid = Guid.NewGuid();
			var test2Guid = Guid.NewGuid();
			var test3Guid = Guid.NewGuid();
			var tedPersonGuid = Guid.NewGuid();
			var madmanPersonGuid = Guid.NewGuid();
			var samuelPersonGuid = Guid.NewGuid();
			var tedApplicantGuid = Guid.NewGuid();
			var madmanApplicantGuid = Guid.NewGuid();
			var samuelApplicantGuid = Guid.NewGuid();
			var samuelCampaignItemGuid = Guid.NewGuid();
			var tedCampaignItemGuid = Guid.NewGuid();
			var madmanCampaignItemGuid = Guid.NewGuid();
			var tedCampaignItem2Guid = Guid.NewGuid();
			var madmanCampaignItem2Guid = Guid.NewGuid();
			var examSetting1Guid = Guid.NewGuid();
			var examSetting2Guid = Guid.NewGuid();
			var examSetting3Guid = Guid.NewGuid();

			string commandText = string.Format(@"
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'SSK', 'Some Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkill (HS_PK, HS_Code, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{14}', 'SOS', 'Some Other Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES	('{1}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~',  'TST00001000', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{2}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~',  'TST00001001', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{15}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001002', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{19}', 'somecode1', '{1}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{20}', 'somecode2', '{2}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{21}', 'somecode3', '{15}')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{3}', '{0}', 'Some form of Test', '{19}', 5, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{4}', '{0}', 'Some form of Test', '{20}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{16}', '{14}', 'Some form of Test', '{21}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{5}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{6}', 'heyoua', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{7}', 'mistua', 'AU')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{8}', 'ted@gmail.com', '{5}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{9}', 'madman@gmail.com', '{6}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{10}', 'samuel@gmail.com', '{7}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{11}', '{1}', 'HA', '{8}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{17}', '{2}', 'HA', '{8}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{11}', 80, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{11}', 40, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{11}', 90, DATEADD(DAY, -4, GETUTCDATE()), GETUTCDATE())
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{17}', 90, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{12}', '{1}', 'HA', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{18}', '{2}', 'HA', '{9}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{12}', 50, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{18}', 50, DATEADD(DAY, -4, GETUTCDATE()))

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{13}', '{15}', 'HA', '{10}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{13}', 33, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
", skill1Guid, campaignGuid, campaign2Guid, test1Guid, test2Guid, tedPersonGuid, madmanPersonGuid, samuelPersonGuid, tedApplicantGuid, madmanApplicantGuid, samuelApplicantGuid, tedCampaignItemGuid,
				madmanCampaignItemGuid, samuelCampaignItemGuid, skill2Guid, campaign3Guid, test3Guid, tedCampaignItem2Guid, madmanCampaignItem2Guid, examSetting1Guid, examSetting2Guid, examSetting3Guid);

			TestConnection.ExecuteNonQuery(commandText);

			var result = GetReportResults(tedPersonGuid, skill1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(1));
			AssertEquals("Test1 has 5 shares while test2 has 1", (decimal)90, result);
			result = GetReportResults(tedPersonGuid, skill1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-1));
			AssertEquals("Test1 has 5 shares while test2 has 1", Round((decimal)(80.0 * 5 / 6 + (90.0 / 6)), 2), Round(result, 2));
			result = GetReportResults(tedPersonGuid, skill1Guid, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(-3));
			AssertEquals("No results in this range", (decimal)0, result);

			result = GetReportResults(madmanPersonGuid, skill1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-1));
			AssertEquals("Only test1 has been completed", Round((decimal)(50.0 * 5 / 6), 2), Round(result, 2));

			result = GetReportResults(samuelPersonGuid, skill1Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-1));
			AssertEquals("No results for skill1", (decimal)0, result);
			result = GetReportResults(samuelPersonGuid, skill2Guid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-1));
			AssertEquals("Test3 is the only test for skill2", (decimal)33, Round(result, 2));
		}

		static decimal Round(decimal value, int decimalPlaces) => Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);

		decimal GetReportResults(Guid personPk, Guid jobSkillPk, DateTime completionDateFrom, DateTime completionDateTo)
		{
			using (var command = TestConnection.Command("SELECT SkillScore FROM dbo.GetHRJobSkillScore(@PersonPk, @JobSkillPk, @CompletionDateFrom, @CompletionDateTo)"))
			{
				command.AddParameter("@PersonPk", SqlDbType.UniqueIdentifier, personPk);
				command.AddParameter("@JobSkillPk", SqlDbType.UniqueIdentifier, jobSkillPk);
				command.AddParameter("@CompletionDateFrom", SqlDbType.DateTime, completionDateFrom);
				command.AddParameter("@CompletionDateTo", SqlDbType.DateTime, completionDateTo);

				return (decimal)command.ExecuteScalar();
			}
		}
	}
}
