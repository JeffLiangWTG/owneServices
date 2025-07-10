using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.GB.GVMS
{
	public partial class GVMSManifestType
	{
		public IManifestType MAN => man ?? (man = new ManifestType(
			Codes.GoodsVehicleMovementSystemGvms,
			Descriptions.GoodsVehicleMovementSystemGvms,
			new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.RollOnRollOff },
			new[] { ApplicationCodeTypeList.Codes.ShippingLine, ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			new GVMSManifestNature()
		));
		IManifestType man;

		public IReadOnlyList<IManifestType> All => new[] { MAN };
	}
}
