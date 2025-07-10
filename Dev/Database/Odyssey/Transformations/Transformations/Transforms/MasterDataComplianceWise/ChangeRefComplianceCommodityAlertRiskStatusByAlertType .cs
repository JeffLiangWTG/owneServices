using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	public class ChangeRefComplianceCommodityAlertRiskStatusByAlertType : DataTransformation
	{
		public override string UserDescription => "Set Risk Status To 'PRS - Possible Risk' When Alert Type Is 'NOM - Nomenclature'";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (!DbObjectCreator.TableExists(Db.Connection, RefComplianceCommodityAlertSchema.Constants.TableName))
			{
				return;
			}

			_ = Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.RefComplianceCommodityAlert
SET RCR_CommodityRiskStatus = 'PRS',
	RCR_SystemLastEditTimeUtc = GETUTCDATE(),
	RCR_SystemLastEditUser = '~BP'
WHERE RCR_AlertType = 'NOM' AND RCR_CommodityRiskStatus = 'HSK';
");
		}
	}
}
