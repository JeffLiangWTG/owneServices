using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.IN.Registry;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Manifest.Business
{
	public partial class INManifestTypes
	{
		public IManifestType CGM => cgm ??= GetCGM();
		IManifestType cgm;

		IManifestType GetCGM()
		{
			return new ManifestType(
				Codes.CGM,
				Descriptions.CGM,
				new[] { Core.Constants.TransportModes.Air },
				new[] { ApplicationCodeTypeList.Codes.Consolidator },
				MessageLevel.Manifest,
				CGMManifestNatures)
			{ Enabled = INCustomsDataRegistry.Instance.INEnableConsolGeneralManifest.Value };
		}

		public IManifestType IGM => igm ??= GetIGM();
		IManifestType igm;

		IManifestType GetIGM()
		{
			return new ManifestType(
				Codes.IGM,
				Descriptions.IGM,
				new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea },
				new[] { ApplicationCodeTypeList.Codes.ShippingLine },
				MessageLevel.Manifest)
			{ Enabled = INCustomsDataRegistry.Instance.INEnableImportGeneralManifest.Value };
		}

		public IReadOnlyList<IManifestType> All => new[] { CGM, IGM };

		CodeDescriptionPairList CGMManifestNatures => new CodeDescriptionPairList() { new CodeDescriptionPair(INManifestNatures.Codes.IMP, INManifestNatures.Descriptions.IMP) };
	}
}
