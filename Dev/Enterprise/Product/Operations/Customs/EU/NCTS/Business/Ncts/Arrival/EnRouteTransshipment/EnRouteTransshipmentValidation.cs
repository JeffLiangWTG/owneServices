using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteTransshipmentValidation : CusInBondEventValidation
	{
		public EnRouteTransshipmentValidation(EnRouteTransshipment enRouteTransshipment) : base(enRouteTransshipment)
		{ }

		protected new EnRouteTransshipment Parent => (EnRouteTransshipment)base.Parent;

		protected override void CheckBN_EventCountryCode()
		{
			base.CheckBN_EventCountryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BN_EventCountryCodeInfo);
		}

		protected override void CheckBN_TransportCountryCode()
		{
			base.CheckBN_TransportCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BN_TransportCountryCodeInfo);
		}

		protected override void CheckBN_EndorsementDate()
		{
			base.CheckBN_EndorsementDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BN_EndorsementDateInfo);
		}

		protected override void CheckBN_EndorsementCountryCode()
		{
			base.CheckBN_EndorsementCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BN_EndorsementCountryCodeInfo);
		}

		protected override void CheckBN_EndorsementPlace()
		{
			base.CheckBN_EndorsementPlace();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BN_EndorsementPlaceInfo);
		}

		protected override void CheckBN_TransportAtDepartureID()
		{
			base.CheckBN_TransportAtDepartureID();
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.Header?.IsInPhase5TransitionPeriod ?? false, parent.BN_TransportAtDepartureIDInfo, 27);
		}
	}
}
