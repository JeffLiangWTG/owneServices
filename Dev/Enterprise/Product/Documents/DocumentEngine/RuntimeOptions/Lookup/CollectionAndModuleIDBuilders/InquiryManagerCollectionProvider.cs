using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class InquiryManagerCollectionProvider : CollectionProvider
	{
		public InquiryManagerCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new SalesEnquiryCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(SalesEnquiry)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new SalesEnquiryCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.SalesEnquiry;
	}
}
