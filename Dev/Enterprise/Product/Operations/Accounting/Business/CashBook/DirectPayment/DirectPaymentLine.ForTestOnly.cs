#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public partial class DirectPaymentLine
	{
		public bool InvertSigns_ForTestOnly => InvertSigns;
	}
}

#endif
