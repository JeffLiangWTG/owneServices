namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxTransactionEmptyValidation : AccTaxTransactionValidation
	{
		public AccTaxTransactionEmptyValidation(AutoAccTaxTransaction parent) : base(parent)
		{
		}

		protected override void CheckATT_Rate()
		{
		}

		protected override void CheckATT_OSTaxAmount()
		{
		}

		protected override void CheckATT_OSTaxBaseAmount()
		{
		}

		protected override void CheckATT_TaxAuthorityServiceCode()
		{
		}

		protected override void CheckATT_TaxAuthorityServiceCodeDescription()
		{
		}
	}
}
