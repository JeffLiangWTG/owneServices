using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.MasterData.MasterDataComplianceWise
{
	[TestedType(typeof(ChangeRefComplianceCommodityAlertRiskStatusByAlertType))]
	public class ChangeRefComplianceCommodityAlertRiskStatusByAlertTypeTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"SELECT RCR_AlertCode, RCR_AlertType, RCR_CommodityRiskStatus FROM dbo.RefComplianceCommodityAlert");
			AssertContainsExactElementsInAnyOrder(new[] { ("123456", "NOM", "PRS"), ("234567", "COM", "HSK") }, result.Rows.Cast<DataRow>().Select(x => (x["RCR_AlertCode"].ToString(), x["RCR_AlertType"].ToString(), x["RCR_CommodityRiskStatus"].ToString())).ToList());
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new ChangeRefComplianceCommodityAlertRiskStatusByAlertType();
		}

		protected override void PrepareTestData()
		{
			if (TestConnection.Exists(RefComplianceCommodityAlertSchema.Constants.TableName))
			{
				DbObjectCreator.DropTriggerIfExists(TestConnection, "TG_RefComplianceCommodityAlert_CommodityRiskStatus_Insert");
				var sql = @"
INSERT INTO [dbo].[RefComplianceCommodityAlert] 
([RCR_PK], [RCR_IsActive], [RCR_AlertCode], [RCR_AlertDescription], [RCR_AlertName], [RCR_AlertType], [RCR_CommodityRiskStatus], [RCR_CountryRegion], [RCR_PublishYear], [RCR_SourceURL], [RCR_TradeDirection], [RCR_SystemCreateTimeUtc], [RCR_SystemCreateUser], [RCR_SystemLastEditTimeUtc], [RCR_SystemLastEditUser])
VALUES 
(NEWID(), 1, '123456', 'Description for alert 1', 'Alert Name 1', 'NOM', 'HSK', 'FR', 2019, 'http://example.com/5', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR'),
(NEWID(), 1, '234567', 'Description for alert 2', 'Alert Name 2', 'COM', 'HSK', 'FR', 2019, 'http://example.com/5', 'EXP', GETDATE(), 'USR', GETDATE(), 'USR');";
				TestConnection.ExecuteNonQuery(sql);
			}
		}
	}
}
