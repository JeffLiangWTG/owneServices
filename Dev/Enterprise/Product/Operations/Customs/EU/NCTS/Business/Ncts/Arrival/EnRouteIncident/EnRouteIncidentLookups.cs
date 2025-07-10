using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteIncidentLookups : CusInBondEventLookups
	{
		public EnRouteIncidentLookups(EnRouteIncident parent)
			: base(parent)
		{
		}

		protected new EnRouteIncident Parent => (EnRouteIncident)base.Parent;

		ZString CountryCode => Parent.Header?.Branch?.Country?.Code ?? GlbCompany.CurrentCompany.Country.Code;

		public ICollection EventCountries => Parent.IsPhase5 ? Factory.GetCountryC0009List(CountryCode) : Factory.GetCountryList(CountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);

		public ICollection IncidentEndorsementCountries => Parent.IsPhase5 ? Factory.GetCountryC0009List(CountryCode) : Factory.GetCountryList(CountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);

		public CodeDescriptionPairList TransportAtDepartureTypes => Factory.GetCachedValue<NctsTransportTypeOfIdList>();

		public override CodeDescriptionPairList IncidentCodeList => Factory.GetCachedValue<IncidentCodeList>();
	}
}
