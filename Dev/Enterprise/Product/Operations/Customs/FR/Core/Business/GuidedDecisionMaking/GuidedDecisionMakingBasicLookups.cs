using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingBasicLookups : EU.Business.GuidedDecisionMakingBasicLookups
	{
		public GuidedDecisionMakingBasicLookups(EU.Business.GuidedDecisionMakingBasic parent) : base(parent)
		{
		}

		protected new GuidedDecisionMakingBasic Parent => (GuidedDecisionMakingBasic)base.Parent;

		public CodeDescriptionPairList RegionOrTerritoryOfDestinationList => Factory.GetCachedValue<FRDomesticOverseasTerritories>();

		protected override CodeDescriptionPairList PreferenceListCore()
		{
			var parent = Parent;
			var result = base.PreferenceListCore();

			if (result.Count == 0)
			{
				result = Factory.GetCachedValue("FR.GuidedDecisionMakingBasicLookups.PrimaryPreferenceList." + parent.CountryOfOrigin + "." + parent.RegionOrTerritoryOfDestination, delegate
				{
					var list = new CodeDescriptionPairList();

					if (parent.CountryOfOrigin != Core.Constants.CountryCodes.France
						&& Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.Contains(parent.CountryOfOrigin.ToString())
						&& (parent.RegionOrTerritoryOfDestination == FRDomesticOverseasTerritories.Codes.CONTI || parent.RegionOrTerritoryOfDestination == FRDomesticOverseasTerritories.Codes.CORSE))
					{
						list.AddPair(Core.Constants.Customs.Universal.RefCusPreference.Codes._100, Core.Constants.Customs.Universal.RefCusPreference.Descriptions._100);
					}

					return list;
				});
			}
			return result;
		}
	}
}
