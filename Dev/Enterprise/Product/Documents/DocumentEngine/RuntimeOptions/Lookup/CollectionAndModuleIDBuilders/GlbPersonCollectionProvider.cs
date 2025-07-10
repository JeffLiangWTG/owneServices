using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class GlbPersonCollectionProvider : CollectionProvider
	{
		public GlbPersonCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new GlbPersonCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(GlbPerson)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new GlbPersonCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbPerson;
	}
}
