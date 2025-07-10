using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;

public class UpdateReleaseBuildHlPackagePathToPointToNewArtifactRepoAlias : DataTransformation
{
	const string CW1Packages = @"\\cw1datfiles.wtg.zone\CW1Packages";

	public override string UserDescription => $"As part of migrating DAT from the AU1 to the AU2 network, update all ReleaseBuild.HL_PackagePath to point to the new alias for Artifact Repository ({CW1Packages})";

	protected override void OfflinePostUpgradeTransform()
	{
		if (!DbObjectCreator.TableExists(Db.Connection, "ReleaseBuild"))
		{
			return;
		}

		const string sql = @"
DECLARE @CW1Packages VARCHAR(50) = '\\cw1datfiles.wtg.zone\';

UPDATE ReleaseBuild
SET HL_PackagePath = CONCAT(@CW1Packages, SUBSTRING(HL_PackagePath, LEN('\\datartifactrepository.wtg.zone\') + 1, LEN(HL_PackagePath)))
WHERE HL_PackagePath LIKE '\\datartifactrepository.wtg.zone\%';
";

		Db.Connection.ExecuteNonQuery(sql);
	}
}
