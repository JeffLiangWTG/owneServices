using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class ViewLocationCollectionProvider : CollectionProvider
	{
		public ViewLocationCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new ViewLocationCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(ViewLocation)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new ViewLocationCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.ViewLocation;
	}
}
