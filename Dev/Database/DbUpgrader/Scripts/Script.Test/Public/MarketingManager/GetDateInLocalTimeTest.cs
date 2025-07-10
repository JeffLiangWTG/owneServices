using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Test
{
	[TestedType(typeof(GetDateInLocalTime))]
	class GetDateInLocalTimeTest : DbCreateScriptTest
	{
		public void TestGetDateInLocalTime()
		{
			var personPK = Guid.NewGuid();
			var orgPK = Guid.NewGuid();
			var contactPK = Guid.NewGuid();
			var contact2PK = Guid.NewGuid();
			var companyPK = Guid.NewGuid();
			var campaignItemPK = Guid.NewGuid();
			var campaignItem2PK = Guid.NewGuid();
			var campaignPK = Guid.NewGuid();
			var campaignLinkPK = Guid.NewGuid();
			var campaignClickPK = Guid.NewGuid();
			var clickDateTime = new DateTime(2021, 8, 3, 20, 0, 0);

			var commandText = FormattableString.Invariant($@"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_RN_NKCountry) VALUES ('{personPK}', 'asdasd', 'NZ')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_RL_NKClosestPort) VALUES ('{orgPK}', 'ABC010', 'USJFK')
INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER) VALUES ('{contactPK}', 'assdgasd', '{orgPK}', '{personPK}')
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{companyPK}', 'C10', 'AU company', 'AU', 'AUD')

INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_Stage, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
VALUES ('{campaignPK}', '{companyPK}', 'Campaign01', 'TST00001000', 'STG', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES ('{campaignItemPK}', '{campaignPK}', 'OC', '{contactPK}', 'UNV', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaignLink(GCL_PK, GCL_G0_Campaign, GCL_IsImage, GCL_IsTracked, GCL_Context, GCL_URL, GCL_SystemCreateTimeUtc, GCL_SystemCreateUser, GCL_SystemLastEditTimeUtc, GCL_SystemLastEditUser)
VALUES('{campaignLinkPK}', '{campaignPK}', 1, 1, 'test', 'url', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.GlbCompanyCampaignClick(GCC_PK, GCC_GCL, GCC_G8_Recipient, GCC_HostAddress, GCC_ClickTimeUtc, GCC_SystemCreateTimeUtc, GCC_SystemCreateUser, GCC_SystemLastEditTimeUtc, GCC_SystemLastEditUser)
VALUES('{campaignClickPK}', '{campaignLinkPK}', '{campaignItemPK}', NULL, '{clickDateTime}', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_LeadStatus, O1_LeadSource, O1_EnquiryType, O1_PortOrCountry, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES('{contactPK}', '00009004', 'CLD', 'BBB', 'CCR', 'MYKUL', GetUtcDate(), 'E', GetUtcDate(), 'E');

INSERT INTO dbo.GlbCompanyCampaignItem (G8_PK, G8_G0, G8_RecipientTableCode, G8_RecipientID, G8_TrackingStatus, G8_SystemCreateTimeUtc, G8_SystemCreateUser, G8_SystemLastEditTimeUtc, G8_SystemLastEditUser)
VALUES ('{campaignItem2PK}', '{campaignPK}', 'OC', '{contact2PK}', 'UNV', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_LeadStatus, O1_LeadSource, O1_EnquiryType, O1_PortOrCountry, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES('{contact2PK}', '00009005', 'CLD', 'BBB', 'CCR', 'MY', GetUtcDate(), 'E', GetUtcDate(), 'E');
");

			TestConnection.Command(commandText).ExecuteNonQuery();

			AssertLocalDateTime(campaignClickPK, 0, "OC", campaignItemPK, "03/08/2021 00:00:00");
			AssertLocalDateTime(campaignClickPK, 720, "OC", campaignItemPK, "03/08/2021 00:00:00");
			AssertLocalDateTime(campaignClickPK, 720, "O1", campaignItemPK, "04/08/2021 00:00:00");
			AssertLocalDateTime(campaignClickPK, 720, "O1", Guid.Empty, "04/08/2021 00:00:00");
			AssertLocalDateTime(campaignClickPK, 720, "", Guid.Empty, "03/08/2021 00:00:00");
			AssertLocalDateTime(campaignClickPK, 720, "O1", campaignItem2PK, "04/08/2021 00:00:00");
		}

		void AssertLocalDateTime(Guid campaignClickPK, int currentBranchOffset, string columnType, Guid campaignItemPK, string expectedLocalDateTime)
		{
			using (var command = TestConnection.Command($@"SELECT localdatetime FROM dbo.GetDateInLocalTime('{campaignClickPK}', {currentBranchOffset}, '{columnType}', '{campaignItemPK}')"))
			{
				var dataTable = DataUtils.GetDataTableFromCommand(command);

				AssertEquals("GetDateInLocalTime should return 1 row", 1, dataTable.Rows.Count);
				AssertEquals(expectedLocalDateTime, dataTable.Select().Select(x => x[0]).Cast<DateTime>().First().ToString("dd/MM/yyyy HH:mm:ss"));
			}
		}
	}
}

