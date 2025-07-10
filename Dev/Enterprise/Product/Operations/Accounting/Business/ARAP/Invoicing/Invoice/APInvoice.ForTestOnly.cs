#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class APInvoice
	{
		public HotCheque.AccHotCheque FImportedHotCheque_ForTestOnly
		{
			get { return fImportedHotCheque; }
			set { fImportedHotCheque = value; }
		}

		public void PopulateFieldsUsingHotCheque_ForTestOnly(HotCheque.AccHotCheque hotCheque)
		{
			PopulateFieldsUsingHotCheque(hotCheque);
		}

		public void SetHotChequeInactiveWhenPosting_ForTestOnly(HotCheque.AccHotCheque hotCheque)
		{
			SetHotChequeInactiveWhenPosting(hotCheque);
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

		public void IncrementChequeCurrentNumber_ForTestOnly()
		{
			IncrementChequeCurrentNumber();
		}
	}
}

#endif
