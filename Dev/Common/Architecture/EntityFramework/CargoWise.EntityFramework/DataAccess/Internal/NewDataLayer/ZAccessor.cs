using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	abstract class ZAccessor
	{
		protected ZAccessor(DataSet data)
		{
			this.Data = data;
			tableHitCounter = new TableHitCounter();
			tableHitCounterThisAction = new TableHitCounter();
			SchemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
		}

		protected IApplicationSchemaResolver SchemaResolver { get; }

		#region Statistics

		public void IncreaseDatabaseLoadCount(LoadStat difference)
		{
			IncreaseDatabaseLoadCount(difference.DatabaseLoadCount);
			foreach (var counter in difference.HitCounts)
			{
#if DEBUG
				tableHitCounter.IncreaseAccessCount(counter.TableName, counter.Queries, counter.Value);
#else
				tableHitCounter.IncreaseAccessCount(counter.TableName, counter.Value);
#endif
			}
		}

		void IncreaseDatabaseLoadCount()
		{
			IncreaseDatabaseLoadCount(1);
		}

		void IncreaseDatabaseLoadCount(int quantity)
		{
			databaseLoadCount += quantity;
		}

		public LoadStat CurrentDatabaseInfo
		{
			get { return new LoadStat(DatabaseLoadCount, TableSelects); }
		}

		public int DatabaseLoadCount
		{
			get { return databaseLoadCount; }
		}

		public int DatabaseSaveCount
		{
			get { return databaseSaveCount; }
		}

#if DEBUG
		public void ResetDatabaseLoadCount()
		{
			databaseLoadCount = 0;
			tableHitCounter.Clear();
			tableHitCounterThisAction.Clear();
		}
#endif

		public TableHitCount[] TableSelects
		{
			get { return tableHitCounter.Values.ToArray(); }
		}

		readonly TableHitCounter tableHitCounter;
		readonly TableHitCounter tableHitCounterThisAction;
		int lastActionCount;

		int databaseLoadCount;
		int databaseSaveCount;

		[System.Diagnostics.Conditional("DEBUG")]
		internal void IncreaseAccessCount(string tableName, string query)
		{
#if DEBUG
			if (!IsTableHitCounterSuspended)
			{
				var queries = IsTableHitQueryCollectionEnabled(tableName) ? CreateTableHitQuery(query) : null;
				tableHitCounter.IncreaseAccessCount(tableName, queries);
			}
#else
			tableHitCounter.IncreaseAccessCount(tableName);
#endif

			if (lastActionCount != ActionCounter.ActionCount)
			{
				tableHitCounterThisAction.Clear();
				lastActionCount = ActionCounter.ActionCount;
			}
#if DEBUG
			//if (ActionCounter.ActionCount > 0)
			//{
			//  if (tableHitCounterThisAction.IncreaseAccessCount(tableName) > NumberOfAccessesPerTablePerActionBeforeReportingError)
			//  {
			//    if (!TestingState.IsRunningTests)
			//    {
			//      ErrorReporter.ReportOnce("MultipleDbHits:" + tableName, string.Format(
			//        "You have made more than {0} database hits for {1} during the same action. Add some fetch hints or improve your algorithm.\r\nLast query to db:\r\n{2}",
			//        NumberOfAccessesPerTablePerActionBeforeReportingError, tableName, query));
			//    }
			//  }
			//}
#endif
		}

		//private const int NumberOfAccessesPerTablePerActionBeforeReportingError = 5;

		#endregion

		protected abstract ZLoader Loader { get; }
		protected abstract ZSaver Saver { get; }

		public ZDataTable GetTable(string tableName, bool createIfNotExisting = true)
		{
			return Loader.GetTable(tableName, createIfNotExisting);
		}

		public DataRowLoadResponse[] LoadPersistentRowsIntoDataSet(IList<ZDataQuery> queries)
		{
			foreach (ZDataQuery query in queries)
			{
				IncreaseAccessCount(query.TableName, query.LiteralTextADO);
			}

			LoaderResponse response = Loader.LoadPersistentRowsIntoDataSet(queries);
			IncreaseDatabaseLoadCount(response.DatabaseRoundTrips);

			return response.DataRowLoadResponses;
		}

		public NonPersistentLoaderResponse LoadNonPersistentRows(ZNonPersistentDataQuery query, bool logNonPersistentTableHitCount = false)
		{
			IncreaseDatabaseLoadCount();
			if (logNonPersistentTableHitCount)
			{
				var queryText = query.ParameterisedQueryText.Replace('\r', ' ').Replace('\t', ' ').Replace('\n', ' ').ToUpper();
				while (queryText.Contains("  "))
				{
					queryText = queryText.Replace("  ", " ");
				}

				var whereClausePosition = queryText.IndexOf(" WHERE ");
				var tableName = NonPersitentTableName;
				if (whereClausePosition > 0)
				{
					queryText = queryText.Substring(0, whereClausePosition);
					var fromPosition = queryText.IndexOf(" FROM ");
					tableName += " " + queryText.Substring(fromPosition + 5).Trim();
				}
				IncreaseAccessCount(tableName, query.LiteralTextSql);
			}
			return Loader.LoadNonPersistentRows(query);
		}
		const string NonPersitentTableName = "NonPersitentTable";

		internal Stream GetBinaryFieldStream(DataRow row, string tableName, string columnName)
		{
			if ((row is ZDataRow) && ((ZDataRow)row).HasSource(columnName))
			{
				return ((ZDataRow)row).GetStreamSource(columnName).GetStream();
			}
			else
			{
				return Loader.GetBinaryFieldStream(row, tableName, columnName);
			}
		}

		// Return the raw binary stream from the database without decompression
		// Exception will be thrown if the row has not been saved to the db
		internal Stream GetBinaryFieldStreamRawFromDb(DataRow row, string tableName, string columnName)
		{
			if (row.RowState == DataRowState.Added)
			{
				throw new ZException("The record has not been saved in the database yet.");
			}

			return Loader.GetStream(row, tableName, columnName);
		}

		internal TextReader GetTextFieldReader(DataRow row, string tableName, string columnName, bool closeReaderBetweenReads)
		{
			if ((row is ZDataRow) && ((ZDataRow)row).HasSource(columnName))
			{
				return ((ZDataRow)row).GetReaderSource(columnName).GetReader(closeReaderBetweenReads);
			}
			else
			{
				return Loader.GetTextFieldReader(row, tableName, columnName, closeReaderBetweenReads);
			}
		}

		public void LoadBlobField(DataRow row, SchemaColumn column)
		{
			IncreaseAccessCount(row.Table.TableName, (NoResString)"Load blob field");
			IncreaseDatabaseLoadCount();
			Loader.LoadBlobField(row, column);
		}

		public void LoadBlobFieldsForTable(string tableName, ZDataRowDictionary rows, IEnumerable<SchemaColumn> columns)
		{
			if (rows.Count > 0)
			{
				IncreaseAccessCount(tableName, (NoResString)"Load blob fields for a table");
				IncreaseDatabaseLoadCount();
				Loader.LoadBlobFieldsForTable(tableName, rows, columns);
			}
		}

		internal int GetCount(ZCountDataQuery query)
		{
			IncreaseAccessCount(query.TableName, (NoResString)"Get count of\r\n" + query.ParameterisedQueryText);
			IncreaseDatabaseLoadCount();
			return Loader.GetCount(query);
		}

		public IChangedTableNames Save()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ZAccessor.Save"))
			{
				databaseSaveCount++;
				return Saver.Save();
			}
		}

		public IReadOnlyList<DataRow> GetModifiedPersistentRowsInSaveOrder()
		{
			return Saver.GetModifiedPersistentRowsInSaveOrder(SchemaResolver).AsReadOnly();
		}

		internal IEnumerable<DataTable> GetTablesThatNeedToBeSaved()
		{
			return Saver.GetTablesThatNeedToBeSaved();
		}

		protected readonly DataSet Data;

		#region For Test Purposes
