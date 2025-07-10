namespace Enterprise.Customs.IE.Business.Declaration
{
	public class LineMerger : EU.Business.Declaration.LineMerger
	{
		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);
	}
}
