//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientIncidentQuoteValidation
//
//    This class should be used for overriding validation in AutoClientIncidentQuoteValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	using CargoWise.EntityFramework;

	public class ClientIncidentQuoteValidation : AutoClientIncidentQuoteValidation
	{
		public ClientIncidentQuoteValidation(AutoClientIncidentQuote parent) : base(parent)
		{
		}

		new ClientIncidentQuote Parent
		{
			get { return (ClientIncidentQuote)base.Parent; }
		}

		protected override void CheckCIQ_PaymentTerms()
		{
			base.CheckCIQ_PaymentTerms();
			ListValidation.ErrorIfInvalidCode(Parent.CIQ_PaymentTermsInfo);
		}

		protected override void CheckCIQ_Type()
		{
			base.CheckCIQ_Type();
			ListValidation.ErrorIfInvalidCode(Parent.CIQ_TypeInfo);
		}

		protected override void CheckCIQ_RX_NKCurrency()
		{
			base.CheckCIQ_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.CIQ_RX_NKCurrencyInfo);

			if (!Parent.CIQ_CancellationFee.IsEmpty || !Parent.CIQ_QuoteAmount.IsEmpty ||
				!Parent.CIQ_OneoffUpfront.IsEmpty || !Parent.CIQ_HeadStartSurcharge.IsEmpty ||
				!Parent.CIQ_ExpressDeliverySurcharge.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CIQ_RX_NKCurrencyInfo);
			}
		}
	}
}

