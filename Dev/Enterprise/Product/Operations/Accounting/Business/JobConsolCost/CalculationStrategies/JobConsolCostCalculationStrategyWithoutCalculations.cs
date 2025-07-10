using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class JobConsolCost
	{
		#region Calculation Strategies

		public class ConsolCostCalculationStrategyWithoutCalculations : JobConsolCostCalculationStrategyBase
		{
			public ConsolCostCalculationStrategyWithoutCalculations(JobConsolCost cost)
				: base(cost)
			{
			}

			public override void HandleDelete()
			{
				Cost.PaymentBases.DeleteAll();
				Cost.DeleteCostOnly();
			}
		}

		#endregion
	}
}