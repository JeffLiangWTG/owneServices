using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business
{
	public partial class EUH7ManifestTypes : Integration.Customs.EUH7.IEUH7ManifestTypes
	{
		public IManifestType EH7 => eh7 ?? (eh7 = new ManifestType(
			Codes.EH7,
			Descriptions.EH7,
			new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road },
			new[] { ApplicationCodeTypeList.Codes.EuH7 },
			MessageLevel.Bill,
			ShipmentTypeList.Import23Only()
		));

		IManifestType eh7;

		public IReadOnlyList<IManifestType> All => new[] { EH7 };

		public ICodeDescription EH7CodeDescription => EH7;
	}
}
