using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class RefContainerCollectionProvider : CollectionProvider
	{
		public RefContainerCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefContainerCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefContainer)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefContainerCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefContainer;
	}
}
