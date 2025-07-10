using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GBAddInfoCusEntryHeaderLookups : EU.Business.Declaration.AddInfoCusEntryHeaderLookups
	{
		public GBAddInfoCusEntryHeaderLookups(EU.Business.Declaration.AddInfoCusEntryHeader parent) : base(parent)
		{
		}

		public CodeDescriptionPairList AmendmentReasonCodeList => Factory.GetCachedValue<AmendmentCancellationReasonCode>();
	}
}
