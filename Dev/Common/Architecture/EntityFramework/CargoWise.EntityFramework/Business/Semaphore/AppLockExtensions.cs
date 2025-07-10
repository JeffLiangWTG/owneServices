using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Extensions
{
	public static partial class AppLockExtensions
	{
		/// <summary>
		/// Take a DB lock out on a set of rows.
		/// </summary>
		public static LoadWithAppLockResult<T> LoadWithApplocks<T>(this BusinessObjectFactory factory, string appLockKey, ZDBOnlyQuery query, int batchSize)
			where T : BusinessObject
		{
			return LoadWithApplocks<T>(factory, appLockKey, new[] { query.PKColumn }, query, batchSize);
		}

		/// <summary>
		/// Take a DB lock out on a set of rows.
		/// </summary>
		public static LoadWithAppLockResult<T> LoadWithApplocks<T>(this BusinessObjectFactory factory, string appLockKey, ISqlApplicationLockProvider lockProvider, ZDBOnlyQuery query, int batchSize)
			where T : BusinessObject
		{
			return LoadWithApplocks<T>(factory, appLockKey, lockProvider, new[] { query.PKColumn }, query, null, batchSize);
		}

		public static LoadWithAppLockResult<T> LoadWithApplocks<T>(this BusinessObjectFactory factory, string appLockKey, SchemaGuidColumn[] columns, ZQuery query, int batchSize)
			where T : BusinessObject
		{
			return LoadWithApplocks<T>(factory, appLockKey, new DefaultSqlApplicationLockProvider(), columns, query, null, batchSize);
		}

		/// <summary>
		/// Take a DB lock out on a set of rows.
		/// Note: Columns are not cumulative.
		/// The first column on a given row that has a value will be the key.
		/// </summary>
		/// <typeparam name="T">Type of BusinessObject</typeparam>
		/// <param name="factory">BusinessObjectFactory to load locked rows into</param>
		/// <param name="appLockKey">Unique prefix for the locks that will be created for each object</param>
		/// <param name="lockProvider">SQL application lock provider</param>
		/// <param name="columns">Array of columns that will be coalesced to provide the key for the lock</param>
		/// <param name="selectQuery">
		///		Query to retrieve the rows that will be locked. 
		///		Note that coherency guarantees can only be provided for filters that target the row being locked. 
		///		If additional filters are required that target other tables these must be added to the check query.
		/// </param>
		/// <param name="checkFilter">
		///		Optional query filter to further filter locked rows. 
		///		The resultant Check Query will be of the form: SELECT primaryTablePK in (lockedPK1, lockedPK2, ...) FROM primaryTable WHERE [checkFilter].
		///		This is required when filtering with other tables. 
		///		Without this separate query a race condition occurs where the database may retrieve the filter table data
		///		prior to performing the select and lock on the primary table, leading to inconsistent state.
		/// </param>
		/// <param name="batchSize">Maximum number of BusinessObjects to be returned</param>
		/// <returns></returns>
		public static LoadWithAppLockResult<T> LoadWithApplocks<T>(this BusinessObjectFactory factory, string appLockKey, ISqlApplicationLockProvider lockProvider, SchemaGuidColumn[] columns, ZQuery selectQuery, ZQuery checkFilter, int batchSize, AppLockLogger<T> logger = null)
			where T : BusinessObject
		{
			var keyExpression = GetKeySqlExpression(columns);
			const string AppLockTestExpression = "APPLOCK_TEST('public',  UPPER(CONCAT(@AppLockKey, ',', CAST({0} AS NVARCHAR(36)))), 'exclusive', 'session') = 1";

			var improvedQuery = new ZDBOnlyQuery(typeof(T));
			improvedQuery.ReLoadExistingRows = true;
			improvedQuery.AddToFilter(selectQuery);

			var appLockFilter = string.Format(CultureInfo.InvariantCulture, AppLockTestExpression, keyExpression);
			var parameters = new ZSqlParameterCollection();
			parameters.Add(
				ZSqlParameter.New(
					"@AppLockKey",
					appLockKey,
					new SchemaStringColumn(
						Schema.Schema.GenericTableSchema,
						"GenericStringColumn",
						0,
						System.Data.SqlDbType.NVarChar,
						string.Empty,
						isNullable: true,
						maxLength: 200,
						isLiteralOnly: false)));

			improvedQuery.AddFilterAndZSQLParameterCollection(appLockFilter, parameters);
			// We use the following combination of table hints to acheive the following. 
			//	UPDLOCK will mark rows as locked for update during the select. Normal read operations will not be blocked and rows can be added.
			//	READPAST will ensure the that locked rows are skipped when selecting. The combination of these two ensures that parallel queries do not receive the same rows.
			//	ROWLOCK forces SQL server to use the row locks rather than page or table so that this locking strategy is valid.
			improvedQuery.TableHints |= TableHints.READPAST | TableHints.UPDLOCK | TableHints.ROWLOCK;

			if (batchSize > 0 && batchSize != int.MaxValue)
			{
				improvedQuery.MaximumRows = batchSize;
			}

			// The Transaction is required for the SQL row locking, but no actual data changes are required.
			// The AppLocks are session based and hence not affected.
			using (((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				var items = factory.Load<T>(improvedQuery);

				RunUserAction_ForTest();

				logger?.OnLoad(items);
				var result = items.ApplyAppLocks(appLockKey, lockProvider, columns, logger);

				var reloded = checkFilter == null ? result : result.ReloadRowsToAvoidRaceCondition(factory, checkFilter);
				reloded.ItemsLoaded = items.Length;

				return reloded;
			}
		}

		/// <summary>
		/// There is a race condition when taking out applocks, that is:
		/// It is possible the row was processed before the applock is applied, so we have to reload the row afterwards. 
		/// </summary>
		public static LoadWithAppLockResult<T1> ReloadRowsToAvoidRaceCondition<T1>(this LoadWithAppLockResult<T1> lockedItems, BusinessObjectFactory factory, ZQuery query, bool disposeLeakedLocks = true)
			where T1 : BusinessObject
		{
			var bizo = lockedItems.ItemsWithLocks.Select(s => s.Item).FirstOrDefault();
			if (bizo != null)
			{
				var queryWithPks = new ZQuery(bizo.PKSchemaColumn, lockedItems.Values.Select(i => i.PK).ToArray());
				queryWithPks.AddToFilter(query);
				return ReloadRowsToAvoidRaceCondition<T1, T1>(lockedItems, factory, queryWithPks, i => i.PK, disposeLeakedLocks);
			}
			else
			{
				return new LoadWithAppLockResult<T1>(Array.Empty<AppLockedItem<T1>>());
			}
		}

		public static LoadWithAppLockResult<T2> ReloadRowsToAvoidRaceCondition<T1, T2>(this LoadWithAppLockResult<T1> lockedItems, BusinessObjectFactory factory, ZQuery query, Func<T1, ZGuid> getKey, bool disposeLeakedLocks = true)
			where T2 : BusinessObject
		{
			var containingQuery = new ZDBOnlyQuery(typeof(T2));
			containingQuery.AddToFilter(query);
			containingQuery.ReLoadExistingRows = true;
			var reloaded = factory.Load<T2>(containingQuery).ToDictionary(k => k.PK);
			var result = new List<AppLockedItem<T2>>(reloaded.Count);
			foreach (var item in lockedItems.ItemsWithLocks)
			{
				if (reloaded.TryGetValue(getKey(item.Item), out T2 reloadedItem))
				{
					result.Add(new AppLockedItem<T2>(reloadedItem, item.Lock));
				}
				else if (disposeLeakedLocks)
				{
					item.Lock.Dispose();
				}
			}

			return new LoadWithAppLockResult<T2>(result);
		}

		public static LoadWithAppLockResult<T> ApplyAppLocks<T>(this IList<T> set, string appLockKey, SchemaGuidColumn[] columns)
		{
			return set.ApplyAppLocks(appLockKey, new DefaultSqlApplicationLockProvider(), columns);
		}

		public static LoadWithAppLockResult<T> ApplyAppLocks<T>(this IList<T> set, string appLockKey, ISqlApplicationLockProvider lockProvider, SchemaGuidColumn[] columns, AppLockLogger<T> logger = null)
		{
			var keeps = new List<AppLockedItem<T>>(set.Count);
			foreach (var item in set)
			{
				ISqlApplicationLock mutex;
				if (item is BusinessObject bizo)
				{
					var pk = GetKeyFromBizo(bizo, columns);
					var key = GenAppLockKey(appLockKey, pk);
					if (lockProvider.TryGetLock(key, out mutex))
					{
						logger?.OnLockTaken(key);
						keeps.Add(new AppLockedItem<T>(item, mutex));
					}
				}
				else
				{
					keeps.Add(new AppLockedItem<T>(item, null));
				}
			}

			return new LoadWithAppLockResult<T>(keeps);
		}

		public static LoadWithAppLockResult<T> ApplyAppLocks<T>(this IList<T> pks, string appLockKey, ISqlApplicationLockProvider lockProvider = null)
		{
			var keeps = new List<AppLockedItem<T>>(pks.Count);
			foreach (var id in pks)
			{
				var key = GenAppLockKey(appLockKey, id);
				if ((lockProvider ?? new DefaultSqlApplicationLockProvider()).TryGetLock(key, out var mutex))
				{
					keeps.Add(new AppLockedItem<T>(id, mutex));
				}
			}
			return new LoadWithAppLockResult<T>(keeps);
		}

		static string GenAppLockKey(string appLockKey, object pk) => (appLockKey + "," + pk.ToString()).ToUpperInvariant();

		static string GetKeySqlExpression(SchemaGuidColumn[] columns)
		{
			if (columns.Length == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(columns), "Should be at least one column.");
			}
			else if (columns.Length == 1)
			{
				return columns[0].Name;
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, "COALESCE({0})", string.Join(",", columns.Select(c => c.Name)));
			}
		}

		static ZGuid GetKeyFromBizo(BusinessObject bizo, SchemaGuidColumn[] cols)
		{
			return cols.Select(col => (ZGuid)bizo[col]).FirstOrDefault(c => c.IsValid);
		}

		static partial void RunUserAction_ForTest();
	}

	public sealed class AppLockedItem<T> : Disposable
	{
		public AppLockedItem(T item, ISqlApplicationLock appLock)
		   : base(isListened: false)
		{
			Item = item;
			Lock = appLock;
		}

		public T Item { get; }
		public ISqlApplicationLock Lock { get; }

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				Lock?.Dispose();
			}
		}
	}

	public sealed class LoadWithAppLockResult<T> : IDisposable
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public LoadWithAppLockResult(IList<AppLockedItem<T>> set)
		{
			this.ItemsWithLocks = set;
		}

		public IEnumerable<T> Values => ItemsWithLocks.Select(s => s.Item);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IList<AppLockedItem<T>> ItemsWithLocks { get; }

		public int ItemsLoaded { get; internal set; }

		#region IDisposable Support

		bool disposedValue;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					foreach (var item in ItemsWithLocks)
					{
						item.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}

	public sealed class AppLockLogger<T>
	{
		public AppLockLogger(Action<string> onLockTaken, Action<T[]> onLoad)
		{
			this.onLockTaken = onLockTaken;
			this.onLoad = onLoad;
		}

		readonly Action<string> onLockTaken;
		readonly Action<T[]> onLoad;

		internal void OnLockTaken(string lockStr) => onLockTaken?.Invoke(lockStr);
		internal void OnLoad(T[] items) => onLoad?.Invoke(items);
	}
}

#region Test
#if DEBUG

#region Partial class

namespace CargoWise.EntityFramework.Extensions
{
	public static partial class AppLockExtensions
	{
		public static readonly Overridable<Action> UserAction_ForTest = new Overridable<Action>(null);

		static partial void RunUserAction_ForTest()
		{
			UserAction_ForTest.Value?.Invoke();
		}
	}
}

#endregion // Partial class

#endif
#endregion
