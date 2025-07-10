using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent) : base(parent)
		{
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		public CodeDescriptionPairList KindList => Factory.GetCachedValue("ES.AdditionalInfoLookups.KindList", () => new AdditionalDocList());
	}
}
