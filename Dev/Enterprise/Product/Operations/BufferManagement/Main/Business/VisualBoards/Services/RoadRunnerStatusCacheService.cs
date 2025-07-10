using System;
using System.Collections.Concurrent;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class RoadRunnerStatusCacheService : ResettableService<ConcurrentDictionary<string, ConcurrentDictionary<string, object>>>
	{
		public RoadRunnerStatusCacheService()
			: base(delegate
			{ return new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>(); })
		{
		}

		protected override BoardServiceStalenessPolicy StalenessPolicy => BoardServiceStalenessPolicy.StaleBeforeBoardRefresh | BoardServiceStalenessPolicy.StaleBeforeSavingTicket;

		#region Implementation
		public T GetOrCacheValue<T>(GlbStaff staff, string cacheKey, Func<T> getValueFunc)
		{
			return GetOrCacheValueCore(staff, cacheKey, getValueFunc);
		}

		protected virtual T GetOrCacheValueCore<T>(GlbStaff staff, string cacheKey, Func<T> getValueFunc)
		{
			var cache = this.GetOrCreateStaffCache(staff.GS_Code);
			return (T)cache.GetOrAdd(cacheKey, (key) => getValueFunc());
		}

		ConcurrentDictionary<string, object> GetOrCreateStaffCache(ZString staffCode)
		{
			return this.ServiceData.GetOrAdd(staffCode, (code) => new ConcurrentDictionary<string, object>());
		}

		#endregion

#if DEBUG
		public ConcurrentDictionary<string, object> GetStaffCache_ForTesting(GlbStaff staff)
		{
			return this.GetOrCreateStaffCache(staff.GS_Code);
		}
#endif
	}
}
