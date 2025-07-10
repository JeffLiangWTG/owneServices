using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SalesTeamCollectionProvider : CollectionProviderWithCodeSupport
	{
		public SalesTeamCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new SalesTeamCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(SalesTeam)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new SalesTeamCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.SalesTeam;

		public override int MaxLength => GlbGroupSchema.GG_Code.MaxLength;
	}
}
