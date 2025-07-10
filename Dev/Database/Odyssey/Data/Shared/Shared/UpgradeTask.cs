using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Types;

namespace Enterprise.DbUpgrader.Data
{
	public interface IUpgradeTask
	{
		void Run();
		bool IsRequired { get; }
		string TaskNameWhenUpgrading { get; }
	}

	/// <summary>
	/// A single task in the Data Upgrade process
	/// </summary>
	public abstract class UpgradeTask : IUpgradeTask
	{
		public UpgradeTask(DataFile resourceDataFile)
		{
			ResourceFile = resourceDataFile;
		}

		public abstract bool IsRequired { get; }

		public virtual bool ExternalVersionBump { get { return false; } }

		public readonly DataFile ResourceFile;

		public string TableNames
		{
			get
			{
				var builder = new StringBuilder();
				foreach (var tableName in ResourceFile.TableNames)
				{
					_ = builder.Append(tableName);
					_ = builder.Append(", ");
				}
				return builder.ToString(0, builder.Length - 2);
			}
		}

		public virtual string TaskNameWhenUpgrading
		{
			get
			{
				var builder = new StringBuilder();
				_ = builder.Append("Updating ");
				_ = builder.Append(TableNames);
				return builder.ToString();
			}
		}

		protected virtual ApplicationException GetNewException(Exception e)
		{
			return new ApplicationException("Error encountered while upgrading data for '" + ResourceFile.FileResourceName + "'" + System.Environment.NewLine + e.Message, e);
		}

		protected abstract void UpdateVersionNumber();

		public void Run()
		{
			var sourceData = ResourceFile.DataSet;
			Run(sourceData);
		}

		public void Run(DataSet sourceData)
		{
			var currentCulture = CultureInfo.CurrentCulture;
			var dsCulture = sourceData.Locale;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				sourceData.Locale = CultureInfo.InvariantCulture;
				RunStartUtc = DateTime.UtcNow;

				if (Db.Connection.IsInTransaction)
				{
					RunCore(sourceData);
				}
				else
				{
					RunInTransaction(sourceData);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ApplicationException excep = GetWrappedExceptionSafe(e);
				throw excep;
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
				sourceData.Locale = dsCulture;
				RunStartUtc = null;
			}
		}

		DateTime? RunStartUtc;

		void RunInTransaction(DataSet sourceData)
		{
			try
			{
				Db.Connection.ExecuteNonQuery("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE");
				using (var manager = Db.Connection.BeginTransactionWithManager())
				{
					RunCore(sourceData);
					manager.CommitTransaction();
				}
			}
			finally
			{
				Db.Connection.ExecuteNonQuery("SET TRANSACTION ISOLATION LEVEL READ COMMITTED");
			}
		}

		void RunCore(DataSet sourceData)
		{
			// Loading existing data from databases
			var targetData = ResourceFile.LoadDataFromDatabase();
			// Removing rowversion and autoversion columns
			RemoveAllRowversionAndAutoVersionColumns(targetData);
			// Synchronising data
			var dataSetToUpdate = GenerateDataSetToUpdate(sourceData, targetData);
			// Saving changes to the databases
			ResourceFile.SaveDataToDatabase(dataSetToUpdate);
			// Updating version
			UpdateVersionNumber();
		}

		ApplicationException GetWrappedExceptionSafe(Exception originalEx)
		{
			ApplicationException wrappingExcep;

			try
			{
				wrappingExcep = GetNewException(originalEx);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				wrappingExcep = new ApplicationException(e.Message, originalEx);
			}

			return wrappingExcep;
		}

		#region Run Task for Setup

		public bool IsRunningForSetup;

		#region Testing
#if DEBUG

		public void RunForSetupFromFile()
		{
			var dataSetFromFile = ResourceFile.LoadDataFromFile();
			SaveXmlDataToDatabaseForSetup(dataSetFromFile);
		}

		public void RunForSetup()
		{
			SaveXmlDataToDatabaseForSetup(ResourceFile.DataSet);
		}

