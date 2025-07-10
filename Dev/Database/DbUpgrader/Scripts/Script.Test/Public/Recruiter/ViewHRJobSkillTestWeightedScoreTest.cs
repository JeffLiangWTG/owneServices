using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Recruiter;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Recruiter
{
	[TestedType(typeof(ViewHRJobSkillTestWeightedScore))]
	class ViewWeightedSkillTestScoreTest : DbCreateScriptTest
	{
		public void TestView()
		{
			var skillGuid = Guid.NewGuid();
			var campaignGuid = Guid.NewGuid();
			var campaign2Guid = Guid.NewGuid();
			var testGuid = Guid.NewGuid();
			var test2Guid = Guid.NewGuid();
			var tedPersonGuid = Guid.NewGuid();
			var madmanPersonGuid = Guid.NewGuid();
			var samuelPersonGuid = Guid.NewGuid();
			var tedApplicantGuid = Guid.NewGuid();
			var madmanApplicantGuid = Guid.NewGuid();
			var samuelApplicantGuid = Guid.NewGuid();
			var samuelCampaignItemGuid = Guid.NewGuid();
			var tedCampaignItemGuid = Guid.NewGuid();
			var madmanCampaignItemGuid = Guid.NewGuid();
			var examSetting1Guid = Guid.NewGuid();
			var examSetting2Guid = Guid.NewGuid();

			string commandText = string.Format(@"
INSERT INTO dbo.HRJobSkill (HS_PK, HS_SkillDescription, HS_SystemCreateTimeUtc, HS_SystemCreateUser, HS_SystemLastEditTimeUtc, HS_SystemLastEditUser) VALUES ('{0}', 'Some Skill', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES	('{1}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001000', GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{2}', '03052ED3-2C64-49AC-97D8-C6079D5015B5', '~name~', 'TST00001001', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{14}', 'somecode1', '{1}')
INSERT INTO dbo.ExamSetting (EXS_PK, EXS_Code, EXS_G0) VALUES ('{15}', 'somecode2', '{2}')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{3}', '{0}', 'Some form of Test', '{14}', 4, GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobSkillTest (HT_PK, HT_HS, HT_TestName, HT_EXS, HT_TestWeighting, HT_SystemCreateTimeUtc, HT_SystemCreateUser, HT_SystemLastEditTimeUtc, HT_SystemLastEditUser) VALUES ('{4}', '{0}', 'Some form of Test', '{15}', 1, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{5}', 'asdasd', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{6}', 'heyoua', 'NZ')
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) values ('{7}', 'mistua', 'AU')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{8}', 'ted@gmail.com', '{5}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{9}', 'madman@gmail.com', '{6}', GetUtcDate(), 'E', GetUtcDate(), 'E')
INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser) VALUES ('{10}', 'samuel@gmail.com', '{7}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES	('{11}', '{1}', 'HA', '{8}' , GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{12}', '{2}', 'HA', '{9}' , GetUtcDate(), 'E', GetUtcDate(), 'E'),
		('{13}', '{1}', 'HA', '{10}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{11}', 69, DATEADD(DAY, -4, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc, EXA_TestCompletedUtc) VALUES (NEWID(), '{12}', 50, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, -2, GETUTCDATE()))
INSERT INTO dbo.ExamAttempt (EXA_PK, EXA_G8, EXA_Score, EXA_TestCommencedUtc) VALUES (NEWID(), '{13}', 82, DATEADD(DAY, -2, GETUTCDATE()))
", skillGuid, campaignGuid, campaign2Guid, testGuid, test2Guid, tedPersonGuid, madmanPersonGuid, samuelPersonGuid, tedApplicantGuid, madmanApplicantGuid, samuelApplicantGuid, tedCampaignItemGuid,
				madmanCampaignItemGuid, samuelCampaignItemGuid, examSetting1Guid, examSetting2Guid);

			TestConnection.ExecuteNonQuery(commandText);

			var command = TestConnection.Command(FormattableString.Invariant($"SELECT * FROM dbo.ViewHRJobSkillTestWeightedScore ORDER BY VWS_WeightedScore DESC"));

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("1 row for complete each exam attempt (3rd attempt has no EXA_TestCompletedUtc)", 2, result.Rows.Count);

			AssertEquals("VWS_HT", testGuid, (Guid)result.Rows[0][1]);
			AssertEquals("VWS_HS", skillGuid, (Guid)result.Rows[0][2]);
			AssertEquals("VWS_PER", tedPersonGuid, (Guid)result.Rows[0][3]);
			AssertEquals("VWS_CompletionUtc", DateTime.UtcNow.Date.AddDays(-2), ((DateTime)result.Rows[0][4]).Date);
			AssertEquals("VWS_WeightedScore", (decimal)55.2, Round((decimal)result.Rows[0][5], 1));

			AssertEquals("VWS_HT", test2Guid, (Guid)result.Rows[1][1]);
			AssertEquals("VWS_HS", skillGuid, (Guid)result.Rows[1][2]);
			AssertEquals("VWS_PER", madmanPersonGuid, (Guid)result.Rows[1][3]);
			AssertEquals("VWS_CompletionUtc", DateTime.UtcNow.Date.AddDays(-2), ((DateTime)result.Rows[1][4]).Date);
			AssertEquals("VWS_WeightedScore", (decimal)10, (decimal)result.Rows[1][5]);
		}

		static decimal Round(decimal value, int decimals) => Math.Round(value, decimals, MidpointRounding.AwayFromZero);
	}
}
