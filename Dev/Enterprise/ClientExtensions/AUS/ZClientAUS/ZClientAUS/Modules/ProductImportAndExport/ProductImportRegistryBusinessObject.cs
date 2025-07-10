using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Modules
{
	public class ProductImportRegistryBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddGuidFilter("Importer", ModuleIDs.Organisation, ClientAUSProductImportRegistrySchema.T6_OH_Importer, OrganisationList);
			filters.AddGuidFilter("Supplier", ModuleIDs.Organisation, ClientAUSProductImportRegistrySchema.T6_OH_Supplier, OrganisationList);

			return filters;
		}

		#region Lookups

		OrgHeaderCollection OrganisationList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#endregion
	}
}
