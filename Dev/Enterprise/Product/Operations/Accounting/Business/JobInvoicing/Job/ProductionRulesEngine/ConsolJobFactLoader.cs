using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolJobFactLoader : BaseJobFactLoader, IConsolJobFactLoader
	{
		public ConsolJobFactLoader(RulesContextType rulesContextType) : base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var invoiceSupporter = parentPlugin.InvoicingSupporter;
			var sendingAgent = invoiceSupporter.SendingAgent;
			var sendingAgentFact = CreateOrNull(sendingAgent, factAccumulator.CreateUniqueOrganisationFact);
			var receivingAgent = invoiceSupporter.ReceivingAgent;
			var receivingAgentFact = CreateOrNull(receivingAgent, factAccumulator.CreateUniqueOrganisationFact);
			var loadPort = invoiceSupporter.Origin;
			var loadPortFact = CreateOrNull(loadPort, factAccumulator.CreateUniqueUNLOCOFact);
			var dischargePort = invoiceSupporter.Destination;
			var dischargePortFact = CreateOrNull(dischargePort, factAccumulator.CreateUniqueUNLOCOFact);

			var jobFact = RulesContextType == RulesContextType.JobBillingTaxBranchDefaulting
				? new ConsolJobForTaxBranchFact(parentPlugin, EnvironmentFact, JobBranchDepartmentFact, LocalClientFact, SalesRepFact, sendingAgent, sendingAgentFact, receivingAgent, receivingAgentFact, loadPort, loadPortFact, dischargePort, dischargePortFact)
				: new ConsolJobFact(parentPlugin, EnvironmentFact, LocalClientFact, SalesRepFact, sendingAgent, sendingAgentFact, receivingAgent, receivingAgentFact, loadPort, loadPortFact, dischargePort, dischargePortFact);

			return new[] { jobFact };
		}
	}
}
