using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class GridLayoutRegistry
	{
		public bool HasLayoutInDB(string layoutName, Guid categoryPK)
		{
			bool result = false;

			IRegistryItem gridLayout = GridLayout;
			if (gridLayout != null)
			{
				gridLayout.Name = layoutName;
				result = ((IRegistryItemInternals)gridLayout).HasActualValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, categoryPK);
			}

			return result;
		}

		public void DeleteLayout(string layoutName)
		{
			DeleteLayout(layoutName, Guid.Empty);
		}

		public void DeleteLayout(string layoutName, Guid categoryPK)
		{
			IRegistryItem gridLayout = GridLayout;
			if (gridLayout != null)
			{
				gridLayout.Name = layoutName;
				((IRegistryItemInternals)gridLayout).DeleteValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, categoryPK);

				string cacheKey = GetCacheKey(layoutName, categoryPK);
				lock (cache)
				{
					if (cache.ContainsKey(cacheKey))
					{
						cache.Remove(cacheKey);
					}
				}
			}
		}

		public MemoryStream GetGridLayout(string gridName)
		{
			return GetGridLayout(gridName, Guid.Empty);
		}

		public MemoryStream GetGridLayout(string layoutName, Guid categoryPK)
		{
			MemoryStream result = null;
			string cacheKey = GetCacheKey(layoutName, categoryPK);
			byte[] bytes;

			if (cache.TryGetValue(cacheKey, out bytes))
			{
				result = new MemoryStream(bytes);
			}
			else
			{
				IRegistryItem gridLayout = GridLayout;
				if (gridLayout != null)
				{
					gridLayout.Name = layoutName;
					byte[] layoutBytes = (byte[])gridLayout.GetValueWithoutFallback(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, categoryPK);

					if (layoutBytes != null)
					{
						lock (cache)
						{
							cache[cacheKey] = layoutBytes;
						}
						result = new MemoryStream(layoutBytes);
					}
				}
			}

			return result;
		}

		public void SetGridLayout(string layoutName, MemoryStream layoutData)
		{
			SetGridLayout(layoutName, Guid.Empty, layoutData);
		}

		public void SetGridLayout(string layoutName, Guid categoryPK, MemoryStream layoutData)
		{
			IRegistryItem gridLayout = GridLayout;
			if (gridLayout != null)
			{
				byte[] layoutBytes = layoutData.ToArray();

				string cacheKey = GetCacheKey(layoutName, categoryPK);
				byte[] cachedBytes = null;

				cache.TryGetValue(cacheKey, out cachedBytes);

				string layoutString = layoutBytes == null ? "" : Encoding.ASCII.GetString(layoutBytes);
				string cachedString = cachedBytes == null ? "" : Encoding.ASCII.GetString(cachedBytes);

				if (!layoutString.Equals(cachedString))
				{
					gridLayout.Name = layoutName;
					gridLayout.SetValue(EnvProxy.Instance.CurrentUser.PK, Guid.Empty, categoryPK, layoutBytes);
					lock (cache)
					{
						cache[cacheKey] = layoutBytes;
					}
				}
			}
		}

		public readonly static int KeyMaxLength = 300;

		public void InvalidateCache()
		{
			cache.Clear();
		}

		#region Implementation

		readonly Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();

		protected virtual IRegistryItem GridLayout
		{
			get
			{
				DataRegistry registry = EnvProxy.Instance.Registry;
				return (registry == null) ? null : registry.RawRegistry.GridLayout;
			}
		}

		protected string GetCacheKey(string gridName, Guid categoryPK)
		{
			return gridName + "|" + categoryPK;
		}

		#endregion
	}
}
