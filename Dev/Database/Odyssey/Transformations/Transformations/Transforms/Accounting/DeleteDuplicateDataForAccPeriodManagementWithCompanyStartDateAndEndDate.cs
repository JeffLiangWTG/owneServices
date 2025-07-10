using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class DeleteDuplicateDataForAccPeriodManagementWithCompanyStartDateAndEndDate : DataTransformation
	{
		public override string UserDescription => "Delete duplicate data for AccPeriodManagement with company_startDate_endDate";

		protected override void OfflinePreUpgradeTransform()
		{
			var tableName = AccPeriodManagementSchema.Constants.TableName;
			var sqlSchemaName = AccPeriodManagementSchema.Constants.SqlSchemaName;
			var company = AccPeriodManagementSchema.Constants.AM_GC_Company;
			var startDate = AccPeriodManagementSchema.Constants.AM_StartDate;
			var endDate = AccPeriodManagementSchema.Constants.AM_EndDate;

			if (!DbObjectCreator.ColumnsExist(
					Db.Connection,
					Db.Connection.CurrentDatabase,
					sqlSchemaName,
					tableName,
					new string[] { company, startDate, endDate }))
			{
				return;
			}

			var sql = @"
	WITH CTE AS (
		SELECT *,
		   ROW_NUMBER() OVER (PARTITION BY AM_GC_Company, AM_StartDate, AM_EndDate ORDER BY AM_Period) AS RowNumber
		FROM dbo.AccPeriodManagement
	)
	DELETE FROM CTE
		WHERE RowNumber > 1";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
