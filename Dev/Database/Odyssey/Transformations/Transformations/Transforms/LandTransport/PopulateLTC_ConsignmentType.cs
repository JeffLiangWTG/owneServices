using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.LandTransport
{
	class PopulateLTC_ConsignmentType : DataTransformation
	{
		public override string UserDescription => "Populating Land Transport Consignment Type.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE dbo.DtbConsignment
SET    LTC_ConsignmentType = 'LTC',
       LTC_SystemLastEditTimeUtc=GETUTCDATE(),
       LTC_SystemLastEditUser='~BP'
WHERE (LTC_ConsignmentType='')
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
