using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public class CDSLineMerger : EU.Business.Declaration.LineMerger
	{
		public CDSLineMerger(JobDeclaration declaration)
			: base(declaration)
		{
			jobDeclaration = declaration;
		}
		readonly JobDeclaration jobDeclaration;

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => new Customs.Business.EntryCreationStrategy[] { jobDeclaration.CreateEntryCreationStrategy() };

		protected override Customs.Business.IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new CDSDutyCalculatorStrategy(jobDeclaration);
	}
}
