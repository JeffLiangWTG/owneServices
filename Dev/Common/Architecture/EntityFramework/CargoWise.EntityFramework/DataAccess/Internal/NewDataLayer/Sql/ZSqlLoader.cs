using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.DataAccess.Internal.NewDataLayer.Sql;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Microsoft.SqlServer.Types;

namespace CargoWise.EntityFramework
{
	public class ZSqlLoader : ZLoader
	{
		static readonly Regex expensiveCusClassPartPivotQueryRegex = new Regex(@"^(.*WHERE\s+)?CI_RN_NKCountry\s*=\s*'\w+'\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		[SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public ZSqlLoader(DataSet data, ZSqlConnectionInfo connectionInfo, IApplicationSchemaResolver schemaResolver)
			: base(data)
		{
			ConnectionInfo = Argument.NotNull(connectionInfo, nameof(connectionInfo));
			SchemaResolver = Argument.NotNull(schemaResolver, nameof(schemaResolver));
		}

		public override int GetCount(ZCountDataQuery query)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}
			DbCommand cmd = ConnectionInfo.GetNewDbCommandForSelect(query.ParameterisedQueryText, query.Parameters);
			return (int)cmd.ExecuteScalar();
		}

		internal bool DoesTableExist(string tableName)
		{
			// if tableNameAndOrPath contains two periods it is a full database name path and takes precedence over database in the connection
			// otherwise we just use the connection (and its currently selected database)
			// this is likely an invalid test condition, but being supported to make sys.object keep working
			// if you refactor for real support of schemas, it can probably be simplified
			// this does not work with synonyms
			var split = tableName.Split('.');
			if (split.Length > 1)
			{
				throw new NotSupportedException("2, or 3 or 4 part names are not supported - " + tableName + " includes at least schema name");
			}

			var tableExistsCmdText = "SELECT COUNT(*) FROM SYS.OBJECTS WHERE TYPE IN ('U', 'V') AND NAME= @TableName";
			var tableNameParam = ZSqlParameter.New("@TableName", tableName, new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, (NoResString)"name", 0, SqlDbType.NVarChar, string.Empty, false, 128));
			var cmd = ConnectionInfo.GetNewDbCommandForSelect(tableExistsCmdText, tableNameParam);
			return ((int)cmd.ExecuteScalar() == 1);
		}

		public const int MaximumCommandTextLengthInCharacters = 39000;

		public override LoaderResponse LoadPersistentRowsIntoDataSet(IList<ZDataQuery> queries)
		{
			var responses = new List<DataRowLoadResponse>();
			var dbHits = 0;

			foreach (var query in queries)
			{
				ReportIfInappropriateLoad(query);

				void AddResponse(ZDataQuery q)
				{
					var cmd = q.isStoredProc
						? ConnectionInfo.GetNewDbCommandForStoredProcedure(q.ParameterisedQueryText, q.Parameters)
						: q.Timeout > 0 ? ConnectionInfo.GetNewDbCommandForSelect(q.ParameterisedQueryText, q.Timeout, q.Parameters)
											: ConnectionInfo.GetNewDbCommandForSelect(q.ParameterisedQueryText, q.Parameters);

					responses.AddRange(FillFromDbReportingSlowQueries(cmd, new[] { new QueryTable(q, GetTable(q.TableName)) }));
					dbHits++;
				}

				try
				{
					AddResponse(query);
				}
				catch (SqlException ex) when (IsQueryPlanHintException(ex))
				{
					query.Query.ClearTableIndexHintsIncludingSubQueries();
					query.Query.DisableForceSeekIncludingSubQueries();
					AddResponse(new ZDataQuery(ConnectionInfo, query.TableName, query.Query));
				}
			}

			return new LoaderResponse(dbHits, responses.ToArray());
		}

		bool IsQueryPlanHintException(SqlException ex)
		{
			var errorType = new DbErrorMatch(ex).ExceptionType;
			return errorType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseOfHints || errorType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseMinimumWorktableWasTooLong;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Refactoring is needed in future")]
		void ReportIfInappropriateLoad(ZDataQuery query)
		{
			var queryLiteralText = query.LiteralTextSql;
			MissingFetchHintDetector.Instance.AddQueryToRecords(queryLiteralText, ConnectionInfo.DbConnection);

			if (query.TableName == "StmALog" || query.TableName == "ViewGenericJob" || query.TableName == "ViewProcessHeader" || query.TableName == "EDIMessage")
			{
				if (query.TableName == "StmALog"
					&& (query.Parameters.Length > 0)
					&& query.Parameters.All(p => p.SchemaColumn.Name == "SL_PK"))
				{
					throw new InvalidOperationException("StmALog may not be queried by PK as it has no index.\r\nAttempted query : " + queryLiteralText);
				}
#if DEBUG
				// note: #if DEBUG is used because of search in EDIMessages module, other collections generally do not filter by status and do not have ORDER BY
				if (query.TableName == "EDIMessage")
				{
					if
					(
						queryLiteralText.Contains("EM_Status = 'QUE'") ||
						queryLiteralText.Contains("EM_Status = 'PND'") ||
						queryLiteralText.Contains("EM_Status = 'PPS'") ||
						queryLiteralText.Contains("EM_Status IN")
					)
					{
						if (queryLiteralText.IndexOf("INDEX(", StringComparison.OrdinalIgnoreCase) == -1)
						{
							ErrorReporter.ReportOnce($@"
EDIMessage has filtered indexes to support optimized querying.
Please nominate the appropriate index to use by hinting on ZQuery:
{queryLiteralText}
							".Trim());
						}
					}
				}
#endif
				if (queryLiteralText.Contains("VJ_PK in") || queryLiteralText.Contains("VJ_PK = "))
				{
					if (!queryLiteralText.Contains("VJ_TableName"))
					{
						ErrorReporter.ReportOnce("ZSqlLoaderViewGenericJobByPK", "ViewGenericJob has to be queried by its PK and ParentTableCode. You should use the extension method for BusinessObjectFactory in MasterFiles. i.e) Factory.LoadGenericJobFromPKAndTableCode(Job) or Factory.LoadGenericJobFromPKAndTableCode(pk, tableCode)");
					}
				}

				if (queryLiteralText.Contains((NoResString)"VFH_PK in") || queryLiteralText.Contains("VFH_PK = "))
				{
					if (!queryLiteralText.Contains((NoResString)"VFH_ParentTableCode in") && !queryLiteralText.Contains("VFH_ParentTableCode ="))
					{
						ErrorReporter.ReportOnce("ZSqlLoaderViewProcessHeaderByPK", "ViewProcessHeader has to be queried by its PK and ParentTableCode. You should use the extension method for BusinessObjectFactory in BufferManagement.Business. i.e) Factory.LoadFromPrimaryKeysAndTableCode(pks, tableCode)");
					}
				}

				if (query.TableName == "ViewGenericJob" && query.Parameters.Length == 1 && query.Parameters[0].SchemaColumn.Name == "VJ_PK")
				{
					if (!queryLiteralText.Contains("VJ_TableName in") && !queryLiteralText.Contains("VJ_TableName ="))
					{
						ErrorReporter.ReportOnce("ZSqlLoaderViewGenericJobByPK", "ViewGenericJob has to be queried by its PK and ParentTableCode. You should use the extension method for BusinessObjectFactory in MasterFiles. i.e) Factory.LoadGenericJobFromPKAndTableCode(Job) or Factory.LoadGenericJobFromPKAndTableCode(pk, tableCode)");
					}
				}
			}

			if (query.TableName == "CusClassPartPivot")
			{
				// note: (WI00339138) this is a temporary code that will be removed when we find the source of the query
				if (expensiveCusClassPartPivotQueryRegex.IsMatch(queryLiteralText))
				{
					ErrorReporter.ReportOnce
					(
						"CusClassPartPivotLoadByCountryOnly",
						"Query loads excessive number of records from dbo.CusClassPartPivot table."
					);
				}
			}

			if (query.TableName == "MailDBItems")
			{
				if (queryLiteralText.Contains("MI_Application = 'STD'") && queryLiteralText.Contains("MI_Direction = 'RCV'") && queryLiteralText.Contains("MI_Status = 'QUE'"))
				{
					ErrorReporter.ReportOnce("ZSqlLoaderMailDbItems", "Mails shoud be categorised according to the code that will process it. That code should be saved in MI_Application field. Please ignore if it comes from FilterStrip.");
				}
			}
		}

		struct QueryTable
		{
			public QueryTable(ZDataQuery query, ZDataTable table)
			{
				Query = query;
				Table = table;
			}

			public readonly ZDataQuery Query;
			public readonly ZDataTable Table;
		}

		List<DataRowLoadResponse> FillFromDbReportingSlowQueries(DbCommand cmd, IList<QueryTable> queryTables)
		{
			List<DataRowLoadResponse> result = new List<DataRowLoadResponse>();

			bool tryAgainDueToTransientSqlError;
			do
			{
				tryAgainDueToTransientSqlError = false;
				try
				{
					result.Clear();
					using (var dataReader = cmd.ExecuteReader())
					{
						int i = 0;
						do
						{
							ZDataTable table = queryTables[i].Table;
							ZDataQuery query = queryTables[i].Query;
							result.Add(FillDataTableFromDB(table, dataReader, query.Query));
							i++;
						}
						while (dataReader.NextResult());
					}
				}
				catch (SqlException ex)
				{
					if (IsTransientSqlFailure(ex.Number))
					{
						tryAgainDueToTransientSqlError = true;
					}
					else
					{
						throw;
					}
				}
			} while (tryAgainDueToTransientSqlError);

			return result;
		}

		bool IsTransientSqlFailure(int errorCode)
		{
			return errorCode == 601 // Could not continue with scan
				|| errorCode == 1205;   // Transaction was deadlocked on lock | communication buffer resources with another process and has been chosen as the deadlock victim. Rerun the transaction;
		}

		public override NonPersistentLoaderResponse LoadNonPersistentRows(ZNonPersistentDataQuery filter)
		{
			using (var dataReader = ExecuteQueryWithTimeout(filter))
			{
				var table = BuildTableFromReader(dataReader);
				CopyRowsFromReaderToDataTable(table, dataReader);
				var result = new DataRow[table.Rows.Count];
				table.Rows.CopyTo(result, 0);
				return new NonPersistentLoaderResponse { DataTable = table, Rows = result };
			}
		}

		IDataReader ExecuteQueryWithTimeout(ZNonPersistentDataQuery query)
		{
			var cmd = ConnectionInfo.GetNewDbCommandForSelect(query.ParameterisedQueryText, query.Parameters);
			if (!query.CmdTimeout.HasValue)
			{
				return cmd.ExecuteReader();
			}

			cmd.CommandTimeout = query.CmdTimeout.Value;
			var stopWatch = Stopwatch.StartNew();
			return new DataReaderTimeoutWrapper(cmd.ExecuteReader(), cmd.CommandTimeout, stopWatch);
		}

		#region Loading Blob Fields

		public override void LoadBlobFieldsForTable(string tableName, ZDataRowDictionary rows, IEnumerable<SchemaColumn> columns)
		{
			if (rows.Count > 0)
			{
				ZString sqlQuery = GetQueryForBlobLoading(out var paramsAction, tableName, rows, columns);

				var cmd = Db.Connection.Command(sqlQuery);
				paramsAction(cmd);
				using (var dataReader = cmd.ExecuteReader())
				{
					while (dataReader.Read())
					{
						Guid pK = dataReader.GetGuid(0);
						DataRow rowToUpdate = rows[pK];

						for (int i = 1; i < dataReader.FieldCount; i++)
						{
							bool rowHadChanges = (rowToUpdate.RowState != DataRowState.Unchanged);

							string blobColumnName = dataReader.GetName(i);
							rowToUpdate[blobColumnName] = ZCompressor.GetUncompressedVersion(dataReader[i], blobColumnName);

							if (!rowHadChanges)
							{
								rowToUpdate.AcceptChanges();
							}
						}
					}
				}
			}
		}

		ZString GetQueryForBlobLoading(out Action<DbCommand> paramsAction, string tableName, ZDataRowDictionary rows, IEnumerable<SchemaColumn> columns)
		{
			paramsAction = null;
			var pKColumn = SchemaResolver.GetPkColumn(tableName);
			ZString blobColumns = pKColumn.Name;
			ZString rowPKs = "";

			foreach (SchemaColumn schemaColumn in columns)
			{
				blobColumns += ", " + schemaColumn.Name;
			}

			var count = 0;
			foreach (DataRow row in rows.Values)
			{
				var paramName = $"@PK{count}";
				rowPKs += $"{paramName}, ";
				paramsAction += cmd => cmd.AddParameterBasedOnDbColumn(paramName, row[0], pKColumn);
				count++;
			}

			return ZString.Format("select {0} from {1} where {2} in ({3})", blobColumns, tableName, pKColumn.Name, rowPKs.TrimEndIncludingWhiteSpace(','));
		}

		public override void LoadBlobField(DataRow row, SchemaColumn schemaColumn)
		{
			Argument.NotNull(row, "row");
			Argument.NotNull(schemaColumn, "column");
			Argument.NotNull(row.Table, "row.Table");
			Argument.NotNullOrEmpty(row.Table.TableName, "row.Table.TableName");

			bool rebuildRow = false;
			Hashtable currentChanges = null;
			if (row.HasVersion(DataRowVersion.Original))
			{
				rebuildRow = true;
				currentChanges = StoreCurrentChanges(row);
				row.RejectChanges();
			}

			if (schemaColumn.IsBinary)
			{
				try
				{
					byte[] buff = new byte[0x8000];
					using (var stream = GetBinaryFieldStream(row, schemaColumn.TableName, schemaColumn.Name))
					using (var ms = new MemoryStream())
					{
						int read;
						while ((read = stream.Read(buff, 0, buff.Length)) != 0)
						{
							ms.Write(buff, 0, read);
						}
						row[schemaColumn.Name] = ms.ToArray();
					}
				}
				catch (SqlNullValueException)
				{
					//row concurrently updated to null in the underlying database. No value to lazy load anymore
					row[schemaColumn.Name] = schemaColumn.IsNullable ? null : Array.Empty<byte>();
				}
			}
			else
			{
				try
				{
					using (var reader = GetTextFieldReader(row, schemaColumn.TableName, schemaColumn.Name, false))
					{
						row[schemaColumn.Name] = reader.ReadToEnd();
					}
				}
				catch (SqlNullValueException)
				{
					//row concurrently updated to null in the underlying database. No value to lazy load anymore
					row[schemaColumn.Name] = schemaColumn.IsNullable ? null : string.Empty;
				}
			}

			row.AcceptChanges();
			if (rebuildRow)
			{
				RestoreRowChanges(row, currentChanges);
			}
		}

		const string RowStateDeleted = "ROW_STATE_DELETED";
		internal static Hashtable StoreCurrentChanges(DataRow row)
		{
			Hashtable result = new Hashtable();
			if (row.RowState == DataRowState.Deleted)
			{
				result[RowStateDeleted] = true;
			}
			else
			{
				foreach (DataColumn col in row.Table.Columns)
				{
					try
					{
						object currentValue = row[col, DataRowVersion.Original];
						object proposedValue = row[col];
						if (!currentValue.Equals(proposedValue))
						{
							result[col.ColumnName] = proposedValue;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
			}

			return result;
		}

		internal static void RestoreRowChanges(DataRow row, Hashtable changes)
		{
			foreach (DictionaryEntry entry in changes)
			{
				if (entry.Key.ToString() == RowStateDeleted)
				{
					row.Delete();
				}
				else
				{
					row[entry.Key.ToString()] = entry.Value;
				}
			}
		}

		public override Stream GetStream(DataRow row, string tableName, string columnName)
		{
			var pkColumnName = SchemaResolver.GetPkColumn(tableName).Name;
			var columnType = SchemaResolver.GetSchemaColumn(columnName, tableName).SqlDbType;
			return SqlBinaryFieldStream.OpenReader(ConnectionInfo.DbConnection, ConnectionInfo.PathToTables + tableName, pkColumnName, (Guid)row[pkColumnName], columnName, columnType);
		}

		[SuppressMessage("Microsoft.Design", "CA1031", Justification = "Disposal of factory method should not hide original error")]
		public override Stream GetBinaryFieldStream(DataRow row, string tableName, string columnName)
		{
			Stream stream;
			IStreamSource streamSource;
			if (row.RowState != DataRowState.Added && LazyLoading.LoadRequired(row[columnName]))
			{
				streamSource = ((ZDataRow)row).GetStreamSource(columnName);
				if (streamSource != null)
				{
					stream = streamSource.GetStream();
				}
				else
				{
					Stream originalStream = GetStream(row, tableName, columnName);
					try
					{
						stream = ZCompressor.GetUncompressedVersion(originalStream, columnName);
					}
					catch
					{
						try
						{
							originalStream.Dispose();
						}
						catch { }
						throw;
					}
				}
			}
			else
			{
				stream = new MemoryStream(row[columnName] == DBNull.Value ? Array.Empty<byte>() : (byte[])row[columnName]);
			}
			return stream;
		}

		public override TextReader GetTextFieldReader(DataRow row, string tableName, string columnName, bool closeReaderBetweenReads)
		{
			TextReader reader;
			ITextReaderSource readerSource;

			if (row == null || row.RowState == DataRowState.Deleted)
			{
				reader = new StringReader(string.Empty);
			}
			else if (LazyLoading.LoadRequired(row[columnName]))
			{
				readerSource = ((ZDataRow)row).GetReaderSource(columnName);
				if (readerSource != null)
				{
					reader = readerSource.GetReader();
				}
				else
				{
					var pkColumnName = SchemaResolver.GetPkColumn(tableName).Name;
					var schemaColumn = SchemaResolver.GetSchemaColumn(columnName, tableName);
					if (closeReaderBetweenReads)
					{
						reader = new BufferedTextReader(new ZTextReader(ConnectionInfo, (SchemaStringColumn)schemaColumn, new ZGuid(row[pkColumnName])), 80400);
					}
					else
					{
						var prefix = ConnectionInfo.PathToTables;
						if (string.IsNullOrEmpty(prefix))
						{
							prefix = $"[{schemaColumn.TableSchema.SqlSchemaName}].";
						}

						reader = new SqlTextFieldReader(ConnectionInfo.DbConnection, prefix + tableName, pkColumnName, (Guid)row[pkColumnName], columnName, schemaColumn.SqlDbType);
					}
				}
			}
			else
			{
				reader = new StringReader(row[columnName] == DBNull.Value ? string.Empty : (string)row[columnName]);
			}

			return reader;
		}
		#endregion

		internal void AddDataColumnToTable(DataTable destTable, SchemaColumn sourceSchema)
		{
			if (sourceSchema.Name == null || string.IsNullOrEmpty(sourceSchema.Name))
			{
				ErrorReporter.ReportOnce("MissingColName", "Cannot add column with no name. Type = " + sourceSchema.SqlDbType.ToString() + " Table = " + destTable.TableName); // extra debugging to find source of test failure
			}

			DataColumn dataColumn = new DataColumn(sourceSchema.Name, sourceSchema.DotNetType);
			dataColumn.DefaultValue = sourceSchema.SqlDbDefault;
			dataColumn.AllowDBNull = sourceSchema.IsNullable;
			if (sourceSchema.HasMaxLength)
			{
				dataColumn.MaxLength = sourceSchema.MaxLength;
			}
			destTable.Columns.Add(dataColumn);
			//hack to fix https://stackoverflow.com/questions/73875363/sqlgeography-does-not-implement-icomparable-interface-preventing-dataset-reject
			if (sourceSchema.DotNetType == typeof(SqlGeography))
			{
				var storageField = typeof(DataColumn).GetField("_storage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var objectStorageType = typeof(DataColumn).Assembly.GetType("System.Data.Common.ObjectStorage");
				var objectStorageConstructor = objectStorageType.GetConstructor(System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[2] { typeof(DataColumn), typeof(Type) }, null);
				var objectStorageInstance = objectStorageConstructor.Invoke(new object[2] { dataColumn, typeof(SqlGeography) });
				storageField.SetValue(dataColumn, objectStorageInstance);
			}
		}

		protected override ZDataTable AddTableToDataSet(string tableName)
		{
			ZDataTable newTable = new ZDataTable(tableName);
			try
			{
				var schemaColumns = SchemaResolver.GetSchemaColumns(tableName);
				newTable.DataColumnFromSchemaColumnCacheCapacity = schemaColumns.Max(x => x.Ordinal) + 1;
				SchemaColumn pkSchemaColumn = schemaColumns.PK;

				AddDataColumnToTable(newTable, pkSchemaColumn);

				foreach (var column in schemaColumns)
				{
					if (column.IsComputed)
					{
						continue;
					}

					if (column != pkSchemaColumn)
					{
						AddDataColumnToTable(newTable, column);
					}
				}

				newTable.PrimaryKey = new DataColumn[] { newTable.Columns[0] };
				Data.Tables.Add(newTable);
			}
			catch (InvalidTableNameException)
			{
				// Work around for .NET bug
				// If you do a FillSchema on a non-existent table, and then you do a data reader
				// on a view that exists, the data reader includes all view fields plus PKs from other tables that make up the view,
				// even though these PKs are not part of the view.
				// If you do another FillSchema on an existing table, afterwards, this seems to fix the bug.
				// Our fix here avoids doing fill schemas on non-existent tables.
				if (DoesTableExist(tableName))
				{
					var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
					collection.Load("select top 0 * FROM " + ConnectionInfo.PathToTables + tableName);
					newTable = ((INeedTable)collection).Table;
					newTable.TableName = tableName;
					Data.Tables.Add(newTable);
				}
				else
				{
					throw new ZException("Table " + ConnectionInfo.PathToTables + tableName + " does not exist.");
				}
			}
			catch (NullReferenceException e)
			{
				string message = string.Format((NoResString)"NullReferenceException during adding Table {0} to DataSet.", tableName);
				if (!Data.Tables.Contains(tableName))
				{
					throw new ZException(message, e);
				}
			}
			return newTable;
		}

		string GetTableNameWithoutDots(string tableName)
		{
			int lastDotPos = tableName.LastIndexOf(".");
			if (lastDotPos > -1)
			{
				tableName = tableName.Substring(lastDotPos + 1);
			}
			return tableName;
		}

		StringInterner stringInterner;
		DataRowLoadResponse FillDataTableFromDB(ZDataTable table, IDataReader dataReader, ZQuery query)
		{
			if (stringInterner == null)
			{
				stringInterner = new StringInterner();
			}

			var allRows = new List<DataRow>();
			var newRows = new List<DataRow>();

			if (dataReader.Read())
			{
				if (dataReader.FieldCount != table.Columns.Count)
				{
					var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Data reader contains a different number of fields when compared to the table columns. Table: {0}, Field Count: {1}, Table Column Count: {2}", table.TableName, dataReader.FieldCount, table.Columns.Count);
					throw new ZException(message); //Most likely from a stored procedure that has not been updated for table changes
				}

				var pkName = ZDataUtils.GetPKNameFromTable(table);
				var tableName = GetTableNameWithoutDots(table.TableName);

				var guids = (tableName == "StmALog") ? new Dictionary<Guid, bool>() : null;

				var schemaColumnCache = new SchemaColumn[dataReader.FieldCount];

				do
				{
					var pk = (Guid)dataReader[pkName];
					var row = table.GetRowIncludingDeleted(pk);

					if (guids != null)
					{
						if (guids.ContainsKey(pk))
						{
							continue;
						}
						guids.Add(pk, true);
					}

					var isRowDeletedOrDetached = (row?.RowState == DataRowState.Deleted || row?.RowState == DataRowState.Detached);

					if (row == null || !isRowDeletedOrDetached)
					{
						if (row == null || query.ReLoadExistingRows)
						{
							if (row == null)
							{
								row = table.NewRow();
								PopulateRow(row, dataReader, schemaColumnCache, tableName);
								newRows.Add(row);
							}
							else
							{
								PopulateRow(row, dataReader, schemaColumnCache, tableName);

								if (row.RowState == DataRowState.Modified)
								{
									row.AcceptChanges();
								}
							}
						}
						else if (query.LoadWithBlobs.Any() && row is ZDataRow dataRow)
						{
							FillBlobColumns(dataRow, dataReader, query);

							if (dataRow.RowState == DataRowState.Modified)
							{
								dataRow.AcceptChanges();
							}
						}

						allRows.Add(row);
					}
				} while (dataReader.Read());

				foreach (var row in newRows)
				{
					try
					{
						table.Rows.Add(row);

						if (row.RowState != DataRowState.Detached)
						{
							row.AcceptChanges();
						}
					}
					catch (ConstraintException ex)
					{
						//Only report this error if union all isn't in the query. For union all, we expect this to happen sometimes.
						if (query.LiteralTextSql.IndexOf((NoResString)"union all", StringComparison.OrdinalIgnoreCase) < 0)
						{
							var stringBuilder = new StringBuilder();
							stringBuilder.AppendLine($"Result have duplicate record with same PK. Table Name: {tableName}");
							stringBuilder.AppendLine($"PK Name : {pkName}");
							stringBuilder.AppendLine($"Repetitive PK: {row.ItemArray[0]}");
							stringBuilder.AppendLine($"SQL: {query.GetAsCompleteSQLStatementWithParameters(tableName)}");
							ErrorReporter.ReportOnce("Constrained_to_be_unique", stringBuilder.ToString(), ex);
						}
					}
				}
			}

			return new DataRowLoadResponse(table.TableName, query, allRows.ToArray(), newRows.ToArray());
		}

		void PopulateRow(DataRow row, IDataReader dataReader, SchemaColumn[] schemaColumnCache, string tableName)
		{
			row.BeginEdit();

			try
			{
				SetRowFields(dataReader, schemaColumnCache, tableName, row, true);

				try
				{
					HandleDBNullColumns(tableName, row);
				}
				catch (InvalidTableNameException)
				{
				}
			}
			finally
			{
				row.EndEdit();
			}
		}

		void HandleDBNullColumns(string tableName, DataRow row)
		{
			foreach (SchemaColumn column in SchemaResolver.GetSchemaColumns(tableName))
			{
				if (column.IsLargeBinaryOrText && row[column.Name] == DBNull.Value)
				{
					if (column.DotNetType == typeof(string) && !column.IsSparse)
					{
						row[column.Name] = "";
					}
					else if (column.DotNetType == typeof(byte[]))
					{
						row[column.Name] = column.IsNullable ? null : Array.Empty<byte>();
					}
				}
			}
		}

		void FillBlobColumns(ZDataRow dataRow, IDataReader dataReader, ZQuery query)
		{
			foreach (var blobColumn in query.LoadWithBlobs)
			{
				var columnName = blobColumn.Name;
				if (dataRow.Table.Columns.Contains(columnName))
				{
					if (LazyLoading.LoadRequired(dataRow[columnName]))
					{
						dataRow[columnName] = GetUncompressedRowValue(false, columnName, dataReader[columnName], blobColumn);
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		void SetRowFields(IDataReader dataReader, SchemaColumn[] schemaColumnCache, string tableName, DataRow row, bool internValue)
		{
			for (var i = 0; i < dataReader.FieldCount; i++)
			{
				var columnName = dataReader.GetName(i);
				var data = dataReader[i];
				var schemaColumn = schemaColumnCache[i] ?? (schemaColumnCache[i] = SchemaResolver.GetSchemaColumnSafe(columnName, tableName));
				row[columnName] = GetUncompressedRowValue(internValue, columnName, data, schemaColumn);
			}
		}

		object GetUncompressedRowValue(bool internValue, string columnName, object data, SchemaColumn schemaColumn)
		{
			var sanitised = GetUncompressedSanitisedRowValue(columnName, data, schemaColumn);

			return internValue
				? stringInterner.InternValue(sanitised)
				: (sanitised ?? DBNull.Value);
		}

		const int MaxLengthForStringColunmInterning = 3;

		public static object GetUncompressedSanitisedRowValue(string columnName, object data, SchemaColumn schemaColumn)
		{
			var uncompressedData = ZCompressor.GetUncompressedVersion(data, columnName);
			var dataAsString = uncompressedData as string;

			if (dataAsString != null)
			{
				if (schemaColumn is SchemaBoolColumn)
				{
					uncompressedData = new ZBool(data) == ZBool.True;
				}
				else if (dataAsString.Length > 0 && dataAsString[dataAsString.Length - 1] == ' ')
				{
					uncompressedData = dataAsString = dataAsString.TrimEnd(' ');
				}

				if (dataAsString.Length > 0 && schemaColumn is SchemaStringColumn { MaxLength: <= MaxLengthForStringColunmInterning })
				{
					// Intern all short codes string - they repeat frequently
					uncompressedData = string.Intern(dataAsString);
				}
			}
			else if (schemaColumn is SchemaGeographyColumn)
			{
				var sqlGeoValue = data as SqlGeography;
				if (sqlGeoValue == null)
				{
					var sqlBytes = new SqlBytes((byte[])data);
					sqlGeoValue = SqlGeography.Deserialize(sqlBytes);
				}

				if (sqlGeoValue.IsNull)
				{
					sqlGeoValue = ZGeography.EmptySqlGeography;
				}

				uncompressedData = sqlGeoValue;
			}

			return uncompressedData;
		}

		ZDataTable BuildTableFromReader(IDataReader dataReader)
		{
			ZDataTable table = new ZDataTable();
			DataTable schema = dataReader.GetSchemaTable();

			if (schema != null)
			{
				foreach (DataRow schemaRow in schema.Rows)
				{
					string columnName = (string)schemaRow["ColumnName"];
					if (!table.Columns.Contains(columnName))
					{
						Type dataType = (Type)schemaRow["DataType"];
						table.Columns.Add(columnName, dataType);
					}
				}
			}

			return table;
		}

		void CopyRowsFromReaderToDataTable(DataTable table, IDataReader dataReader)
		{
			var tableName = GetTableNameWithoutDots(table.TableName);
			var schemaColumnCache = new SchemaColumn[dataReader.FieldCount];

			while (dataReader.Read())
			{
				var row = table.NewRow();

				SetRowFields(dataReader, schemaColumnCache, tableName, row, false);

				table.Rows.Add(row);
				row.AcceptChanges();
			}
		}

		ZSqlConnectionInfo ConnectionInfo { get; }
		IApplicationSchemaResolver SchemaResolver { get; }
	}
}
