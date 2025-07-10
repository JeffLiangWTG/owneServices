using System.Collections.Generic;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICustomsRateUnitConverter
		{
			void AddConvertableUnitValuesToDictionary(IDictionary<string, decimal> unitQtyValueDictionary);
			bool CanConvertUnit(string unit);
		}

		public interface ICustomsRateUnitConvertersProvider
		{
			ICustomsRateUnitConverter GetCustomsRateWeightConverter();
			ICustomsRateUnitConverter GetCustomsRateVolumeConverter();
			ICustomsRateUnitConverter GetCustomsRateAlcoholConverter();
			ICustomsRateUnitConverter GetCustomsRateNumberConverter();
			ICustomsRateUnitConverter GetCustomsRateLengthConverter();
			ICustomsRateUnitConverter GetCustomsRateSurfaceConverter();
			ICustomsRateUnitConverter GetCustomsRateEnergyConverter();
		}

		public interface ICountrySpecificValueProvider
		{
			IDictionary<string, decimal> GetCountrySpecificValueList(ICusEntryLine entryLine);
		}
	}
}
