using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IVessel
	{
		ZString Name { get; set; }
		ZString LloydsIMO { get; set; }
		ZString RadioCallSign { get; set; }
		ICodeDescription Type { get; set; }
		ICountry CountryOfRegistration { get; set; }
	}
}
