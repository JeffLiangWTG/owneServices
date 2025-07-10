using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class DefaultDiagramEntityCountChangedStrategy : EntityCountChangedStrategy
	{
		internal override void HandleCountChanged(JobNetwork network, IEnumerable<BMNCNShape> entities)
		{
			var duplicates = entities.Where(e => e.BNS_RelatedEntityID.IsValid).FindDuplicates(s => s.BNS_RelatedEntityID, IsFirstCreatedAfterSecond).ToArray();

			if (duplicates.Length == 0) // We expect recursion here.
			{
				base.HandleCountChanged(network, entities);
			}
			else
			{
				using (new DisposableList(duplicates.Select(e => ((ISingleElementListInternal)e).SuspendListChanged())))
				{
					duplicates.ForEach(d => d.Delete());
				}
			}
		}

		static bool IsFirstCreatedAfterSecond(BMNCNShape first, BMNCNShape second)
		{
			if (first.IsInDatabase && second.IsInDatabase)
			{
				return first.BNS_SystemCreateTimeUtc > second.BNS_SystemCreateTimeUtc;
			}
			else if (first.IsInDatabase)
			{
				return false;
			}
			else if (second.IsInDatabase)
			{
				return true;
			}
			else
			{
				return first.InstantiationTime > second.InstantiationTime;
			}
		}
	}
}
