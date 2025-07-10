namespace Enterprise.Customs.DE.Business.Declaration
{
	public class LineMerger : EU.Business.Declaration.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			var result = Declaration.CreateEntryCreationStrategy();
			return new Customs.Business.EntryCreationStrategy[] { result };
		}

		protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
