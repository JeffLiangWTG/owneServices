using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE829 : IEMCSInboundProvider
	{
		ZString SendingCustomsOffice { get; }
		ZDate AcceptanceDate { get; }
		ZDate ReleaseDate { get; }
		ZString Mrn { get; }
		IReadOnlyCollection<IEMCSEvent> ExciseMovementEads { get; }
	}
}
