using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class TransitDispatchConsignmentJobFact : JobFact, ITransitDispatchConsignmentJobFact
	{
		public TransitDispatchConsignmentJobFact(IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			Warehouse = new FactLeftJoin<IWarehouseFact>(null);
			Direction = string.Empty;
			TransportMode = string.Empty;

			if (jobPlugin is IConsignment)
			{
				var dispatchConsignment = jobPlugin as IConsignment;
				DispatchConsignmentPK = jobPlugin.PK.IsValid ? jobPlugin.PK.ToGuid() : Guid.Empty;

				var warehouse = dispatchConsignment.Warehouse;
				var warehouseFact = warehouse == null ? null : new WarehouseFact(warehouse.PK.ToGuid(), warehouse.WW_WarehouseCode);

				Warehouse = new FactLeftJoin<IWarehouseFact>(warehouseFact);
				Direction = dispatchConsignment.Direction;
				TransportMode = dispatchConsignment.TransportMode;
			}
		}

		public Guid DispatchConsignmentPK { get; }
		public FactLeftJoin<IWarehouseFact> Warehouse { get; }
		public string Direction { get; }
		public string TransportMode { get; }
	}
}
