using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Rating;

public class AddConstraintToOrgProfitShareDetails : DataTransformation
{
	public override string UserDescription => "Apply constraints to O4_SendingPortOrCountry and O4_ReceivingPortOrCountry";

	protected override void OfflinePreUpgradeTransform()
	{
		// Prevent transformation execution in a newly created and empty database where tables are yet to be created.
		// For instance, the TestCanUpgradeEmptyMainDatabaseAndReadOnlyStorageDocDatabasesWithBI serves as a test case for this scenario.
		if (!DbObjectCreator.TableExists(Db.Connection, "OrgProfitShareDetails"))
		{
			return;
		}

		var sql = @"
		IF NOT EXISTS(
			SELECT 1 FROM information_schema.table_constraints con 
			WHERE con.table_name = 'OrgProfitShareDetails' 
				  and con.constraint_name in ('Constraint_O4_SendingPortOrCountry', 'Constraint_O4_ReceivingPortOrCountry')
		)
		BEGIN
			DELETE FROM dbo.OrgProfitShareDetails WHERE LEN(O4_SendingPortOrCountry) in (1, 3) or LEN(O4_ReceivingPortOrCountry) in (1, 3);
		END";

		Db.Connection.ExecuteNonQuery(sql);
	}
}