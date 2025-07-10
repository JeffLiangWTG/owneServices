using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingBasicValueSetStrategy : EU.Business.GuidedDecisionMakingBasicValueSetStrategy
	{
		public GuidedDecisionMakingBasicValueSetStrategy(EU.Business.GuidedDecisionMakingBasic guidedDecisionMakingBasic) : base(guidedDecisionMakingBasic)
		{
		}

		protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			base.ValueSetCore(valueThatHasChanged, oldValue);
			switch (valueThatHasChanged.Name)
			{
				case GuidedDecisionMakingBasic.Schema.RegionOrTerritoryOfDestination:
					guidedDecisionMakingBasic.ClearAdditionalCodesCache();
					guidedDecisionMakingBasic.ClearVATApplicabilitiesCache();
					break;
			}
		}
	}
}
