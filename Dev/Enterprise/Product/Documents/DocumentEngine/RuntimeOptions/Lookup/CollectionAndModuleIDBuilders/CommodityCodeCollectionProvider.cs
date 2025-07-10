using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class RefCommodityCodeCollectionProvider : CollectionProvider
	{
		public RefCommodityCodeCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			if (Globals.IsWeb)
			{
				return GetCollectionForFindbox();
			}
			return new RefCommodityCodeCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefCommodityCode)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefCommodityCodeCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefCommodityCode;
	}
}
