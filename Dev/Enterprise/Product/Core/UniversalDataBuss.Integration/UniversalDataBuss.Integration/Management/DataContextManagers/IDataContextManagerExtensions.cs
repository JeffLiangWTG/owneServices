using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public static class IDataContextManagerExtensions
	{
		#region Get IDataContextManager

		public static IDataContextManager GetUniversalDataContextManager(this Type parentType)
		{
			Argument.NotNull(parentType, "Type parentType");
			return GetDataContextManagerCore(parentType);
		}

		public static IDataContextManager GetUniversalDataContextManager(this BusinessObject parent)
		{
			Argument.NotNull(parent, "BusinessObject parent");

			var result = GetDataContextManagerCore(parent);
			if (result != null)
			{
				result.Init(parent);
			}

			return result;
		}

		static IDataContextManager GetDataContextManagerCore(object parent)
		{
			var attribute = parent.GetAttribute<UniversalDataContextAttribute>();
			if (attribute != null)
			{
				var dataContextType = attribute.DataContextType;
				return dataContextType.GetUniversalDataContextManager();
			}
			else if (parent is BusinessObject bizo && bizo.Factory.TryGetValueFromCacheOnly(DataContextManagerCacheKey, out IDataContextManagerForManyTypes contextManager))
			{
				return contextManager;
			}

			return null;
		}

		public static IDataContextManager GetUniversalDataContextManager(this DataContextType dataContextType)
		{
			var types = (Hashtable)ObjectFactory.Get("UniversalDataContextManagers");
			var objectHandle = (ObjectHandle)types[dataContextType.ToString()];
			if (objectHandle != null)
			{
				var result = (IDataContextManager)objectHandle.GetObject();
				return result;
			}

			throw new InvalidOperationException("All DataContextTypes must be mapped to DataContextManagers in the spring list UniversalDataContextManagers. Missing Mapping for: DataContextType." + dataContextType.ToString());
		}

		public static IDisposable SetUniversalDataContextManagerForManyTypes(this BusinessObject parent, IDataContextManagerForManyTypes manager)
		{
			Argument.NotNull(manager, nameof(IDataContextManagerForManyTypes));

			if (parent.Factory.TryGetValueFromCacheOnly<IDataContextManagerForManyTypes>(DataContextManagerCacheKey, out _))
			{
				throw new InvalidOperationException("Invalid scope");
			}

			parent.Factory.GetCachedValue(DataContextManagerCacheKey, () => manager);
			return new DisposableAction(() => parent.Factory.ClearCachedValue<IDataContextManagerForManyTypes>(DataContextManagerCacheKey));
		}

		const string DataContextManagerCacheKey = nameof(IDataContextManagerForManyTypes);

		#endregion

		#region Get and Set ContextKey for a business object

		public static void SetDataManagerContextKey(this BusinessObject parent, ZString contextKey)
		{
			var cacheKey = ContextKeyCacheKey(parent);
			parent.Factory.ClearCachedValue<ZString>(cacheKey);
			parent.Factory.GetCachedValue(ContextKeyCacheKey(parent), () => contextKey, CacheStalenessPolicy.StaleOnFactorySave);
		}

		public static ZString GetDataManagerContextKey(this BusinessObject parent)
		{
			if (parent.Factory.TryGetValueFromCacheOnly(ContextKeyCacheKey(parent), out ZString contextKey))
			{
				return contextKey;
			}
			return ZString.Empty;
		}

		static string ContextKeyCacheKey(BusinessObject parent) => $"{nameof(ContextKeyCacheKey)}{parent.PK}";

		#endregion

		public static bool ManagesEvents(this IDataContextManager manager)
		{
			var eventManager = manager as IEventDataContextManager;
			return eventManager != null && eventManager.ManagesEvents;
		}

		public static bool ManagesShipments(this IDataContextManager manager)
		{
			var shipmentManager = manager as IShipmentDataContextManager;
			return shipmentManager != null && shipmentManager.ManagesShipments;
		}

		public static bool ManagesTransactions(this IDataContextManager manager)
		{
			var transactionManager = manager as ITransactionDataContextManager;
			return transactionManager != null && transactionManager.ManagesTransactions;
		}

		public static bool ManagesSchedules(this IDataContextManager manager)
		{
			var scheduleManager = manager as IScheduleDataContextManager;
			return scheduleManager != null && scheduleManager.ManagesSchedules;
		}

		public static bool ManagesTransactionBatches(this IDataContextManager manager)
		{
			var batchManager = manager as ITransactionBatchDataContextManager;
			return batchManager != null && batchManager.ManagesTransactionBatches;
		}

		public static bool ManagesActivities(this IDataContextManager manager)
		{
			var activityManager = manager as IActivityDataContextManager;
			return activityManager != null && activityManager.ManagesActivities;
		}
	}
}
