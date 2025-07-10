using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class EntryHeaderFilterLookups : Customs.Module.EntryHeaderFilterLookups
	{
		public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList EntryInstructionStyleList => EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGrouping(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
	}
}
