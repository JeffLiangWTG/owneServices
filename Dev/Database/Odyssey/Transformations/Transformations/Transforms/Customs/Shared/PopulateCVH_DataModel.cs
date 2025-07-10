using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class PopulateCVH_DataModel : DataTransformation
	{
		public override string UserDescription => "Populate new column CVH_DataModel";

		const string StatusName = "OnlinePopulateCVH_DataModel_Status";

		const string WatermarkName = "OnlinePopulateCVH_DataModel_Watermark";

		bool SourceColumnsExists()
		{
			return DbObjectCreator.TableExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName)
			&& DbObjectCreator.TableExists(Db.Connection, CusVehicleSchema.Constants.TableName)
			&& DbObjectCreator.ColumnExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.JI_DataModel)
			&& DbObjectCreator.ColumnExists(Db.Connection, CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_ParentID);
		}

		protected override void OnlinePreUpgradeTransform()
		{
			if (SourceColumnsExists())
			{
				var tablePreSynchroniser = new TablePreSynchroniser(manager, Db.Connection.CurrentDatabase, TemplateDb);

				IEnumerable<string> populatingColumnName = new[] { CusVehicleSchema.Constants.CVH_DataModel };
				var columnsToPopulate = OneOffAddAndPopulateTransformation.GetColumnsToPopulateDataTable(Db.Connection.CurrentDatabase, TemplateDb, StatusName, WatermarkName, new[] { (CusVehicleSchema.Constants.SqlSchemaName, CusVehicleSchema.Constants.TableName, populatingColumnName) });
				var populatedTables = tablePreSynchroniser.GetColumnsToPopulate(columnsToPopulate, CreatePopulatedColumns);

				tablePreSynchroniser.DoPopulate(
					populatedTables,
					populatedTables.Count,
					populatedTables.Sum(x => x.TargetTableSize),
					UserDescription,
					"(+)",
					StatusName,
					WatermarkName);
			}
		}

		void CreatePopulatedColumns(IGrouping<string, ColumnChangeMetadata> table, PopulatedTable populatedTable)
		{
			var col = table.Single();
			var populatedColumn = new PopulatedColumn
			{
				ColumnId = int.Parse(col.GetColumnId(), CultureInfo.InvariantCulture),
				ColumnName = col.ColumnName,
				ColumnType = col.GetFullTypeDeclaration(),
				ColumnNullable = "NOT NULL",
				IsSparse = false,
				ColumnDefaultConstraint = $"CONSTRAINT {col.CalculatedDefaultConstraintName} DEFAULT {col.GetDefaultClause()}",
				PopulateSource = @"INNER JOIN (
	SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
) A ON PK = CVH_ParentID",
				PopulateExpression = @"CASE
		WHEN DataModel IN ('NA', 'LS', 'BW', 'SZ') THEN 'ASY'
		ELSE DataModel
	END",
				PopulatePreAddWhereClause = "OR CVH_DataModel = '' AND DataModel <> ''"
			};

			if (!populatedTable.TargetTableStatus.HasValue)
			{
				populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
			}

			populatedTable.ColumnList.Add(populatedColumn);
			populatedTable.ColumnList.Add(new ()
			{
				ColumnName = CusVehicleSchema.Constants.CVH_SystemLastEditUser,
				IsSparse = false,
				PopulateExpression = "'~BP'"
			});
			populatedTable.ColumnList.Add(new ()
			{
				ColumnName = CusVehicleSchema.Constants.CVH_SystemLastEditTimeUtc,
				IsSparse = false,
				PopulateExpression = "GetUtcDate()"
			});
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
UPDATE dbo.CusVehicle
SET
	CVH_DataModel = CASE
		WHEN DataModel IN ('NA', 'LS', 'BW', 'SZ') THEN 'ASY'
		ELSE DataModel
	END,
	CVH_SystemLastEditUser = '~BP',
	CVH_SystemLastEditTimeUtc = GetUtcDate()
FROM dbo.CusVehicle
INNER JOIN (
	SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
) A ON PK = CVH_ParentID
WHERE CVH_DataModel = ''");

			Db.Connection.ExecuteNonQuery(sql);

			Db.Connection.ExecuteNonQuery(@"
DELETE FROM dbo.CusVehicle
WHERE CVH_ParentTableCode = 'JI' AND
CVH_ParentID NOT IN (SELECT JI_PK FROM dbo.JobComInvoiceLine)");
		}
	}
}
