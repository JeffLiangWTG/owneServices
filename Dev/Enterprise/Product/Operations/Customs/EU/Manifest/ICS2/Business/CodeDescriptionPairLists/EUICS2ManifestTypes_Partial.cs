using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public partial class EUICS2ManifestTypes : Integration.Customs.EUICS2.IEUICS2ManifestTypes
	{
		public IManifestType ENS => ens ?? (ens = new ManifestType(
			Codes.ENS,
			Descriptions.ENS,
			new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.InlandWaterwayTransport, Core.Constants.TransportModes.Rail },
			new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()
		));
		IManifestType ens;

		public IReadOnlyList<IManifestType> All => new[] { ENS };

		public ICodeDescription ENSCodeDescription => ENS;
	}
}
