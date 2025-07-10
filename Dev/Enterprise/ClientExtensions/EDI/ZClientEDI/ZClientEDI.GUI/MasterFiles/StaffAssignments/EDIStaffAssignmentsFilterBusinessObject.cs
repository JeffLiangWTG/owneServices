using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class EDIStaffAssignmentsFilterBusinessObject : StaffAssignmentsFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var productFilter = filters.AddTextFilter("Product", OrgStaffAssignmentsSchema.O8_Product, IncidentDetailsLookupsHelper.ProductList);
			productFilter.MultilingualDescription = ResString.GetMultilingualString("207cd577-4a95-41b6-ba00-9e3946886d0a", "Product");
			productFilter.Visibility = FilterVisibility.AlwaysVisible;
		}
	}
}
