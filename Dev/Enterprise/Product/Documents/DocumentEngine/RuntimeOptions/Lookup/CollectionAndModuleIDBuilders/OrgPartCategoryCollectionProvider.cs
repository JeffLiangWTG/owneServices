using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class OrgPartCategoryCollectionProvider : CollectionProvider
	{
		public OrgPartCategoryCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new OrgPartCategoryCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(OrgPartCategory)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new OrgPartCategoryCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefOrgPartCategory;
	}
}
