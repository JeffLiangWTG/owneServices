using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList SpecialMentionsList => SpecialMentionsListCore;

		protected virtual CodeDescriptionPairList SpecialMentionsListCore
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSSpecialMentions); }
		}
	}
}
