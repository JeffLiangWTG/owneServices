#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class APPayment
	{
		public HotCheque.AccHotCheque FImportedHotCheque_ForTestOnly
		{
			get { return fImportedHotCheque; }
			set { fImportedHotCheque = value; }
		}

		public HotCheque.AccHotChequeCollection GetActiveHotCheques_ForTestOnly()
		{
			return GetActiveHotCheques();
		}

		public void PopulateFieldsUsingHotCheque_ForTestOnly(HotCheque.AccHotCheque hotCheque)
		{
			PopulateFieldsUsingHotCheque(hotCheque);
		}

		public void BeginImportingHotCheque_ForTestOnly()
		{
			BeginImportingHotCheque();
		}

		public void FireNotifyUserPaymentUneditable_ForTestOnly(string message)
		{
			FireNotifyUserPaymentUneditable(message);
		}

		public void FinishImportingHotCheque_ForTestOnly()
		{
			FinishImportingHotCheque();
		}
	}
}

#endif
