using CargoWise.ComponentModel;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IUnloco
	{
		[List(nameof(Unlocos))]
		ZString Code { get; set; }
		ZString Name { get; set; }
		ZString IATACode { get; set; }

		ICountry Country { get; }

		[MacroIgnore]
		IRefUNLOCOCollection Unlocos { get; }
	}
}