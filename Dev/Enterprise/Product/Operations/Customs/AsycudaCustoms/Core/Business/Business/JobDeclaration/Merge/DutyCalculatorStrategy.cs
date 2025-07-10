namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class DutyCalculatorStrategy : Customs.Business.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool ShouldCalculateDuties => false;
	}
}
