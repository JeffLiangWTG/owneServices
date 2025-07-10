#if DEBUG

namespace Enterprise.Accounting.Registry.Business
{
	public partial class InvoiceTotalRounding
	{
		public void ValidateCurrency_ForTestOnly()
		{
			ValidateCurrency();
		}

		public void ValidateRoundingOption_ForTestOnly()
		{
			ValidateRoundingOption();
		}
	}
}

#endif
