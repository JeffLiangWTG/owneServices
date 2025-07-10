using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class RouteEntryLookups : CusCodeDataLookups
	{
		public RouteEntryLookups(CusCodeData parent) : base(parent)
		{
		}

		public RefCountryCollection CountryCodeList
		{
			get { return new RefCountryCollection(Factory); }
		}
	}
}
