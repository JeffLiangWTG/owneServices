using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ISCIPEDHeader : IMonthlyClosingDecHeader
	{
		IReadOnlyCollection<ISCIPEDBody> Bodies { get; }
	}
}
