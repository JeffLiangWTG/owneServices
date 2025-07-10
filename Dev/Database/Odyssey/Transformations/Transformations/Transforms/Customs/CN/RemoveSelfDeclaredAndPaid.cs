using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN
{
	public class RemoveSelfDeclaredAndPaid : DataTransformation
	{
		public override string UserDescription => "Remove ZO_IsSelfDeclaredAndPaid";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.OrgCountryData
SET
	OV_ImportCustomsDefaultAddInfo=REPLACE(OV_ImportCustomsDefaultAddInfo, '<IsSelfDeclaredAndPaid>Y</IsSelfDeclaredAndPaid>', ''),
	OV_SystemLastEditTimeUtc=GETUTCDATE(),
	OV_SystemLastEditUser='~BP'
WHERE OV_RN_NKClientCountryRelation = 'CN' AND OV_ImportCustomsDefaultAddInfo like '%<IsSelfDeclaredAndPaid>Y</IsSelfDeclaredAndPaid>%'";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