		void SaveXmlDataToDatabaseForSetup(DataSet dataSet)
		{
			try
			{
				IsRunningForSetup = true;
				Db.Connection.BeginTransaction();

				if (ResourceFile.MustCleanDataBeforeSetup)
				{
					ClearExistingDataAndSaveXmlDataToDatabase(dataSet);
				}
				else
				{
					Run(dataSet);
				}

				Db.Connection.CommitTransaction();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Db.Connection.RollbackTransaction();
				throw;
			}
			finally
			{
				IsRunningForSetup = false;
			}
		}

		void ClearExistingDataAndSaveXmlDataToDatabase(DataSet dataSet)
		{
			DisableDependingFksAndClearSetupTables();
			Run(dataSet);
			ReenableDependingFksToSetupTables();
		}

		void DisableDependingFksAndClearSetupTables()
		{
			foreach (string tableName in ResourceFile.TableNames)
			{
				var sqlText = String.Format(@"
					DECLARE @SqlCommand NVARCHAR(max); SET @SqlCommand = '';
					SELECT @SqlCommand = @SqlCommand + 'ALTER TABLE ' + FkTable + ' NOCHECK CONSTRAINT ' + FkName + ';'
						FROM dbo.vw_FkReferences
						WHERE PkTable = '{0}';
					EXEC sp_executesql @SqlCommand;
					DELETE {0};",
					tableName);
				Db.Connection.ExecuteNonQuery(sqlText);
			}
		}

		void ReenableDependingFksToSetupTables()
		{
			foreach (string tableName in ResourceFile.TableNames)
			{
				var sqlText = String.Format(@"
					DECLARE @SqlCommand NVARCHAR(max); SET @SqlCommand = '';
					SELECT @SqlCommand = @SqlCommand + 'ALTER TABLE ' + FkTable + ' WITH CHECK CHECK CONSTRAINT ' + FkName + ';'
						FROM dbo.vw_FkReferences
						WHERE PkTable = '{0}';
					EXEC sp_executesql @SqlCommand;",
					tableName);
				Db.Connection.ExecuteNonQuery(sqlText);
			}
		}

#endif
		#endregion

		#endregion

		#region Implementation

		// Composite key requires padding as order of record is essential in lef walk left right algorithm
		// Say you have record with fields AA BB CC
		// and another set				   A  A  BBCC
		// composite keys without padding for these records will be the same "AABBCC"
		// therefore we need to go with padding till column max length
		// then keys will be different
		// Assume "_" is padding char and max lenght of columns are 2,2,4 respectively
		// then keys will be different:
		// "AABBCC__"
		// "A_A_BBCC"

		string GetCompositeKey(DataRow row, string comparingColumns, int[] maxLength)
		{
			if (row == null)
			{
				return null;
			}

			var result = string.Empty;
			var columns = comparingColumns.Split(',');

			for (var i = 0; i < columns.Length; i++)
			{
				result += row[columns[i]].ToString().ToUpper(CultureInfo.InvariantCulture).Trim().PadRight(maxLength[i] > 0 ? maxLength[i] : 0);
			}

			return result;
		}

		int[] GetColumnsMaxLength(string comparingColumns, DataTable targetTable)
		{
			var columns = comparingColumns.Split(',');
			var maxLengths = new int[columns.Length];
			var schemaResolver = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>();

			for (var i = 0; i < columns.Length; i++)
			{
				ISchemaColumn schemaColumn = schemaResolver.GetSchemaColumnSafe(columns[i], targetTable.TableName);
				maxLengths[i] = schemaColumn != null ? schemaColumn.MaxLength : 0;
			}

			return maxLengths;
		}

		void RemoveAllRowversionAndAutoVersionColumns(DataSet dataSet)
		{
			// remove the excluded columns
			// currently they are defined in build.xml
			// after finishing WI00301342 - Create a systematic way to change schema to add Auto/Row version columns
			// we should use the infomation provided on schema level, like ITableSchema
			var pattern = @"^.+_(auto|row)version$";
			foreach (DataTable table in dataSet.Tables)
			{
				foreach (var column in table.Columns.Cast<DataColumn>().Where(c => Regex.IsMatch(c.ColumnName, pattern, RegexOptions.IgnoreCase)).ToArray())
				{
					table.Columns.Remove(column);
				}
			}
		}

