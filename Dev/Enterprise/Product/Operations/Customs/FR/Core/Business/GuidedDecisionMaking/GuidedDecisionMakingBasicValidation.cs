using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingBasicValidation : EU.Business.GuidedDecisionMakingBasicValidation
	{
		public GuidedDecisionMakingBasicValidation(GuidedDecisionMakingBasic parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateRegionOrTerritoryOfDestination();
		}

		public void ValidateRegionOrTerritoryOfDestination()
		{
			validationInternals.Validate(Parent.RegionOrTerritoryOfDestinationInfo, new RunValidationInvoker(CheckRegionOrTerritoryOfDestination));
		}

		protected virtual void CheckRegionOrTerritoryOfDestination()
		{
			MandatoryValidation.CheckEntered(Parent.RegionOrTerritoryOfDestinationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RegionOrTerritoryOfDestinationInfo);
		}

		public new GuidedDecisionMakingBasic Parent => (GuidedDecisionMakingBasic)base.Parent;
	}
}