#if DEBUG

		IEnumerable<TableHitQuery> CreateTableHitQuery(string query)
		{
			yield return new TableHitQuery(query, Environment.StackTrace);
		}

		internal IDisposable EnableTableHitQueryCollection(string[] tableNames)
		{
			var actualTableNames = tableNames == null ? null : tableNames.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
			return actualTableNames == null || actualTableNames.Length == 0 ? DisposableAction.NoAction : new TableHitQueryCollectionEnabler(this, tableNames);
		}

		sealed class TableHitQueryCollectionEnabler : IDisposable
		{
			public TableHitQueryCollectionEnabler(ZAccessor dataAccessor, string[] tableNames)
			{
				this.dataAccessor = dataAccessor;
				this.tableNames = tableNames;
				this.dataAccessor.tablesNeedsHitQueryCollection ??= new Dictionary<string, int>();
				var tablesNeedsHitQueryCollection = this.dataAccessor.tablesNeedsHitQueryCollection;
				foreach (var tableName in this.tableNames)
				{
					int index;
					if (tablesNeedsHitQueryCollection.TryGetValue(tableName, out index))
					{
						tablesNeedsHitQueryCollection[tableName] = ++index;
					}
					else
					{
						tablesNeedsHitQueryCollection.Add(tableName, 1);
					}
				}
			}
			readonly ZAccessor dataAccessor;
			readonly string[] tableNames;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				var tablesNeedsHitQueryCollection = dataAccessor.tablesNeedsHitQueryCollection;
				if (tablesNeedsHitQueryCollection != null)
				{
					foreach (var tableName in this.tableNames)
					{
						int index;
						if (tablesNeedsHitQueryCollection.TryGetValue(tableName, out index))
						{
							--index;
							if (index == 0)
							{
								tablesNeedsHitQueryCollection.Remove(tableName);
							}
							else
							{
								tablesNeedsHitQueryCollection[tableName] = index;
							}
						}
					}
					if (tablesNeedsHitQueryCollection.Count == 0)
					{
						this.dataAccessor.tablesNeedsHitQueryCollection = null;
					}
				}
			}

			#endregion
		}
		Dictionary<string, int> tablesNeedsHitQueryCollection;

		bool IsTableHitQueryCollectionEnabled(string tableName)
		{
			int index;
			return tablesNeedsHitQueryCollection != null && (tablesNeedsHitQueryCollection.TryGetValue(tableName, out index) && index > 0 || tableName.StartsWith(NonPersitentTableName));
		}

		public IEnumerable<string> TableNamesNeedingHitQueryCollection => tablesNeedsHitQueryCollection?.Keys.ToList() ?? Enumerable.Empty<string>();

		internal IDisposable SuspendTableHitCounter()
		{
			return new TableHitCounterSuspender(this);
		}

		bool IsTableHitCounterSuspended
		{
			get { return tableHitCounterSuspenderIndex > 0; }
		}

		int tableHitCounterSuspenderIndex;

		sealed class TableHitCounterSuspender : IDisposable
		{
			public TableHitCounterSuspender(ZAccessor dataAccessor)
			{
				this.dataAccessor = dataAccessor;
				this.dataAccessor.tableHitCounterSuspenderIndex++;
			}
			readonly ZAccessor dataAccessor;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				dataAccessor.tableHitCounterSuspenderIndex--;
			}

			#endregion
		}

		public TableHitCount GetTableHitCount(string tableName)
		{
			return tableHitCounter.GetTableHitCount(tableName);
		}

#endif
		#endregion
	}
}
