using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDICreditNoteValidation : CreditNoteValidation
	{
		public EDICreditNoteValidation(EDIARCreditNote parent)
			: base(parent)
		{ }

		protected override void CheckAH_ExchangeRate()
		{
			var parent = (EDIARCreditNote)Parent;
			if (!parent.DisableExchangeRateValidation)
			{
				base.CheckAH_ExchangeRate();
			}
		}
	}
}
