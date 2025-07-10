using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class ConsolJobForTaxBranchFact : ConsolJobFact, IConsolJobFactForTaxBranch
	{
		public ConsolJobForTaxBranchFact(IJobInvoicingPlugIn jobPlugin, IEnvironmentFact environmentFact, IJobBranchDepartmentFact jobBranchDepartmentFact, IOrganisationWithMainAddressFact localClientFact = null, IStaffFact salesRepFact = null, OrgHeader sendingAgent = null, IOrganisationWithMainAddressFact sendingAgentFact = null, OrgHeader receivingAgent = null, IOrganisationWithMainAddressFact receivingAgentFact = null, RefUNLOCO loadPort = null, IUNLOCOFact loadPortFact = null, RefUNLOCO dischargePort = null, IUNLOCOFact dischargePortFact = null)
			: base(jobPlugin, environmentFact, localClientFact, salesRepFact, sendingAgent, sendingAgentFact, receivingAgent, receivingAgentFact, loadPort, loadPortFact, dischargePort, dischargePortFact)
		{
			JobBranch = jobBranchDepartmentFact.JobBranch;
			JobDepartment = jobBranchDepartmentFact.JobDepartment;
		}

		public FactLeftJoin<IBranchFact> JobBranch { get; }
		public FactLeftJoin<IDepartmentFact> JobDepartment { get; }
	}
}
