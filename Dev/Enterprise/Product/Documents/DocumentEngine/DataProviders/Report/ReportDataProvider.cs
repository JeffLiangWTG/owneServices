using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.DataProviders
{
	internal class ReportDataProvider : IDataProvider
	{
		public ReportDataProvider(Report report)
		{
			Report = report;
		}
		protected readonly Report Report;
		protected readonly DataSet DS = new DataSet(); // This is an SQL data provider for reports
		internal static readonly Regex adsRegex = new Regex(@"^(?<dataSource>[\w#]+):(?<originalQuery>.+?)(?:\s*:\s*(?<noWhereClause>NoWhereClause)\s*)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

		public void AddDataSource(string dataSourceString, bool isForDataSection = false)
		{
			AddDataSource(dataSourceString, isForDataSection, -1);
		}

		public void AddDataSource(string dataSourceString, bool isForDataSection, int maximumNumberOfRows)
		{
			bool needsToAddWhereClause = isForDataSection;  //Report.Analyser.ReportDataSources.IndexOf(DataSourceString) == 0 || 

			Match parameters = adsRegex.Match(dataSourceString);

			var dataSource = parameters.Groups["dataSource"].Value;
			var originalQuery = parameters.Groups["originalQuery"].Value;
			bool noWhereClause = parameters.Groups["noWhereClause"].Success;

			if (noWhereClause)
			{
				needsToAddWhereClause = false;
			}

			DataTable result;
			var cacheKey = $"{Report.WorkSheetCurrentlyBeingProcessed.SheetName}:{dataSource}:{Report.PK}";
			if (shouldCacheReportSqlResult.Value)
			{
				result = reportSqlResultCache.Value.GetOrAdd(cacheKey, () => GetNewDataTable(originalQuery, dataSource, needsToAddWhereClause, maximumNumberOfRows));
				result?.DataSet?.Tables.Remove(result);
			}
			else
			{
				result = GetNewDataTable(originalQuery, dataSource, needsToAddWhereClause, maximumNumberOfRows);
			}

			DS.Tables.Add(result);
		}

		DataTable GetNewDataTable(string originalQuery, string dataSource, bool needsToAddWhereClause, int maximumNumberOfRows)
		{
			var tableProvider = GetProvider(originalQuery);
			DataTable result;
			var tbl = tableProvider.GetDataTable(dataSource, originalQuery, Report, needsToAddWhereClause, maximumNumberOfRows);
			tbl.TableName = dataSource;
			if (tableProvider.HandlesSortInternally || Report.SortOrderCollection.SelectedOrder.FieldList == "NULL" || !needsToAddWhereClause)
			{
				result = tbl;
			}
			else
			{
				var rows = tbl.Select("", Report.SortOrderCollection.SelectedOrder.FieldList);
				result = tbl.Clone();
				foreach (DataRow row in rows)
				{
					result.ImportRow(row);
				}
			}

			return result;
		}

		void TryAddDataSource(string tableName, bool isForDataSection)
		{
			TryAddDataSource(tableName, isForDataSection, -1);
		}

		void TryAddDataSource(string tableName, bool isForDataSection, int maximumNumberOfRows)
		{
			foreach (var reportSQLSource in Report.Analyser.ReportSQLSources)
			{
				var tableNameAndSelectStatement = reportSQLSource.TableNameAndSelectStatement;
				if (tableNameAndSelectStatement.StartsWith(tableName + ":", StringComparison.OrdinalIgnoreCase))
				{
					var isEDWData = reportSQLSource.DataSourceType == DataSourceTypes.EdwData;
					using (Report.TrySwitchConnection(isEDWData))
					{
						AddDataSource(tableNameAndSelectStatement, isForDataSection, maximumNumberOfRows);
						break;
					}
				}
			}
		}

		void TryAddDataSource(string tableName)
		{
			TryAddDataSource(tableName, false);
		}

		public IDataRowSource GetDataRowSource(string tableIdentifier, bool isForDataSection)
		{
			return GetDataRowSource(tableIdentifier, isForDataSection, -1);
		}

		public IDataRowSource GetDataRowSource(string tableIdentifier, bool isForDataSection, int maximumNumberOfRows)
		{
			if (!DS.Tables.Contains(tableIdentifier))
			{
				try
				{
					TryAddDataSource(tableIdentifier, isForDataSection, maximumNumberOfRows);
				}
				catch (DuplicateNameException)
				{ }
			}

			if (!DS.Tables.Contains(tableIdentifier))
			{
				throw new DataProviderException("Table " + tableIdentifier + " Does not belong to data provider.");
			}

			var rows = DS.Tables[tableIdentifier].Rows;
			if (Enumerable.Range(0, rows.Count).Any(rowNumber => (rows[rowNumber].RowState & DataRowState.Deleted) == 0))
			{
				var wrappedRowIndexes =
					Enumerable.Range(0, rows.Count)
					.Where(rowNumber => (rows[rowNumber].RowState & DataRowState.Deleted) == 0)
					.ToArray();
				return new ReportDataSource(DS.Tables[tableIdentifier], wrappedRowIndexes);
			}
			else
			{
				return new ReportDataSource(DS.Tables[tableIdentifier]);
			}
		}

		public IDataRowSource GetDataRowSource(string tableIdentifier)
		{
			return GetDataRowSource(tableIdentifier, false);
		}

		protected TableProvider GetProvider(string dataSource)
		{
			if (dataSource.TrimStart().StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
			{
				return new NativeSqlTableProvider();
			}
			else
			{
				Match tableName = Regex.Match(dataSource, @"\s*([^(\s]*)");
				if (tableName.Success && tableProviderFactory.IsProviderAvailable(tableName.Groups[1].Value))
				{
					return tableProviderFactory.GetProvider(tableName.Groups[1].Value);
				}
				else
				{
					return new NativeSqlTableProvider();
				}
			}
		}

		TableProviderFactory tableProviderFactory
		{
			get { return _tableProviderFactory ?? (_tableProviderFactory = new TableProviderFactory()); }
		}
		TableProviderFactory _tableProviderFactory;

#if DEBUG
		internal TableProviderFactory tableProviderFactoryForTesting
		{
			get { return tableProviderFactory; }
		}
#endif

		public ICollection GetDataElements(string tableName)
		{
			if (DS.Tables.IndexOf(tableName) == -1)
			{
				throw new DataProviderException("Table " + tableName + " does not exist in data sources!");
			}
			return DS.Tables[tableName].Rows;
		}

		public int GetRowCount(string tableName)
		{
			if (DS.Tables.IndexOf(tableName) == -1)
			{
				return -1;
			}
			return DS.Tables[tableName].Rows.Count;
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public object GetColumnValue(IDataRowSource dataSource, int rowIndex, string columnName, bool onlyForCurrentSource = false, Area area = null)
		{
			var split = columnName.Split(".".ToCharArray(), 2);
			if (split.Length == 1)
			{
				Report.ErrorManager.Add(new ReportProcessingError(Res.GetString("555529e6-ee37-4d72-8412-fdacf507d007", "Column '{0}' is not part of any table.", columnName), ReportProcessingErrorSeverity.Warning));
				return string.Empty;
			}

			string tableName = split[0];
			columnName = split[1];
			if (DS.Tables.IndexOf(tableName) == -1)
			{
				TryAddDataSource(tableName);
			}

			if (DS.Tables.IndexOf(tableName) == -1)
			{
				Report.ErrorManager.Add(new ReportProcessingError(Res.GetString("de00767d-581b-4aa4-a4ec-4f4a0ff4bbd3", "Table '{0}' is not found in data set.", tableName), ReportProcessingErrorSeverity.Warning));
				return string.Empty;
			}
			if (DS.Tables[tableName].Columns.IndexOf(columnName) == -1)
			{
				var columnNames = string.Join(", ", DS.Tables[tableName].Columns.Cast<DataColumn>().Select(x => x.ColumnName));
				Report.ErrorManager.Add(new ReportProcessingError(Res.GetString("5834d825-1404-47d0-9bc9-9aa0ebeb4fec", "Column '{0}' is not found in Table '{1}'. Columns: {2}", columnName, tableName, columnNames), ReportProcessingErrorSeverity.Warning));
				return string.Empty;
			}
			if (rowIndex < 0 || rowIndex >= DS.Tables[tableName].Rows.Count)
			{
				return string.Empty;
			}

			if (dataSource != null && !(dataSource is ReportDataSource))
			{
				Report.ErrorManager.Add(new ReportProcessingError(Res.GetString("1acf7e23-16e5-493e-aa0c-0e83837fa935", "The Data Source used in this cell is invalid. Please specify a valid Data Source in the Data option of your Section Body."), ReportProcessingErrorSeverity.ErrorWithoutErrorReport));
				return string.Empty;
			}

			DataRow currentRow = null;
			if (dataSource != null && dataSource.RowCount > 0 && rowIndex < dataSource.RowCount && ((ReportDataSource)dataSource).TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
			{
				currentRow = ((ReportDataSource)dataSource).RowByIndex(rowIndex);
			}

			if (currentRow == null)
			{
				if (DS.Tables[tableName].Rows.Count == 0)
				{
					return "";
				}
				currentRow = DS.Tables[tableName].Rows[0];
			}

			if (currentRow.Table.Columns.IndexOf(columnName) == -1)
			{
				return GetColumnValue(null, 0, tableName + "." + columnName);
			}

			return FormatColumnValue(currentRow[columnName]);
		}

		public int TableCount
		{
			get { return DS.Tables.Count; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected object FormatColumnValue(object columnValue)
		{
			object result = columnValue;
			if (columnValue is byte[] && Compressor.IsCompressed((byte[])columnValue))
			{
				byte[] decompressed = Compressor.Uncompress((byte[])columnValue);
				result = decompressed;
			}
			return result;
		}

		public static IDisposable EnableReportSqlResultCache(bool isReport)
		{
			if (!isReport || shouldCacheReportSqlResult.Value)
			{
				return null;
			}

			shouldCacheReportSqlResult.Value = true;

			Action disposeAction = () =>
			{
				shouldCacheReportSqlResult.Value = false;
				reportSqlResultCache.Value.Clear();
			};

			return new DisposableAction(disposeAction);
		}

		[ThreadSafe]
		static readonly ThreadLocal<bool> shouldCacheReportSqlResult = new ThreadLocal<bool>();

		[ThreadSafe]
		static readonly ThreadLocal<Dictionary<string, DataTable>> reportSqlResultCache = new ThreadLocal<Dictionary<string, DataTable>>(() => new Dictionary<string, DataTable>());
	}
}
