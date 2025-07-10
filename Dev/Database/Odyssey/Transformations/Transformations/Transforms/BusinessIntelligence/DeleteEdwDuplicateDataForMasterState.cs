using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence
{
	public class DeleteEdwDuplicateDataForMasterState : EdwDataTransformation
	{
		public override string UserDescription => "Delete duplicate data for MasterState in EDW.";

		public override void RunEdwTransformation(DbConnection edwConnection, TransformationSection section, CancellationToken token)
		{
			if (section == TransformationSection.OfflinePreUpgrade)
			{
				string sqlText = @"
IF EXISTS (SELECT * FROM sys.tables WHERE name = N'MasterState' AND type = 'U')
BEGIN
				WITH CTE AS
				(
					SELECT *, ROW_NUMBER() OVER(PARTITION BY ParamName ORDER BY ParamID) AS RowNumber
					FROM [biadmin].MasterState
				)
				DELETE FROM CTE
				WHERE RowNumber > 1
END";
				edwConnection.ExecuteNonQuery(sqlText);
			}
		}
	}
}
