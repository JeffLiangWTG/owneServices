using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

public class PopulateCXH_ApplicationCode : DataTransformation
{
	public override string UserDescription => "Populate new CXH_ApplicationCode column with XIT";

	protected override void OfflinePreUpgradeTransform()
	{
		if (!DbObjectCreator.TableExists(Db.Connection, CusExitHeaderSchema.Constants.TableName))
		{
			// If there is no table at all - we do not need to do anything.
			return;
		}

		if (!DbObjectCreator.ColumnExists(Db.Connection, CusExitHeaderSchema.Constants.TableName,
				CusExitHeaderSchema.Constants.CXH_ApplicationCode))
		{
			// When there is no column at all - we create one with appropriate default value XIT that will be changed later when schema synchronized
			DbObjectCreator.CreateColumn(Db.Connection, CusExitHeaderSchema.Constants.TableName,
				CusExitHeaderSchema.Constants.CXH_ApplicationCode, "CHAR(3)", "'XIT'");
			return;
		}
		else
		{
			// If the column exists - most likely gracefully handling re-run. As offline transformation - no chunking - get everything in one go
			const string Query = """
								UPDATE dbo.CusExitHeader
								SET CXH_ApplicationCode  = 'XIT',
									CXH_SystemLastEditTimeUtc = GetUtcDate(),
									CXH_SystemLastEditUser = 'E'
								WHERE CXH_ApplicationCode = ''
								""";
			Db.Connection.ExecuteNonQuery(Query);
		}
	}
}
