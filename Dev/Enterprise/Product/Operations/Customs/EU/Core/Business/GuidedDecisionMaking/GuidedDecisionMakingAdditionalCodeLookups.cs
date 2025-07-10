using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class GuidedDecisionMakingAdditionalCodeLookups : ZLookups
	{
		public GuidedDecisionMakingAdditionalCodeLookups(GuidedDecisionMakingAdditionalCode parent) : base(parent)
		{
		}

		protected new GuidedDecisionMakingAdditionalCode Parent => (GuidedDecisionMakingAdditionalCode)base.Parent;

		public ZZRefCusCodeListCombinedCollection AdditionalCodesList => Factory.GetCachedValue("EU.GuidedDecisionMakingAdditionalCodeLookups.AdditionalCodesList_" + Parent.Parent.DataGrouping, () =>
		{
			return new ZZRefCusCodeListCombinedCollection(Factory, Parent.Parent.DataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, ZDateTime.Today);
		});

		public CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions
			=> Universal.RefCusCodeListTypes.GetCachedList(Factory,
				Parent.Parent.DataGrouping,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes,
				ZDateTime.Today,
				languageCode: Parent.Parent.PreferredLanguage);
	}
}
