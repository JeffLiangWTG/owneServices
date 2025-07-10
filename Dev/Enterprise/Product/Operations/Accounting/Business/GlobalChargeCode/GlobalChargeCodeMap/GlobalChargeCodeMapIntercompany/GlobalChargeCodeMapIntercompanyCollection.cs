using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	[ModuleID(ModuleId.GlobalChargeCodeIntercompany)]
	public class GlobalChargeCodeMapIntercompanyCollection : BusinessObjectCollection<GlobalChargeCodeMapIntercompany>
	{
		public GlobalChargeCodeMapIntercompanyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapSchema.YG_OH, null));
			return filter;
		}
	}
}

