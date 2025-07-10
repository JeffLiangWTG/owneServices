using CargoWise.ComponentModel;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface ICountry
	{
		[List(nameof(Countries))]
		ZString Code { get; set; }
		ZString Name { get; set; }

		[MacroIgnore]
		IRefCountryCollection Countries { get; }
	}
}