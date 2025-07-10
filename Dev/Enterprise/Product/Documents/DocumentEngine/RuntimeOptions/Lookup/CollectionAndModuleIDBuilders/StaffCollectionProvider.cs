using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class StaffCollectionProvider : CollectionProviderWithCodeSupport
	{
		public StaffCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			//Need to set the relationship filter on your collection if you need a master detail relationship between findboxes...
			return new GlbStaffCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(GlbStaff)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new GlbStaffCollection(BusinessObjectFactory, Filter);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbStaff;

		public override int MaxLength => GlbStaffSchema.GS_Code.MaxLength;
	}
}
