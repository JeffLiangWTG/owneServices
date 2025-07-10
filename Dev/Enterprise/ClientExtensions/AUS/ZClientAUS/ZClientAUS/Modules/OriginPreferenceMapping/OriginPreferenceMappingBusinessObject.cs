using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Modules
{
	public class OriginPreferenceMappingBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddGuidFilter("Importer", ModuleIDs.Organisation, ClientAUSOriginPreferenceMappingSchema.T7_OH_Importer, OrganisationList);
			filters.AddGuidFilter("Supplier", ModuleIDs.Organisation, ClientAUSOriginPreferenceMappingSchema.T7_OH_Supplier, OrganisationList);
			filters.AddNkFilter("Origin", ClientAUSOriginPreferenceMappingSchema.T7_RN_NKOrigin, ModuleIDs.RefCountry, OriginList);

			return filters;
		}

		#region Lookups

		OrgHeaderCollection OrganisationList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		RefCountryCollection OriginList
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion
	}
}
