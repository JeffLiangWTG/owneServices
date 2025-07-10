using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class RefUNLOCOCollectionProvider : CollectionProviderWithCodeSupport
	{
		public RefUNLOCOCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new RefUNLOCOCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefUNLOCO)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefUNLOCOCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefUNLOCO;

		public override int MaxLength => RefUNLOCOSchema.RL_Code.MaxLength;
	}
}
