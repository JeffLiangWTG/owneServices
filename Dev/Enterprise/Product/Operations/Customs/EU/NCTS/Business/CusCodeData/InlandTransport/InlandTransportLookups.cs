using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class InlandTransportLookups : CusCodeDataLookups
	{
		public InlandTransportLookups(InlandTransport inlandTransport) : base(inlandTransport)
		{
		}

		public RefCountryCollection Countries => Factory.GetCachedValue("EUNctsInlandTransportLookups.Countries", () => new RefCountryCollection(Factory));

		public ZZRefCusCodeListCombinedCollection TransportNationalityList => Factory.GetNCNATCountryList();
	}
}
