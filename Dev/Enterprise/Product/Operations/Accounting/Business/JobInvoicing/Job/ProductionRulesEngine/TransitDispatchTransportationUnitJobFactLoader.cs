using System.Collections.Generic;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class TransitDispatchTransportationUnitJobFactLoader : BaseJobFactLoader, ITransitDispatchTransportationUnitJobFactLoader
	{
		public TransitDispatchTransportationUnitJobFactLoader(RulesContextType rulesContextType)
			: base(rulesContextType)
		{
		}

		protected override IEnumerable<IInputFact> GetFactsCore(IJobInvoicingPlugIn parentPlugin, FactAccumulator factAccumulator)
		{
			var jobFact = new TransitDispatchTransportationUnitJobFact(parentPlugin, EnvironmentFact, LocalClientFact, SalesRepFact);

			return new[] { jobFact };
		}
	}
}
