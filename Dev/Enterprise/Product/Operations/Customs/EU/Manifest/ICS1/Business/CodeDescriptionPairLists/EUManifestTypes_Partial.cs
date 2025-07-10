using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public partial class EUManifestTypes
	{
		public IManifestType ICS => ics ?? (ics = new ManifestType(
			Codes.ICS,
			Descriptions.ICS,
			new[] {
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Road,
				Core.Constants.TransportModes.Mail,
				Core.Constants.TransportModes.InlandWaterwayTransport
			},
			ApplicationCodeTypeList.Codes.ShippingLine,
			MessageLevel.Manifest
		));
		IManifestType ics;

		public IReadOnlyList<IManifestType> All => new[] { ICS };
	}
}
