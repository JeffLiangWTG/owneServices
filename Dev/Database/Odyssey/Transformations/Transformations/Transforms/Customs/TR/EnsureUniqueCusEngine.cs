using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TR
{
	class EnsureUniqueCusEngine : DataTransformation
	{
		public override string UserDescription => "Ensure CusEngine is unique for each CusVehicle with CVH_DataModel = 'TR'.";

		protected override void OfflinePreUpgradeTransform()
		{
			EnsureUniqueVehicle();
		}

		static bool SourceColumnsExists()
		{
			return DbObjectCreator.TableExists(Db.Connection, CusEngineSchema.Constants.TableName)
					&& DbObjectCreator.TableExists(Db.Connection, CusVehicleSchema.Constants.TableName)
					&& DbObjectCreator.ColumnExists(Db.Connection, CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.CEG_ParentID)
					&& DbObjectCreator.ColumnExists(Db.Connection, CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_DataModel);
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
	WHERE GC_RN_NKCountryCode = 'TR'
	))
BEGIN
	RETURN
END

DELETE FROM dbo.CusEngine
WHERE CEG_PK IN
(
	SELECT CEG_PK FROM
	(
		SELECT CEG_PK, ROW_NUMBER() OVER (PARTITION BY CEG_ParentID ORDER BY CEG_SystemCreateTimeUtc DESC) AS RowNumber
		FROM dbo.CusEngine INNER JOIN dbo.CusVehicle ON CVH_PK = CEG_ParentID
		WHERE CVH_DataModel = 'TR'
	) AS ordered
	WHERE RowNumber > 1
)
");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
