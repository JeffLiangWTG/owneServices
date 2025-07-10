//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientIncidentEstimateValidation
//
//    This class should be used for overriding validation in AutoClientIncidentEstimateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	using CargoWise.EntityFramework;

	public class ClientIncidentEstimateValidation : AutoClientIncidentEstimateValidation
	{
		public ClientIncidentEstimateValidation(AutoClientIncidentEstimate parent) : base(parent)
		{
		}

		new ClientIncidentEstimate Parent
		{
			get { return (ClientIncidentEstimate)base.Parent; }
		}

		protected override void CheckCIE_PaymentTerms()
		{
			base.CheckCIE_PaymentTerms();
			ListValidation.ErrorIfInvalidCode(Parent.CIE_PaymentTermsInfo);
		}

		protected override void CheckCIE_ExpressDeliveryOptionCutOffDateUTC()
		{
			base.CheckCIE_ExpressDeliveryOptionCutOffDateUTC();
			TypeValidation.CheckValidZDateWithoutRange(Parent.CIE_ExpressDeliveryOptionCutOffDateUTCInfo);
		}

		protected override void CheckCIE_RX_NKCurrency()
		{
			base.CheckCIE_RX_NKCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.CIE_RX_NKCurrencyInfo);

			if (!Parent.CIE_CancellationFee.IsEmpty || !Parent.CIE_MinEstimateMonthly.IsEmpty ||
				!Parent.CIE_MaxEstimateMonthly.IsEmpty || !Parent.CIE_MinEstimateOneoff.IsEmpty ||
				!Parent.CIE_MaxEstimateOneoff.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CIE_RX_NKCurrencyInfo);
			}
		}
	}
}

