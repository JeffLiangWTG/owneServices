using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class ServiceLevelCollectionProvider : CollectionProviderWithCodeSupport
	{
		public ServiceLevelCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new RefServiceLevelCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(RefServiceLevel)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return new RefServiceLevelCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.ServiceLevel;

		public override int MaxLength => RefServiceLevelSchema.RS_Code.MaxLength;
	}
}
