using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodCountryCollection : BusinessObjectCollection<CMRPreferenceSchemePeriodCountry>
	{
		public CMRPreferenceSchemePeriodCountryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
