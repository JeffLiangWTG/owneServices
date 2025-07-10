using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class LandTransportJobFact : JobFact, ILandTransportJobFact
	{
		public LandTransportJobFact(
			IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			ILandTransportJobShipmentFact shipmentFact,
			ILandTransportJobWarehouseFact warehouseFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null)
			: base(jobPlugin?.InvoicingSupporter?.Job, environmentFact, localClientFact, salesRepFact)
		{
			ParentWarehouseOrder = new FactLeftJoin<ILandTransportJobWarehouseFact>(warehouseFact);
			ParentForwardingShipment = new FactLeftJoin<ILandTransportJobShipmentFact>(shipmentFact);
		}

		public FactLeftJoin<ILandTransportJobWarehouseFact> ParentWarehouseOrder { get; }

		public FactLeftJoin<ILandTransportJobShipmentFact> ParentForwardingShipment { get; }
	}
}
