using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.H7.Business
{
	public partial class IEH7ManifestTypes
	{
		public IManifestType EH7 => eh7 ?? (eh7 = new ManifestType(
			EUH7ManifestTypes.Codes.EH7,
			EUH7ManifestTypes.Descriptions.EH7,
			new[] { TransportModes.Air, TransportModes.Sea, TransportModes.Rail, TransportModes.Road },
			new[] { ApplicationCodeTypeList.Codes.EuH7V1, ApplicationCodeTypeList.Codes.EuH7V2 },
			MessageLevel.Bill,
			ShipmentTypeList.Import23Only()
		));

		IManifestType eh7;

		public IReadOnlyList<IManifestType> All => new[] { EH7 };
	}
}
