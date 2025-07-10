using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.HRMS
{
	class GlbWorkPatternDeleteDuplicateRecordsWithSameStaffAndEffectiveDate : DataTransformation
	{
		public override string UserDescription => "Delete duplicate records with the same staff and effective date";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, GlbWorkPatternSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(sqlUpdate);
			}
		}

		const string sqlUpdate = @"
WITH CTE AS (
	SELECT	GWP_PK,
			GWP_GS_Staff,
			GWP_EffectiveDate,
			ROW_NUMBER() OVER (PARTITION BY GWP_GS_Staff, GWP_EffectiveDate ORDER BY GWP_PK) AS RN
	FROM [dbo].[GlbWorkPattern]
	WHERE GWP_IsApproved = 1
)
DELETE FROM [dbo].[GlbWorkPattern] 
WHERE GWP_PK IN (SELECT GWP_PK FROM CTE WHERE RN > 1);
";
	}
}
