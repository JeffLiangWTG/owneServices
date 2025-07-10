
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSProductImportRegistryCollection : BusinessObjectCollection<ClientAUSProductImportRegistry>
	{
		public ClientAUSProductImportRegistryCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ClientAUSProductImportRegistryCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ClientAUSProductImportRegistryCollection(BusinessObjectFactory factory, OrgHeader importer, OrgHeader supplier) : this(factory, GetImporterSupplierFilter(importer, supplier))
		{
			fImporter = importer;
			fSupplier = supplier;

			DefaultModuleFilterFields(fImporter, fSupplier);
		}

		public void DefaultModuleFilterFields(OrgHeader importer, OrgHeader supplier)
		{
			if (importer != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer", "Property", importer.PK));
			}

			if (supplier != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Supplier", "Property", supplier.PK));
			}
		}

		public static ZDBOnlyQuery GetImporterSupplierFilter(OrgHeader importer, OrgHeader supplier)
		{
			ZDBOnlyQuery queryFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			if (importer != null)
			{
				queryFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Importer, importer.PK);
			}

			if (supplier != null)
			{
				queryFilter.AddToFilter(ClientAUSProductImportRegistrySchema.T6_OH_Supplier, supplier.PK);
			}

			return queryFilter;
		}

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			ClientAUSProductImportRegistry importRegistry = (ClientAUSProductImportRegistry)child;
			if (fImporter != null)
			{
				importRegistry.T6_OH_Importer = fImporter.PK;
			}
			if (fSupplier != null)
			{
				importRegistry.T6_OH_Supplier = fSupplier.PK;
			}
		}

		protected readonly OrgHeader fImporter, fSupplier;

		#endregion
	}
}
