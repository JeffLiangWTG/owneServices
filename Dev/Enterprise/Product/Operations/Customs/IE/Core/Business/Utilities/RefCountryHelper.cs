using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
{
	public static class RefCountryHelper
	{
		public static bool IsCountryPartOfEuropeanUnion(CargoWise.EntityFramework.BusinessObjectFactory factory, ZString countryCode)
		{
			return !countryCode.IsEmpty && RefCountry.LoadFromCountryCode(factory, countryCode) is RefCountry refCountry && refCountry.IsPartOfEuropeanUnion;
		}
	}
}
