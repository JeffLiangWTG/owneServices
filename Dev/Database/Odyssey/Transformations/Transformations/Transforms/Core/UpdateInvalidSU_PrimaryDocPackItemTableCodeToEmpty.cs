using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	sealed class UpdateInvalidSU_PrimaryDocPackItemTableCodeToEmpty : DataTransformation
	{
		public override string UserDescription => "Update Invalid SU_PrimaryDocPackItemTableCode to empty";

		protected override void OfflinePreUpgradeTransform()
		{
			var sqlText = @"
IF OBJECT_ID('dbo.StmMenuItem', 'U') IS NOT NULL
BEGIN
    UPDATE
        dbo.StmMenuItem
    SET
        SU_PrimaryDocPackItemTableCode = '',
        SU_SystemLastEditTimeUtc = GETUTCDATE(),
        SU_SystemLastEditUser = '~BP'
    WHERE
        SU_PrimaryDocPackItemTableCode NOT IN ('', 'SF', 'SI');
END
";
			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
