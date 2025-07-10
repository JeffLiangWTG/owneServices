using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class TransitReceiveConsignmentJobFactLoader : BaseJobFactLoader, ITransitReceiveConsignmentJobFactLoader
	{
		public TransitReceiveConsignmentJobFactLoader(RulesContextType rulesContextType)
			: base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var jobFact = new TransitReceiveConsignmentJobFact(parentPlugin, EnvironmentFact, LocalClientFact, SalesRepFact);

			return new[] { jobFact };
		}
	}
}
