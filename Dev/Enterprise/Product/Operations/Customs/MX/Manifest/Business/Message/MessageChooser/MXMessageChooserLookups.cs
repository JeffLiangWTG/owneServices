using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.MX.Manifest.Business.CodeDescriptionPairLists;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public sealed class MXMessageChooserLookups : MessageChooserLookups
	{
		public MXMessageChooserLookups(MXMessageChooser parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ReasonList => Parent.Header.Factory.GetCachedValue<SEAReasonCodes>();
	}
}
