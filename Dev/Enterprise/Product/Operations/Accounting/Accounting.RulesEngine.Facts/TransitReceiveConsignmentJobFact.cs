using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class TransitReceiveConsignmentJobFact : JobFact, ITransitReceiveConsignmentJobFact
	{
		public TransitReceiveConsignmentJobFact(IJobInvoicingPlugIn jobPlugin,
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
				var receiveConsignment = jobPlugin as IConsignment;
				ReceiveConsignmentPK = jobPlugin.PK.IsValid ? jobPlugin.PK.ToGuid() : Guid.Empty;

				var warehouse = receiveConsignment.Warehouse;
				var warehouseFact = warehouse == null ? null : new WarehouseFact(warehouse.PK.ToGuid(), warehouse.WW_WarehouseCode);

				Warehouse = new FactLeftJoin<IWarehouseFact>(warehouseFact);
				Direction = receiveConsignment.Direction;
				TransportMode = receiveConsignment.TransportMode;
			}
		}

		public Guid ReceiveConsignmentPK { get; }
		public FactLeftJoin<IWarehouseFact> Warehouse { get; }
		public string Direction { get; }
		public string TransportMode { get; }
	}
}
