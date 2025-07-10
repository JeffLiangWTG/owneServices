using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
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

		public RefUNLOCOCollection RefUNLOCOList => new RefUNLOCOCollection(Factory);
	}
}
