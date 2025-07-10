using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

#if DEBUG
using CargoWise.Common.Testing;
using NUnit.Framework;
#endif

namespace CargoWise.EntityFramework
{
	[DebuggerDisplay("RowFactory.Instance = {_Instance}")]
	public class RowFactory :
		INeedDataSet,
		ITransactionParticipant,
		IReadonlyDatabaseSupport
	{
		#region Constructors

		public RowFactory(BusinessObjectFactory parent = null)
			: this(null, parent)
		{
		}

		public RowFactory(string databaseName, BusinessObjectFactory parent = null)
			: this(null, databaseName, parent)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "This is for caching, not for synchronisation")]
		public RowFactory(DbConnection connection, string databaseName, BusinessObjectFactory parent = null)
		{
			var sentry = parent?.ThreadSentry ?? ThreadSentryProvider.GetThreadSentry(false);
			this.IndexingEnabled = true;
			rowIndexManager = new RowIndexManager();
			this.DatabaseName = databaseName;
			connectionInfo = new ZSqlConnectionInfo(object.ReferenceEquals(connection, Db.Connection) ? null : connection, databaseName);
			dbOnlyQueryCache = new DBQueryCacheManager(sentry);
			narrowDbOnlyQueryCache = new DBQueryCacheManager(sentry);
			QueryCache = new QueryCacheManager(sentry);
			data = new DataSet();
			data.Locale = null; // CA1306
			dataAccessor = new ZSqlDataAccessor(data, connectionInfo);
			fetcher = LoadedFetchHintRecordingEnabled ? new FetchHintManagerWithFetchHints(this) : new FetchHintManager(this);
			SetupTraceData();
			ConstructionTime = DateTime.UtcNow; // This is for caching, not for synchronisation
		}

		public DateTime ConstructionTime
		{
			get;
			private set;
		}

		readonly ZSqlConnectionInfo connectionInfo;
		ZSqlDataAccessor dataAccessor;
		internal readonly DBQueryCacheManager dbOnlyQueryCache;
		internal readonly DBQueryCacheManager narrowDbOnlyQueryCache;
		internal readonly FetchHintManager fetcher;

		#endregion

		#region Name / Debug Info

		internal ZString NameForDebugging;

#if DEBUG
		[SuppressThreadStaticFieldMessage]
		static long _NextInstance = 1;
		internal long _Instance;
#endif
		[Conditional("DEBUG")]
		void SetupTraceData()
		{
#if DEBUG
			_Instance = Interlocked.Increment(ref _NextInstance);

			if (CreateStackTraceOnConstruction)
			{
				ConstructionStackTrace = new StackTrace();
			}

#endif
		}

#if DEBUG
		[ThreadStatic]
		internal static bool CreateStackTraceOnConstruction;

		internal StackTrace ConstructionStackTrace;
