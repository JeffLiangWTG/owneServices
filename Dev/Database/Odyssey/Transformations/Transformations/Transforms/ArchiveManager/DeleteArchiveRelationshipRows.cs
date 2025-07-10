using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	public class DeleteArchiveRelationshipRows : RegistryDataTransformation
	{
		public override string UserDescription => "Delete Archive Relationship Rows";

		protected override void OfflinePostUpgradeTransform()
		{
			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmData WHERE SD_Name LIKE '%LastVersionRelationshipsTableWasCreatedForStage%'");
		}
	}
}
