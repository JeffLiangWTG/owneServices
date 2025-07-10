using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class InternationalZonesCollectionProvider : CollectionProviderWithCodeSupport
	{
		public InternationalZonesCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefZoneHeaderCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefZoneHeader)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new RefZoneHeaderCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.InternationalZone;

		public override int MaxLength => RefZoneHeaderSchema.FZ_Code.MaxLength;
	}
}
