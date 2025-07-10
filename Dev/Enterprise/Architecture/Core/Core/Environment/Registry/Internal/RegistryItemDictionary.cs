using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Core.Environment.Internal;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IRegistryItemDictionaryInternals
	{
#if DEBUG
		int Count { get; }
		int ItemThreshold { get; }

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int TimeoutMinutes { get; }
		RegistryItemHolder GetHolder(string key);
		void Purge(string key);
#endif
	}

	public sealed class RegistryItemDictionary : IRegistryItemDictionaryInternals
	{
		RegistryItemDictionary()
		{
			registryCacheVersion = GetCurrentRegistryVersion();
#if DEBUG
			StaticRegisteredResetTestListener.Instance.Register(PurgeAll);
#endif
		}

		public static RegistryItemDictionary Instance
		{
			get { return instance ?? (instance = new RegistryItemDictionary()); }
		}

		internal IRegistryItem GetOrAdd(string key, Func<IRegistryItem> create)
		{
			return items.GetOrAdd(key, _ => new RegistryItemHolder(create())).Item;
		}

		internal void Add(IRegistryItem item)
		{
			var value = new RegistryItemHolder(item);
			items.AddOrUpdate(item.Name, _ => value, (_, x_) => value);
		}

		internal IRegistryItem GetItem(string key)
		{
			RegistryItemHolder holder;
			if (items.TryGetValue(key, out holder))
			{
				return holder.Item;
			}
			return null;
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		internal void PurgeExpired()
		{
			var itemsCount = items.Count;
			if (itemsCount > ItemThreshold)
			{
				var itemsDictionaryCopiedToBeSortedWithoutConstructorRace = items.ToArray();

				var itemsFilteredAndSorted = itemsDictionaryCopiedToBeSortedWithoutConstructorRace.OrderByDescending(i => i.Value.ElapsedSinceLastUse).TakeWhile(i => i.Value.IsOlderThan(TimeSpan.FromMinutes(TimeoutMinutes))).ToArray();
				foreach (var item in itemsFilteredAndSorted)
				{
					if (itemsCount <= ItemThreshold)
					{
						return;
					}

					items.TryRemove(item.Key, out var ignored);
					itemsCount--;
				}
			}
		}

		public void PurgeAll()
		{
			items.Clear();
		}

		/// <summary>
		/// Called when the user context is set.
		/// See BaseEnvironment.SetUserContext.
		/// </summary>
		public void PurgeAllIfUpdatedByUser()
		{
			int currentRegistryVersion = GetCurrentRegistryVersion();
			if (currentRegistryVersion != registryCacheVersion)
			{
				PurgeAll();
				registryCacheVersion = currentRegistryVersion;
			}
		}

		int GetCurrentRegistryVersion()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				var bytes = registryDataAccessor.GetBinaryValue(RawDataRegistry.RegistryUserUpdateVersionItemName, Guid.Empty, Guid.Empty);
				return bytes == null ? 0 : new IntRegistryDataType().Deserialise(bytes);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		internal void PurgeIfOlderThan(string key, TimeSpan ageSinceLastAccess)
		{
			RegistryItemHolder holder;
			if (items.TryGetValue(key, out holder) && holder.IsOlderThan(ageSinceLastAccess))
			{
				items.TryRemove(key, out holder);
			}
		}

		static RegistryItemDictionary instance;
		readonly ConcurrentDictionary<string, RegistryItemHolder> items = new ConcurrentDictionary<string, RegistryItemHolder>();
		const int ItemThreshold = 25;

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int TimeoutMinutes = 30;
		int registryCacheVersion;

		#region IRegistryItemDictionaryInternals Members
#if DEBUG

		int IRegistryItemDictionaryInternals.Count
		{
			get { return items.Count; }
		}

		int IRegistryItemDictionaryInternals.ItemThreshold
		{
			get { return ItemThreshold; }
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IRegistryItemDictionaryInternals.TimeoutMinutes
		{
			get { return TimeoutMinutes; }
		}

		RegistryItemHolder IRegistryItemDictionaryInternals.GetHolder(string key)
		{
			RegistryItemHolder result;
			items.TryGetValue(key, out result);
			return result;
		}

		void IRegistryItemDictionaryInternals.Purge(string key)
		{
			RegistryItemHolder holder;
			items.TryRemove(key, out holder);
		}

#endif
		#endregion
	}
}
