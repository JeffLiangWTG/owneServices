using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class CommunicationCollectionProvider : CollectionProvider
	{
		public CommunicationCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new OrgSalesCallCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(OrgSalesCall)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new OrgSalesCallCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Communication;
	}
}
