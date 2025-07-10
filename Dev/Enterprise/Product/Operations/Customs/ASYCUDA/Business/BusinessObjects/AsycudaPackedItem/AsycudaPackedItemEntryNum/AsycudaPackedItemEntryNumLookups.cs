using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemEntryNumLookups : CusEntryNumLookups
	{
		public AsycudaPackedItemEntryNumLookups(AsycudaPackedItemEntryNum parent)
			: base(parent)
		{
		}

		public new AsycudaPackedItemEntryNum Parent => (AsycudaPackedItemEntryNum)base.Parent;

		public CodeDescriptionPairList CustomsEntryNumberTypes => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.CE_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes);
	}
}
