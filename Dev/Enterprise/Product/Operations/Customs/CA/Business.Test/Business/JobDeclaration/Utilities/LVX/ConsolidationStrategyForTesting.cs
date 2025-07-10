namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsolidationStrategyForTesting
	{
		public bool ConsolidateByImporter;
		public bool ConsolidateByBranch;
		public bool ConsolidateByProvinceOfClearance;
		public bool ConsolidateByBroker;

		public ConsolidationStrategyForTesting(bool consolidateByImporter, bool consolidateByBranch, bool consolidateByProvinceOfClearance, bool consolidateByBroker)
		{
			ConsolidateByImporter = consolidateByImporter;
			ConsolidateByBranch = consolidateByBranch;
			ConsolidateByBroker = consolidateByBroker;
			ConsolidateByProvinceOfClearance = consolidateByProvinceOfClearance;
		}
	}
}
