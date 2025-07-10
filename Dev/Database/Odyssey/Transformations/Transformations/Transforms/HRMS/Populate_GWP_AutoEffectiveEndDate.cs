using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.HRMS
{
	class Populate_GWP_AutoEffectiveEndDate : DataTransformation
	{
		public override string UserDescription => "Update GWP_AutoEffectiveEndDate field for existing records";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, GlbWorkPatternSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GlbWorkPatternSchema.Constants.TableName, GlbWorkPatternSchema.Constants.GWP_AutoEffectiveEndDate, "DATETIMEOFFSET(0)"))
			{
				_ = Db.Connection.ExecuteNonQuery(sqlUpdate);
			}
		}

		const string sqlUpdate = @"
UPDATE [dbo].[GlbWorkPattern]
SET
	[GWP_AutoEffectiveEndDate] = nextRecord.[GWP_EffectiveDate],
	[GWP_SystemLastEditTimeUtc] = GETUTCDATE(),
	[GWP_SystemLastEditUser] = '~BP'
FROM [dbo].[GlbWorkPattern] updating
CROSS APPLY (
	SELECT TOP 1 [GWP_EffectiveDate]
	FROM [dbo].[GlbWorkPattern] prospectiveNext
	WHERE
		prospectiveNext.[GWP_GS_Staff] = updating.[GWP_GS_Staff] AND
		prospectiveNext.[GWP_EffectiveDate] > updating.[GWP_EffectiveDate] AND
		prospectiveNext.[GWP_IsApproved] = 1
	ORDER BY [GWP_EffectiveDate] ASC
) AS nextRecord
WHERE updating.[GWP_IsApproved] = 1;
";
	}
}
