using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IReferenceNumber
	{
		ICodeDescription Type { get; }
		ICountry CountryOfIssue { get; }
		ZString Value { get; set; }
	}
}