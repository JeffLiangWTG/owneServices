using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE829 : IEMCSInboundProvider
	{
		ZString SendingCustomsOffice { get; }
		ZDate AcceptanceDate { get; }
		ZString Mrn { get; }
		IReadOnlyCollection<IEMCSEvent> ExciseMovementEads { get; }
	}
}
