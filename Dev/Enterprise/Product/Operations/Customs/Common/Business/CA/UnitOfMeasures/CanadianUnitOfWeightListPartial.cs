namespace Enterprise.Customs.Common.CA
{
	public partial class CanadianUnitOfWeightList
	{
		public static string ConvertWeightUnitToCanadianWeightUnit(string weightUnit)
		{
			switch (weightUnit)
			{
				case Core.Constants.Weight.Decitons:
					return CanadianUnitOfWeightList.Codes.Deciton;
				case Core.Constants.Weight.Grams:
					return CanadianUnitOfWeightList.Codes.Gram;
				case Core.Constants.Weight.Hectograms:
					return CanadianUnitOfWeightList.Codes.Hectogram;
				case Core.Constants.Weight.Kilograms:
					return CanadianUnitOfWeightList.Codes.Kilogram;
				case Core.Constants.Weight.Kilotonnes:
					return CanadianUnitOfWeightList.Codes.Kiloton;
				case Core.Constants.Weight.MetricCarat:
					return CanadianUnitOfWeightList.Codes.MetricCarat;
				case Core.Constants.Weight.Milligrams:
					return CanadianUnitOfWeightList.Codes.Milligram;
				default:
					return string.Empty;
			}
		}

		public static string GetUnitAndDescription(string shortUnitCode)
		{
			switch (shortUnitCode)
			{
				case UnitOfWeightList.Codes.MetricCarat:
					return Codes.MetricCarat + " " + Descriptions.MetricCarat;
				case UnitOfWeightList.Codes.Deciton:
					return Codes.Deciton + " " + Descriptions.Deciton;
				case UnitOfWeightList.Codes.Gram:
					return Codes.Gram + " " + Descriptions.Gram;
				case UnitOfWeightList.Codes.Hectogram:
					return Codes.Hectogram + " " + Descriptions.Hectogram;
				case UnitOfWeightList.Codes.Kilogram:
					return Codes.Kilogram + " " + Descriptions.Kilogram;
				case UnitOfWeightList.Codes.KilogramOfNamedSubstance:
					return Codes.KilogramOfNamedSubstance + " " + Descriptions.KilogramOfNamedSubstance;
				case UnitOfWeightList.Codes.Kilogram90PercentAirDry:
					return Codes.Kilogram90PercentAirDry + " " + Descriptions.Kilogram90PercentAirDry;
				case UnitOfWeightList.Codes.Kiloton:
					return Codes.Kiloton + " " + Descriptions.Kiloton;
				case UnitOfWeightList.Codes.Milligram:
					return Codes.Milligram + " " + Descriptions.Milligram;
				case UnitOfWeightList.Codes.MetricTon:
					return Codes.MetricTon + " " + Descriptions.MetricTon;
				case UnitOfWeightList.Codes.MetricTonneAirDry:
					return Codes.MetricTonneAirDry + " " + Descriptions.MetricTonneAirDry;
				default:
					return shortUnitCode;
			}
		}
	}
}

