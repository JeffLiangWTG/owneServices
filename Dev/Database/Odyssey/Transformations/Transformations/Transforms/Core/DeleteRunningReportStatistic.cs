using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core;
public class DeleteRunningReportStatistic : DataTransformation
{
	public override string UserDescription => "Delete report statistics whose status is running in StmReportRun";
	public override bool IsRequired => base.IsRequired && DbObjectCreator.ColumnExists(Db.Connection, "StmReportRun", "RRI_ReportDescription");

	protected override void OfflinePreUpgradeTransform()
	{
		var sql = @"DELETE FROM dbo.StmReportRun WHERE RRI_ReportDescription ='Preview Task' AND RRI_Status = 'RUN'";

		Db.Connection.ExecuteNonQuery(sql);
	}
}