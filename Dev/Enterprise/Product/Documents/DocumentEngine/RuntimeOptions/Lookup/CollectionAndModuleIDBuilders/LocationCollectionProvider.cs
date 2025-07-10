using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class LocationCollectionProvider : CollectionProviderWithCodeSupport
	{
		public LocationCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			var requiredZoneTypes = new ZoneTypeList();
			requiredZoneTypes.Add(ZoneTypeCodeDescriptionPair.All);
			requiredZoneTypes.Add(ZoneTypeCodeDescriptionPair.Reporting);
			return new LocationCollection(BusinessObjectFactory, requiredZoneTypes);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Location;

		public override int MaxLength => RefUNLOCOSchema.RL_Code.MaxLength;
	}
}
