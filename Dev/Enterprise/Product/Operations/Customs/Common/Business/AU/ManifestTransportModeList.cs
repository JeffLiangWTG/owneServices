using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class ManifestTransportModeList : CodeDescriptionPairList
	{
		public ManifestTransportModeList()
		{
			AddPair(Enterprise.Core.Constants.TransportModes.Air, Enterprise.Core.Constants.TransportModes.Air);
			AddPair(Enterprise.Core.Constants.TransportModes.Sea, Enterprise.Core.Constants.TransportModes.Sea);
		}
	}
}
