
namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class EmptyConsolCostValidation : JobConsolCostValidation
	{
		public EmptyConsolCostValidation(JobConsolCost parent)
			: base(parent)
		{
		}

		protected override void CheckE6_InvoiceDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckE6_PaymentDateIsValidZDateTimeRange()
		{
		}
	}
}