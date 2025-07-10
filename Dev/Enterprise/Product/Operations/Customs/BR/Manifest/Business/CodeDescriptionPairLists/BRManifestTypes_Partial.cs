using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.Business
{
	public partial class BRManifestTypes
	{
		public IManifestType MER => mer ?? (mer = new ManifestType(
			BRManifestTypes.Codes.MER,
			(NoResString)"Mercante",
			new[] { Core.Constants.TransportModes.Sea },
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Import23Only()));
		IManifestType mer;

		public IManifestType MUCR => mucr ?? (mucr = new ManifestType(
			BRManifestTypes.Codes.MUCR,
			BRManifestTypes.Descriptions.MUCR,
			new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea },
			new[] { ApplicationCodeTypeList.Codes.Consolidator },
			MessageLevel.Manifest,
			ShipmentTypeList.Export22Only()));
		IManifestType mucr;

		public IReadOnlyList<IManifestType> All => new[] { MER, MUCR };
	}
}
