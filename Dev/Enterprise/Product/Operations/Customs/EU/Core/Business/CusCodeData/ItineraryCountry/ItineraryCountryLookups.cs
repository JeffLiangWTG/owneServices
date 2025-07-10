using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class ItineraryCountryLookups : CusCodeDataLookups
	{
		public ItineraryCountryLookups(AutoCusCodeData parent) : base(parent)
		{
		}

		public RefCountryCollection CountryList => new RefCountryCollection(Factory);
	}
}
