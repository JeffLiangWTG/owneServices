using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class UpdateEdiTokenAuthOnBoardingDataTenantToB2C01DataTransformation : DataTransformation
	{
		public override string UserDescription => "Update the existing onboarding data's TOD_IDT column to B2C01's tenant ID.";

		const string EdiTokenAuthOnBoardingDataTableName = "EdiTokenAuthOnBoardingData";

		const string EdiIdentityTenantTableName = "EdiIdentityTenant";

		const string ColumnName = "TOD_IDT";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, EdiTokenAuthOnBoardingDataTableName) && DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, EdiIdentityTenantTableName) && !DbObjectCreator.ColumnExists(Db.Connection, EdiTokenAuthOnBoardingDataTableName, ColumnName))
			{
				DbObjectCreator.CreateColumn(Db.Connection, EdiTokenAuthOnBoardingDataTableName, ColumnName, "uniqueidentifier");

				var updateTenantFkSql = "update dbo.EdiTokenAuthOnBoardingData set TOD_IDT = (SELECT TOP 1 IDT_PK FROM EdiIdentityTenant WHERE IDT_TenantId = '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1')";
				Db.Connection.ExecuteNonQuery(updateTenantFkSql);
			}
		}
	}
}
