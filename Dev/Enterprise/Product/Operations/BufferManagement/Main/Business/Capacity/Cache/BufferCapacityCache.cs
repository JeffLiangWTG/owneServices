using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.MemoryManagement;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Implementation;
using CargoWise.Types;
using Enterprise.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	[ThreadSafe]
	public sealed class BufferCapacityCache
	{
		public static BufferCapacityCache Get(ZGuid bufferPK)
		{
			return memoryCache.GetCachedOrCalculateValue(bufferPK.ToString(), () => new BufferCapacityCache(bufferPK)) ?? new BufferCapacityCache(bufferPK);
		}

		static readonly CapacityMemoryCache memoryCache = new CapacityMemoryCache();

		BufferCapacityCache(ZGuid bufferPK)
		{
			this.bufferPK = bufferPK;
		}

		readonly ZGuid bufferPK;
		readonly ConcurrentDictionary<string, IResourceCapacity> cache = new ConcurrentDictionary<string, IResourceCapacity>();

		#region API

		public IResourceCapacity GetCapacity(string resourceCode, BusinessObjectFactory factory, bool clearIfStale = true)
		{
			EnsurePopulated(factory, resourceCode);
			EnsureNotStale(clearIfStale, resourceCode);

			return GetCapacityCore(resourceCode);
		}

		IResourceCapacity GetCapacityCore(string resourceCode)
		{
			if (!cache.TryGetValue(resourceCode, out var capacity))
			{
				return null;
			}

			return capacity;
		}

		internal static void MergeWithLocalCache(ZGuid bufferPK, Dictionary<string, IResourceCapacity> resourceCapacities)
		{
			if (!BMSRegistry.Instance.CacheCalculatedCapacity.Value || !resourceCapacities.Any())
			{
				return;
			}

			var bufferCapacityCache = Get(bufferPK);
			var cache = bufferCapacityCache.cache;
			resourceCapacities.ForEach(kv => cache[kv.Key] = kv.Value);
		}

		#endregion

		#region PersistedCache

		class PersistedCapacityCache
		{
			public decimal Full { get; set; }

			public Dictionary<int, decimal> Allocated { get; set; }

			public Dictionary<int, decimal> Reserved { get; set; }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "sql parameters")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Internal table")]
		public static void SaveCapacityOnCache(ZGuid bufferPK, Dictionary<string, IResourceCapacity> capacities)
		{
			if (capacities == null || capacities.Count == 0)
			{
				return;
			}

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.BMCapacityCache WHERE BMC_FC_Component = @bufferPK", dbParams => dbParams.AddParameter("@bufferPK", System.Data.SqlDbType.UniqueIdentifier, bufferPK.ToGuid()));

			var now = ZDateTime.UtcNow.ToDateTime();
			var currentUserCode = Env.CurrentUser.Initials;

			var insertSql = @"INSERT INTO dbo.BMCapacityCache (BMC_PK,BMC_FC_Component,BMC_GS_NKStaff,BMC_Value,BMC_CalculatedTimeUtc,BMC_SystemCreateTimeUtc,BMC_SystemCreateUser,BMC_SystemLastEditTimeUtc,BMC_SystemLastEditUser) VALUES
(NEWID(), @bufferPK, @staffCode, @capacity, @calculatedTime, @nowDateTime, @currentUserCode, @nowDateTime, @currentUserCode)";

			void Insert(string staffCode, string capacity, DateTime calculatedTime)
			{
				Db.Connection.ExecuteNonQuery(insertSql, dbParams =>
				{
					dbParams.AddParameter("@bufferPK", System.Data.SqlDbType.UniqueIdentifier, bufferPK.ToGuid());
					dbParams.AddParameter("@staffCode", System.Data.SqlDbType.VarChar, 3, staffCode);
					dbParams.AddParameter("@calculatedTime", System.Data.SqlDbType.DateTime, calculatedTime);
					dbParams.AddParameter("@nowDateTime", System.Data.SqlDbType.SmallDateTime, now);
					dbParams.AddParameter("@currentUserCode", System.Data.SqlDbType.VarChar, 3, currentUserCode);

					dbParams.AddParameter("@capacity", System.Data.SqlDbType.NVarChar, -1, capacity);
				});
			}

			var zones = new[] { 0, 1, 2, 3 };

			foreach (var capacity in capacities)
			{
				var staffCode = capacity.Key;
				var staffCapacity = capacity.Value;
				var calculatedTime = capacity.Value.CalculatedTimeUtc.ToDateTime();

				var persistedCapacityCache = new PersistedCapacityCache
				{
					Full = Limit(staffCapacity.FullCapacity),
					Allocated = zones.ToDictionary(zone => zone, zone => Limit(staffCapacity.GetZoneAllocatedCapacity(zone))),
					Reserved = zones.ToDictionary(zone => zone, zone => Limit(staffCapacity.GetZoneReservedCapacity(zone))),
				}.JsonSerialize();

				Insert(staffCode, persistedCapacityCache, calculatedTime);
			}

			MergeWithLocalCache(bufferPK, capacities);
		}

		static decimal Limit(decimal capacity)
		{
			if (capacity < -99999.99m)
			{
				return -99999.99m;
			}
			if (capacity > 99999.99m)
			{
				return 99999.99m;
			}
			return capacity;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "sql parameters")]
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Internal table")]
		void PopulateFromPersistedCache(BusinessObjectFactory factory, string staffCode)
		{
			var command = Db.Connection.Command(
	@"SELECT BMC_GS_NKStaff, BMC_Value, BMC_CalculatedTimeUtc
FROM dbo.BMCapacityCache WITH(NOLOCK)
WHERE BMC_FC_Component = @buffer AND BMC_GS_NKStaff = @staffCode");

			command.AddParameter("@buffer", System.Data.SqlDbType.UniqueIdentifier, bufferPK.ToGuid());
			command.AddParameter("@staffCode", System.Data.SqlDbType.VarChar, staffCode);

			string code;
			string value;
			DateTime calcTime;

			using (var result = command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
			{
				if (!result.Read())
				{
					return;
				}

				code = result.ReadString(0);
				value = result.ReadString(1);
				calcTime = result.ReadDate(2);
			}

			var capacityCache = value.JsonDeserialize<PersistedCapacityCache>();

			// we must have all relevant capacities present, otherwise we will not cache anything
			if (capacityCache.Allocated == null || capacityCache.Allocated.Count == 0 || capacityCache.Reserved == null || capacityCache.Reserved.Count == 0)
			{
				return;
			}

			var capacity = ResourceCapacity.CreateResourceCapacity(staffCode, calcTime, capacityCache.Full, capacityCache.Allocated, capacityCache.Reserved, considerZoneMultipliers: true);
			var ccrOverloadMultiplier = ConstrainedModeHelper.GetFullCapacityMultiplierConsideringCapacityConstrainedResources(capacity.StaffCode, factory.Load<BMComponent>(bufferPK));
			cache[capacity.StaffCode] = capacity.WithNonCCROverloadMultiplier(ccrOverloadMultiplier);
		}

		#endregion

		#region Memory Cache

		internal void DeductInMemoryCapacity(string resourceCode, decimal capacityToReduce)
		{
			var capacity = GetCapacityCore(resourceCode);

			if (capacity != null)
			{
				cache[resourceCode] = capacity.Deduct(capacityToReduce);
			}
		}

		void EnsureNotStale(bool clearIfStale, string staffCode)
		{
			if (!clearIfStale)
			{
				return;
			}

			// Cached data expires after (default of) 5 times the default scheduled running interval of the release gate
			var staleInterval = TimeSpan.FromMinutes(15 * BMSRegistry.Instance.CachedCapacityStaleTimeMultiplier.Value);
			if (cache.TryGetValue(staffCode, out var capacity))
			{
				var staleTime = capacity.CalculatedTimeUtc.Add(staleInterval);
				if (staleTime < ZDateTime.UtcNow)
				{
					// Atomically remove the KeyValuePair from the cache if both the key and value match.
					// This interface is exposed in .NET 5+, but for now we have to cast to ICollection<>
					ICollection<KeyValuePair<string, IResourceCapacity>> collection = cache;
					collection.Remove(new KeyValuePair<string, IResourceCapacity>(staffCode, capacity));
				}
			}
		}

		void EnsurePopulated(BusinessObjectFactory factory, string staffCode)
		{
			if (cache.ContainsKey(staffCode))
			{
				return;
			}

			PopulateFromPersistedCache(factory, staffCode);
		}

		[ThreadSafe]
		sealed class CapacityMemoryCache : MemoryCacheWrapper
		{
			protected override TimeSpan GetAbsoluteExpiry(string key) => TimeSpan.FromMinutes(15);
		}

		public static void Clear() => memoryCache.Clear();

		#endregion
	}
}
