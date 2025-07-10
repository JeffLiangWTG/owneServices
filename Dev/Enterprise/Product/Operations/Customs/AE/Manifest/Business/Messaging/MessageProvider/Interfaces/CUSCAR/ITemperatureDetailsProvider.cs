namespace Enterprise.Customs.AE.Manifest.Business;

public interface ITemperatureDetailsProvider
{
	string TemperatureTypeCode { get; }

	decimal TemperatureDegree { get; }

	string TemperatureUnit { get; }
}
