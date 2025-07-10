using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteSealValidation : Customs.Business.CusInBondEventValidation
	{
		public EnRouteSealValidation(EnRouteSeal enRouteSeal)
			: base(enRouteSeal)
		{
		}

		protected new EnRouteSeal Parent => (EnRouteSeal)base.Parent;

		protected override void CheckBN_EventCountryCode()
		{
			base.CheckBN_EventCountryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.BN_EventCountryCodeInfo);
		}

		protected override void CheckBN_NoOfSeals()
		{
			base.CheckBN_NoOfSeals();
			var parent = Parent;
			var maxNumberOfSeals = parent.BN_NoOfSeals;
			if (parent.SealContainers.Count > maxNumberOfSeals)
			{
				parent.BN_NoOfSealsInfo.AddMessageError(Res.GetString("42C2662C-A332-4930-BDD5-E9B52CC14E9B",
					"The maximum number of seals entered in the grid should be {0}.", maxNumberOfSeals));
			}
		}

		protected override void CheckBN_TransportAtDepartureID()
		{
			base.CheckBN_TransportAtDepartureID();
			var parent = Parent;
			UniversalValidationHelper.CheckMaxLengthIfPhase5TransitionPeriod(parent.Header?.IsInPhase5TransitionPeriod ?? false, parent.BN_TransportAtDepartureIDInfo, 27);
		}
	}
}
