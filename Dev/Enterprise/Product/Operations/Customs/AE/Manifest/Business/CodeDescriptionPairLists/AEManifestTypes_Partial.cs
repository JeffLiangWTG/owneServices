using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.AE.Manifest.Business;

public partial class AEManifestTypes
{
	public IManifestType ACI => aci ??= new ManifestType(
		Codes.ACI,
		Descriptions.ACI,
		[Core.Constants.TransportModes.Sea],
		ApplicationCodeTypeList.Codes.Consolidator,
		MessageLevel.Bill
	);
	IManifestType aci;
}
