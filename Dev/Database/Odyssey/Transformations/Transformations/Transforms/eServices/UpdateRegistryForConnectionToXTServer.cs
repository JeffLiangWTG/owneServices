using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.eServices;

public class UpdateRegistryForConnectionToXTServer : RegistryDataTransformation
{
	public override string UserDescription => "Update PreserveTestValue For ConnectionToXTServer Registry.";

	protected override void OfflinePostUpgradeTransform()
	{
		Db.Connection.ExecuteNonQuery(@"
UPDATE
	dbo.StmData
SET
	SD_PreserveTestValue = 1,
	SD_SystemLastEditTimeUtc = GETUTCDATE(),
	SD_SystemLastEditUser = '~BP'
WHERE
	SD_Name = @name
",
			command =>
			{
				command.AddParameterBasedOnDbColumn("@name", "ConnectionToXTServer", StmDataSchema.SD_Name);
			});
	}
}

