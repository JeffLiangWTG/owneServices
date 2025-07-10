using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public sealed class CLMessageChooserLookups : MessageChooserLookups
	{
		public CLMessageChooserLookups(CLMessageChooser parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList AmendReasonList
		{
			get
			{
				var isAir = Parent.Header.IsAir;
				return Parent.Header.Factory.GetCachedValue("CLMessageChooserLookups.AmendReasonList" + isAir, () =>
				{
					return isAir ? new AirAmendReasonCodeList() : (CodeDescriptionPairList)new AmendReasonCodeList();
				});
			}
		}

		public CodeDescriptionPairList AmendTypeList => Parent.Header.Factory.GetCachedValue<AmendTypeCodeList>();
	}
}