#endif

		public int UberFactoryTimeOutPeriodInSeconds { get; set; } = ObjectFactory.Get<IEntityFrameworkSettings>().UberFactoryTimeoutPeriod;

		public ZDateTime TransactionStartedTime
		{
			get { return transactionStartedTime; }
		}
		ZDateTime transactionStartedTime;

		public ZDateTime TransactionStartedTimeUtc
		{
			get { return transactionStartedTimeUtc; }
		}
		ZDateTime transactionStartedTimeUtc;

		public static bool LoadedFetchHintRecordingEnabled { get; set; }

		#endregion

		#region New Rows

		public DataRow New(string tableName)
		{
			DataTable table = GetTable(tableName);
			DataRow row = table.NewRow();
			return row;
		}

		#endregion

		#region DataView management

		public int MaximumRowsBeforeUsingIndex
		{
			get { return rowIndexManager.MaximumRowsBeforeUsingIndex; }
			set { rowIndexManager.MaximumRowsBeforeUsingIndex = value; }
		}

		public bool IndexingEnabled { get; set; }

		#endregion

		#region Loading / Reloading Rows

		public DataRow LoadFromPK(string tableName, ZGuid pK)
		{
			return LoadFromPK(tableName, pK, false);
		}

		public bool IsDbOnlyQueryCached(string tableName, ZQuery query)
		{
			return dbOnlyQueryCache.GetCachedValue(tableName, query) != null;
		}

		internal bool IsCached(string tableName, ZQuery filter) => QueryCache.IsCached(tableName, filter);

		internal DataRow[] GetCachedDbOnlyQueryResult(string tableName, ZQuery query)
		{
			return dbOnlyQueryCache.GetCachedValue(tableName, query);
		}

		internal DataRow[] GetCachedNarrowDbOnlyQueryResult(string tableName, ZQuery query)
		{
			return narrowDbOnlyQueryCache.GetCachedValue(tableName, query);
		}

		public DataRow[] Load(string tableName, ZQuery filter)
		{
			DataRow[] result;
			if (filter.IsNoResultQuery)
			{
				result = Array.Empty<DataRow>();
			}
			else if (filter.IsDBOnlyQuery)
			{
				result = dbOnlyQueryCache.GetCachedValue(tableName, filter);
				if (result == null || filter.ReLoadExistingRows)
				{
					DataRowLoadResponse response = LoadPersistentRowsIntoDataSet(filter.GetDataQuery(connectionInfo, tableName));

					result = response.AllRows;
					AddRelatedTableHints(tableName, result);
					dbOnlyQueryCache.Store(result, tableName, filter);

					if (result != null && result.Length > 0 && (!filter.MaximumRows.HasValue || filter.MaximumRows.Value > 1))
					{
						ZQuery[] compositeParts = filter.GetCompositeParts();
						if (!compositeParts.Any(part => part.IsPrimaryKeyQuery) && (compositeParts.Length > 1 || filter.MaximumRows.HasValue))
						{
							foreach (ZQuery compositePart in compositeParts)
							{
								ZQuery queryToStore = compositePart;
								if (compositePart.MaximumRows.HasValue)
								{
									queryToStore = compositePart.DeepClone();
									queryToStore.MaximumRows = null;
								}

								DataRow[] previousResult = narrowDbOnlyQueryCache.GetCachedValue(tableName, queryToStore);
								if (previousResult == null || previousResult.Length < result.Length || filter.ReLoadExistingRows)
								{
									narrowDbOnlyQueryCache.Store(result, tableName, queryToStore);
								}
							}
						}
					}
				}
			}
			else
			{
				result = SelectCombiningLocalAndDatabaseData(tableName, filter);
			}

			return result;
		}

		public DataRow Reload(string tableName, ZGuid pK)
		{
			return LoadFromPK(tableName, pK, true);
		}

		public DataRow LoadFromPK(string tableName, ZGuid pK, bool reloadExistingRows)
		{
			DataRow result = null;
			if (!reloadExistingRows)
			{
				result = GetRow(tableName, pK);
				if (result == null)
				{
					fetcher.FetchTable(tableName);
					result = GetRow(tableName, pK);
				}
			}

			if (result == null)
			{
				if (reloadExistingRows)
				{
					fetcher.FetchTable(tableName);
				}
				SchemaGuidColumn pKSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName);

				ZQuery sQLFilter = new ZQuery(pKSchemaColumn, pK);
				if (tableName == "StmALog")
				{
					throw new InvalidOperationException("StmaLog has no index and so cannot be reloaded by its PK");
				}
				if (tableName == "ViewGenericJob")
				{
					ErrorReporter.ReportOnce("ReloadViewGenericJob", "ViewGenericJob has to be reloaded by using its PK and ParentTableCode. You should use the extension method for BusinessObjectFactory in MasterFiles. i.e) Factory.LoadGenericJobFromPKAndTableCode(Job) or Factory.LoadGenericJobFromPKAndTableCode(pk, tableCode)");
				}
				sQLFilter.ReLoadExistingRows = reloadExistingRows;

				DataRow[] rows = Load(tableName, sQLFilter);

				if (rows.Length == 1)
				{
					result = rows[0];
				}
				else if (rows.Length > 1)
				{
					throw new Exception(pKSchemaColumn.Name + " '" + pK.ToString() + "' is not unique in table '" + tableName + "'!");
				}
			}
			return result;
		}

		public DataRow LoadFromNaturalKey(string tableName, SchemaColumn field, IZType naturalKey, bool reloadExistingRows)
		{
			CheckKeyIsUnique(tableName, field.Name);
			DataTable table = GetTable(tableName);
			DataRow result = null;
			if (!naturalKey.IsEmpty)
			{
				ZQuery sQLFilter = new ZQuery(field, naturalKey);
				if (!reloadExistingRows)
				{
					DataRow[] dataRows = GetResultUsingDataView(table, sQLFilter) ?? table.Select(sQLFilter.LiteralTextADO);
					if (dataRows.Length == 1)
					{
						result = dataRows[0];
					}
				}

				if (result == null)
				{
					DataRow[] rows = Load(tableName, sQLFilter);

					if (rows.Length == 1)
					{
						result = rows[0];
					}
					else if (rows.Length > 1)
					{
						result = rows[0];

						SchemaGuidColumn pKSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName);
						var pks = rows.Select(x => x[pKSchemaColumn.Name].ToString()).ToArray();
						var equal = pks?.All(x => x == pks[0]);
						ErrorReporter.ReportOnce(
							string.Format("NaturalKeyIsNotUnique[{0}.{1}]", tableName, field.Name),
							string.Format("{0} '{1}' is not unique in table '{2}'!Rows count : {3}  First row returned. Same PK: {4}.",
								field.Name, naturalKey, tableName, rows.Length.ToString(), equal.ToString()));
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Only available for SQL queries
		/// </summary>
		internal NonPersistentLoaderResponse LoadDynamicNonPersistent(string query, ZSqlParameterCollection @params)
		{
			return LoadDynamicNonPersistent(new ZNonPersistentDataQuery(query, @params));
		}

		/// <summary>
		/// Only available for SQL queries
		/// </summary>
		internal NonPersistentLoaderResponse LoadDynamicNonPersistent(ZNonPersistentDataQuery dataQuery, bool logNonPersistentTableHitCount = false)
		{
			return dataAccessor.LoadNonPersistentRows(dataQuery, logNonPersistentTableHitCount: logNonPersistentTableHitCount);
		}

		public void SeedQueryCache(string tableName, ZQuery query)
		{
			if (query.IsTopNQuery)
			{
				throw new NotSupportedException("Query Cache may not be seeded with TopN queries");
			}
			if (!query.OrderBy.IsEmpty)
			{
				throw new NotSupportedException("Query Cache may not be seeded with OrderBy queries");
			}

			QueryCache.Store(tableName, query);
		}

		public void ExecuteAllFetchHints()
		{
			fetcher.FetchAllTables();
		}

		public void ExecuteFetchHintsForTable(string tableName)
		{
			fetcher.FetchTable(tableName);
		}

		public bool AddFetchHint(IFetchHint fetchHint)
		{
			var isNeeded = fetchHint.IsNeeded(new QueryHistoryProvider(this));

			if (isNeeded)
			{
				fetcher.AddFetchHint(fetchHint);
			}
			return isNeeded;
		}

		public DataRow GetRow(string tableName, ZGuid pK)
		{
			DataRow result = null;
			if (pK.IsValid)
			{
				DataTable targetTable = GetTable(tableName, false);
				if (targetTable != null)
				{
					result = targetTable.Rows.Find(pK.ToGuid());
				}
			}

			return result;
		}

		public bool HasLoadedRowCompletely(string tableName, ZGuid pK)
		{
			bool loaded = true;
			DataRow row = GetRow(tableName, pK);
			if (row == null)
			{
				loaded = false;
			}
			else
			{
				foreach (var item in row.ItemArray)
				{
					if (LazyLoading.LoadRequired(item))
					{
						loaded = false;
						break;
					}
				}
			}
			return loaded;
		}

		#region Loading Blob Fields

		internal Stream GetBinaryFieldStream(DataRow row, string tableName, string columnName)
		{
			return dataAccessor.GetBinaryFieldStream(row, tableName, columnName);
		}
		internal Stream GetBinaryFieldStreamRawFromDb(DataRow row, string tableName, string columnName)
		{
			return dataAccessor.GetBinaryFieldStreamRawFromDb(row, tableName, columnName);
		}

		internal TextReader GetTextFieldReader(DataRow row, string tableName, string columnName, bool closeReaderBetweenReads)
		{
			return dataAccessor.GetTextFieldReader(row, tableName, columnName, closeReaderBetweenReads);
		}

		public void LoadBlobField(DataRow row, SchemaColumn schemaColumn)
		{
			string tableName = schemaColumn.TableName;

			ExecuteFetchHintsForTable(tableName);

			if (LazyLoading.LoadRequired(row[schemaColumn.Name]))
			{
				dataAccessor.LoadBlobField(row, schemaColumn);
			}
		}

		void LoadBlobFieldsForTable(string tableName, IEnumerable<SchemaColumn> columns)
		{
			ZDataRowDictionary rowsToLoad = new ZDataRowDictionary();

			foreach (DataRow row in GetTable(tableName).Rows)
			{
				if (row.RowState != DataRowState.Added &&
						row.RowState != DataRowState.Deleted)
				{
					foreach (var column in columns)
					{
						if (LazyLoading.LoadRequired(row[column.Name]))
						{
							rowsToLoad.Add(row[0], row);
							break;
						}
					}
				}
			}

			LoadBlobFieldsForRows(tableName, rowsToLoad, columns);
		}

		void LoadBlobFieldsForRows(string tableName, ZDataRowDictionary rowDictionary, IEnumerable<SchemaColumn> columns)
		{
			if (rowDictionary.Count > 0)
			{
				dataAccessor.LoadBlobFieldsForTable(tableName, rowDictionary, columns);
			}
		}

		#endregion

		#region Implementation

		public static void ResetCacheAfterDbUpgrade()
		{
			lock (UberFactoryMutex)
			{
				uberFactory = null;
			}
		}

		public static void ClearSpecificTableFromUberFactory(string tableName)
		{
			if (IsCachedTable(tableName))
			{
				lock (UberFactoryMutex)
				{
					var uberRowFactory = UberFactoryRememberToLock.RowFactory;
					if (uberRowFactory.data.Tables.Contains(tableName))
					{
						uberRowFactory.data.Tables.Remove(tableName);
					}
					uberFactory.RowFactory.ClearQueryCache(tableName);
				}
			}

			PersistentFactoryCacheManager.Instance.ClearAllQueryCaches(new[] { tableName });
		}

		static readonly object UberFactoryMutex = new object();

		[ThreadSafe(WTG.StaticAnalysis.Annotation.ThreadSafeAttribute.Mechanism.Lock)]
		internal static BusinessObjectFactory uberFactory;

		internal static BusinessObjectFactory UberFactoryRememberToLock
		{
			get
			{
				var result = uberFactory;
				if (result == null)
				{
					lock (UberFactoryMutex)
					{
						result = uberFactory;
						if (result == null)
						{
							result = new ReadOnlyBusinessObjectFactory(false) { NameForDebugging = "Client side Cache" };
							result.RowFactory.IsUberFactory = true;
							result.SuspendValidation();
							uberFactory = result;
						}
					}
				}
				return result;
			}
		}

		internal bool IsUberFactory
		{
			get;
			private set;
		}

		internal void FetchRowsIntoDataSet(string tableName, ZQuery filter)
		{
			FetchRowsIntoDataSet(new TableNameQuery[] { new TableNameQuery(tableName, filter) });
		}

		internal void FetchRowsIntoDataSet(IEnumerable<TableNameQuery> tableNameFilters)
		{
			foreach (TableNameQuery filter in tableNameFilters)
			{
				if (!QueryCache.IsCached(filter.TableName, filter.Query) && !filter.Query.IsNoResultQuery)
				{
#if DEBUG
					BreakOnRegisteredTableAccess(filter.TableName, filter.Query);
					LogTable(filter.TableName);
#endif
					var dataQuery = new ZDataQuery(connectionInfo, filter.TableName, filter.Query);

					if (!PreLoadRowsFromUberFactory(dataQuery))
					{
						foreach (DataRowLoadResponse response in dataAccessor.LoadPersistentRowsIntoDataSet(new[] { dataQuery }))
						{
							if (!response.Query.IsPrimaryKeyQuery || response.AllRows.Length == 0)
							{
								if (response.Query.IsDBOnlyQuery)
								{
									dbOnlyQueryCache.Store(response.AllRows, response.TableName, response.Query);
								}
								QueryCache.Store(response.TableName, response.Query);
							}
							AddRelatedTableHints(response.TableName, response.AllRows);
						}
					}
				}
			}
		}

		internal static bool IsCachedTable(string tableName)
		{
			return CachedTables.Contains(tableName);
		}

#if DEBUG

		#region Log Load Tables

		internal IList<string> LogLoadTables(Action action, IList<string> exceptTables)
		{
			var result = loadedTables = new List<string>();
			ignoreTables = exceptTables;
			try
			{
				action();
			}
			finally
			{
				loadedTables = null;
				ignoreTables = null;
			}
			return result;
		}

		void LogTable(string tableName)
		{
			if (loadedTables != null && !string.IsNullOrEmpty(tableName) &&
				(ignoreTables == null || !ignoreTables.Contains(tableName)) &&
				!loadedTables.Contains(tableName))
			{
				loadedTables.Add(tableName);
			}
		}

		IList<string> loadedTables, ignoreTables;

		#endregion

		public static IDisposable RemoveCachedTablesTemporarily(params string[] tables)
		{
			var currentTables = CachedTables.ToHashSet();
			foreach (var table in tables)
			{
				currentTables.Remove(table);
			}

			return SetCachedTables(currentTables.ToArray());
		}

		public static IDisposable SetCachedTables(params string[] tables)
		{
			cachedTables.Value = tables.ToImmutableHashSet();
			return new DisposableAction(cachedTables.ResetValue);
		}
#endif
		static ImmutableHashSet<string> CachedTables => cachedTables.Value;
		static readonly LazyOverridable<ImmutableHashSet<string>> cachedTables = new LazyOverridable<ImmutableHashSet<string>>(
			() => ObjectFactory.Get<IEntityFrameworkSettings>().CachedTables.Split(',').ToImmutableHashSet());

		bool PreLoadRowsFromUberFactory(ZDataQuery query)
		{
			if (!IsUberFactory)
			{
				if (IsCachedTable(query.TableName) && !query.Query.IsDBOnlyQuery && !query.Query.ReLoadExistingRows)
				{
					var relatedTableHints = GetRelatedTableFetchHintCreators(query.TableName);
					// this stops the current factory from reacting to the query
					var table = GetTable(query.TableName);

					lock (UberFactoryMutex)
					{
						var uberFactory = UberFactoryRememberToLock;
						var originalValue = uberFactory.RowFactory.dataAccessor.CurrentDatabaseInfo;

						foreach (var dataRow in LoadRowsFromUberFactoryWithQueryCollectionIfNeeded(query, uberFactory))
						{
							// now all that needs to happen is to copy the row from one rowFactory to another (if they don't already exist)
							var pk = (Guid)dataRow[0];
							if (GetRow(query.TableName, new ZGuid(pk)) == null)
							{
								if (!table.HasDeletedRowWithPK(pk))
								{
									var theRow = table.NewRow();
									theRow.ItemArray = dataRow.ItemArray;
									table.Rows.Add(theRow);
									theRow.AcceptChanges();
									AddRelatedTableHints(relatedTableHints, theRow as IColumnIndexer);
								}
							}
						}

						var diff = uberFactory.RowFactory.dataAccessor.CurrentDatabaseInfo - originalValue;
						dataAccessor.IncreaseDatabaseLoadCount(diff);
						// don't do this too early otherwise the current RowFactory will not hit the DB if this step fails
						QueryCache.Store(query.TableName, query.Query);
					}
					return true;
				}
			}
			return false;
		}

		DataRow[] LoadRowsFromUberFactoryWithQueryCollectionIfNeeded(ZDataQuery query, BusinessObjectFactory uberFactory)
		{
			var tableName = query.TableName;
#if DEBUG
			using (TableNamesNeedingHitQueryCollection.Contains(tableName)
				? uberFactory.EnableTableHitQueryCollection(new[] { tableName })
				: null)
#endif
			{
				return uberFactory.RowFactory.Load(tableName, query.Query);
			}
		}

		internal void IncreaseDatabaseLoadCount(string tableName)
		{
			dataAccessor.IncreaseAccessCount(tableName, query: null);
		}

		protected virtual DataRowLoadResponse LoadPersistentRowsIntoDataSet(ZDataQuery query)
		{
			return dataAccessor.LoadPersistentRowsIntoDataSet(new ZDataQuery[] { query })[0];
		}

		bool FilterSetAlreadyMet(string tableName, ZQuery filter1)
		{
			bool filterAlreadyMet = filter1.FetchOnlyFromLocalCache;
			if (!filterAlreadyMet && !filter1.ReLoadExistingRows)
			{
				foreach (ZQuery compositePart in filter1.GetCompositeParts())
				{
					filterAlreadyMet = FilterIsPKAndInRow(tableName, compositePart) || QueryCache.IsCached(tableName, compositePart);
					if (filterAlreadyMet)
					{
						break;
					}
				}
				if (!filterAlreadyMet)
				{
					ZQuery[] orFilters = filter1.GetOrParts();
					if (orFilters.Length > 0)
					{
						filterAlreadyMet = true;
						foreach (ZQuery filter in orFilters)
						{
							if (!FilterIsPKAndInRow(tableName, filter) && !QueryCache.IsCached(tableName, filter))
							{
								filterAlreadyMet = false;
								break;
							}
						}
					}
				}
			}
			return filterAlreadyMet;
		}

		bool FilterIsPKAndInRow(string tableName, ZQuery filter)
		{
			bool result = false;
			if (filter.FilterParts.Count == 1)
			{
				if (filter.FilterParts.GetFilterPart(0) is ZSqlParameter parameter && parameter.ComparisonOperator == SQLComparisonOperator.Equal)
				{
					if (parameter.SchemaColumn.IsPKColumn && !filter.FetchOnlyFromLocalCache)
					{
						if (parameter.Value is Guid || parameter.Value is ZGuid)
						{
							ZGuid pK = new ZGuid((Guid)parameter.Value);
							result = GetRow(tableName, pK) != null;
						}
					}
				}
			}
			return result;
		}

#if DEBUG
		public IDisposable EnableTableHitQueryCollection(string[] tableNames)
		{
			return dataAccessor.EnableTableHitQueryCollection(tableNames);
		}

		public IEnumerable<string> TableNamesNeedingHitQueryCollection => dataAccessor.TableNamesNeedingHitQueryCollection;

		internal IDisposable SuspendTableHitCounterForFetchHintsIfNeeded()
		{
			return IsFetchHintsProcessingWithoutTableHitCounterEnabled ? dataAccessor.SuspendTableHitCounter() : null;
		}

		bool IsFetchHintsProcessingWithoutTableHitCounterEnabled
		{
			get { return fetchHintsProcessingWithoutTableHitCounterEnablerIndex > 0; }
		}

		int fetchHintsProcessingWithoutTableHitCounterEnablerIndex;

		internal IDisposable EnableFetchHintsProcessingWithoutTableHitCounter()
		{
			return new FetchHintsProcessingWithoutTableHitCounterEnabler(this);
		}

		sealed class FetchHintsProcessingWithoutTableHitCounterEnabler : IDisposable
		{
			public FetchHintsProcessingWithoutTableHitCounterEnabler(RowFactory factory)
			{
				this.factory = factory;
				this.factory.fetchHintsProcessingWithoutTableHitCounterEnablerIndex++;
			}
			readonly RowFactory factory;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				factory.fetchHintsProcessingWithoutTableHitCounterEnablerIndex--;
			}

			#endregion
		}

		void BreakOnRegisteredTableAccess(string tableName, ZQuery filter)
		{
			if (
				tableName == NUnit.Framework.TestingState.TableToBreakOn
				&& NUnit.Framework.TestingState.BreakOnPerformanceIssues
				&& filter.LiteralTextADO.IndexOf(" in [") == -1)
			{
				System.Diagnostics.Debugger.Break();
			}
		}
#endif

		DataRow[] LoadFromDataTableIfPossible(string tableName, ZQuery filter)
		{
			if (filter.ReLoadExistingRows && !filter.FetchOnlyFromLocalCache)
			{
				return null;
			}

			DataRow[] result = null;
			DataRow[] resultFromFirstSelect = null;

			if (filter.IsTopNQuery && filter.OrderBy.IsEmpty)
			{
				result = Select(tableName, filter);
				resultFromFirstSelect = result;

				if (result.Length < filter.MaximumRows && !filter.FetchOnlyFromLocalCache)
				{
					result = null;  // clear the result - may be reinstated below
				}
			}

			if (result == null && FilterSetAlreadyMet(tableName, filter))
			{
				result = resultFromFirstSelect ?? Select(tableName, filter);
			}

			return result;
		}

		internal bool shouldAddDiagnosisForFactoryQueryCache;

		[ThreadStatic]
		static bool diagnosisForFactoryQueryCacheAlreadyAddedOnce;

		public IDisposable AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany()
		{
			shouldAddDiagnosisForFactoryQueryCache = true;
			return new DisposableAction(() => { shouldAddDiagnosisForFactoryQueryCache = false; });
		}

		void ReportErrorIfLoadFromDataTableGetBadResult(DataRow[] result, string tableName, ZQuery filter, int position)
		{
			if (!diagnosisForFactoryQueryCacheAlreadyAddedOnce && shouldAddDiagnosisForFactoryQueryCache && result != null && result.Length == 0)
			{
				diagnosisForFactoryQueryCacheAlreadyAddedOnce = true;
				ErrorReporter.ReportOnce($"LoadFromDataTableIfPossibleReturnBadResult{position}", GetLoadFromPKErrorMessage(result, tableName, filter));
			}
		}

		string GetLoadFromPKErrorMessage(DataRow[] result, string tableName, ZQuery filter)
		{
			var sb = new StringBuilder();
			sb.AppendLine((NoResString)"load Env.CurrentBranch/Env.CurrentCompany return unexpected result, result should be null or a non-empty datarow collection.");
			sb.AppendLine($"filterIsPKAndInRow: {FilterIsPKAndInRow(tableName, filter)}");
			sb.AppendLine($"queryCacheIsCached: {QueryCache.IsCached(tableName, filter)}");
			lock(UberFactoryMutex)
			{
				sb.AppendLine($"filterIsPKAndInRow_UberFactory: {UberFactoryRememberToLock.RowFactory.FilterIsPKAndInRow(tableName, filter)}");
				sb.AppendLine($"queryCacheIsCached_UberFactory: {UberFactoryRememberToLock.RowFactory.QueryCache.IsCached(tableName, filter)}");
			}
			sb.AppendLine($"filter: {filter.LiteralTextSql}");
			return sb.ToString();
		}

		DataRow[] SelectCombiningLocalAndDatabaseData(string tableName, ZQuery filter)
		{
			var blobFilters = filter.BlobFilters.Where(x => x.TableName == tableName).ToArray();
			if (blobFilters.Length > 0)
			{
				LoadBlobFieldsForTable(tableName, blobFilters);
			}

			DataRow[] result = LoadFromDataTableIfPossible(tableName, filter);

			ReportErrorIfLoadFromDataTableGetBadResult(result, tableName, filter, 0);

			if (!filter.FetchOnlyFromLocalCache)
			{
				if (result == null)
				{
					if (fetcher.HasFetchHints(tableName))
					{
						fetcher.FetchTable(tableName);
						if (blobFilters.Length > 0)
						{
							LoadBlobFieldsForTable(tableName, blobFilters);
						}
						result = LoadFromDataTableIfPossible(tableName, filter);
						ReportErrorIfLoadFromDataTableGetBadResult(result, tableName, filter, 1);
					}
				}

				if (result == null)
				{
					FetchRowsIntoDataSet(tableName, filter);
					if (blobFilters.Length > 0)
					{
						LoadBlobFieldsForTable(tableName, blobFilters);
					}

					result = Select(tableName, filter);
				}
			}

			return result ?? Array.Empty<DataRow>();
		}

		internal DataRow[] Select(string tableName, ZQuery filter)
		{
			if (filter.IsNoResultQuery)
			{
				return Array.Empty<DataRow>();
			}

			DataTable table = GetTable(tableName);

			IEnumerable<ZSqlParameter> suspendedComplicatedLikeOperators = null;
			if (ContainsComplicatedLikeOperator(filter))
			{
				/*
				 * ADO.Net supports only wildacrds % and * (which are same) at the start or end of template. So:
				 *   column_name like 'a%b' -- will throw exception (unsupported position of wildcard %);
				 *   column_name like 'a_b' -- will return incorrect result (will ignore wildcard _);
				 *   column_name like '*a%' -- will return incorrect result either (will process * as a wildcard);
				 */

				if (filter.ContainsOrOperator)
				{
					// table.Select won't work with query that Or-joins with complicated-LIKE operator, so we breakup and process select on each part
					var partResult = BreakupAndSelectEachPart(tableName, filter);
					if (partResult.Any())
					{
						if (filter.MaximumRows != null && filter.MaximumRows.Value > 0)
						{
							partResult = partResult.Take(filter.MaximumRows.Value);
						}
						return partResult.ToArray();
					}
				}
				else
				{
					SuspendComplicatedLikeOperators(filter, out suspendedComplicatedLikeOperators);
				}
			}

			DataRow[] result = DoSelectOnNonSuspendedParts(table, filter);

			if (suspendedComplicatedLikeOperators != null)
			{
				result = ApplySuspendedComplicatedLikeOperatorsWithAndJoin(result, suspendedComplicatedLikeOperators);
			}

			if (filter.MaximumRows != null && result != null && result.Length > filter.MaximumRows.Value)
			{
				result = ZDataUtils.TopN(result, filter.MaximumRows.Value);
			}

			return result;
		}

		DataRow[] ApplySuspendedComplicatedLikeOperatorsWithAndJoin(DataRow[] result, IEnumerable<ZSqlParameter> suspendedComplicatedLikeOperators)
		{
			try
			{
				result = ProcessComplicatedLikeOperatorsWithAndJoin(suspendedComplicatedLikeOperators, result);
			}
			finally
			{
				foreach (ZSqlParameter sqlParameter in suspendedComplicatedLikeOperators)
				{
					sqlParameter.Resume();
				}
			}

			return result;
		}

		void SuspendComplicatedLikeOperators(ZQuery filter, out IEnumerable<ZSqlParameter> suspendedComplicatedLikeOperators)
		{
			suspendedComplicatedLikeOperators = GetComplicatedLikeOperators(filter);
			foreach (ZSqlParameter sqlParameter in suspendedComplicatedLikeOperators)
			{
				sqlParameter.Suspend();
			}
		}

		IEnumerable<DataRow> BreakupAndSelectEachPart(string tableName, ZQuery filter)
		{
			IEnumerable<DataRow> result = new List<DataRow>();
			var orParts = filter.GetOrParts();
			if (orParts.Length > 1)
			{
				var orResultCombined = new List<DataRow>();
				foreach (var orPart in orParts)
				{
					var orPartResult = Select(tableName, orPart);
					orResultCombined.AddRange(orPartResult);
				}
				result = orResultCombined.Distinct();
			}
			else
			{
				var andParts = filter.GetAndParts();
				if (andParts.Length > 1)
				{
					IEnumerable<DataRow> andResultCombined = null;
					foreach (var andPart in andParts)
					{
						var andPartResult = Select(tableName, andPart);
						if (andResultCombined == null)
						{
							andResultCombined = andPartResult;
						}
						else
						{
							andResultCombined = andResultCombined.Intersect(andPartResult);
						}

						if (!andResultCombined.Any())
						{
							// And-join with an empty set will be empty anyway
							break;
						}
					}

					result = andResultCombined;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		DataRow[] DoSelectOnNonSuspendedParts(DataTable table, ZQuery filter)
		{
			var orderBy = filter.OrderBy;

			DataRow[] result = GetResultUsingDataView(table, filter);
			if (result == null)
			{
				try
				{
					result = table.Select(filter.LiteralTextADO, orderBy, ZDataUtils.CurrentRowsNoDeletedFilter);
				}
				catch (InvalidExpressionException ex)
				{
					string msg = string.Format(CultureInfo.InvariantCulture, (NoResString)"Error selecting data from table {0} with following filter\r\n{1}", table.TableName, filter.LiteralTextADO);
					throw new ApplicationException(msg, ex);
				}
			}

			if (result != null)
			{
				var orderByColumn = table.Columns[orderBy];
				if (orderByColumn != null && orderByColumn.DataType == typeof(Guid))
				{
					try
					{
						result = result.OrderBy(x => x[orderBy], new GuidAsSqlGuidComparer()).ToArray();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						//out of an abundance of caution, since this would be really bad if it threw
						ErrorReporter.ReportOnce("GuidAsSqlGuidComparerException", string.Format("orderBy = {0}", orderBy), e);
					}
				}
			}

			return result;
		}

		// The SQL query will order Guid with a different logic than the Guid implementation
		// see this link for details:  https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/comparing-guid-and-uniqueidentifier-values 
		internal class GuidAsSqlGuidComparer : IComparer<object>
		{
			public int Compare(object x, object y)
			{
				//NULL values are treated as the lowest possible values.
				// https://learn.microsoft.com/en-us/sql/t-sql/queries/select-order-by-clause-transact-sql?view=sql-server-ver15
				if (x is DBNull)
				{
					if (y is DBNull)
					{
						return 0;
					}
					return -1;
				}
				else if (y is DBNull)
				{
					return 1;
				}

				var sqlX = new SqlGuid((Guid)x);
				var sqlY = new SqlGuid((Guid)y);

				return sqlX.CompareTo(sqlY);
			}
		}

		DataRow[] GetResultUsingDataView(DataTable table, ZQuery filter)
		{
			try
			{
				return IndexingEnabled ? rowIndexManager.GetRows(table, filter, this) : null;
			}
			catch (InvalidExpressionException ex)
			{
				ErrorReporter.ReportOnce("GetResultUsingDataView_GetResultUsingDataView",
					string.Format("Expression evaluation error on filtering DataRows using RowIndexManager on table '{0}' with following filter:\r\n{1}\r\nError message: {2}",
						table != null ? table.TableName : string.Empty,
						filter.LiteralTextSqlFormatted, // LiteralTextADO and LiteralTextADOFormatted will not have parts with LIKE operator
						ex.Message),
					ex);

				return null; // RowFactory will use other methods to find result, i.e. Table.Select(filter.LiteralTextADO)
			}
		}

		readonly RowIndexManager rowIndexManager;

		public LRUCache<string, RowFilterComparer> RowFilterComparerCache
		{
			get
			{
				if (cache == null)
				{
					cache = new LRUCache<string, RowFilterComparer>(RowFilterCacheLimit);
				}
				return cache;
			}
		}

		LRUCache<string, RowFilterComparer> cache;
		const int RowFilterCacheLimit = 28;

		#region Select with Like Comparison

		/// <summary>
		/// Checks if filter has LIKE operator with wildcard '_' anywhere or wildcard '%' inside template or symbol '*' anywhere.
		/// </summary>
		/// <param name="filter">Filter to test.</param>
		/// <returns>True if filter has LIKE operator with wildcard '_' anywhere or wildcard '%' inside template.</returns>
		/// <remarks>Really should not be used on queries with OR operator. And even more important - not for queries with subqueries.</remarks>
		bool ContainsComplicatedLikeOperator(IFilterPart filter)
		{
			if (filter is ZSqlParameter sqlParameter && IsComplicatedLikeOperator(sqlParameter))
			{
				return true;
			}

			if (filter is IFilterPartsProvider filterPartProvider)
			{
				foreach (var part in filterPartProvider.FilterParts)
				{
					if (ContainsComplicatedLikeOperator(part))
					{
						return true;
					}
				}
			}

			return false;
		}

		bool IsComplicatedLikeOperator(ZSqlParameter sqlParameter)
		{
			if (sqlParameter.ComparisonOperator is LikeComparisonOperator)
			{
				var template = sqlParameter.Value.ToString().Replace("[_]", "").Replace("[%]", "");
				if (template.IndexOf('_') != -1 || template.IndexOf('*') != -1)
				{
					return true;
				}
				if (template.Length > 0)
				{
					var wildcardPos = template.IndexOf('%', 1);
					if (wildcardPos > 0 && wildcardPos < template.Length - 1)
					{
						return true;
					}
				}
			}

			return false;
		}

		IEnumerable<ZSqlParameter> GetComplicatedLikeOperators(IFilterPart filter)
		{
			List<ZSqlParameter> result = new List<ZSqlParameter>();

			if (filter is ZSqlParameter sqlParameter && IsComplicatedLikeOperator(sqlParameter))
			{
				result.Add(sqlParameter);
			}

			if (filter is IFilterPartsProvider filterPartsProvider)
			{
				foreach (var part in filterPartsProvider.FilterParts)
				{
					result.AddRange(GetComplicatedLikeOperators(part));
				}
			}

			return result;
		}

		DataRow[] ProcessComplicatedLikeOperatorsWithAndJoin(IEnumerable<ZSqlParameter> complicatedLikeOperators, DataRow[] rows)
		{
			if (rows == null || rows.Length == 0)
			{
				return rows;
			}

			IEnumerable<DataRow> result = null;
			foreach (ZSqlParameter sqlParameter in complicatedLikeOperators)
			{
				var rowResult = ProcessComplicatedLikeOperator(sqlParameter, rows);
				if (result == null)
				{
					result = rowResult;
				}
				else
				{
					result = result.Intersect(rowResult);
				}

				if (!result.Any())
				{
					// And-join with an empty set will be empty anyway
					break;
				}
			}

			return result?.ToArray() ?? Array.Empty<DataRow>();
		}

		IEnumerable<DataRow> ProcessComplicatedLikeOperator(ZSqlParameter sqlParameter, DataRow[] rows)
		{
			StringBuilder sb = new StringBuilder(sqlParameter.Value.ToString());

			sb.Replace(BraketedOneCharWildcard, TemporarySubstitute1);
			sb.Replace(BraketedMultiCharWildcard, TemporarySubstitute2);

			sb.Replace("[[]", "[");
			sb.Replace("[]]", "]");

			sb.Replace(Escape, Escape + Escape);
			sb.Replace(".", Escape + ".");
			sb.Replace("*", Escape + "*");
			sb.Replace("(", Escape + "(");
			sb.Replace(")", Escape + ")");
			sb.Replace("[", Escape + "[");
			sb.Replace("]", Escape + "]");
			sb.Replace("^", Escape + "^");
			sb.Replace("$", Escape + "$");

			sb.Replace(OneCharWildcard, ".");
			sb.Replace(MultiCharWildcard, ".*");

			sb.Replace(TemporarySubstitute1, EscapedOneCharWildcard);
			sb.Replace(TemporarySubstitute2, EscapedMultiCharWildcard);

			sb.Insert(0, '^');
			sb.Append('$');

			Regex regex = new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);

			return rows.Where(row => regex.IsMatch(row[sqlParameter.SchemaColumn.Name].ToString()));
		}

		const string OneCharWildcard = "_";
		const string MultiCharWildcard = "%";
		const string Escape = @"\";
		const string BraketedOneCharWildcard = "[" + OneCharWildcard + "]";
		const string BraketedMultiCharWildcard = "[" + MultiCharWildcard + "]";
		const string EscapedOneCharWildcard = Escape + OneCharWildcard;
		const string EscapedMultiCharWildcard = Escape + MultiCharWildcard;
		const string TemporarySubstitute1 = "<<substitute1/>>"; // Internal constant
		const string TemporarySubstitute2 = "<<substitute2/>>"; // Internal constant

		#endregion

		#endregion

		public int GetDataRowCount()
		{
			int result = 0;
			foreach (DataTable datatable in data.Tables)
			{
				result += datatable.Rows.Count;
			}
			return result;
		}

		#endregion

		#region Save

		public void Save()
		{
			SaveTogether(this);
		}

		public void ClearQueryCache()
		{
#if DEBUG
			if (!DisableQueryCacheReset)
#endif
			{
				queryCacheClearCount++;
				dbOnlyQueryCache.Clear();
				QueryCache.Clear();
			}
		}

		internal bool ShouldPerformFullQueryCacheCleanOnSave { get; set; }

		public void ClearQueryCache(string tableName)
		{
#if DEBUG
			if (!DisableQueryCacheReset)
#endif
			{
				if (ShouldPerformFullQueryCacheCleanOnSave)
				{
					ClearQueryCache();
				}
				else
				{
					dbOnlyQueryCache.Clear(tableName);
					QueryCache.Clear(tableName);
				}
			}
		}

		public void ClearViewsQueryCache(IEnumerable<string> tableNames)
		{
#if DEBUG
			if (!DisableQueryCacheReset)
#endif
			{
				dbOnlyQueryCache.ClearViewsQueries();
				QueryCache.ClearViewsQueries(tableNames);
			}
		}

		public int QueryCacheClearCount
		{
			get { return queryCacheClearCount; }
		}
		int queryCacheClearCount;

		public IReadOnlyList<DataRow> GetModifiedPersistentRowsInSaveOrder()
		{
			return dataAccessor.GetModifiedPersistentRowsInSaveOrder();
		}

		internal IEnumerable<DataTable> GetTablesThatNeedToBeSaved()
		{
			return dataAccessor.GetTablesThatNeedToBeSaved();
		}

#if DEBUG
		public bool DisableQueryCacheReset;
#endif

		internal QueryCacheManager QueryCache { get; private set; }

		public static void SaveTogether(params ITransactionParticipant[] participants)
		{
			SaveTogether(new TransactionCoordinator(participants));
		}

		public static void SaveTogether(TransactionCoordinator coordinator)
		{
			using (var transactionManager = coordinator.BeginTransactionWithManager())
			{
				SaveInTransactionResult result = null;

				try
				{
					result = coordinator.SaveInTransactions();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					try
					{
						transactionManager.RollbackTransaction();
					}
					catch (SqlLockLostException)
					{
						throw;
					}
					catch (Exception rollbackEx) when (!rollbackEx.IsCriticalException())
					{
						coordinator.SavedParticipants.OfType<IBusinessObjectFactoryInternals>().ForEach(x => x.LastSavingRollbackHadException = true);

						if (!(rollbackEx.GetType().Equals(typeof(SqlException)) && ((SqlException)rollbackEx).Number == -2))
						{
							ErrorReporter.ReportOnce("Exception during Rollback", rollbackEx);
						}
					}

					if (ex is IConcurrencyException concurrencyException)
					{
						HandleConcurrencyExceptionSafe(concurrencyException, coordinator);
					}

					throw;
				}

				try
				{
					var uberCachedTables = result.SavedTables.Where(t => IsCachedTable(t)).ToArray();

					var uberRowFactory = UberFactoryRememberToLock.RowFactory;
					if (DateTime.UtcNow - uberRowFactory.ConstructionTime > TimeSpan.FromMinutes(uberRowFactory.UberFactoryTimeOutPeriodInSeconds / 60))
					{
						lock (UberFactoryMutex)
						{
							uberFactory = null;
						}
					}
					else if (uberCachedTables.Length > 0)
					{
						lock (UberFactoryMutex)
						{
							var tables = UberFactoryRememberToLock.RowFactory.data.Tables;
							foreach (var tableName in uberCachedTables)
							{
								if (tables.Contains(tableName))
								{
									tables.Remove(tableName);
									uberFactory.RowFactory.ClearQueryCache(tableName);
								}
							}
						}
					}

					PersistentFactoryCacheManager.Instance.ClearAllQueryCaches(result.SavedTables);
				}
				finally
				{
					transactionManager.CommitTransaction(result);
				}
			}
		}

		static void HandleConcurrencyExceptionSafe(IConcurrencyException concurrencyException, TransactionCoordinator coordinator)
		{
			try
			{
				if (concurrencyException.Row != null)
				{
					SendDebugMailBackToEDIAboutMultipleFactoriesIfAppropriate(concurrencyException.Row, coordinator.SavedParticipants);
				}
			}
			catch (Exception generatingDebugException) when (!generatingDebugException.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error in RowFactory building multi-factory error info", generatingDebugException);
			}
		}

		#region SuppressResourceStringsCheckRegion

		static void SendDebugMailBackToEDIAboutMultipleFactoriesIfAppropriate(DataRow noSaveRow, ITransactionParticipant[] savedParticipants)
		{
			try
			{
				Guid pK = ZDataUtils.GetPK(noSaveRow);
				var message = new StringBuilder("\r\nDifferent Factories trying to save row in Factory.SaveTogether (" + noSaveRow.Table.TableName + ", " + pK + "):\r\n)");
				int countOfFactoriesSavingSameRow = 0;

				var concurrencyRowDebugInfo = string.Empty;

				foreach (var participant in savedParticipants)
				{
					var rowFactory = RowFactory.FromITransactionParticipant(participant);
					if (rowFactory != null)
					{
						foreach (DataTable table in rowFactory.data.Tables)
						{
							if (noSaveRow.Table.TableName == table.TableName)
							{
								var row = table.Rows.Find(pK);
								if (row != null && row.RowState == DataRowState.Modified)
								{
									countOfFactoriesSavingSameRow++;

									concurrencyRowDebugInfo = string.Format(CultureInfo.InvariantCulture, "\r\n- {0} created {1}. Edited through {2}", rowFactory.NameForDebugging, rowFactory.ConstructionTime, GetDebuggingInformation(participant));
									message.Append(concurrencyRowDebugInfo);
								}
							}
						}
					}
				}

				if (countOfFactoriesSavingSameRow > 1) // 2+ means bug in this SaveTogether
				{
					ErrorReporter.ReportOnce("MultipleFactories_" + noSaveRow.Table.TableName, "Multiple Row Factories editing the same record." + System.Environment.NewLine + message);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Concurrency error reporting failed.", ex);
			}
		}

		static string GetDebuggingInformation(ITransactionParticipant participant)
		{
			if (participant is BusinessObjectFactory factory)
			{
				return FormattableString.Invariant($"BizoFactory {factory.NameForDebugging}, Instance {factory._Instance}");
			}
			else if (participant is RowFactory)
			{
				return "RowFactory directly";
			}
			else
			{
				return participant.ToString() + " of type " + participant.GetType().FullName;
			}
		}

		#endregion

		internal static RowFactory FromITransactionParticipant(ITransactionParticipant participant)
		{
			RowFactory result = participant as RowFactory;
			if (result == null && participant is BusinessObjectFactory)
			{
				result = ((BusinessObjectFactory)participant).RowFactory;
			}

			return result;
		}

		#endregion

		#region Getting Database Count

		public int GetDatabaseCount(string tableName)
		{
			return GetDatabaseCount(tableName, new ZQuery());
		}

		public int GetDatabaseCount(string tableName, ZQuery filter)
		{
			return dataAccessor.GetCount(new ZCountDataQuery(connectionInfo, tableName, filter));
		}

		#endregion

		#region Loading Table Schema

		public ZDataTable GetTable(string tableName, bool createIfNotExisting = true)
		{
			return dataAccessor.GetTable(tableName, createIfNotExisting);
		}

		#endregion

		#region Debugging Usage

#if DEBUG
		public static void ResetUniqueColumnInfo()
		{
			if (uniqueColumns?.uniqueColumns != null)
			{
				lock (uniqueColumnsLock)
				{
					uniqueColumns.uniqueColumns = null;
				}
			}
		}

		internal class UniqueColumnInfo
		{
			internal UniqueColumnInfo()
			{
				clientHookLoader = ObjectFactory.Get<IClientHookLoader>();
			}

			internal HashSet<string> uniqueColumns;
			string clientId;
			string databaseName;
			readonly IClientHookLoader clientHookLoader;

			internal bool ContainsKey(object uniqueColumnsLock, string key)
			{
				var clientHook = clientHookLoader.ClientHook;
				var currentClientId = clientHook != null ? clientHook.UniqueId : string.Empty;
				var currentDatabaseName = Db.DatabaseName;
				if (clientId != currentClientId || databaseName != currentDatabaseName || uniqueColumns == null)
				{
					lock (uniqueColumnsLock)
					{
						if (clientId != currentClientId || databaseName != currentDatabaseName || uniqueColumns == null)
						{
							uniqueColumns = DataUtils.BuildUniqueSingleKeyList();
							clientId = currentClientId;
							databaseName = Db.DatabaseName;
						}
					}
				}

				return uniqueColumns.Contains(key);
			}
		}

		[SuppressThreadStaticFieldMessage]
		internal static UniqueColumnInfo uniqueColumns;

		static readonly object uniqueColumnsLock = new object();
#endif

		[Conditional("DEBUG")]
		internal void CheckKeyIsUnique(string tableName, string columnName)
		{
#if DEBUG
			if (TestingState.IsRunningTests)
			{
				if (uniqueColumns == null)
				{
					lock (uniqueColumnsLock)
					{
						if (uniqueColumns == null)
						{
							uniqueColumns = new UniqueColumnInfo();
						}
					}
				}

				string lookupValue = tableName + "." + columnName;
				if (!uniqueColumns.ContainsKey(uniqueColumnsLock, lookupValue.ToUpperInvariant()))
				{
					throw new ApplicationException(
							"LoadFromNaturalKey was used on a table + field that does not have a unique index." + System.Environment.NewLine + System.Environment.NewLine +
							"This message will only appear in a test case" + System.Environment.NewLine +
							"TableName = " + tableName + System.Environment.NewLine +
							"ColumnName = " + columnName + System.Environment.NewLine + System.Environment.NewLine +
							"If this key value is forced to be unique using a 'fake' column GUID, or through other locking" + System.Environment.NewLine +
							"mechanisms, add to DataUtils.BuildUniqueSingleKeyList() in ZArchitecture." + System.Environment.NewLine +
							"Otherwise, please use the Load() method.");
				}
			}
#endif
		}

		//TODO Remove on resolution of WI00704670. Added to aid diagnosis. 
		public string GetDebugInformation(string tableName, ZGuid primaryKey)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Table name: {tableName}");
			sb.AppendLine($"Is the table a Cached Table: {IsCachedTable(tableName)}");

			lock (UberFactoryMutex)
			{
				var uberFactory = UberFactoryRememberToLock;
				DataRow row = uberFactory.RowFactory.LoadFromPK(tableName, primaryKey);
				sb.AppendLine($"Is the data row in the UberFactory: {row != null}, ({primaryKey})");

				if (tableName == "GlbBranch")
				{
					var filter = new ZQuery(GlbBranchSchema.PK, primaryKey);
					sb.AppendLine($"Is the filter cached in the UberFactory: {uberFactory.RowFactory.QueryCache.IsCached(tableName, filter)}");
				}
				else if (tableName == "GlbCompany")
				{
					var filter = new ZQuery(GlbCompanySchema.PK, primaryKey);
					sb.AppendLine($"Is the filter cached in the UberFactory: {uberFactory.RowFactory.QueryCache.IsCached(tableName, filter)}");
				}
			}

			return sb.ToString();
		}

		#endregion

		#region Memory Leaks
#if DEBUG

		public void ResetDatabaseLoadCount()
		{
			dataAccessor.ResetDatabaseLoadCount();
		}

		internal static RowFactory[] GetActiveRowFactories()
		{
			return PersistentFactoryCacheManager.Instance.GetRowFactories().Except(new[] { RowFactory.UberFactoryRememberToLock.RowFactory }).ToArray();
		}

#endif
		#endregion

		#region Statistics

		public int DatabaseLoadCount
		{
			get { return dataAccessor.DatabaseLoadCount; }
		}

		public TableHitCount[] TableSelects
		{
			get { return dataAccessor.TableSelects; }
		}

		public int ActiveTableFetchHints
		{
			get { return fetcher.ActiveTableFetchHints; }
		}

		public int ActiveFetchHintsForTable(string tableName)
		{
			return fetcher.ActiveFetchHintsForTable(tableName);
		}

		public int GetLoadedFetchHintCountForTable(string tableName)
		{
			return fetcher.GetLoadedFetchHintCountForTable(tableName);
		}

		public void ClearLoadedFetchHintCountForTable(string tableName)
		{
			fetcher.ClearLoadedFetchHintCountForTable(tableName);
		}

#if DEBUG

		public IEnumerable<string> GetAllFetchHintedTableNames()
		{
			return fetcher.GetAllFetchHintedTableNames();
		}

		public void DropHints()
		{
			fetcher.Clear();
		}
#endif
		#endregion

		#region DBConnection & Database Name

		public void EnsureConnectionIsOpen()
		{
			DbConnection.EnsureIsOpen();
		}

		protected internal DbConnection DbConnection
		{
			get { return connectionInfo.DbConnection; }
		}

		internal string DatabaseName { get; private set; }

		void IReadonlyDatabaseSupport.ChangeDatabaseNameToAWriteableOne(string newDbName)
		{
			if (DatabaseName != newDbName)
			{
				var isDocManagerDatabase = false;
				if (!string.IsNullOrEmpty(DatabaseName))
				{
					isDocManagerDatabase = DocManagerUtils.IsDocManagerDatabase(DatabaseName);
				}

				if (dataAccessor == null || string.IsNullOrEmpty(DatabaseName) || DbConnection.IsDbWriteable(DatabaseName) && !isDocManagerDatabase ||
						isDocManagerDatabase && DocManagerUtils.IsDbWriteableForDocManager(DatabaseName))
				{
					throw new InvalidOperationException("[RowFactory] Changing the database is only allowed if the original one is readonly.");
				}

				DatabaseName = newDbName;
				dataAccessor = new ZSqlDataAccessor(data, new ZSqlConnectionInfo(DbConnection, newDbName));
			}
		}

		#endregion

		#region Related Table Hints

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void AddTableFetchHintCreator(ITableSchema tableSchema, Func<IColumnIndexer, IEnumerable<IFetchHint>> getFetchHints, bool applyToExistingRows = false)
		{
			if (tableFetchHintCreators == null)
			{
				ErrorReporter.ReportOnce("AddTableFetchHintCreator was called without setting up the creator on the Factory that is being used, search for ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator() for an example");
			}
			else
			{
				if (!tableFetchHintCreators.TryGetValue(tableSchema.TableName, out List<Func<IColumnIndexer, IEnumerable<IFetchHint>>> list))
				{
					list = new List<Func<IColumnIndexer, IEnumerable<IFetchHint>>>();
					tableFetchHintCreators.Add(tableSchema.TableName, list);
				}

				if (!list.Contains(getFetchHints))
				{
					list.Add(getFetchHints);

					var targetTable = applyToExistingRows ? GetTable(tableSchema.TableName, false) : null;
					if (targetTable != null)
					{
						var hintsToAdd = new Func<IColumnIndexer, IEnumerable<IFetchHint>>[] { getFetchHints };
						var existingRows = targetTable.Select().OfType<IColumnIndexer>();
						existingRows.ForEach(row => AddRelatedTableHints(hintsToAdd, row));
					}
				}
			}
		}

		internal IDisposable SetupTableFetchHintCreators()
		{
			if (tableFetchHintCreators != null)
			{
				return new DisposableObject();
			}
			else
			{
				tableFetchHintCreators = new Dictionary<string, List<Func<IColumnIndexer, IEnumerable<IFetchHint>>>>();
				return new DisposableAction(() =>
				{
					if (tableFetchHintCreators != null)
					{
						tableFetchHintCreators.Clear();
						tableFetchHintCreators = null;
					}
				});
			}
		}

		Dictionary<string, List<Func<IColumnIndexer, IEnumerable<IFetchHint>>>> tableFetchHintCreators;

		IEnumerable<Func<IColumnIndexer, IEnumerable<IFetchHint>>> GetRelatedTableFetchHintCreators(string tableName)
		{
			List<Func<IColumnIndexer, IEnumerable<IFetchHint>>> result = null;
			if (tableFetchHintCreators != null)
			{
				tableFetchHintCreators.TryGetValue(tableName, out result);
			}
			return result;
		}

		void AddRelatedTableHints(string tableName, DataRow[] dataRows)
		{
			if (dataRows != null)
			{
				AddRelatedTableHints(tableName, dataRows.OfType<IColumnIndexer>().ToArray());
			}
		}

		internal void AddRelatedTableHints(string tableName, IColumnIndexer[] dataRows)
		{
			if (dataRows != null && dataRows.Length > 0)
			{
				var relatedTableFetchHintCreators = GetRelatedTableFetchHintCreators(tableName);
				if (relatedTableFetchHintCreators != null)
				{
					dataRows.ForEach(x => AddRelatedTableHints(relatedTableFetchHintCreators, x));
				}
			}
		}

		void AddRelatedTableHints(IEnumerable<Func<IColumnIndexer, IEnumerable<IFetchHint>>> relatedTableFetchHintCreators, IColumnIndexer row)
		{
			if (relatedTableFetchHintCreators != null && row != null)
			{
				foreach (var creator in relatedTableFetchHintCreators.Where(x => x != null))
				{
					foreach (var fetchHint in creator(row))
					{
						if (!QueryCache.IsCached(fetchHint.TableName, fetchHint.GetQuery()))
						{
							AddFetchHint(fetchHint);
						}
					}
				}
			}
		}

		#endregion

		#region INeedDataSet Members

		DataSet INeedDataSet.Data
		{
			get { return data; }
		}

		internal readonly DataSet data;

		#endregion

		#region ITransactionParticipant Members

		ITransactionManager ITransactionStarter.BeginTransactionWithManager()
		{
			if (transactionStartedTime.IsEmpty)
			{
				transactionStartedTime = ZDateTime.Now;
				transactionStartedTimeUtc = ZDateTime.UtcNow;
			}

			return new TransactionManager(this, DbConnection.BeginTransactionWithManager());
		}

		IChangedTableNames ITransactionParticipant.SaveInTransaction()
		{
			var result = dataAccessor.Save();
			narrowDbOnlyQueryCache.Clear();

			return result;
		}

		bool ITransactionParticipant.IsInTransaction
		{
			get { return IsInTransaction; }
		}

		public bool AllowTransactionWithOtherParticipant => false;

		bool IsInTransaction
		{
			get { return DbConnection.IsInTransactionOtherThanTransactionedTestCase; }
		}

		void ITransactionParticipant.OnAllTransactionsBeginning()
		{
		}

		void ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			if (!IsInTransaction)
			{
				transactionStartedTime = ZDateTime.Empty;
				transactionStartedTimeUtc = ZDateTime.Empty;
			}
		}

		void ITransactionParticipant.OnAllTransactionsRolledBack()
		{
			if (!IsInTransaction)
			{
				transactionStartedTime = ZDateTime.Empty;
				transactionStartedTimeUtc = ZDateTime.Empty;
			}
		}

		void AcceptChangesOnPersistentRows()
		{
			foreach (DataTable table in data.Tables)
			{
				for (int i = table.Rows.Count - 1; i >= 0; i--)
				{
					var row = table.Rows[i];
					if (ZDataUtils.ShouldRowBeSaved(row))
					{
						try
						{
							row.AcceptChanges();
							row.ClearSources();
						}
						catch (RowNotInTableException ex)
						{
							throw new ZRowNotInTableException(ex, row, connectionInfo.DbConnection ?? Db.Connection);
						}
					}
				}
			}
		}

		internal void Rollback()
		{
			data.RejectChanges();
		}

		ITransactionParticipant[] ITransactionParticipant.ChildParticipants
		{
			get { return null; }
		}

		IEnumerable<ISqlApplicationLock> ITransactionParticipant.TransactionLocks => Enumerable.Empty<ISqlApplicationLock>();

		#endregion

		class TransactionManager : AggregateTransactionManager<RowFactory>
		{
			public TransactionManager(RowFactory owner, ITransactionManager innerTransactionManager) : base(owner, innerTransactionManager)
			{
			}

			protected override void Commit()
			{
				base.Commit();

#if DEBUG
				owner.OnCommittingTransaction();
#endif

				owner.AcceptChangesOnPersistentRows();
			}
		}

		#region For Test Purposes
#if DEBUG

		public TableHitCount GetTableHitCount(string tableName)
		{
			return dataAccessor.GetTableHitCount(tableName);
		}

		void OnCommittingTransaction()
		{
			CommittingTransaction?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler CommittingTransaction;

		public DBQueryCacheManager DbOnlyQueryCache => dbOnlyQueryCache;

#endif
		#endregion
	}

	#region IReadonlyDatabaseSupport

	public interface IReadonlyDatabaseSupport
	{
		void ChangeDatabaseNameToAWriteableOne(string newDbName);
	}

	#endregion
}
