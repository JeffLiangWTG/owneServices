using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteTransshipmentLookups : CusInBondEventLookups
	{
		public EnRouteTransshipmentLookups(EnRouteTransshipment parent)
			: base(parent)
		{
		}

		protected new EnRouteTransshipment Parent => (EnRouteTransshipment)base.Parent;

		ZString CountryCode => Parent.Header?.Branch?.Country?.Code ?? GlbCompany.CurrentCompany.Country.Code;

		public ZZRefCusCodeListCombinedCollection EventCountries => Factory.GetCountryList(CountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);

		public CodeDescriptionPairList IncidentEndorsementCountries => UniversalLookupsHelper.GetCountryC0009List(Factory, CountryCode);

		public RefCountryCollection NewTransportCountries => new RefCountryCollection(Factory);
	}
}