		DataSet GenerateDataSetToUpdate(DataSet sourceData, DataSet targetData)
		{
			for (int i = 0; i < ResourceFile.TableNames.Length; i++)
			{
				var sourceTableName = ResourceFile.TableNames[i];
				var sourceTable = sourceData.Tables[sourceTableName];
				var targetTable = targetData.Tables[sourceTableName];

				if (sourceTable != null && targetTable != null)
				{
					var comparingColumns = ComparingColumnsNames(sourceTable);
					var sortString = comparingColumns + " ASC";

					var sourceTableView = new DataView(sourceTable);
					sourceTableView.Sort = sortString;

					var targetTableView = new DataView(targetTable);
					targetTableView.Sort = sortString;

					var maxLengths = GetColumnsMaxLength(comparingColumns, targetTable);

					// Stepping through the two sorted tables using the Walk-Left-Walk-Right algorithm.
					for (int sourceIndex = 0, targetIndex = 0; sourceIndex < sourceTableView.Count || targetIndex < targetTableView.Count;)
					{
						var sourceRow = sourceIndex < sourceTableView.Count ? sourceTableView[sourceIndex].Row : null;
						var targetRow = targetIndex < targetTableView.Count ? targetTableView[targetIndex].Row : null;
						var sourceKey = GetCompositeKey(sourceRow, comparingColumns, maxLengths);
						var targetKey = GetCompositeKey(targetRow, comparingColumns, maxLengths);

						var sourceToTargetCompareResult = String.Compare(sourceKey, targetKey, false, CultureInfo.InvariantCulture);

						if (sourceRow != null && targetRow != null && sourceToTargetCompareResult == 0)
						{
							Update(sourceRow, targetRow);
							sourceIndex++;
							targetIndex++;
						}
						else if (targetRow != null && (sourceRow == null || sourceToTargetCompareResult > 0))
						{
							Delete(targetRow, ref targetIndex);
						}
						else if (sourceRow != null && (targetRow == null || sourceToTargetCompareResult < 0))
						{
							Insert(sourceRow, targetTable, ref targetIndex);
							sourceIndex++;
						}
					}
				}
			}

			return targetData;
		}

		protected virtual string ComparingColumnsNames(DataTable upgradeTable)
		{
			return upgradeTable.Columns[0].ColumnName;
		}

		#region Delete

		/// <summary>
		/// If is running task for Setup purposes, do the standard DELETE (ie. remove row from target DataSet).
		/// Otherwise call the virtual (overridable) DoDelete method.
		/// </summary>
		/// <param name="targetRow">Row to have the "delete" action taken on</param>
		/// <param name="targetIndex">
		/// Reference to the Target DataTable index. 
		/// If the row is not actually deleted it might be necessary to increment it.
		/// </param>
		protected void Delete(DataRow targetRow, ref int targetIndex)
		{
			if (IsRunningForSetup)
			{
				DoDeleteForSetup(targetRow, ref targetIndex);
			}
			else
			{
				DoDelete(targetRow, ref targetIndex);
			}
		}

		protected virtual void DoDeleteForSetup(DataRow targetRow, ref int targetIndex)
		{
			DoDeleteCore(targetRow);
		}

		/// <summary>
		/// Takes the required action for a target row not found in the source data.
		/// The default action is to delete the row, but it may be overriden.
		/// </summary>
		/// <param name="targetRow">Row to have the action taken on</param>
		/// <param name="targetIndex">
		/// Reference to the Target DataTable index. 
		/// In some cases it might be necessary to increment it.
		/// (eg: when TargetRow is not deleted to force a skip to the next row).
		/// </param>
		protected virtual void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			DoDeleteCore(targetRow);
		}

		protected void DoDeleteCore(DataRow targetRow)
		{
			targetRow.Delete();
		}

		#endregion

		#region Update

