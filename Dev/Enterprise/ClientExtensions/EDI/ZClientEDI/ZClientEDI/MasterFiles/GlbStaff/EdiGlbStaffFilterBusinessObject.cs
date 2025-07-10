using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class EdiGlbStaffFilterBusinessObject : GlbStaffFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			var domesticNamefilter = filters.AddTextFilter("Domestic Name", EdiGlbStaffExSchema.GS9_DomesticName);
			domesticNamefilter.SubGroup = new DomesticNameSubGroup();
			domesticNamefilter.MultilingualDescription = ResString.GetMultilingualString("EB3D1DAA-E571-4D59-8BC6-2219158C5CA1", "Domestic Name");
			domesticNamefilter.ShowDescription = false;
			return filters;
		}

		class DomesticNameSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(GlbStaff));
				var subQuery = new ZDBOnlySubQuery(typeof(EdiGlbStaffEx), EdiGlbStaffExSchema.GS9_GS);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
