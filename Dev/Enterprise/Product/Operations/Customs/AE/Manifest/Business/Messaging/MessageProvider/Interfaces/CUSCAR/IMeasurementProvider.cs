namespace Enterprise.Customs.AE.Manifest.Business;

public interface IMeasurementProvider
{
	string MeasurementPurpose { get; }

	string MeasurementUnit { get; }

	decimal MeasurementValue { get; }
}
