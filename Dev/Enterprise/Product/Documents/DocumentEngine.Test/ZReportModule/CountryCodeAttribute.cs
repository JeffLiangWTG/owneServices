using System;

namespace Enterprise.DocumentEngine.Testing
{
	public class CountryCodeAttribute : Attribute
	{
		public CountryCodeAttribute(string countryCode)
		{
			this.CountryCode = countryCode;
		}

		public readonly string CountryCode;
	}
}
