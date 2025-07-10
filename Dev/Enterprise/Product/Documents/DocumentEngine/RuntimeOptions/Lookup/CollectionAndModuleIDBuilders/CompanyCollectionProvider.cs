using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class GlbCompanyCollectionProvider : CollectionProvider
	{
		public GlbCompanyCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new GlbCompanyCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(GlbCompany)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new GlbCompanyCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbCompany;
	}
}
