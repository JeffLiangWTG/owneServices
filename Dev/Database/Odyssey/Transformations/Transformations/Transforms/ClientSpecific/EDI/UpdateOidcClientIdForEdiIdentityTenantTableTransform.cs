using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateOidcClientIdForEdiIdentityTenantTableTransform : DataTransformation
	{
		public override string UserDescription => "Update IDT_OidcClientId for EdiIdentityTenant Table";

		const string TableName = "EdiIdentityTenant";

		const string ColumnName = "IDT_OidcClientId";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, TableName) && !DbObjectCreator.ColumnExists(Db.Connection, TableName, ColumnName))
			{
				DbObjectCreator.CreateColumn(Db.Connection, TableName, ColumnName, "varchar(36)", "''");

				var updateColumnSql = @"
Update dbo.EdiIdentityTenant
Set IDT_OidcClientId = '3492b154-a8ce-4ce3-a956-511276cbd9e6'
Where IDT_TenantId = '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1'
";
				Db.Connection.ExecuteNonQuery(updateColumnSql);
			}
		}
	}
}
