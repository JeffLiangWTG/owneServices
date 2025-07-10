using Enterprise.Edifact.D23A.Elements;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class TemperatureDetailsProvider : ITemperatureDetailsProvider
{
	public TemperatureDetailsProvider(AsycudaContainer container)
	{
		TemperatureTypeCode = TemperatureTypeCodeQualifierList.TransportTemperature;
		TemperatureDegree = container.ACN_SetPointTemperature;
		TemperatureUnit = GetTemperatureUnit(container.ACN_SetPointTemperatureUnit);
	}

	public string TemperatureTypeCode { get; }

	public decimal TemperatureDegree { get; }

	public string TemperatureUnit { get; }

	string GetTemperatureUnit(string temperatureUnit)
	{
		switch (temperatureUnit)
		{
			case AEContainerTemperatureUnitCodes.Codes.Celsius:
				return Celsius;
			case AEContainerTemperatureUnitCodes.Codes.Fahrenheit:
				return Fahrenheit;
			default:
				return temperatureUnit;
		}
	}
	const string Celsius = "CEL";
	const string Fahrenheit = "FAH";
}
