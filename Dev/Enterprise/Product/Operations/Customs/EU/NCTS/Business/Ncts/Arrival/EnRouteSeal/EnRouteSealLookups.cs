using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class EnRouteSealLookups : Customs.Business.CusInBondEventLookups
	{
		public EnRouteSealLookups(EnRouteSeal parent)
			: base(parent)
		{
		}

		protected new EnRouteSeal Parent => (EnRouteSeal)base.Parent;

		ZString CountryCode => Parent.Header?.Branch?.Country?.Code ?? GlbCompany.CurrentCompany.Country.Code;

		public ZZRefCusCodeListCombinedCollection EventCountries => Factory.GetCountryList(CountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
	}
}
