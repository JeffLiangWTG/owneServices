using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSTransitOfficesCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.EU.NCTS.INCTSDestinationOfficesCollectionProvider
	{
		public NCTSTransitOfficesCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		public override int MaxLength => 10;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(BusinessObjectFactory, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
		}
	}
}
