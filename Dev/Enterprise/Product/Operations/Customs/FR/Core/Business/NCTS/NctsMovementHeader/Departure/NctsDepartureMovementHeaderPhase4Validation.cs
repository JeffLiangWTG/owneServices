using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsDepartureMovementHeaderPhase4Validation : EU.NCTS.Business.NctsDepartureMovementHeaderPhase4Validation
	{
		public NctsDepartureMovementHeaderPhase4Validation(NctsDepartureMovementHeader parent)
		   : base(parent)
		{
		}

		public new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateChargePaymentOrDestinationID();
		}

		public void ValidateChargePaymentOrDestinationID()
		{
			ValidateCalculatedProperty(Parent.ChargePaymentOrDestinationIDInfo);
		}

		protected void CheckChargePaymentOrDestinationID()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.ChargePaymentOrDestinationIDInfo);
			if (parent.ChargePaymentOrDestinationID.IsEmpty)
			{
				if (!UniversalReferenceDataHelper.GetChargePaymentOrDestinationID(Parent.Factory, Parent).IsEmpty())
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ChargePaymentOrDestinationIDInfo);
				}
			}
		}
	}
}
