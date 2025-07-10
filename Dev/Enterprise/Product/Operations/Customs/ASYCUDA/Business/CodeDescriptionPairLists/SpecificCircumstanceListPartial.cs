using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business
{
	partial class SpecificCircumstanceList
	{
		public static bool IsTransportModeSupported(ZString code, ZString transportMode)
		{
			var transportModes = GetSupportedTransportModeList(code);
			return !transportModes.Any() || transportModes.Contains(transportMode);
		}

		static ZString[] GetSupportedTransportModeList(ZString code)
		{
			switch (code)
			{
				case SpecificCircumstanceList.Codes.A:
					return new ZString[]
					{
						RefTransportModeList.Codes.SEA,
						RefTransportModeList.Codes.RAI,
						RefTransportModeList.Codes.ROA,
						RefTransportModeList.Codes.AIR,
						RefTransportModeList.Codes.MAI,
						RefTransportModeList.Codes.INW
					};
				case SpecificCircumstanceList.Codes.C:
					return new ZString[]
					{
						RefTransportModeList.Codes.ROA,
						RefTransportModeList.Codes.MAI
					};
				case SpecificCircumstanceList.Codes.D:
					return new ZString[]
					{
						RefTransportModeList.Codes.RAI,
						RefTransportModeList.Codes.MAI
					};
				default:
					return System.Array.Empty<ZString>();
			}
		}
	}
}
