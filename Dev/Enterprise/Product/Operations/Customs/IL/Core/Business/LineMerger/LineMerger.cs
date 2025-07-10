using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
			CalculateInvoiceAmount();
		}

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => new Customs.Business.EntryCreationStrategy[] { new EntryCreationStrategy(Declaration) };

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);
	}
}
