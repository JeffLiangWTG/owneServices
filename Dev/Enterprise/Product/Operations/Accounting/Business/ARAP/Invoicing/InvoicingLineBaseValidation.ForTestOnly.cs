#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoicingLineBaseValidation
	{
		public void CheckAL_JH_ForTestOnly()
		{
			CheckAL_JH();
		}

		public static string EnforceGSTAmountEntry_ForTestOnly => EnforceGSTAmountEntry;

		public static string TaxAmountRangeWarning_ForTestOnly => TaxAmountRangeWarning;
	}
}

#endif
