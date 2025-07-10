using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class DepartureCusTransportMeansValidation : EU.NCTS.Business.DepartureCusTransportMeansValidation
	{
		public DepartureCusTransportMeansValidation(DepartureCusTransportMeans parent)
			: base(parent)
		{
		}

		new DepartureCusTransportMeans Parent => (DepartureCusTransportMeans)base.Parent;

		protected override void CheckTPM_CustomsOffice()
		{
			base.CheckTPM_CustomsOffice();

			var parent = Parent;

			if (!parent.TPM_TypeOfIdentification.IsEmpty
				&& parent.MovementHeaderParent is NctsDepartureMovementHeader movementHeaderParent
				&& parent.Header.Configuration.ValidationRuleConfiguration.IsRuleTR0052Active)
			{
				TransportMeansValidationHelper.CheckOfficeHasRequiredPurpose(movementHeaderParent, parent.TPM_CustomsOfficeInfo);
			}
		}
	}
}
