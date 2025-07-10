using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Schema
{
	public partial class ColumnSynchroniser
	{
		public ColumnSynchroniser(DbConnection upgConnection, IUpgradeTaskWorkflowLogger taskLogger, string dbBeingUpgraded, string templateDb)
		{
			this.dbBeingUpgraded = dbBeingUpgraded;
			this.templateDb = templateDb;
			this.upgConnection = upgConnection;
			this.taskLogger = taskLogger;

			this.changeRetriever = new ColumnChangeRetriever(upgConnection, dbBeingUpgraded, templateDb);
		}

		protected readonly string dbBeingUpgraded;
		protected readonly string templateDb;
		protected readonly DbConnection upgConnection;
		protected readonly IUpgradeTaskWorkflowLogger taskLogger;
		readonly ColumnChangeRetriever changeRetriever;

		public void DropAlterAndAddColumns()
		{
			taskLogger.ActivateSubtaskProgress(9);

			taskLogger.StartSubtask("Convert char flags to bit");
			ConvertCharFlagsToBit();

			taskLogger.StartSubtask("Convert [n][var]char to [n][var]char");
			ConvertCharToChar();

			taskLogger.StartSubtask("Convert decimal to decimal");
			ConvertDecimalToDecimal();

			taskLogger.StartSubtask("Convert [small]datetime[2] to date");
			ConvertDateTimeToDate();

			taskLogger.StartSubtask("Complete Online-Transformed [small]datetime[2] to datetimeoffset");
			CompleteDateTimeToDateTimeOffsetTransform();

			taskLogger.StartSubtask("Complete Online-Transformed Column Updates");
			CompleteColumnUpdateTransform();

			taskLogger.StartSubtask("Drop old columns");
			DropOldColumns();

			taskLogger.StartSubtask("Alter modified columns");
			AlterColumns();

			taskLogger.StartSubtask("Fix column name casing");
			FixColumnNameCasing();

			taskLogger.StartSubtask("Add new columns");
			AddNewColumns();
		}

		#region Drop Column

		void DropOldColumns()
		{
			var columnsToDrop = changeRetriever.GetColumnsToDrop();
			var columns = columnsToDrop.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row));

			foreach (var schema in columns.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					taskLogger.ShowInfoMessage($"    {schema.Key}.{table.Key}:");
					AddModifiedTable(schema.Key, table.Key);

					foreach (var column in table.OrderByDescending(c => c.IsComputedColumnChange).ThenBy(c => c.ColumnName))
					{
						taskLogger.ShowInfoMessage($"        (-) {column.ColumnName}");

						DropColumnAndRelatedObjects(column.TableSchema, column.TableName, column.ColumnName);
					}
				}
			}
		}

		void DropColumnAndRelatedObjects(string tableSchema, string tableName, string columnName)
		{
			var columnDependencyRemover = new DbColumnDependencyRemover(tableSchema, tableName, columnName);
			columnDependencyRemover.DropRelateObjects(upgConnection);
			DropColumn(tableSchema, tableName, columnName);
		}

		void DropColumn(string tableSchema, string tableName, string columnName)
		{
			upgConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture,
				"ALTER TABLE {0}.{1}.{2} DROP COLUMN {3}"
				, dbBeingUpgraded.QuoteName()
				, tableSchema.QuoteName()
				, tableName.QuoteName()
				, columnName.QuoteName()
				));
		}

		#endregion

		#region Alter Column

		void AlterColumns()
		{
			var columnsToAlter = changeRetriever.GetColumnsToAlter();
			var columns = columnsToAlter.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row));

			foreach (var schema in columns.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					taskLogger.ShowInfoMessage($"    {schema.Key}.{table.Key}:");
					AddModifiedTable(schema.Key, table.Key);

					foreach (var column in table.OrderByDescending(c => c.IsComputedColumnChange).ThenBy(c => c.ColumnName))
					{
						taskLogger.ShowInfoMessage($"        (~) {column.ColumnDeclaration}");

						AlterColumnPreemptingErrors(column);
					}
				}
			}
		}

		void AlterColumnPreemptingErrors(ColumnChangeMetadata columnMetadata)
		{
			CheckPopulatingForTest(columnMetadata);

			bool areDataTypesCompatible = AreOldAndNewTypesCompatible(columnMetadata.OldDataType, columnMetadata.DataType);
			bool mustDropAndRecreate = (!areDataTypesCompatible || columnMetadata.IdentityChanged || columnMetadata.IsComputedColumnChange);

			if (mustDropAndRecreate)
			{
				// Drop it and let it be re-added afterwards (by AddNewColumns)
				string message = (areDataTypesCompatible) ?
					String.Format("must recreate column to modify {0}", (columnMetadata.IdentityChanged ? "idenity seed and/or increment" : "computed definition")) :
					String.Format("old [{0}] and new [{1}] data types are not compatible", columnMetadata.OldDataType.Description, columnMetadata.DataType.Description);
				taskLogger.ShowInfoMessage("            (!): " + message);
				DropColumnAndRelatedObjects(columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName);
			}
			else
			{
				new DbColumnDependencyRemover(columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName)
					.DropRelateObjects(upgConnection);

				PrepareColumnForChange(columnMetadata);
				AlterColumn(columnMetadata);
				PerformAfterTypeChangeAdjustments(columnMetadata);
			}
		}

		[Conditional("DEBUG")]
		void CheckPopulatingForTest(ColumnChangeMetadata col)
		{
			var shouldHaveAlreadyBeenPopulated =
				(1 == 2
					|| // Convert [var]char(1) to bit
					(
						col.OldDataType.SqlType.In(SqlDbType.Char, SqlDbType.VarChar)
						&& col.DataType.SqlType == SqlDbType.Bit
					)
					|| // Convert [n][var]char to [n][var]char
					(
						col.OldDataType.IsCharType
						&& col.DataType.IsCharType

						// Ignore computed columns
						&& !col.IsComputedColumnChange

						// Ignore decreasing size
						&& short.TryParse(col.GetOldLengthClause(), out var oldLength)
						&& short.TryParse(col.GetLengthClause(), out var newLength)
						&& ((oldLength < 0) ? short.MaxValue : oldLength) <= ((newLength < 0) ? short.MaxValue : newLength)

						// Ignore metadata-only operations
						&&
						(1 == 2
							|| col.OldDataType.SqlType != col.DataType.SqlType
							||
							(
								col.OldDataType.SqlType == col.DataType.SqlType
								&& oldLength != newLength
								&& col.OldDataType.SqlType.In(SqlDbType.Char, SqlDbType.NChar)
							)
						)

						// Ignore Offline-only compatible types
						// NOT NULL Columns of type varchar(max), nvarchar(max), varbinary(max), xml, text, ntext, image, hierarchyid, geometry, geography, or CLR UDTS, can't be added in an online operation
						&&
						(
							col.IsNullable
							|| newLength != -1
							|| !col.DataType.SqlType.In(SqlDbType.VarChar, SqlDbType.NVarChar)
						)
					)
				);

			if (shouldHaveAlreadyBeenPopulated)
			{
				var oldTypeDefinition = FormattableString.Invariant($"{col.OldDataType.Description}({col.GetOldLengthClause()})");
				var newTypeDefinition = (col.DataType.SqlType == SqlDbType.Bit)
					? col.DataType.Description
					: FormattableString.Invariant($"{col.DataType.Description}({col.GetLengthClause()})");

				var message = FormattableString.Invariant($"Conversion from '{oldTypeDefinition}' to '{newTypeDefinition}' should be done via 'Convert with populating'.");
				throw new InvalidOperationException(message);
			}
		}

		bool AreOldAndNewTypesCompatible(DataTypeInfo oldDataType, DataTypeInfo newDataType)
		{
			bool result = false;

			if (oldDataType.SqlType == newDataType.SqlType)
			{
				result = true;
			}
			else
			{
				if (
					(oldDataType.IsCharType && newDataType.IsCharOrTextType) ||
					(oldDataType.IsCharOrTextType && newDataType.IsCharType) ||
					(oldDataType.IsIntergerOrDecimalType && newDataType.IsIntergerOrDecimalType) ||
					(oldDataType.IsBinaryOrImageType && newDataType.IsBinaryOrImageType) ||
					(oldDataType.IsDateTimeType && newDataType.IsDateTimeType) ||
					(oldDataType.IsDateTimeOffsetType && newDataType.IsDateTimeOffsetType) ||
					(oldDataType.IsDateTimeType && newDataType.IsDateTimeOffsetType) ||
					(oldDataType.IsCharOrTextType && newDataType.SqlType == SqlDbType.Xml) ||
					(oldDataType.SqlType == SqlDbType.Char && newDataType.SqlType == SqlDbType.Bit) ||
					(oldDataType.SqlType == SqlDbType.Bit && newDataType.SqlType == SqlDbType.Char) ||
					(oldDataType.SqlType == SqlDbType.Int && newDataType.SqlType == SqlDbType.Money) ||
					(oldDataType.SqlType == SqlDbType.Money && newDataType.SqlType == SqlDbType.Decimal)
					)
				{
					result = true;
				}
			}

			return result;
		}

		void AlterColumn(ColumnChangeMetadata columnMetadata)
		{
			string sqlText = String.Format("ALTER TABLE [{0}].[{1}].[{2}] ALTER COLUMN {3}", dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnDeclaration);
			upgConnection.ExecuteNonQuery(sqlText);
		}

		/// <summary>
		/// Ensures columns converted to bit data-type have only 0 and 1 values.
		/// Updates null values to the new default value if necessary.
		/// Adjusts values to fit new type precision if necessary.
		/// </summary>
		void PrepareColumnForChange(ColumnChangeMetadata columnMetadata)
		{
			ConvertCharValuesToBitIfApplicable(columnMetadata);
			PopulateNullsWithDefaulValueIfApplicable(columnMetadata);
			FixColumnDownSizeIfNecessary(columnMetadata);
		}

		/// <summary>
		/// If columns is being converted from CHAR(1) to BIT,
		/// modifies Y and N values to 1 and 0 respectively.
		/// </summary>
		void ConvertCharValuesToBitIfApplicable(ColumnChangeMetadata columnMetadata)
		{
			if (columnMetadata.OldDataType.SqlType == SqlDbType.Char && columnMetadata.DataType.SqlType == SqlDbType.Bit)
			{
				string sqlText = String.Format(
					"UPDATE [{0}].[{1}].[{2}] SET {3} = CASE {3} WHEN 'Y' THEN '1' ELSE '0' END;",
					dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName);
				upgConnection.ExecuteNonQuery(sqlText);
			}
		}

		/// <summary>
		/// Updates null values to the new default value if necessary.
		/// </summary>
		void PopulateNullsWithDefaulValueIfApplicable(ColumnChangeMetadata columnMetadata)
		{
			if (!columnMetadata.IsNullable)
			{
				string columnDefault = columnMetadata.GetDefaultClause();

				if (columnDefault != null)
				{
					string sqlText = String.Format(@"
					IF exists(SELECT {3} FROM [{0}].[{1}].[{2}] WHERE {3} is null)
						UPDATE [{0}].[{1}].[{2}] SET {3} = {4} WHERE {3} is null",
						dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName, columnDefault);
					upgConnection.ExecuteNonQuery(sqlText);
				}
			}
		}

		/// <summary>
		/// Adjusts values to fit new type precision if necessary.
		/// </summary>
		void FixColumnDownSizeIfNecessary(ColumnChangeMetadata columnMetadata)
		{
			if (columnMetadata.DataType.IsTypeWithLength)
			{
				if (columnMetadata.IsNewLengthLessThanOld())
				{
					TruncateValuesToNewColumnLength(columnMetadata);
				}
			}
			else if (columnMetadata.DataType.IsIntegerType)
			{
				if (columnMetadata.IsNewPrecisionLessThanOld())
				{
					AdjustColumnValuesToIntegerType(columnMetadata);
				}
			}
			else if (columnMetadata.OldDataType.SqlType == SqlDbType.DateTime && columnMetadata.DataType.SqlType == SqlDbType.SmallDateTime)
			{
				AdjustColumnValuesToSmalldatetimeRange(columnMetadata);
			}
		}

		/// <summary>
		/// Truncate existing values that are longer than the new length
		/// </summary>
		void TruncateValuesToNewColumnLength(ColumnChangeMetadata columnMetadata)
		{
			string lengthClause = columnMetadata.GetLengthClause();

			string sqlText = String.Format(
				"UPDATE [{0}].[{1}].[{2}] SET {3} = convert({4}({5}), {3}) ",
				dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName, columnMetadata.DataType.Description, lengthClause);
			if (columnMetadata.OldDataType.IsTypeWithLength)
			{
				sqlText += String.Format(" WHERE len({0}) > {1}", columnMetadata.ColumnName, lengthClause);
			}

			upgConnection.ExecuteNonQuery(sqlText);
		}

		/// <summary>
		/// Adjust existing values to the SmallDateTime range
		/// </summary>
		void AdjustColumnValuesToSmalldatetimeRange(ColumnChangeMetadata columnMetadata)
		{
			string sqlText = String.Format(@"
				DECLARE @MinSmallDateTime smalldatetime;
				DECLARE @MaxSmallDateTime smalldatetime;
				SET @MinSmallDateTime = convert(smalldatetime, '1900-01-01');
				SET @MaxSmallDateTime = convert(smalldatetime, '2079-01-01');

				UPDATE [{0}].[{1}].[{2}]
				SET {3} =
					CASE
						WHEN {3} < @MinSmallDateTime THEN @MinSmallDateTime
						ELSE @MaxSmallDateTime
					END
				WHERE {3} < @MinSmallDateTime OR {3} > @MaxSmallDateTime",
				dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName);
			upgConnection.ExecuteNonQuery(sqlText);
		}

		/// <summary>
		/// Adjust existing values to the appropriate numeric range
		/// </summary>
		void AdjustColumnValuesToIntegerType(ColumnChangeMetadata columnMetadata)
		{
			long minValue = SqlByte.MinValue.Value;
			long maxValue = SqlByte.MaxValue.Value;

			switch (columnMetadata.DataType.SqlType)
			{
				case SqlDbType.SmallInt:
					minValue = SqlInt16.MinValue.Value;
					maxValue = SqlInt16.MaxValue.Value;
					break;

				case SqlDbType.Int:
					minValue = SqlInt32.MinValue.Value;
					maxValue = SqlInt32.MaxValue.Value;
					break;

				case SqlDbType.BigInt:
					minValue = SqlInt64.MinValue.Value;
					maxValue = SqlInt64.MaxValue.Value;
					break;
			}

			string sqlText = String.Format(@"
				UPDATE [{0}].[{1}].[{2}]
				SET {3} =
					CASE
						WHEN {3} < {4} THEN {4}
						ELSE {5}
					END
				WHERE {3} < {4} OR {3} > {5}",
				dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName, minValue.ToString(), maxValue.ToString());
			upgConnection.ExecuteNonQuery(sqlText);
		}

		void PerformAfterTypeChangeAdjustments(ColumnChangeMetadata columnMetadata)
		{
			TrimTrailingBlanksAfterCharToVarcharChange(columnMetadata);
			WriteSomethingAfterImageToVarbinaryChange(columnMetadata);
		}

		/// <summary>
		/// After converting a CHAR field to VARCHAR, trim possible trailing blank characters.
		/// </summary>
		void TrimTrailingBlanksAfterCharToVarcharChange(ColumnChangeMetadata columnMetadata)
		{
			if (
				(columnMetadata.OldDataType.SqlType == SqlDbType.Char || columnMetadata.OldDataType.SqlType == SqlDbType.NChar)
				&& (columnMetadata.DataType.SqlType == SqlDbType.VarChar || columnMetadata.DataType.SqlType == SqlDbType.NVarChar)
			)
			{
				string sqlText = String.Format(
					"UPDATE [{0}].[{1}].[{2}] SET [{3}] = rtrim([{3}]) WHERE len([{3}]) < {4};",
					dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName, columnMetadata.GetLengthClause());
				upgConnection.ExecuteNonQuery(sqlText);
			}
		}

		/// <summary>
		/// The first WRITE to a column after it is changed from image to varbinary can take a very long time on large tables,
		/// so we do one during the upgrade to prevent timeout prolems after the upgrade
		/// </summary>
		void WriteSomethingAfterImageToVarbinaryChange(ColumnChangeMetadata columnMetadata)
		{
			if (columnMetadata.OldDataType.SqlType == SqlDbType.Image && columnMetadata.DataType.SqlType == SqlDbType.VarBinary)
			{
				string sqlText = String.Format(
					"UPDATE TOP(1) [{0}].[{1}].[{2}] SET [{3}] .WRITE([{3}], 0, null) WHERE [{3}] is not null;",
					dbBeingUpgraded, columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName);
				upgConnection.ExecuteNonQuery(sqlText);
			}
		}

		#endregion

		#region Column Name Casing

		void FixColumnNameCasing()
		{
			var columnsWithDifferentCapitalisation = changeRetriever.GetColumnsToFixCase();
			var columns = columnsWithDifferentCapitalisation.Rows.Cast<DataRow>()
				.Select(row => new
				{
					TableSchema = (string)row["TabSchema"],
					TableName = (string)row["TabName"],
					NewColumnName = (string)row["NewColName"],
					OldColumnName = (string)row["OldColName"],
					IsComputedColumnChange = Convert.ToBoolean(row["IsComputedColumnChange"]),
				});

			foreach (var schema in columns.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					taskLogger.ShowInfoMessage($"    {schema.Key}.{table.Key}:");

					foreach (var column in table.OrderByDescending(c => c.IsComputedColumnChange).ThenBy(c => c.OldColumnName))
					{
						taskLogger.ShowInfoMessage($"        (#) {column.OldColumnName} => {column.NewColumnName}");

						if (column.IsComputedColumnChange)
						{
							// Drop it and let it be re-added afterwards (by AddNewColumns)
							DropColumnAndRelatedObjects(column.TableSchema, column.TableName, column.OldColumnName);
						}
						else
						{
							var columnDependencyRemover = new DbColumnDependencyRemover(column.TableSchema, column.TableName, column.OldColumnName);
							columnDependencyRemover.DropRelateObjectsBeforeRenamingColumn(upgConnection, column.NewColumnName);
							DbObjectCreator.RenameColumn(upgConnection, column.TableSchema, column.TableName, column.OldColumnName, column.NewColumnName);
						}
					}
				}
			}
		}

		#endregion

		#region Add Column

		public virtual void AddNewColumns()
		{
			var columnsToAdd = changeRetriever.GetColumnsToAdd();
			var columns = columnsToAdd.Rows.Cast<DataRow>()
				.Select(row => new ColumnChangeMetadata(row));

			foreach (var schema in columns.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					taskLogger.ShowInfoMessage($"    {schema.Key}.{table.Key}:");
					AddModifiedTable(schema.Key, table.Key);

					foreach (var column in table.OrderBy(c => c.IsComputedColumnChange).ThenBy(c => c.ColumnName))
					{
						taskLogger.ShowInfoMessage($"        (+) {column.ColumnDeclaration}");

						AddColumn(column);
					}
				}
			}
		}

		protected virtual void AddColumn(ColumnChangeMetadata column)
		{
			upgConnection.ExecuteNonQuery($"ALTER TABLE [{dbBeingUpgraded}].[{column.TableSchema}].[{column.TableName}] ADD {column.FullAddColumnDeclaration}");
		}

		#endregion

		#region Convert with populating

		#region Convert Char flags to Bit

		public static string ConvertCharToBitColumnPrefix
		{
			get { return "_2_"; }
		}

		void ConvertCharFlagsToBit()
		{
			OptionallyConvertThenDropAndRenameColumns(changeRetriever.GetColumnsToConvertCharToBit, ConvertCharToBitColumnPrefix, GetUpdateAndWhereClauseCharToBit);
		}

		void OptionallyConvertThenDropAndRenameColumns(Func<DataTable> getColumnsToConvert, string convertedColumnPrefix, Func<IGrouping<string, ColumnChangeMetadata>, (string UpdateClause, string WhereClause)> getUpdateAndWhereClause)
		{
			// Get columns to convert
			var columnsToConvert = getColumnsToConvert();
			var columns = columnsToConvert.Rows.Cast<DataRow>()
					.Select(row => new ColumnChangeMetadata(row))
					.GroupBy(col => col.TableSchema, col => col, StringComparer.OrdinalIgnoreCase);

			foreach (var schema in columns.OrderBy(s => s.Key))
			{
				foreach (var table in schema.GroupBy(s => s.TableName, col => col, StringComparer.OrdinalIgnoreCase).OrderBy(t => t.Key))
				{
					taskLogger.ShowInfoMessage(String.Format("    table {0}.{1}:", schema.Key, table.Key));
					AddModifiedTable(schema.Key, table.Key);

					foreach (var column in table.OrderBy(c => c.ColumnName))
					{
						taskLogger.ShowInfoMessage(String.Format("        (~) {0}", column.FullAddColumnDeclaration));
					}

					// Drop dependent triggers
					taskLogger.ShowInfoMessage("        Drop dependent triggers");
					DropDependentTriggers(schema.Key, table);

					taskLogger.ShowInfoMessage("        Drop Temp Default Constraints");
					DropTempDefaultConstraints(schema.Key, table);

					if (getUpdateAndWhereClause != null)
					{
						// Create new columns if not exist
						CreateColumnsIfNotExist(convertedColumnPrefix, schema.Key, table);

						// Populate new columns
						taskLogger.ShowInfoMessage("        Populate new columns");

						var (updateClause, whereClause) = getUpdateAndWhereClause(table);
						PopulateColumns(schema.Key, table, updateClause, whereClause);
					}

					// Drop old columns and Rename new columns
					taskLogger.ShowInfoMessage("        Drop old columns and Rename new columns");
					DropColumns(schema.Key, table);
					RenameColumnsAfterFieldConversions(convertedColumnPrefix, schema.Key, table);
				}
			}
		}

		static (string UpdateClause, string WhereClause) GetUpdateAndWhereClauseCharToBit(IGrouping<string, ColumnChangeMetadata> table)
		{
			var updateClauseList = new List<string>();
			var whereClauseList = new List<string>();

			foreach (var col in table)
			{
				var newColumnName = ConvertCharToBitColumnPrefix + col.ColumnName;
				if (col.IsNullable)
				{
					updateClauseList.Add(String.Format("[{0}] = CASE [{1}] WHEN 'Y' THEN 1 WHEN 'N' THEN 0 END", newColumnName, col.ColumnName));
					whereClauseList.Add(String.Format("OR [{0}] is NULL AND NULLIF([{1}], '') is NOT NULL OR [{0}] <> CASE [{1}] WHEN 'Y' THEN 1 WHEN 'N' THEN 0 END", newColumnName, col.ColumnName));
				}
				else
				{
					updateClauseList.Add(String.Format("[{0}] = CASE [{1}] WHEN 'Y' THEN 1 ELSE 0 END", newColumnName, col.ColumnName));
					whereClauseList.Add(String.Format("OR [{0}] <> CASE [{1}] WHEN 'Y' THEN 1 ELSE 0 END", newColumnName, col.ColumnName));
				}
			}

			return (String.Join(",\r\n\t", updateClauseList), String.Join("\r\n\t", whereClauseList));
		}

		void DropTempDefaultConstraints(string tableSchema, IGrouping<string, ColumnChangeMetadata> table)
		{
			var columnsToConsider = table
				.Where(c => !string.IsNullOrEmpty(c.OldDefaultName))
				.OrderBy(c => c.ColumnName);

			var dropConstraints = string.Join(", ", columnsToConsider.Select(c => Invariant($"CONSTRAINT {c.OldDefaultName.QuoteName()}")));
			if (dropConstraints.Length > 0)
			{
				upgConnection.ExecuteNonQuery(Invariant($"ALTER TABLE {tableSchema.QuoteName()}.{table.Key.QuoteName()} DROP {dropConstraints}"));
			}
		}

		#endregion // Convert Char flags to Bit

		#region Convert [n][var]char to [n][var]char

		public static string ConvertCharToCharColumnPrefix
		{
			get { return "_3_"; }
		}

		void ConvertCharToChar()
		{
			OptionallyConvertThenDropAndRenameColumns(changeRetriever.GetColumnsToConvertCharToChar, ConvertCharToCharColumnPrefix, GetUpdateAndWhereClauseCharToChar);
		}

		static (string UpdateClause, string WhereClause) GetUpdateAndWhereClauseCharToChar(IGrouping<string, ColumnChangeMetadata> table)
		{
			var newColumnPrefix = ConvertCharToCharColumnPrefix;
			var updateClauseList = new List<string>();
			var whereClauseList = new List<string>();

			foreach (var col in table)
			{
				var newColumnName = newColumnPrefix + col.ColumnName;
				var newColumnTypeDefinition = col.GetFullTypeDeclaration();
				var newColumnDefault = col.GetDefaultClause() ?? "''";
				if (col.IsNullable)
				{
					updateClauseList.Add(String.Format("[{0}] = RTRIM(CONVERT({1}, [{2}]))", newColumnName, newColumnTypeDefinition, col.ColumnName));
				}
				else
				{
					updateClauseList.Add(String.Format("[{0}] = ISNULL(RTRIM(CONVERT({1}, [{2}])), {3})", newColumnName, newColumnTypeDefinition, col.ColumnName, newColumnDefault));
				}

				whereClauseList.Add(String.Format("OR [{0}] is NULL AND [{1}] is NOT NULL OR [{0}] is NOT NULL AND [{1}] is NULL OR [{0}] <> [{1}] COLLATE database_default", newColumnName, col.ColumnName));
			}

			return (String.Join(",\r\n\t", updateClauseList), String.Join("\r\n\t", whereClauseList));
		}

		#endregion // Convert [n][var]char to [n][var]char

		#region Convert Decimal to Decimal

		public static string ConvertDecimalToDecimalColumnPrefix => "_4_";

		void ConvertDecimalToDecimal()
		{
			OptionallyConvertThenDropAndRenameColumns(changeRetriever.GetColumnsToConvertDecimalToDecimal, ConvertDecimalToDecimalColumnPrefix, GetUpdateAndWhereClauseDecimalToDecimal);
		}

		static (string UpdateClause, string WhereClause) GetUpdateAndWhereClauseDecimalToDecimal(IEnumerable<ColumnChangeMetadata> table)
		{
			var newColumnPrefix = ConvertDecimalToDecimalColumnPrefix;
			var updateClauseList = new List<string>();
			var whereClauseList = new List<string>();

			foreach (var col in table)
			{
				var newColumnName = newColumnPrefix + col.ColumnName;
				var newColumnTypeDefinition = col.GetFullTypeDeclaration();
				var newColumnDefault = col.GetDefaultClause() ?? "0.";
				updateClauseList.Add(
					col.IsNullable
						? $"[{newColumnName}] = CONVERT({newColumnTypeDefinition}, [{col.ColumnName}])"
						: $"[{newColumnName}] = ISNULL(CONVERT({newColumnTypeDefinition}, [{col.ColumnName}]), {newColumnDefault})");

				whereClauseList.Add($"OR [{newColumnName}] is NULL AND [{col.ColumnName}] is NOT NULL OR [{newColumnName}] is NOT NULL AND [{col.ColumnName}] is NULL OR [{newColumnName}] <> [{col.ColumnName}]");
			}

			return (string.Join(",\r\n\t", updateClauseList), string.Join("\r\n\t", whereClauseList));
		}

		#endregion

		#region Convert DateTimes to Date

		public static string ConvertDateTimesToDateColumnPrefix => "_DTD_";

		void ConvertDateTimeToDate()
		{
			OptionallyConvertThenDropAndRenameColumns(changeRetriever.GetColumnsToConvertDateTimesToDate, ConvertDateTimesToDateColumnPrefix, null);
		}

		#endregion

		#region Convert DateTimes to DateTimeOffset

		public static string ConvertDateTimeToDateTimeOffsetColumnPrefix => "_DTO_";

		void CompleteDateTimeToDateTimeOffsetTransform()
		{
			// [small]datetime[2] column is transformed online and the offline Schema Sync drops the current column and renames the temp transformed column to current column
			OptionallyConvertThenDropAndRenameColumns(changeRetriever.GetColumnsToCompleteDateTimesToDateTimeOffsetTransform, ConvertDateTimeToDateTimeOffsetColumnPrefix, null);
		}

		#endregion // Complete Online-Transformed [small]datetime[2] to datetimeoffset

		#region Complete Online-Transformed Column Updates

		public static string ColumnRenamePrefix => "_R_";

		void CompleteColumnUpdateTransform()
		{
			OptionallyConvertThenDropAndRenameColumns(changeRetriever.GetColumnsToReplaceWithTargetColumnsTransform, ColumnRenamePrefix, null);
		}

		#endregion // Complete Online-Transformed Column Updates

		void DropDependentTriggers(string tableSchema, IGrouping<string, ColumnChangeMetadata> table)
		{
			var sql = String.Format(@"-- Drop dependent triggers
DECLARE
	@stmt nvarchar(max) = N'';

SELECT
	@stmt += 'DROP TRIGGER [{0}].[' + name + '];' + CHAR(13) + CHAR(10)
FROM sys.triggers
WHERE 1=1
	AND parent_id = OBJECT_ID(N'[{0}].[{1}]', N'U')

--select @stmt
if (@stmt <> N'') EXEC (@stmt);
"
				, tableSchema
				, table.Key
				);

			using (var cmd = upgConnection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		bool CreateColumnsIfNotExist(string columnPrefix, string tableSchema, IGrouping<string, ColumnChangeMetadata> table)
		{
			var columnWasCreated = false;
			foreach (var column in table)
			{
				if (!DbObjectCreator.ColumnExists(upgConnection, dbBeingUpgraded, tableSchema, table.Key, String.Format("{0}{1}", columnPrefix, column.ColumnName)))
				{
					if (!columnWasCreated)
					{
						taskLogger.ShowInfoMessage("        Create new columns");
						columnWasCreated = true;
					}

					var defaultClause = String.Format(
						" CONSTRAINT [{0}{1}] DEFAULT {2}",
						columnPrefix,
						column.CalculatedDefaultConstraintName,
						GetAppropriateDefaultValue(column)
					);

					var columnDefinition = String.Format(
						"[{0}{1}] {2} {3}{4}",
						columnPrefix,                              // 0
						column.ColumnName,                         // 1
						column.GetFullTypeDeclaration(),           // 2
						(column.IsNullable) ? "NULL" : "NOT NULL", // 3
						defaultClause                              // 4
					);

					var sql = String.Format(@"ALTER TABLE [{0}].[{1}].[{2}] ADD {3};",
						dbBeingUpgraded, // 0
						tableSchema,     // 1
						table.Key,       // 2
						columnDefinition // 3
					);

					using (var cmd = upgConnection.Command(sql))
					{
						cmd.ExecuteNonQuery();
					}
				}
			}

			return columnWasCreated;
		}

		string GetAppropriateDefaultValue(ColumnChangeMetadata column)
		{
			var fakeDefault = DataUtils.GetTheMostPopularValueForTheColumn(upgConnection, dbBeingUpgraded, column.TableSchema, column.TableName, column.ColumnName);
			if (column.DataType.SqlType == SqlDbType.Bit) // convert [var]char(1) to bit
			{
				return (String.Equals(fakeDefault, "Y", StringComparison.OrdinalIgnoreCase)) ? "1" : "0";
			}
			else // convert [n][var]char to [n][var]char
			{
				if (fakeDefault == null)
				{
					var defaultClause = column.GetDefaultClause() ?? "''";
					return (column.IsNullable) ? "NULL" : defaultClause;
				}
				else
				{
					return String.Format("'{0}'", DataUtils.EscapeSingleQuotes(fakeDefault.TrimEnd(' ')));
				}
			}
		}

		void PopulateColumns(string tableSchema, IGrouping<string, ColumnChangeMetadata> table, string updateClause, string whereClause)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"
UPDATE [{0}].[{1}].[{2}] WITH(TABLOCKX) SET
	{3}
WHERE 1=2
	{4}
OPTION(RECOMPILE);
"
				, dbBeingUpgraded // 0
				, tableSchema     // 1
				, table.Key       // 2
				, updateClause    // 3
				, whereClause     // 4
				);

			upgConnection.ExecuteNonQuery(sql);
		}

		void DropColumns(string tableSchema, IGrouping<string, ColumnChangeMetadata> table)
		{
			foreach (var column in table)
			{
				DropColumnAndRelatedObjects(tableSchema, table.Key, column.ColumnName);
			}
		}

		void RenameColumnsAfterFieldConversions(string columnPrefix, string tableSchema, IGrouping<string, ColumnChangeMetadata> table)
		{
			const string renameConstraintRaw = @"
SET @SqlText = null;
SELECT @SqlText = ISNULL(@SqlText, '') + 'EXEC [{0}].sys.sp_rename ''' + s.name + '.' + dc.name + ''', ''{5}'', ''OBJECT'';'
FROM
	sys.default_constraints dc
	INNER JOIN sys.tables t ON t.object_id = dc.parent_object_id
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
	INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
WHERE
	s.name = '{1}'
	AND t.name = '{2}'
	AND c.name = '{3}{4}'
	AND dc.name <> '{5}';
IF (@SqlText is not null) EXEC (@SqlText);";

			var renameConstraint = String.Join("\r\n", table
				.Where(c => !String.IsNullOrWhiteSpace(c.DefaultConstraintName))
				.Select(c => String.Format(CultureInfo.InvariantCulture, renameConstraintRaw,
					dbBeingUpgraded,        // 0
					tableSchema,            // 1
					table.Key,              // 2
					columnPrefix,           // 3
					c.ColumnName,           // 4
					c.DefaultConstraintName // 5
			)));

			var renameColumn = String.Join("\r\n", table
				.Select(c => String.Format(CultureInfo.InvariantCulture, "EXEC [{0}].sys.sp_rename '{1}.{2}.{3}{4}', '{4}', 'COLUMN';",
					dbBeingUpgraded, // 0
					tableSchema,     // 1
					table.Key,       // 2
					columnPrefix,    // 3
					c.ColumnName     // 4
			)));

			var sql = String.Format(CultureInfo.InvariantCulture, @"
DECLARE @SqlText nvarchar(max);
-- RENAME new constraints
{0}
-- RENAME new columns
{1}
",
				renameConstraint,
				renameColumn
				);

			try
			{
				using (var cmd = upgConnection.Command(sql))
				{
					cmd.ExecuteNonQuery();
				}
			}
			catch (SqlException ex)
			{
				throw new OdysseyDataException(String.Format(CultureInfo.InvariantCulture, "Error: {0}\r\nCommand: {1}", ex.Message, sql), ex);
			}
		}

		#endregion // Convert with populating

		void AddModifiedTable(string schemaName, string tableName)
			=> modifiedTables.Add(new SchemaTableNamePair(schemaName, tableName));

		public IEnumerable<SchemaTableNamePair> DistinctModifiedTablesIgnoringCase
			=> modifiedTables.Distinct(new SchemaTableNamePair.IgnoreCaseComparer());

		readonly List<SchemaTableNamePair> modifiedTables = new List<SchemaTableNamePair>();
	}
}

#region Test
#if DEBUG

#region Partial class

namespace Enterprise.DbUpgrader.Schema
{
	public partial class ColumnSynchroniser
	{
		public void ConvertCharToChar_Exposed()
		{
			ConvertCharToChar();
		}
	}
}

#endregion // Partial class

#endif
#endregion
