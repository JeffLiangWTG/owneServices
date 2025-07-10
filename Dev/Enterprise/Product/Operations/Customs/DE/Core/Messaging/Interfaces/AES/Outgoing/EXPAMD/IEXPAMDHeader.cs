using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IEXPAMDHeader : IAESHeader
	{
		IReadOnlyCollection<IEXPAMDLine> Lines { get; }
	}
}
