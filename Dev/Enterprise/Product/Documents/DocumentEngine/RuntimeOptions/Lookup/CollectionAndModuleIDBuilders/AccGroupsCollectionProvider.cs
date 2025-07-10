using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class AccGroupsCollectionProvider : CollectionProviderWithCodeSupport
	{
		public AccGroupsCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new AccGroupsCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.AccGroups;

		public override int MaxLength => AccGroupsSchema.AR_Code.MaxLength;
	}
}
