using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterData.ComplianceRisk;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.MasterData.ComplianceRisk
{
	[TestedType(typeof(TG_RefComplianceCommodityAlert_CommodityRiskStatus_Insert))]
	class TG_RefComplianceCommodityAlert_CommodityRiskStatus_InsertTest : DbCreateScriptTest
	{
		public void Test_TG_RefComplianceCommodityAlert_CommodityRiskStatus_Insert()
		{
			var sql = @"
			DELETE FROM dbo.RefComplianceCommodityAlert
			INSERT INTO dbo.RefComplianceCommodityAlert (
				RCR_PK, RCR_IsActive, RCR_AlertCode, RCR_AlertDescription, RCR_AlertName, 
				RCR_AlertType, RCR_CommodityRiskStatus, RCR_CountryRegion, RCR_PublishYear, 
				RCR_SourceURL, RCR_TradeDirection, RCR_SystemCreateTimeUtc, RCR_SystemCreateUser, 
				RCR_SystemLastEditTimeUtc, RCR_SystemLastEditUser
			) VALUES
				(NEWID(), 1, '123456', 'Description for alert 1', 'Alert Name 1', 'NOM', 'HSK', 'FR', 2019, 'http://example.com/5', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR'),
				(NEWID(), 1, '234567', 'Description for alert 2', 'Alert Name 2', 'COM', 'HSK', 'FR', 2019, 'http://example.com/5', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR')";

			TestConnection.ExecuteNonQuery(sql);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"SELECT RCR_AlertCode, RCR_AlertType, RCR_CommodityRiskStatus FROM dbo.RefComplianceCommodityAlert");

			AssertContainsExactElementsInAnyOrder(new[] { ("123456", "NOM", "PRS"), ("234567", "COM", "HSK") }, result.Rows.Cast<DataRow>().Select(x => (x["RCR_AlertCode"].ToString(), x["RCR_AlertType"].ToString(), x["RCR_CommodityRiskStatus"].ToString())).ToList());
		}
	}
}
