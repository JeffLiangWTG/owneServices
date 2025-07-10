using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiInvoiceValidation : InvoiceValidation
	{
		public EdiInvoiceValidation(EDIARInvoice parent)
			: base(parent)
		{ }

		protected override void CheckAH_ExchangeRate()
		{
			var parent = (EDIARInvoice)Parent;
			if (!parent.DisableExchangeRateValidation)
			{
				base.CheckAH_ExchangeRate();
			}
		}
	}
}
