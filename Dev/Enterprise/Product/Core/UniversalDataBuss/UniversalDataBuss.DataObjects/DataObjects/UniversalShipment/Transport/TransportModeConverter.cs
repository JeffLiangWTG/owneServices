using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public class TransportModeConverter : EnumConverter<TransportMode>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				"SEA",
				"AIR",
				"RAI",
				"ROA",
				"STO",
				"IWT",
			};
		}

		protected override TransportMode[] GetEnumValues()
		{
			return new TransportMode[]
			{
				TransportMode.Sea,
				TransportMode.Air,
				TransportMode.Rail,
				TransportMode.Road,
				TransportMode.Storage,
				TransportMode.InlandWaterway,
			};
		}
	}
}
