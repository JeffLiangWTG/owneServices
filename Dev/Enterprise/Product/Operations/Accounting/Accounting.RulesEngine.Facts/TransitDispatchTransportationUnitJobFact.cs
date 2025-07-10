using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class TransitDispatchTransportationUnitJobFact : JobFact, ITransitDispatchTransportationUnitJobFact
	{
		public TransitDispatchTransportationUnitJobFact(IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			Warehouse = new FactLeftJoin<IWarehouseFact>(null);
			TransportMode = string.Empty;

			if (jobPlugin is IWhsItemDispatchTransportationUnit)
			{
				var dispatchTransportationUnit = jobPlugin as IWhsItemDispatchTransportationUnit;
				DispatchTransportationUnitPK = jobPlugin.PK.IsValid ? jobPlugin.PK.ToGuid() : Guid.Empty;

				var warehouse = dispatchTransportationUnit.Warehouse;
				var warehouseFact = warehouse == null ? null : new WarehouseFact(warehouse.PK.ToGuid(), warehouse.WW_WarehouseCode);
				Warehouse = new FactLeftJoin<IWarehouseFact>(warehouseFact);

				var latestDispatchConsignment = dispatchTransportationUnit.LatestDispatchConsignment;
				if (latestDispatchConsignment != null)
				{
					TransportMode = latestDispatchConsignment.TransportMode;
				}
				else
				{
					switch(dispatchTransportationUnit.UnitType)
					{
						case TransitWarehouseTransportUnitTypes.Container:
							TransportMode = TransportModes.Sea;
							break;
						case TransitWarehouseTransportUnitTypes.ULD:
							TransportMode = TransportModes.Air;
							break;
						default:
							TransportMode = TransportModes.Road;
							break;
					}
				}
			}
		}

		public Guid DispatchTransportationUnitPK { get; }
		public FactLeftJoin<IWarehouseFact> Warehouse { get; }
		public string TransportMode { get; }
	}
}
