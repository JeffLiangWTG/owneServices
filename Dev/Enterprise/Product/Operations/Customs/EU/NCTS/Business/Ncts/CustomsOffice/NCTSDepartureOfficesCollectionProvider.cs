using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NCTSDepartureOfficesCollectionProvider : CollectionProviderWithCodeSupport, Integration.Customs.EU.NCTS.INCTSDepartureOfficesCollectionProvider
	{
		public NCTSDepartureOfficesCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		public override int MaxLength => 10;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override IBusinessObjectCollection CreateCollection()
		{
			return EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(BusinessObjectFactory, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		}
	}
}
