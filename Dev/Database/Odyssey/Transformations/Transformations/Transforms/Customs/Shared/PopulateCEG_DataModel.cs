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
	class PopulateCEG_DataModel : DataTransformation
	{
		public override string UserDescription => "Populate new column CEG_DataModel";

		const string StatusName = "OnlinePopulateCEG_DataModel_Status";

		const string WatermarkName = "OnlinePopulateCEG_DataModel_Watermark";

		bool SourceColumnsExists()
		{
			return DbObjectCreator.TableExists(Db.Connection, CusEngineSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, CusVehicleSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, CusEngineSchema.Constants.TableName, CusEngineSchema.Constants.CEG_ParentID)
				&& DbObjectCreator.ColumnExists(Db.Connection, CusVehicleSchema.Constants.TableName, CusVehicleSchema.Constants.CVH_DataModel)
				&& DbObjectCreator.ColumnExists(Db.Connection, JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.JI_DataModel);
		}

		protected override void OnlinePreUpgradeTransform()
		{
			if (SourceColumnsExists())
			{
				var tablePreSynchroniser = new TablePreSynchroniser(manager, Db.Connection.CurrentDatabase, TemplateDb);

				IEnumerable<string> populatingColumnName = new[] { CusEngineSchema.Constants.CEG_DataModel };
				var columnsToPopulate = OneOffAddAndPopulateTransformation.GetColumnsToPopulateDataTable(Db.Connection.CurrentDatabase, TemplateDb, StatusName, WatermarkName, new[] { (CusEngineSchema.Constants.SqlSchemaName, CusEngineSchema.Constants.TableName, populatingColumnName) });
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
	SELECT PK = CVH_PK, DataModel = CVH_DataModel FROM dbo.CusVehicle
	UNION ALL
	SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
) A ON PK = CEG_ParentID",
				PopulateExpression = "DataModel",
				PopulatePreAddWhereClause = "OR CEG_DataModel = ''"
			};

			if (!populatedTable.TargetTableStatus.HasValue)
			{
				populatedTable.TargetTableStatus = (TargetTableStatus)Enum.Parse(typeof(TargetTableStatus), col.GetColumnExtendedProperty(), true);
			}

			populatedTable.ColumnList.Add(populatedColumn);
			populatedTable.ColumnList.Add(new()
			{
				ColumnName = CusEngineSchema.Constants.CEG_SystemLastEditUser,
				IsSparse = false,
				PopulateExpression = "'~BP'"
			});
			populatedTable.ColumnList.Add(new()
			{
				ColumnName = CusEngineSchema.Constants.CEG_SystemLastEditTimeUtc,
				IsSparse = false,
				PopulateExpression = "GetUtcDate()"
			});
		}

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
UPDATE dbo.CusEngine
SET
	CEG_DataModel = DataModel,
	CEG_SystemLastEditUser = '~BP',
	CEG_SystemLastEditTimeUtc = GetUtcDate()
FROM dbo.CusEngine INNER JOIN (
		SELECT PK = CVH_PK, DataModel = CVH_DataModel FROM dbo.CusVehicle
		UNION ALL
		SELECT PK = JI_PK, DataModel = JI_DataModel FROM dbo.JobComInvoiceLine
	) A ON PK = CEG_ParentID
WHERE CEG_DataModel = ''");

			Db.Connection.ExecuteNonQuery(sql);

			Db.Connection.ExecuteNonQuery(@"
DELETE FROM dbo.CusEngine
WHERE CEG_ParentTableCode = 'CVH' AND
CEG_ParentID NOT IN (SELECT CVH_PK FROM dbo.CusVehicle)");

			Db.Connection.ExecuteNonQuery(@"
DELETE FROM dbo.CusEngine
WHERE CEG_ParentTableCode = 'JI' AND
CEG_ParentID NOT IN (SELECT JI_PK FROM dbo.JobComInvoiceLine)");

			// Delete all CusEngine records with CEG_ParentTableCode='OP' since we are removing 'OP' from Constraint_CEG_ParentTableCode
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.CusEngine WHERE CEG_ParentTableCode = 'OP'");
		}
	}
}
