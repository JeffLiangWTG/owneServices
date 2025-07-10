using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SupplierCollectionProvider : CollectionProvider
	{
		public SupplierCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new ConsignorCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Organisation;
	}
}
