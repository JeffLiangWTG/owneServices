using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Module
{
	public class EntryHeaderFilterLookups : EU.Module.EntryHeaderFilterLookups
	{
		public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public CodeDescriptionPairList ParallelList => Factory.GetCachedValue<YesNoList>();
	}
}
