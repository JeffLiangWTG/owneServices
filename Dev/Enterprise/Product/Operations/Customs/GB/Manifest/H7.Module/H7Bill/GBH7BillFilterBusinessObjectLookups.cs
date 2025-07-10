using Enterprise.Customs.EU.H7.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Module
{
	public class GBH7BillFilterBusinessObjectLookups : EUH7BillFilterBusinessObjectLookups
	{
		public GBH7BillFilterBusinessObjectLookups(GBH7BillFilterBusinessObject parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CustomsStatusList => (CodeDescriptionPairList)Common.EntryStatusListHelper.EntryStatusList(Factory, "GBH7", string.Empty);
	}
}
