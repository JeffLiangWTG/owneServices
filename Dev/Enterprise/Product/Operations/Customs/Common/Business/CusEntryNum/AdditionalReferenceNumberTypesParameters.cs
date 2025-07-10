using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class AdditionalReferenceNumberTypesParameters
	{
		public AdditionalReferenceNumberTypesParameters(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public ZString CountryCode { get; }

		public ZString DischargeCountryCode { get; set; }

		public BusinessObject Parent { get; set; }

		public virtual ZString Key => $"{CountryCode}_{Parent?.GetType()}_{DischargeCountryCode}";
	}
}
