using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public partial class ARManifestTypes
	{
		public IManifestType MAN => man ?? (man = new ManifestType(
			Codes.MAN,
			Descriptions.MAN,
			GetTransportModes(),
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest));
		IManifestType man;

		public IReadOnlyList<IManifestType> All => new[] { MAN };

		IEnumerable<string> GetTransportModes()
		{
			return ARCustomsDataRegistry.Instance.IsARManifestShowAIRFunctions
				? (new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Air })
				: (new[] { Core.Constants.TransportModes.Sea });
		}
	}
}
