using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Scim
{
	public class RemoveExternalIdForSystemGroups : DataTransformation
	{
		public override string UserDescription => "Removes ExternalId set to System Defined Groups";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);
			var sql = @"UPDATE dbo.GlbGroup set GG_ExternalId = '', GG_SystemLastEditTimeUtc = getdate(), GG_SystemLastEditUser = 'E' WHERE GG_IsSystemDefined = 1 AND GG_ExternalId <> '';";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
