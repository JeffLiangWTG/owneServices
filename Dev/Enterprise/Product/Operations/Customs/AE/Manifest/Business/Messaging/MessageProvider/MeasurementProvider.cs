namespace Enterprise.Customs.AE.Manifest.Business;

public class MeasurementProvider : IMeasurementProvider
{
	public MeasurementProvider(string purpose, decimal value, string unit)
	{
		MeasurementPurpose = purpose;
		MeasurementUnit = unit;
		MeasurementValue = value;
	}

	public string MeasurementPurpose { get; }

	public string MeasurementUnit { get; }

	public decimal MeasurementValue { get; }
}
