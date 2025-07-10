using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ICountry
	{
		ZString CountryCode { get; }
		ZString TypeCode { get; }
	}

	class CountryWrapper : ICountry
	{
		CountryWrapper(ZString countryCode, ZString typeCode)
		{
			this.countryCode = countryCode;
			this.typeCode = typeCode;
		}

		public static CountryWrapper New(ZString countryCode, ZString typeCode)
		{
			return new CountryWrapper(countryCode, typeCode);
		}

		ZString ICountry.CountryCode => countryCode;

		ZString ICountry.TypeCode => typeCode;

		readonly ZString countryCode;
		readonly ZString typeCode;
	}
}
