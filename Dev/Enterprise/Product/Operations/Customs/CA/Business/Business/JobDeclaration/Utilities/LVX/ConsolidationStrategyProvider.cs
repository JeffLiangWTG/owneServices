using System.Collections.Generic;

namespace Enterprise.Customs.CA.Business
{
	public static class ConsolidationStrategyProvider
	{
		public static IEnumerable<IConsolidationStrategy> GetConsolidationStrategies(IConsolidationOptionsWrapper wrapper)
		{
			return new List<IConsolidationStrategy>()
			{
				new ConsolidateByPeriodStrategy(wrapper),
				new ConsolidateByImporterStrategy(wrapper),
				new ConsolidateByBranchStrategy(wrapper),
				new ConsolidateByProvinceOfClearanceStrategy(wrapper),
				new ConsolidateByBrokerStrategy(wrapper),
				new ConsolidateToOneFTypePerCLVSEntryStrategy(wrapper)
			};
		}
	}
}
