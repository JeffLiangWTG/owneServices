using System;
using System.Collections.Concurrent;
using Enterprise.Integration;
using static Enterprise.ZArchitecture.Environment.RawDataRegistry;

namespace Enterprise.ZArchitecture.Environment
{
	public class SplitterLayoutRegistry
	{
		#region Check cache

		public bool ContainsKey(string key)
		{
			return cache.ContainsKey(key);
		}

		#endregion

		#region Get Form Details

		public decimal GetSplitterLayout(string key)
		{
			var result = 0m;

			if (!cache.TryGetValue(key, out result))
			{
				var splitterLayout = GetSplitterLayoutRegistryItem(key);
				var currentUser = EnvProxy.Instance.CurrentUser;
				var spliterDistance = (currentUser != null) ? splitterLayout.GetValueWithoutFallback(currentUser.PK, Guid.Empty, Guid.Empty) : 0;

				if (spliterDistance != 0)
				{
					result = spliterDistance;
					AddOrUpdateSplitterInCache(key, result);
				}
			}

			return result;
		}

		#endregion

		#region Set Form Details

		public void SetSplitterLayout(string key, decimal splitterDistance)
		{
			var currentUser = EnvProxy.Instance.CurrentUser;
			if (currentUser != null)
			{
				var splitterLayout = GetSplitterLayoutRegistryItem(key);
				splitterLayout.SetValue(currentUser.PK, Guid.Empty, Guid.Empty, splitterDistance);
				AddOrUpdateSplitterInCache(key, splitterDistance);
			}
		}

		#endregion

		#region Clear Splitter Details

		public void ClearSplitterLayout(string key)
		{
			SetSplitterLayout(key, 0);
			decimal size;
			cache.TryRemove(key, out size);
		}

		#endregion

		#region Implementation

		readonly ConcurrentDictionary<string, decimal> cache = new ConcurrentDictionary<string, decimal>();

		void AddOrUpdateSplitterInCache(string splitterKey, decimal splitterPosition)
		{
			cache.AddOrUpdate(splitterKey, splitterPosition, (key, oldForm) => splitterPosition);
		}

		DecimalRegistryItem GetSplitterLayoutRegistryItem(string key)
		{
			return new DecimalRegistryItem(key, Categories.Forms, null, null, RegistryStorageFlags.Company, RegistryOptions.NotLogged | RegistryOptions.IsHidden);
		}

		#endregion
	}
}
