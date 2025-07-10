using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class CACarrierCombinedCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.CA.ICACarrierCombinedCollectionProvider
	{
		public CACarrierCombinedCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new Universal.ZZRefCarrierCombinedCollection(BusinessObjectFactory, new AdhocCollectionRelationship(typeof(Universal.ZZRefCarrierCombined)));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(BusinessObjectFactory, ZString.Empty);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCarrier;

		public override int MaxLength => AutoCAAddInfo.Schema.CA_CarrierCodeMaxLength;
	}
}
