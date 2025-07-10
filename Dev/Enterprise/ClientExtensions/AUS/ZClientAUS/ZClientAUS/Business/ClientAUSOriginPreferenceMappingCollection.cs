
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSOriginPreferenceMappingCollection : BusinessObjectCollection<ClientAUSOriginPreferenceMapping>
	{
		public ClientAUSOriginPreferenceMappingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ClientAUSOriginPreferenceMappingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ClientAUSOriginPreferenceMappingCollection(BusinessObjectFactory factory, OrgHeader importer, OrgHeader supplier)
			: this(factory, GetImporterSupplierFilter(importer, supplier))
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
				queryFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.T7_OH_Importer, importer.PK);
			}

			if (supplier != null)
			{
				queryFilter.AddToFilter(ClientAUSOriginPreferenceMappingSchema.T7_OH_Supplier, supplier.PK);
			}

			return queryFilter;
		}

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			ClientAUSOriginPreferenceMapping mapper = (ClientAUSOriginPreferenceMapping)child;
			if (fImporter != null)
			{
				mapper.T7_OH_Importer = fImporter.PK;
			}
			if (fSupplier != null)
			{
				mapper.T7_OH_Supplier = fSupplier.PK;
			}
		}

		protected readonly OrgHeader fImporter, fSupplier;

		#endregion
	}
}
