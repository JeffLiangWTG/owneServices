using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Integration;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.EntityFramework
{
	public class ZSqlSaverBase : ZSaver, IZSqlSaver
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		static DataSet CreateDataSet(DataTable dataTable)
		{
			var result = new DataSet();
			result.Tables.Add(dataTable);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public ZSqlSaverBase(DataSet data, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: this(data, new ZSqlConnectionInfo(connection, ""), schemaResolver)
		{
		}

		public ZSqlSaverBase(DataTable dataTable, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			: this(CreateDataSet(dataTable), connection, schemaResolver)
		{
		}

		bool compress;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public ZSqlSaverBase(DataSet data, ZSqlConnectionInfo connectionInfo, IApplicationSchemaResolver schemaResolver) : base(data)
		{
			compress = true;
			ConnectionInfo = connectionInfo;
			SchemaResolver = schemaResolver;
		}

		protected internal int RowsToPostPerSqlStatement = GlobalServiceProvider.Instance.GetRequiredService<IZSqlSaverConfiguration>().RowsToPostPerSqlStatement;
		protected internal int BytesToPostPerSqlStatement = 42000; // large object heap..

#if DEBUG
		public
#endif
		static readonly int MaxRowsPerMultiRowInsertStatement = 50; // performance improvement is reduced with too many combined rows

		public IChangedTableNames Save(bool useCompression = true)
		{
			var originalCompress = compress;
			try
			{
				compress = useCompression;
				return base.Save();
			}
			finally
			{
				compress = originalCompress;
			}
		}

		protected virtual int SaveSplitRows(IList<DataRow> rows, int startingIndex, bool canConsolidateInserts = true, bool isBulkUpdate = false)
		{
			ZSaveCommand command = null;

			try
			{
				var result = BuildCommand(rows, startingIndex, RowsToPostPerSqlStatement, BytesToPostPerSqlStatement, canConsolidateInserts, isBulkUpdate);
				command = result.Command;
				command.Execute();
				return Math.Min(rows.Count, startingIndex + result.Rows);
			}
			catch (Exception standardException) when (!standardException.IsCriticalException())
			{
				if (standardException is ZSaveCommandException saveException)
				{
					if (canConsolidateInserts && saveException.ErrorPk == new Guid(MultiRowInsertMockPk) && ConnectionInfo.DbConnection.IsInTransaction)
					{
						return SaveSplitRows(rows, startingIndex, canConsolidateInserts: false, isBulkUpdate: isBulkUpdate);
					}

					var rowsWithError = GetRowsWithErrorFromException(saveException, rows, startingIndex);

					if (saveException.IsConcurrencyError)
					{
						if (saveException is ZSaveCommandRecoverableException recoverableException)
						{
							throw new ZDataConcurrencyException(saveException.InnerException, rowsWithError, true, ConnectionInfo.DbConnection, recoverableException.RecoverAction);
						}

						throw new ZDataConcurrencyException(saveException.InnerException, rowsWithError, true, ConnectionInfo.DbConnection, saveException.IsTriggerException);
					}
					else
					{
						throw new ZDataException(saveException.InnerException, rowsWithError, true, ConnectionInfo.DbConnection);
					}
				}

				throw;
			}
			finally
			{
				if (command != null)
				{
					if (command.ShouldReclaimMemory)
					{
						GCWrapper.ReclaimMemory(ref command);
					}
					else
					{
						command.Dispose();
					}
				}
			}
		}

		List<DataRow> GetRowsWithErrorFromException(ZSaveCommandException saveException, IList<DataRow> rows, int startingIndex)
		{
			if (saveException.ErrorPk == Guid.Empty)
			{
				return new List<DataRow>();
			}

			var errorPks = new HashSet<Guid>() { saveException.ErrorPk };

			if (saveException.ErrorPk == Guid.Parse(MultiRowInsertMockPk))
			{
				var parsedErrorPks = ParseErrorPkFromExceptionMessage(saveException.Message);
				if (parsedErrorPks.Count > 0)
				{
					errorPks = parsedErrorPks;
				}
			}

			return rows.Skip(startingIndex).Where(row => errorPks.Contains(DataUtils.GetPk(row))).ToList();
		}

		protected HashSet<Guid> ParseErrorPkFromExceptionMessage(string message)
		{
			var errorPks = new HashSet<Guid>();

			var guidRegexPattern = @"[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}";
			var multipleGuidsRegexPattern = $@"^([{{(]?{guidRegexPattern}[)}}]?)(,[{{(]?{guidRegexPattern}[)}}]?)*$";

			if (Regex.IsMatch(message, multipleGuidsRegexPattern, RegexOptions.IgnoreCase))
			{
				var pkMatches = Regex.Matches(message, guidRegexPattern, RegexOptions.IgnoreCase);
				foreach (Match match in pkMatches)
				{
					if (Guid.TryParse(match.Value, out Guid parsedGuid))
					{
						errorPks.Add(parsedGuid);
					}
				}
			}

			return errorPks;
		}

		protected override void SaveRows(IList<DataRow> rows)
		{
			var duplicateUniqueIndexKeyRows = new List<DataRow>();
			var rowIndex = 0;
			while (rowIndex < rows.Count)
			{
				try
				{
					rowIndex = SaveSplitRows(rows, rowIndex);
				}
				catch (ZDataException dataException)
				{
					var handled = false;

					if ((rows.Count - rowIndex) <= MaximumRowsForIndividualDependancyOrdering)
					{
						var dbErrorMatch = dataException.InnerException is SqlException innerSqlException ? new DbErrorMatch(innerSqlException) : null;

						if (dbErrorMatch != null && dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
						{
							try
							{
								// Skip rows that have already been saved by last SaveSplitRows until row with error
								if (dataException.Row != null)
								{
									for (var i = rowIndex; i < rows.Count; i++)
									{
										if (rows[i] == dataException.Row)
										{
											rowIndex = i;
											break;
										}
									}
								}

								if (rowIndex > 0)
								{
									rows = rows.Skip(rowIndex).ToList();
								}

								rowIndex = SortAndResaveRows(rows, duplicateUniqueIndexKeyRows);

								handled = true;
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								// Original exception will be rethrown
							}
						}
					}

					if (!handled)
					{
						throw;
					}
				}
			}

			SaveDuplicateUniqueIndexKeyRows(duplicateUniqueIndexKeyRows);
		}

		protected virtual int SortAndResaveRows(IList<DataRow> rows, IList<DataRow> duplicateUniqueIndexKeyRows)
		{
			var rowIndex = 0;
			new ZRowSaveOrderSorter().Sort(rows);
			try
			{
				rowIndex = SaveSplitRows(rows, rowIndex);
			}
			catch (ZDataException dataException)
			{
				if (dataException.InnerException is TransactionException)
				{
					throw;
				}

				var dbErrorMatch = dataException.InnerException is SqlException innerSqlException ? new DbErrorMatch(innerSqlException) : null;
				if (dbErrorMatch != null && dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
				{
					var beginIndex = rowIndex;
					int endIndex = Math.Min(rows.Count, beginIndex + RowsToPostPerSqlStatement);
					for (var i = beginIndex; i < endIndex; i++)
					{
						var row = rows[i];
						SaveOneRow(row, duplicateUniqueIndexKeyRows);
						rowIndex++;
					}
				}
			}

			return rowIndex;
		}

		void SaveOneRow(DataRow row, IList<DataRow> duplicateUniqueIndexKeyRows)
		{
			try
			{
				SaveSplitRows(new List<DataRow>() { row }, 0);
			}
			catch (ZDataException dataException)
			{
				var dbErrorMatch = dataException.InnerException is SqlException innerSqlException ? new DbErrorMatch(innerSqlException) : null;
				if (dbErrorMatch != null && dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateUniqueIndexKey && row.HasVersion(DataRowVersion.Original))
				{
					duplicateUniqueIndexKeyRows.Add(row);
				}
				else
				{
					throw;
				}
			}
		}

		void SaveDuplicateUniqueIndexKeyRows(IList<DataRow> duplicateUniqueIndexKeyRows)
		{
			if (null != duplicateUniqueIndexKeyRows && duplicateUniqueIndexKeyRows.Count > 0)
			{
				SaveSplitRows(duplicateUniqueIndexKeyRows, 0, isBulkUpdate: true);
			}
		}

		const int MaximumRowsForIndividualDependancyOrdering = 100;

		protected BuildResult BuildCommand(IList<DataRow> rows, int startIndex, int maximumRows, int maximumBytes, bool canConsolidateInserts, bool isBulkUpdate = false)
		{
			var blobSaverCollection = new List<ILargeColumnSaver>();
			var commandText = GetNewStringBuilderForSQL();
			commandText.Append(BeginningText);

			var endIndex = isBulkUpdate ? rows.Count : Math.Min(rows.Count, startIndex + maximumRows);
			var rowsIncludedInOutput = 0;
			var isMultiRowInsertContinuation = false;
			var hasMultiRowInsertJustCompleted = false;
			var rowsToUpdate = new Dictionary<string, List<DataRow>>();

			List<ILargeColumnSaver> previousInsertBlobSaverCollection = null;
			string previousInsertCommandText = null;
			var previousUpdateBlobSaverCollection = new Dictionary<string, List<ILargeColumnSaver>>();
			var previousUpdateCommandText = new Dictionary<string, string>();
			var statementIndex = 1;

			ZSqlCommandBuilder statementBuilder = null;

			for (var i = startIndex; i < endIndex; i++)
			{
				// If the last statement was the last of a multi-row insert, or has reached the combined multirow insert limit
				// => break to start a new command
				if (hasMultiRowInsertJustCompleted || (isMultiRowInsertContinuation && rowsIncludedInOutput >= MaxRowsPerMultiRowInsertStatement))
				{
					break;
				}

				var row = rows[i];
				var rowTableName = row.Table.TableName;
				var fullPathAndTableName = ConnectionInfo.PathToTables + rowTableName;
				var schema = SchemaCache.GetOrAdd(fullPathAndTableName, () => SchemaResolver.GetTableSchema(fullPathAndTableName.Split('.').Last()));
				var isBulkRowUpdate = false;

				if (row.RowState == DataRowState.Deleted)
				{
					statementBuilder = GetZDeleteCommandBuilder(row, fullPathAndTableName, compress, statementIndex++, schema);
				}
				else if (row.HasVersion(DataRowVersion.Original) && isBulkUpdate)
				{
					isBulkRowUpdate = true;
					statementBuilder = GetBulkUpdateCommandBuilder(row, fullPathAndTableName, rowTableName, rowsToUpdate, schema);
				}
				else if (row.HasVersion(DataRowVersion.Original))
				{
					statementBuilder = GetZUpdateCommandBuilder(row, fullPathAndTableName, compress, statementIndex++, schema);
				}
				else
				{
					var isNextRowSameTableInsert = false;

					if (canConsolidateInserts)
					{
						var nextRow = (i + 1 < endIndex) ? rows[i + 1] : null;
						isNextRowSameTableInsert = IsNextRowSameTableInsert(nextRow, rowTableName);

						// If output command already has other statements and it is about to start a multi-row insert,
						// => break to start a new command
						if (rowsIncludedInOutput > 0 && isNextRowSameTableInsert && !isMultiRowInsertContinuation)
						{
							break;
						}

						// If currently building a multi-row insert and next row is not an insert into the same table,
						// => marks output command build to stop after this row
						hasMultiRowInsertJustCompleted = isMultiRowInsertContinuation && !isNextRowSameTableInsert;
					}

					isMultiRowInsertContinuation |= isNextRowSameTableInsert;
					if (isMultiRowInsertContinuation)
					{
						if (statementBuilder == null)
						{
							statementBuilder = GetZInsertCommandBuilder(fullPathAndTableName, compress, statementIndex, schema, endIndex);
						}
					}
					else
					{
						if (i > startIndex)
						{
							statementIndex++;
						}
						statementBuilder = GetZInsertCommandBuilder(fullPathAndTableName, compress, statementIndex, schema, endIndex);
					}
					((ZInsertCommandBuilderBase)statementBuilder).AddRow(row);
				}

				var primaryKey = string.Empty;
				if (isMultiRowInsertContinuation && rowsIncludedInOutput > 0)
				{
					primaryKey = MultiRowInsertMockPk;
				}
				else if (isBulkUpdate && rowsIncludedInOutput > 0)
				{
					primaryKey = BulkUpdateMockPk;
				}
				else
				{
					primaryKey = row[0, (row.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current)].ToString();
				}

				string currentCommandText = PrepareCurrentRowCommand(statementBuilder, primaryKey);

				// If output command already has at least one statement and it will exceed the maximum lengh with the current one,
				// => break to start a new command
				if (rowsIncludedInOutput > 0 && commandText.Length + currentCommandText.Length + EndingText.Length > maximumBytes)
				{
					break;
				}

				rowsIncludedInOutput++;

				if (isMultiRowInsertContinuation)
				{
					previousInsertBlobSaverCollection = new List<ILargeColumnSaver>(statementBuilder.LargeColumnSavers);
					previousInsertCommandText = currentCommandText;
				}
				else if (isBulkUpdate && isBulkRowUpdate)
				{
					UpdatePreviousBlobSaverCollectionAndCommandTextForBulkUpdate(rowTableName, previousUpdateBlobSaverCollection, previousUpdateCommandText, statementBuilder.LargeColumnSavers, currentCommandText);
				}
				else
				{
					commandText.Append(currentCommandText);
					blobSaverCollection.AddRange(statementBuilder.LargeColumnSavers);
				}

				//If next row is the starting row of a table that needs bulk copy
				// => break to start a new round
				if (ShouldStartCollectingBulkCopyRows(rows, i + 1))
				{
					break;
				}
			}

			UpdateCurrentBlobSaverCollectionAndCommandTextForMultiRowInsert(isMultiRowInsertContinuation, previousInsertBlobSaverCollection, previousInsertCommandText, blobSaverCollection, commandText);
			UpdateCurrentBlobSaverCollectionAndCommandTextForBulkUpdate(isBulkUpdate, previousUpdateBlobSaverCollection, previousUpdateCommandText, blobSaverCollection, commandText);

			commandText.Append(EndingText);

			DbCommand command = ConnectionInfo.GetNewDbCommandForUpdate(commandText.ToString(), null);
			ZSaveCommand result = new ZSaveCommand(command, blobSaverCollection);

			return new BuildResult { Command = result, Rows = rowsIncludedInOutput };
		}

		ZSqlCommandBuilder GetBulkUpdateCommandBuilder(DataRow row, string fullPathAndTableName, string rowTableName, Dictionary<string, List<DataRow>> rowsToUpdate, ITableSchema schema)
		{
			if (!rowsToUpdate.ContainsKey(rowTableName))
			{
				rowsToUpdate.Add(rowTableName, new List<DataRow>());
			}
			rowsToUpdate[rowTableName].Add(row);
			return GetZBulkUpdateCommandBuilder(rowsToUpdate[rowTableName], fullPathAndTableName, compress, schema);
		}

		void UpdatePreviousBlobSaverCollectionAndCommandTextForBulkUpdate(string rowTableName, IDictionary<string, List<ILargeColumnSaver>> previousUpdateBlobSaverCollection, IDictionary<string, string> previousUpdateCommandText, IList<ILargeColumnSaver> currentBlobSaverCollection, string currentCommandText)
		{
			if (!previousUpdateBlobSaverCollection.ContainsKey(rowTableName))
			{
				previousUpdateBlobSaverCollection.Add(rowTableName, new List<ILargeColumnSaver>());
			}
			previousUpdateBlobSaverCollection[rowTableName] = new List<ILargeColumnSaver>(currentBlobSaverCollection);

			if (!previousUpdateCommandText.ContainsKey(rowTableName))
			{
				previousUpdateCommandText.Add(rowTableName, "");
			}
			previousUpdateCommandText[rowTableName] = currentCommandText;
		}

		void UpdateCurrentBlobSaverCollectionAndCommandTextForBulkUpdate(bool isBulkUpdate, IDictionary<string, List<ILargeColumnSaver>> previousUpdateBlobSaverCollection, IDictionary<string, string> previousUpdateCommandText, List<ILargeColumnSaver> blobSaverCollection, StringBuilder commandText)
		{
			if (isBulkUpdate && null != previousUpdateCommandText && null != previousUpdateBlobSaverCollection)
			{
				foreach (var text in previousUpdateCommandText)
				{
					commandText.Append(text.Value);
					blobSaverCollection.AddRange(previousUpdateBlobSaverCollection[text.Key]);
				}
			}
		}

		void UpdateCurrentBlobSaverCollectionAndCommandTextForMultiRowInsert(bool isMultiRowInsertContinuation, IList<ILargeColumnSaver> previousInsertBlobSaverCollection, string previousInsertCommandText, List<ILargeColumnSaver> blobSaverCollection, StringBuilder commandText)
		{
			if (isMultiRowInsertContinuation)
			{
				commandText.Append(previousInsertCommandText);
				blobSaverCollection.AddRange(previousInsertBlobSaverCollection);
			}
		}

		protected static bool IsNextRowSameTableInsert(DataRow nextRow, string currentRowTableName) => IsInsertRow(nextRow) && nextRow.Table.TableName == currentRowTableName;

		protected static bool IsInsertRow(DataRow row) =>
			row != null
			&& row.RowState != DataRowState.Deleted
			&& !row.HasVersion(DataRowVersion.Original);

		protected static bool ShouldStartCollectingBulkCopyRows(IList<DataRow> rows, int startingIndex) =>
			rows != null
			&& startingIndex < rows.Count
			&& IsInsertRow(rows[startingIndex])
			&& rows[startingIndex].Table.ExtendedProperties[typeof(BulkCopySetting)] != null;

		string PrepareCurrentRowCommand(ZSqlCommandBuilder commandBuilder, string primaryKey)
		{
			var currentCommandText = GetNewStringBuilderForSQL();

			if (primaryKey != null)
			{
				currentCommandText.AppendLine().AppendLine(string.Format(CultureInfo.InvariantCulture, "SET @PK = '{0}';", primaryKey)); // part of a sql command
			}

			commandBuilder.AppendCommandTextAndBlobSaver(currentCommandText);
			commandBuilder.AddConcurrencyCheck(currentCommandText, ConcurrencyErrorText);

			return currentCommandText.ToString();
		}

		StringBuilder GetNewStringBuilderForSQL()
		{
			return new StringBuilder();
		}

		protected override bool RowExistsInDatabaseCore(DataRow row)
		{
			var sql = "select count(1) from {0} where {1} = @PK";

			var tableName = row.Table.TableName;
			var pkColumnName = DataUtils.GetPkNameFromTable(row.Table);

			var commandText = string.Format(CultureInfo.InvariantCulture, sql, tableName, pkColumnName);

			var command = ConnectionInfo.GetNewDbCommandForSelect(commandText, parameters: null);
			command.AddParameter("@PK", SqlDbType.UniqueIdentifier, DataUtils.GetPk(row)); 

			var resultObject = command.ExecuteScalar();
			return resultObject is > 0;
		}

		#region command builder getter

		public virtual ZSqlCommandBuilder GetZBulkUpdateCommandBuilder(IEnumerable<DataRow> rowsToUpdate, string tableName, bool compress, ITableSchema schema)
		{
			return new ZBulkUpdateCommandBuilderBase(rowsToUpdate, tableName, compress, schema);
		}

		public virtual ZSqlCommandBuilder GetZUpdateCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema, bool isBulk = false, StringBuilder commandText = null, List<ILargeColumnSaver> largeColumnSavers = null)
		{
			return new ZUpdateCommandBuilderBase(row, tableName, compress, statementIndex, schema, isBulk, commandText, largeColumnSavers);
		}

		public virtual ZSqlCommandBuilder GetZDeleteCommandBuilder(DataRow row, string tableName, bool compress, int statementIndex, ITableSchema schema)
		{
			return new ZDeleteCommandBuilderBase(row, tableName, compress, statementIndex, schema);
		}

		public virtual ZSqlCommandBuilder GetZInsertCommandBuilder(string tableName, bool compress, int statementIndex, ITableSchema schema, int expectedNumberOfRows)
		{
			return new ZInsertCommandBuilderBase(tableName, compress, statementIndex, schema, expectedNumberOfRows);
		}

		#endregion

		#region Constants
		#region SuppressResourceStringsCheckRegion

		const string MultiRowInsertMockPk = "DeadBeef-Add1-Add2-Add3-DeadBeefC0de";
		const string BulkUpdateMockPk = "Baada555-Cafe-babe-4b1d-CafeBabe4b1d";
		const string ConcurrencyErrorText = "~ConcurrencyError~";
		const string BeginningText =
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY
";
		const string EndingText =
@"
END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{' + @PK + ',' + case when @ErrMsg = '" + ConcurrencyErrorText + @"' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH
";

		#endregion
		#endregion // Constants

		protected ZSqlConnectionInfo ConnectionInfo { get; }
		protected IApplicationSchemaResolver SchemaResolver { get; }
		protected Dictionary<string, ITableSchema> SchemaCache = new Dictionary<string, ITableSchema>();
	}

	public class BuildResult
	{
		public ZSaveCommand Command { get; set; }
		public int Rows { get; set; }
	}
}
