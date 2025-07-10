using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public static class CDSIncoTermPlaceValidator
	{
		public static void Validate(BusinessObjectFactory factory, ZString place, ZPropertyInfo info)
		{
			if (!place.IsEmpty && place.Length >= 2 &&
				new RefUNLOCO.Loader(factory).Load(place) == null && RefUNLOCO.GetPortFromNameAndCountryCode(factory, place.Remove(0, 2), place.Left(2)) == null)
			{
				if (RefCountry.LoadFromCountryCode(factory, place.Left(2)) == null)
				{
					info.AddMessageError("Use UNLOCO or country/region code prefix + name, 'AUSydney'");
				}
				else
				{
					info.AddWarning("When not using a UNLOCO, the location must be given with a country/region code prefix, e.g. 'AUSydney', not just 'Sydney'");
				}
			}
		}
	}
}
