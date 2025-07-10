using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public static class GreenhouseGasEmissionExtensions
	{
		public static bool TryGetCO2eValueInKG(this GreenhouseGasEmission greenhouseGasEmission, out decimal value)
		{
			value = 0m;
			if (greenhouseGasEmission?.CO2ePerTonneUnit == null || !greenhouseGasEmission.CO2ePerTonne.HasValue)
			{
				return false;
			}
			value = Weight.ConvertSafe(greenhouseGasEmission.CO2ePerTonne.Value, greenhouseGasEmission.CO2ePerTonneUnit.Code, Weight.Kilograms, false);
			return true;
		}

		public static bool TryGetCO2eTEUValueInKG(this GreenhouseGasEmission greenhouseGasEmission, out decimal value)
		{
			value = 0m;
			if (greenhouseGasEmission?.CO2ePerTEUUnit == null || !greenhouseGasEmission.CO2ePerTEU.HasValue)
			{
				return false;
			}
			value = Weight.ConvertSafe(greenhouseGasEmission.CO2ePerTEU.Value, greenhouseGasEmission.CO2ePerTEUUnit.Code, Weight.Kilograms, false);
			return true;
		}

		public static bool TryGetTotalCO2eValueInKG(this GreenhouseGasEmission greenhouseGasEmission, out decimal value)
		{
			value = 0m;
			if (greenhouseGasEmission?.CO2eUnit == null || !greenhouseGasEmission.CO2e.HasValue)
			{
				return false;
			}
			value = Weight.ConvertSafe(greenhouseGasEmission.CO2e.Value, greenhouseGasEmission.CO2eUnit.Code, Weight.Kilograms, false);
			return true;
		}

		public static bool IsCO2eValueValid(this GreenhouseGasEmission emission, out decimal value, out bool isTEU)
		{
			isTEU = false;

			if (emission.TryGetCO2eValueInKG(out value) && !IsCO2eValueInValidRange(value, JobCO2eSchema.JCO_CO2ePerTonneInKg))
			{
				isTEU = false;
				return false;
			}

			if (emission.TryGetCO2eTEUValueInKG(out value) && !IsCO2eValueInValidRange(value, JobCO2eSchema.JCO_CO2ePerTEUInKg))
			{
				isTEU = true;
				return false;
			}

			return true;
		}

		public static bool IsTotalCO2eValueValid(this GreenhouseGasEmission emission, out decimal value)
		{
			if (emission.TryGetTotalCO2eValueInKG(out value) && !IsCO2eValueInValidRange(value, JobCO2eSchema.JCO_TotalCO2e))
			{
				return false;
			}

			return true;
		}

		static bool IsCO2eValueInValidRange(ZDecimal value, SchemaDecimalColumn column)
		{
			return value.IsWithinSqlPrecisionAndScale(column.Precision, column.Scale);
		}
	}
}
