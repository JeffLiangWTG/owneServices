using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NCTSEventSealsProvider : INCTSSeals
	{
		public static NCTSEventSealsProvider NewOrNull(EnRouteSeal seal) => seal == null ? null : new NCTSEventSealsProvider(seal);

		readonly EnRouteSeal seal;

		NCTSEventSealsProvider(EnRouteSeal seal)
		{
			this.seal = Argument.NotNull(seal, nameof(seal));
		}

		public int Number => seal.BN_NoOfSeals;

		public IReadOnlyCollection<string> Identities => identities ?? (identities = seal.SealContainersSealNumbers.Select(s => s.ToString()).ToArray());
		IReadOnlyCollection<string> identities;
	}
}