		void Update(DataRow sourceRow, DataRow targetRow)
		{
			var auditColumns = GetAuditColumns();
			var auditDataColumns = new List<DataColumn>(2);

			foreach (DataColumn column in targetRow.Table.Columns)
			{
				if (sourceRow.Table.Columns.Contains(column.ColumnName))
				{
					var sourceValue = sourceRow[column.ColumnName];
					var targetValue = targetRow[column.ColumnName];
					var isColumnEqual = sourceValue is IStructuralEquatable equatable ? equatable.Equals(targetValue, StructuralComparisons.StructuralEqualityComparer) : sourceValue.Equals(targetValue);
					if (!isColumnEqual)
					{
						UpdateColumn(column.ColumnName, targetRow, sourceRow);
					}
				}
				else if (auditColumns.Where(c => !c.IsInsertOnly).Any(c => column.ColumnName.EndsWith(c.Name, StringComparison.InvariantCulture)))
				{
					auditDataColumns.Add(column);
				}
			}

			if (targetRow.RowState == DataRowState.Modified)
			{
				foreach (var column in auditDataColumns)
				{
					SetAuditField(targetRow, column);
				}
			}
		}

		protected virtual void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			targetRow[columnName] = sourceRow[columnName];
		}

		#endregion

		#region Insert

		/// <summary>
		/// Takes the required action for a source row not found in the target data.
		/// The default action is to insert the source row into the target data, but it may be overriden.
		/// </summary>
		/// <param name="sourceRow">Row to be copied(inserted) to target data</param>
		/// <param name="targetTable">Target DataTable</param>
		/// <param name="targetIndex">
		/// Reference to the Target DataTable index. 
		/// Depending on the action taken it should be incremented or not.
		/// For instance, if no row is inserted on the TargetTable, it should not be incremented.
		/// </param>
		void Insert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			sourceRow.Table.Columns.Cast<DataColumn>().Where(c => c.DataType == typeof(SqlGeography)).ToList().ForEach(c =>
			{
				var value = sourceRow[c.ColumnName];
				if (value == null || value == DBNull.Value || ((SqlGeography)value).STGeometryType() == new SqlString("GeometryCollection"))
				{
					const int InvalidCoordinateSystem = 4995; // See also: ZGeography
					sourceRow[c.ColumnName] = SqlGeography.STGeomFromText(new SqlChars(new SqlString("POINT EMPTY")), InvalidCoordinateSystem);
				}
			});

			if (IsRunningForSetup)
			{
				InsertTargetRow(sourceRow, targetTable, ref targetIndex);
			}
			else
			{
				DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		/// <summary>
		/// Takes the required action for a source row not found in the target data.
		/// The default action is to insert the source row into the target data, but it may be overriden.
		/// </summary>
		/// <param name="sourceRow">Row to be copied(inserted) to target data</param>
		/// <param name="targetTable">Target DataTable</param>
		/// <param name="targetIndex">
		/// Reference to the Target DataTable index. 
		/// Depending on the action taken it should be incremented or not.
		/// For instance, if no row is inserted on the TargetTable, it should not be incremented.
		/// </param>
		protected virtual void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			InsertTargetRow(sourceRow, targetTable, ref targetIndex);
		}

		void InsertTargetRow(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			var auditColumns = GetAuditColumns();
			var rowToAdd = targetTable.NewRow();
			foreach (DataColumn column in targetTable.Columns)
			{
				if (sourceRow.Table.Columns.Contains(column.ColumnName))
				{
					rowToAdd[column] = sourceRow[column.ColumnName];
				}
				else if (auditColumns.Any(c => column.ColumnName.EndsWith(c.Name, StringComparison.InvariantCulture)))
				{
					SetAuditField(rowToAdd, column);
				}
			}
			targetTable.Rows.Add(rowToAdd);

			targetIndex++;
		}

		void SetAuditField(DataRow row, DataColumn column)
		{
			if (column.DataType == typeof(DateTime))
			{
				row[column] = RunStartUtc;
			}
			else
			{
				row[column] = ServiceUserCode;
			}
		}

		// Not reusing constants as this project does not reference ZArchitecture.Core
		const string ServiceUserCode = "~BP";

		static IEnumerable<(string Name, bool IsInsertOnly)> GetAuditColumns() => new[]
		{
			("_SystemCreateTimeUtc", true),
			("_SystemCreateUser", true),
			("_SystemLastEditTimeUtc", false),
			("_SystemLastEditUser", false),
		};

		#endregion

		#endregion
	}
}
