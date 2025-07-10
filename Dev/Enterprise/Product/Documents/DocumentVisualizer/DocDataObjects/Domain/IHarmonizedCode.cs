using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IHarmonizedCode
	{
		ZString Code { get; set; }
		ICountry Country { get; set; }
	}
}
