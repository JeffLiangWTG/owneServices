using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CACSubLocationCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.CA.ICACSubLocationCollectionProvider
	{
		public CACSubLocationCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new CACSubLocationCollection(BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.SubLocation;

		public override int MaxLength => CACSubLocation.Schema.CodeMaxLength;
	}
}
