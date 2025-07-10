using System.Collections.Generic;
using Enterprise.Customs.DutyCalculator;
using ICustomsRateUnitConverter = Enterprise.Integration.Customs.ICustomsRateUnitConverter;
using ICustomsRateUnitConvertersProvider = Enterprise.Integration.Customs.ICustomsRateUnitConvertersProvider;

namespace Enterprise.Customs.FR.Business
{
	public class FRCustomsRateUnitConvertersProvider : ICustomsRateUnitConvertersProvider
	{
		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateAlcoholConverter()
		{
			return new FRCustomsRateAlcoholConverter();
		}

		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateEnergyConverter()
		{
			return new FRCustomsRateEnergyConverter();
		}

		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateLengthConverter()
		{
			return new FRCustomsRateLengthConverter();
		}

		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateNumberConverter()
		{
			return new FRCustomsRateNumberConverter();
		}

		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateSurfaceConverter()
		{
			return new FRCustomsRateSurfaceConverter();
		}

		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateVolumeConverter()
		{
			return new FRCustomsRateVolumeConverter();
		}

		ICustomsRateUnitConverter ICustomsRateUnitConvertersProvider.GetCustomsRateWeightConverter()
		{
			return new FRCustomsRateWeightConverter();
		}
	}

	public class FRCustomsRateWeightConverter : CustomsRateWeightConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.WeightConversionDictionary;
	}

	public class FRCustomsRateVolumeConverter : CustomsRateVolumeConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.VolumeConversionDictionary;
	}

	public class FRCustomsRateAlcoholConverter : CustomsRateAlcoholConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.AlcoholConversionDictionary;
	}

	public class FRCustomsRateNumberConverter : CustomsRateNumberConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.NumberConversionDictionary;
	}

	public class FRCustomsRateLengthConverter : CustomsRateLengthConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.LengthConversionDictionary;
	}

	public class FRCustomsRateSurfaceConverter : CustomsRateSurfaceConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.SurfaceConversionDictionary;
	}

	public class FRCustomsRateEnergyConverter : CustomsRateEnergyConverter
	{
		protected override IDictionary<string, decimal> CountrySpecificConversionDictionary => FRConvertibleUnitsOfMeasure.EnergyConversionDictionary;
	}
}
