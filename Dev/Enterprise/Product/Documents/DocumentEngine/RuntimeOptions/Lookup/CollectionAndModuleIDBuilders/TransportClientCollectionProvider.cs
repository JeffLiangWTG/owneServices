using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class TransportClientCollectionProvider : CollectionProvider
	{
		public TransportClientCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new OrgHeaderCollection(BusinessObjectFactory);
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new TransportClientCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Organisation;
	}
}
