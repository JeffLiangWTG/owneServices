using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(ViewCampaignChildNode))]
	class ViewCampaignChildNodeTest : DbCreateScriptTest
	{
		public void TestOpportunitySourceCampaign()
		{
			Guid opportunity1;
			Guid opportunity2;
			Guid campaign1;
			Guid campaign2;
			Guid orgHeader = Guid.NewGuid();
			Guid company = Guid.NewGuid();

			Guid.TryParse("4e33c461-ed10-465c-a064-f8e5a52054cc", out opportunity1);
			Guid.TryParse("eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c", out opportunity2);
			Guid.TryParse("fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626", out campaign1);
			Guid.TryParse("aeb333e2-5afd-48f7-8dbc-59fe3136e061", out campaign2);

			var sqlInsert = @"
INSERT INTO dbo.GenPivot
	(XX_PK, XX_RelationType, XX_Relation1TableCode, XX_Relation1ID, XX_Relation2TableCode, XX_Relation2ID)
VALUES
	(newid(), 'RAT', 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'P8', '4e33c461-ed10-465c-a064-f8e5a52054cc'),
	(newid(), 'RAT', 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'O1', '3e1fc5e3-b779-4177-b273-1d36461089e1'),
	(newid(), 'RAT', 'O1', '3e1fc5e3-b779-4177-b273-1d36461089e1', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8'),
	(newid(), 'RAT', 'OQ', '6a03dc3a-da7e-4de0-b08a-0c86d6ca0673', 'P8', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c'),
	(newid(), 'RAT', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'TH', 'bd41879b-62ea-406f-8549-9c48c21da95b'),
	(newid(), 'RAT', 'VB', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8'),
	(newid(), 'RAT', 'G0', 'aeb333e2-5afd-48f7-8dbc-59fe3136e061', 'P8', '4e33c461-ed10-465c-a064-f8e5a52054cc'),
	(newid(), 'RAT', 'AH', '7dc9fe58-15bb-4d5c-88f9-2fd5ab3f24ca', 'VB', 'eecba5d5-2390-4361-b982-cd38dad9d2bd') -- non sales-relation pivot
";

			Db.Connection.ExecuteScalar("INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('" + company.ToString() + "', 'DAN', 'AU company', 'AU', 'AUD')");
			Db.Connection.ExecuteScalar(@"INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
										VALUES ('" + campaign1.ToString() + "','" + company.ToString() + "', '~boobies1~', 'TST00001000', GetUtcDate(), 'E', GetUtcDate(), 'E')");
			Db.Connection.ExecuteScalar(@"INSERT INTO dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
										VALUES ('" + campaign2.ToString() + "','" + company.ToString() + "', '~boobies2~', 'TST00001001', GetUtcDate(), 'E', GetUtcDate(), 'E')");
			Db.Connection.ExecuteScalar("INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('" + orgHeader.ToString() + "', 'ZZZ')");
			Db.Connection.ExecuteScalar(@"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OH, P8_OpportunityID, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
										VALUES ('" + opportunity1.ToString() + "','" + orgHeader + "', 'OppId1', '" + company.ToString() + "', GetUtcDate(), 'E', GetUtcDate(), 'E')");
			Db.Connection.ExecuteScalar(@"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OH, P8_OpportunityID, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
										VALUES ('" + opportunity2.ToString() + "','" + orgHeader + "', 'OppId2', '" + company.ToString() + "', GetUtcDate(), 'E', GetUtcDate(), 'E')");
			Db.Connection.ExecuteScalar(sqlInsert);

			AssertEquals(campaign2, Db.Connection.ExecuteScalar("SELECT [CCN_G0_RootCampaignID] FROM dbo.ViewCampaignChildNode WHERE CCN_ActivityID = '" + opportunity1.ToString() + "'"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT [CCN_G0_RootCampaignID] FROM dbo.ViewCampaignChildNode WHERE CCN_ActivityID = '" + opportunity2.ToString() + "'"));
		}
	}
}

