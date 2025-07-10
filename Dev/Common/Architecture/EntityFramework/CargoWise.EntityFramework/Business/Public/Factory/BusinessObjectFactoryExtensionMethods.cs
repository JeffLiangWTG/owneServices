using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Extensions
{
	public static class BusinessObjectFactoryExtensionMethods
	{
		#region BusinessObjectFactory.SynchroniseCachedBusinessObjectsWithDB

		/// <summary>
		/// TODO:
		/// Modify to return conflicting BizO's that were deleted or updated outside of current instance as well as updated in current instance. 
		/// Currently concurrency policy will take care of it.
		/// </summary>
		public static void SynchroniseCachedBusinessObjectsWithDB<T>(this BusinessObjectFactory factory, bool includeBusinessObjectsWithChanges, bool deleteAtDatabaseRowLevel) where T : BusinessObject
		{
			if (includeBusinessObjectsWithChanges && deleteAtDatabaseRowLevel)
			{
				throw new ArgumentException("Deleting at database row level not supported when including business objects with changes");
			}
			var cachedBusinessObjectsToReload = GetAllBusinessObjectsThatNeedReloading<T>(factory, includeBusinessObjectsWithChanges);
			var reloadedBusinessObjects = ReloadBusinessObjectsFromDB(factory, cachedBusinessObjectsToReload);
			DeleteBusinessObjectsFromCacheThatWereDeletedInAnotherEnterpriseInstance(cachedBusinessObjectsToReload, reloadedBusinessObjects, deleteAtDatabaseRowLevel);
		}

		/// <summary>
		/// Returns all business objects of type T that are in the Database but do not have changes.
		/// </summary>
		static Dictionary<ZGuid, T> GetAllBusinessObjectsThatNeedReloading<T>(BusinessObjectFactory factory, bool includeBusinessObjectsWithChanges) where T : BusinessObject
		{
			// reload all business objects of type T that exist in the factory (will *not* go to the database)
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;
			query.ReLoadExistingRows = false;
			var cachedBusinessObjects = factory.Load<T>(query);

			// build a list of all business objects that are in the database but do not have changes.
			var dictionary = new Dictionary<ZGuid, T>();

			foreach (var bizO in cachedBusinessObjects)
			{
				if (bizO.IsInDatabase && (includeBusinessObjectsWithChanges || !bizO.HasChanges)) //#warning test this -- ((IBusinessObjectState)bizO).HasChangesNotIncludingChildren
				{
					dictionary.Add(bizO.PK, bizO);
				}
			}

			return dictionary;
		}

		/// <summary>
		/// Reloads all business objects in the dictionary.
		/// </summary>
		static IEnumerable<ZGuid> ReloadBusinessObjectsFromDB<T>(BusinessObjectFactory factory, Dictionary<ZGuid, T> dictionary) where T : BusinessObject
		{
			var result = new List<ZGuid>();
			ITableSchema schema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));

			foreach (var keySet in dictionary.Keys.Partition(1000))
			{
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(T));
				dbOnlyQuery.AddToFilter(schema.PK, keySet.ToArray());
				dbOnlyQuery.ReLoadExistingRows = true;
				result.AddRange(factory.Load<T>(dbOnlyQuery).Select(x => x.PK));
			}
			return result;
		}

		/// <summary>
		/// Removes all business objects that were deleted outside this instance of Enterprise.
		/// </summary>
		static void DeleteBusinessObjectsFromCacheThatWereDeletedInAnotherEnterpriseInstance<T>(Dictionary<ZGuid, T> cachedBusinessObjectsToReload, IEnumerable<ZGuid> reloadedBusinessObjects, bool deleteAtDatabaseRowLevel) where T : BusinessObject
		{
			foreach (var pk in reloadedBusinessObjects)
			{
				if (cachedBusinessObjectsToReload.ContainsKey(pk))
				{
					cachedBusinessObjectsToReload.Remove(pk);
				}
			}

			foreach (var bizO in cachedBusinessObjectsToReload.Values)
			{
				DeleteBusinessObject(bizO, deleteAtDatabaseRowLevel);
				// to stop deleting from DB since they already are not in DB.
				if (deleteAtDatabaseRowLevel)
				{
					bizO.Row.AcceptChanges();
				}
				else
				{
					bizO.ClearHasChanges();
				}
			}
		}

		static void DeleteBusinessObject(BusinessObject bizO, bool deleteAtDatabaseRowLevel)
		{
			var bizO_ISyncWithDB = bizO as ISyncWithDB;
			if (bizO_ISyncWithDB != null)
			{
				bizO_ISyncWithDB.SafeDeleteForBizOAlreadyDeletedInDatabase_DoNotUse();
			}
			else
			{
				if (deleteAtDatabaseRowLevel)
				{
					bizO.Row.Delete();
				}
				else
				{
					bizO.Delete();
				}
			}
		}

		/// <summary>
		/// Splits a collection into a collection of smaller collections each of which has the number of elements in the partitionSize parameter
		/// </summary>
		static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> seq, int partitionSize)
		{
			var enumerator = seq.GetEnumerator();
			while (enumerator.MoveNext())
			{
				yield return enumerator.TakeFromCurrent(partitionSize);
			}
		}

		/// <summary>
		/// Returns a collection containing the number of elements specified in the count parameter
		/// </summary>
		static IEnumerable<T> TakeFromCurrent<T>(this IEnumerator<T> enumerator, int count)
		{
			while (count > 0)
			{
				yield return enumerator.Current;
				if (--count > 0 && !enumerator.MoveNext())
				{
					yield break;
				}
			}
		}

		#endregion

		#region Bulk Copy

		static void SetBulkCopyOnTable(BusinessObjectFactory factory, string tableName, BulkCopySetting bulkCopySetting)
		{
			var table = ((INeedDataSet)factory).Data.Tables[tableName];
			if (table != null && table.ExtendedProperties[typeof(BulkCopySetting)] == null)
			{
				table.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
			}
		}

		public static void SetBulkCopyOnTable(
			this BusinessObjectFactory factory,
			string tableName,
			int threshold = BulkCopyThresholdValue,
			int batchSize = 1000,
			bool checkRowsShouldBePersistent = true,
			bool keepIdentity = false,
			bool checkConstraints = false,
			bool tableLock = false,
			bool keepNulls = false,
			bool fireTriggers = false,
			bool useInternalTransaction = false,
			bool allowEncryptedValueModifications = false,
			EventHandler onBulkCopyingHandler = null)
		{
			var bulkCopySetting = new BulkCopySetting(
				threshold,
				batchSize,
				checkRowsShouldBePersistent,
				keepIdentity,
				checkConstraints,
				tableLock,
				keepNulls,
				fireTriggers,
				useInternalTransaction,
				allowEncryptedValueModifications,
				onBulkCopyingHandler);

			SetBulkCopyOnTable(factory, tableName, bulkCopySetting);
		}

		public static void SetBulkCopyOnTables(
			this BusinessObjectFactory factory,
			IEnumerable<string> tableNames,
			int threshold = BulkCopyThresholdValue,
			int batchSize = 1000,
			bool checkRowsShouldBePersistent = true,
			bool keepIdentity = false,
			bool checkConstraints = false,
			bool tableLock = false,
			bool keepNulls = false,
			bool fireTriggers = false,
			bool useInternalTransaction = false,
			bool allowEncryptedValueModifications = false,
			EventHandler onBulkCopyingHandler = null)
		{
			var bulkCopySetting = new BulkCopySetting(
				threshold,
				batchSize,
				checkRowsShouldBePersistent,
				keepIdentity,
				checkConstraints,
				tableLock,
				keepNulls,
				fireTriggers,
				useInternalTransaction,
				allowEncryptedValueModifications,
				onBulkCopyingHandler);

			foreach (var tableName in tableNames)
			{
				SetBulkCopyOnTable(factory, tableName, bulkCopySetting);
			}
		}

		public static int DefaultBulkCopyThreshold(this BusinessObjectFactory factory) => BulkCopyThresholdValue;

		const int BulkCopyThresholdValue = 100;

		#endregion
	}
}
