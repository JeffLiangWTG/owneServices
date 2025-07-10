using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence
{
	public class DeleteAuditDuplicateDataForMasterStateAndAlterColumns : AuditDataTransformation
	{
		public override string UserDescription => "Delete duplicate data for MasterState and alter columns in Audit.";

		public override void RunAuditTransformation(DbConnection auditConnection, TransformationSection section, CancellationToken token)
		{
			if (section == TransformationSection.OfflinePreUpgrade)
			{
				string sqlText = @"
IF EXISTS (SELECT * FROM sys.tables WHERE name = N'MasterState' AND type = 'U')
BEGIN
				ALTER TABLE [biadmin].[MasterState] ALTER COLUMN ParamName NVARCHAR(128) NULL;
				ALTER TABLE [biadmin].[MasterState] ALTER COLUMN ParamValue NVARCHAR(128) NULL;
				WITH CTE AS
				(
					SELECT *, ROW_NUMBER() OVER(PARTITION BY ParamName ORDER BY ParamID) AS RowNumber
					FROM [biadmin].MasterState
				)
				DELETE FROM CTE
				WHERE RowNumber > 1
END";
				auditConnection.ExecuteNonQuery(sqlText);
			}
		}
	}
}
