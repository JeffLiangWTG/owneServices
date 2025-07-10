using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IContext
	{
		BusinessObjectFactory Factory { get; }

		ICodeDescriptionPairList TransportModes { get; }
		ICodeDescriptionPairList TransportTypes { get; }
		IRefContainerCollection ContainerTypes { get; }
		IRefUNLOCOCollection Unlocos { get; }
		IRefCountryCollection Countries { get; }
		IRefCurrencyCollection Currencies { get; }
		ICodeDescriptionPairList AirVentFlow { get; }
		ICodeDescriptionPairList TemperatureUnits { get; }
		ICodeDescriptionPairList Humidity { get; }
		ICodeDescriptionPairList WeightUnits { get; }
		ICodeDescriptionPairList VolumeUnits { get; }
		ICodeDescriptionPairList DimensionUnits { get; }
		ICodeDescriptionPairList RadioactiveUnits { get; }
	}
}
