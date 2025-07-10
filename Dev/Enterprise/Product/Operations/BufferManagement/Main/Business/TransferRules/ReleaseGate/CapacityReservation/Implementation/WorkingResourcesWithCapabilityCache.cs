using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class WorkingResourcesWithCapabilityCache
	{
		internal WorkingResourcesWithCapabilityCache(BMComponent buffer)
		{
			factory = buffer.Factory;
			context = WorkingTimeContext.Create(buffer);
		}

		readonly BusinessObjectFactory factory;
		readonly WorkingTimeContext context;
		readonly Dictionary<Tuple<ZGuid, ZGuid>, IEnumerable<GlbStaff>> staffByCapabilityAndReleaseGroupCache = new Dictionary<Tuple<ZGuid, ZGuid>, IEnumerable<GlbStaff>>();

		internal IEnumerable<GlbStaff> GetWorkingResourcesWithCapability(ZGuid capabilityPK, ZGuid releaseGroupPK)
		{
			var key = Tuple.Create(capabilityPK, releaseGroupPK);

			return staffByCapabilityAndReleaseGroupCache.GetOrAdd(key, () =>
			{
				return WorkingResourcesCalculator.GetWorkingResourcesWithCapability(capabilityPK, factory, context, releaseGroupPK);
			});
		}
	}
}
