using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	public class UpdateComplianceCommodityRiskStatusToNotChecked : DataTransformation
	{
		public override string UserDescription => "For compliance commodity detail, update CCD_RiskStatus from 'UNK' to 'NCH'";
		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, ComplianceCommodityDetailSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, ComplianceCommodityDetailSchema.Constants.TableName, ComplianceCommodityDetailSchema.Constants.CCD_RiskStatus)
				&& DbObjectCreator.ColumnExists(Db.Connection, ComplianceCommodityDetailSchema.Constants.TableName, ComplianceCommodityDetailSchema.Constants.CCD_SystemLastEditTimeUtc)
				&& DbObjectCreator.ColumnExists(Db.Connection, ComplianceCommodityDetailSchema.Constants.TableName, ComplianceCommodityDetailSchema.Constants.CCD_SystemLastEditUser))
			{
				var sql = @"
ALTER TABLE dbo.ComplianceCommodityDetail DROP CONSTRAINT IF EXISTS Constraint_CCD_RiskStatus

UPDATE dbo.ComplianceCommodityDetail
SET CCD_RiskStatus = 'NCH', CCD_SystemLastEditTimeUtc = GETUTCDATE(), CCD_SystemLastEditUser = '~BP'
WHERE CCD_RiskStatus = 'UNK'";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
