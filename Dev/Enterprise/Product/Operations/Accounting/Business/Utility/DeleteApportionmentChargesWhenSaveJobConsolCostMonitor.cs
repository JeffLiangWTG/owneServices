using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class DeleteApportionmentChargesWhenSaveJobConsolCostMonitor : IService
	{
		public DeleteApportionmentChargesWhenSaveJobConsolCostMonitor()
		{
			Mapping = new Dictionary<ZGuid, ZGuid>();
		}

		public static IDisposable AddTempService(BusinessObjectFactory factory)
		{
			return new DisposableAction(
				() => factory.ServiceContainer.AddService(new DeleteApportionmentChargesWhenSaveJobConsolCostMonitor()),
				() => factory.ServiceContainer.RemoveService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>());
		}

		public void CollectApportionmentChargesInfo(JobConsolCost cost)
		{
			if (!cost.IsInDatabase)
			{
				cost.ApportionmentCharges?.ToList().ForEach(charge =>
				{
					if (Mapping.ContainsKey(charge.PK))
					{
						Mapping[charge.PK] = cost.PK;
					}
					else
					{
						Mapping.Add(charge.PK, cost.PK);
					}
				});
			}
		}

		public bool TryGetRelativeJobConsolCostPK(JobCharge charge, out ZGuid jobConsolCostPK )
		{
			return Mapping.TryGetValue(charge.PK, out jobConsolCostPK);
		}

		Dictionary<ZGuid, ZGuid> Mapping { get; }

#if DEBUG
		public Dictionary<ZGuid, ZGuid> Mapping_ForTestOnly => Mapping;
#endif
	}
}
