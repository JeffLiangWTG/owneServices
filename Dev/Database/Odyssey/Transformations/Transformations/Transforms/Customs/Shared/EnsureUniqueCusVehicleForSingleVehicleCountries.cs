using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	sealed class EnsureUniqueCusVehicleForSingleVehicleCountries : DataTransformation
	{
		public override string UserDescription => "Ensure CusVehicle is unique for countries with single vehicle.";

		protected override void OfflinePreUpgradeTransform()
		{
			EnsureUniqueVehicle();
		}

		static bool SourceColumnsExists()
		{
			return DbObjectCreator.TableExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, CusVehicleSchema.Constants.TableName)
					&& DbObjectCreator.ColumnExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.JI_DataModel)
					&& DbObjectCreator.ColumnExists(Db.Connection, CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_ParentID);
		}

		static void EnsureUniqueVehicle()
		{
			if (!SourceColumnsExists())
			{
				return;
			}

			var sql = FormattableString.Invariant($@"
IF (NOT EXISTS(
	SELECT TOP 1 GC_RN_NKCountryCode FROM dbo.GlbCompany
	WHERE GC_RN_NKCountryCode IN ('ES', 'PL', 'NA', 'LS', 'BW', 'SZ')
	))
BEGIN
	RETURN
END

DELETE FROM dbo.CusVehicle
WHERE CVH_PK IN
(
	SELECT CVH_PK FROM
	(
		SELECT CVH_PK, ROW_NUMBER() OVER (PARTITION BY CVH_ParentID ORDER BY CVH_ParentID) AS RowNumber
		FROM dbo.CusVehicle INNER JOIN dbo.JobComInvoiceLine ON JI_PK = CVH_ParentID
		WHERE JI_DataModel = 'ES'
		UNION ALL
		SELECT CVH_PK, ROW_NUMBER() OVER (PARTITION BY CVH_ParentID ORDER BY CVH_SystemCreateTimeUtc DESC) AS RowNumber
		FROM dbo.CusVehicle INNER JOIN dbo.JobComInvoiceLine ON JI_PK = CVH_ParentID
		WHERE JI_DataModel = 'PL'
		UNION ALL
		SELECT CVH_PK, ROW_NUMBER() OVER (PARTITION BY CVH_ParentID ORDER BY CVH_PK) AS RowNumber
		FROM dbo.CusVehicle INNER JOIN dbo.JobComInvoiceLine ON JI_PK = CVH_ParentID
		WHERE JI_DataModel IN ('NA', 'LS', 'BW', 'SZ')
	) AS ordered
	WHERE RowNumber > 1
)
");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
