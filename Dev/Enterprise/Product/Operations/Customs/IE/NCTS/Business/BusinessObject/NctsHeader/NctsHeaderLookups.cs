using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
	{
		public NctsHeaderLookups(NctsHeader parent) : base(parent)
		{
		}

		public new NctsHeader Parent => (NctsHeader)base.Parent;

		public override CodeDescriptionPairList NctsMessageStatusList => Factory.GetCachedValue("IE.NCTS.Business.NctsMessageStatusList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new NCTS5ArrivalCustomsStatusList());
			result.AddRange(new NCTS5DepartureCustomsStatusList());
			result.SortByDescriptionAndCombineIfSameCode();
			return result;
		});
	}
}
