using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.PBN.Business
{
	public partial class PBNManifestTypes
	{
		public IManifestType PBN => pbn ?? (pbn = new ManifestType(
			Codes.PBN,
			Descriptions.PBN,
			new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road },
			new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest
		));
		IManifestType pbn;

		public IReadOnlyList<IManifestType> All => new[] { PBN };
	}
}
