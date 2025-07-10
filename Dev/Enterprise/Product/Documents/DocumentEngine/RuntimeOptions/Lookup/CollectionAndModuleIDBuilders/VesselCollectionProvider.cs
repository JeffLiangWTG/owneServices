using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class RefVesselCollectionProvider : CollectionProviderWithCodeSupport
	{
		public RefVesselCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefVesselCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefVessel)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new RefVesselCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.RefVessel;

		public override int MaxLength => RefVesselSchema.RV_Code.MaxLength;
	}
}
