using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSSealsProvider : INCTSSeals
	{
		public static NCTSSealsProvider NewOrNull(IEnumerable<ZString> seals) => seals != null ? new NCTSSealsProvider(seals) : null;

		NCTSSealsProvider(IEnumerable<ZString> seals)
		{
			this.seals = Argument.NotNull(seals, nameof(seals));
		}

		public int Number => seals.Count();

		public IReadOnlyCollection<string> Identities => identities ?? (identities = seals.Select(x => x.ToString()).OrderBy(x => x).ToArray());
		IReadOnlyCollection<string> identities;

		readonly IEnumerable<ZString> seals;
	}
}
