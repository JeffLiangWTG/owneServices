namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override bool ShouldCalculateDutiesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine) => true;

		protected override bool ShouldCalculateTaxesForEntryLine(EU.Business.Declaration.CusEntryLine entryLine) => ShouldCalculateDutiesForEntryLine(entryLine);
	}
}
