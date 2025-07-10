using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public partial class ILManifestTypes
	{
		public IManifestType ImportManifest => importManifest ??= GetImportManifest();
		IManifestType importManifest;

		public IManifestType ExportManifest => exportManifest ??= GetExportManifest();
		IManifestType exportManifest;

		public IManifestType AllManifest => allManifest ??= GetAllManifest();
		IManifestType allManifest;

		IManifestType GetImportManifest()
		{
			return new ManifestType(
				code: Codes._785,
				description: ResString.GetMultilingualString("ILManifestTypes|_785_Import", "Import Manifest"),
				applicableTransportMode: new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road },
				applicableManifestStyles: new[] { ApplicationCodeTypeList.Codes.Consolidator },
				messageLevel: MessageLevel.Manifest,
				manifestNatures: ShipmentTypeList.Import23Only());
		}

		IManifestType GetExportManifest()
		{
			return new ManifestType(
				code: Codes._785,
				description: ResString.GetMultilingualString("ILManifestTypes|_785_Export", "Export Manifest"),
				applicableTransportMode: new[] { Core.Constants.TransportModes.Road },
				applicableManifestStyles: new[] { ApplicationCodeTypeList.Codes.Consolidator },
				messageLevel: MessageLevel.Manifest,
				manifestNatures: ShipmentTypeList.Export22Only());
		}

		IManifestType GetAllManifest()
		{
			return new ManifestType(
				code: Codes._785,
				description: ResString.GetMultilingualString("ILManifestTypes|_785_Import_Export", "Import/Export Manifest"),
				applicableTransportMode: new[] { Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Road },
				applicableManifestStyles: new[] { ApplicationCodeTypeList.Codes.Consolidator },
				messageLevel: MessageLevel.Manifest,
				manifestNatures: ShipmentTypeList.Export22AndImport23())
			{
				Enabled = ILCustomsDataRegistry.Instance.ILEnableILManifest.Value == ILManifestRegistryOptions.Codes.ALL
			};
		}
	}
}
