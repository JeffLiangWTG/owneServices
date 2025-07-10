using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class ShipmentJobForTaxBranchFact : ShipmentJobFact, IShipmentJobFactForTaxBranch
	{
		public FactLeftJoin<IBranchFact> JobBranch { get; }
		public FactLeftJoin<IDepartmentFact> JobDepartment { get; }
		public FactLeftJoin<IOrganisationWithMainAddressFact> ConsolSendingAgent { get; }
		public FactLeftJoin<IOrganisationWithMainAddressFact> ConsolReceivingAgent { get; }

		public ShipmentJobForTaxBranchFact(
			IJobInvoicingPlugIn jobPlugin,
			IEnvironmentFact environmentFact,
			IJobBranchDepartmentFact jobBranchDepartmentFact,
			IOrganisationWithMainAddressFact localClientFact = null,
			IStaffFact salesRepFact = null, RefUNLOCO origin = null,
			IUNLOCOFact originFact = null,
			RefUNLOCO destination = null,
			IUNLOCOFact destinationFact = null,
			IOrganisationWithMainAddressFact consigneeFact = null,
			IOrganisationWithMainAddressFact consignorFact = null,
			OrgHeader controllingAgent = null,
			IOrganisationWithMainAddressFact controllingAgentFact = null,
			OrgHeader controllingCustomer = null,
			IOrganisationWithMainAddressFact controllingCustomerFact = null,
			OrgHeader pickupAgent = null,
			IOrganisationWithMainAddressFact pickupAgentFact = null,
			OrgHeader deliveryAgent = null,
			IOrganisationWithMainAddressFact deliveryAgentFact = null,
			IOrganisationWithMainAddressFact pickupLocalTransportFact = null,
			IOrganisationWithMainAddressFact deliveryLocalTransportFact = null,
			IOrganisationWithMainAddressFact consolSendingAgentFact = null,
			IOrganisationWithMainAddressFact consolReceivingAgentFact = null) : base(jobPlugin, environmentFact, localClientFact, salesRepFact, origin, originFact, destination, destinationFact, consigneeFact, consignorFact, controllingAgent, controllingAgentFact, controllingCustomer, controllingCustomerFact, pickupAgent, pickupAgentFact, deliveryAgent, deliveryAgentFact, pickupLocalTransportFact, deliveryLocalTransportFact)
		{
			JobBranch = jobBranchDepartmentFact.JobBranch;
			JobDepartment = jobBranchDepartmentFact.JobDepartment;
			ConsolSendingAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(consolSendingAgentFact);
			ConsolReceivingAgent = new FactLeftJoin<IOrganisationWithMainAddressFact>(consolReceivingAgentFact);
		}
	}
}
