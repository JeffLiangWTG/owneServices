using Enterprise.Accounting.ElectronicMessaging.efatura.uyumsoft.com.tr;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	internal class PaymentMeans
	{
		const string ZZZ = "ZZZ";

		internal PaymentMeansType[] BuildPaymentMeans(EInvoiceHelper helper)
		{
			var paymentMeans = new PaymentMeansType()
			{
				PaymentMeansCode = new PaymentMeansCodeType()
				{
					Value = helper.UInvoice.AgreedPaymentMethod.HasValue ? helper.UInvoice.AgreedPaymentMethod.Value.ToString() : ZZZ  // Payment Means codes document is added to e-docs.
				},
				PaymentDueDate = new PaymentDueDateType()
				{
					Value = helper.UInvoice.DueDate.Value.ToDateTime()
				}
			};

			var bankAccount = helper.GetBankAccount();
			if (bankAccount != null)
			{
				paymentMeans.PayeeFinancialAccount = new FinancialAccountType()
				{
					ID = new IDType()
					{
						Value = bankAccount.IBAN
					},
					CurrencyCode = new CurrencyCodeType()
					{
						Value = bankAccount.AccountCurrency.Code
					},
					PaymentNote = new PaymentNoteType()
					{
						Value = bankAccount.AB_Desc
					}
				};
			}
			return new PaymentMeansType[] { paymentMeans };
		}
	}
}
