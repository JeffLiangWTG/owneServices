using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXJobsConsolidateRunnerForTesting : LVXJobsConsolidateRunner
	{
		public LVXJobsConsolidateRunnerForTesting(BusinessObjectFactory factory)
			: base(new OperationalActionSectionLogWrapper(new DummyOperationalActionSectionLog()), factory)
		{
		}

		public DummyOperationalActionSectionLog ExposedLog
		{
			get
			{
				return (DummyOperationalActionSectionLog)((OperationalActionSectionLogWrapper)Log).log;
			}
		}

		protected override IEnumerable<IConsolidationStrategy> GetConsolidationStrategies(JobDeclaration lvxJob)
		{
			var consolidationStrategy = ConsolidationStrategyDict[lvxJob];
			var wrapper = new JobDeclarationConsolidationOptionsWrapper(lvxJob);

			return new List<IConsolidationStrategy>()
				{
					new ConsolidateByPeriodStrategy(wrapper),
					new ConsolidateByImporterStrategy(wrapper, consolidationStrategy.ConsolidateByImporter),
					new ConsolidateByBranchStrategy(wrapper, consolidationStrategy.ConsolidateByBranch),
					new ConsolidateByProvinceOfClearanceStrategy(wrapper, consolidationStrategy.ConsolidateByProvinceOfClearance),
					new ConsolidateByBrokerStrategy(wrapper, consolidationStrategy.ConsolidateByBroker)
				};
		}

		public Dictionary<JobDeclaration, ConsolidationStrategyForTesting> ConsolidationStrategyDict = new Dictionary<JobDeclaration, ConsolidationStrategyForTesting>();
	}
}
