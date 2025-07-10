#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class PaymentValidation
	{
		public void CheckChequeBook_ForTestOnly()
		{
			CheckChequeBook();
		}
	}
}

#endif
