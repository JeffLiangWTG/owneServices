using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class UpdateEdiIdentityCertificateCARootDefaultValueIfIsEmptyOrS2STrustDataTransformation : DataTransformation
	{
		public override string UserDescription => "Setting default value when ICE_CARoot is empty or S2STrust";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityCertificate"))
			{
				var sql = "update dbo.EdiIdentityCertificate set ICE_CARoot = 'S2S', ICE_SystemLastEditTimeUtc = GETUTCDATE(), ICE_SystemLastEditUser = '~BP' where (ICE_CARoot = '' or ICE_CARoot = 'S2STrust')";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
