using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CachedRegistryItem<T> : RegistryItemWrapper
	{
		public CachedRegistryItem(string cacheKey, IRegistryItem inner) : base(inner)
		{
			this.cacheKey = cacheKey;
		}

		readonly string cacheKey;

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
#if DEBUG
			ResetCache();
#endif
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		public T Value
		{
			get { return (T)((IRegistryItem)this).Value; }
		}

		public T GetCacheValue()
		{
			return (T)SystemDataRegistryMemoryCacheHandler.GetCacheValue(cacheKey);
		}

		public bool AddCachedValue(object value, DateTimeOffset absoluteExpiration)
		{
			return SystemDataRegistryMemoryCacheHandler.AddCachedValue(cacheKey, value, absoluteExpiration);
		}

#if DEBUG
		public void SetValue(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, T newValue) => SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

		public void ResetCache()
		{
			SystemDataRegistryMemoryCacheHandler.ResetCache(cacheKey);
		}
#endif
	}
}
