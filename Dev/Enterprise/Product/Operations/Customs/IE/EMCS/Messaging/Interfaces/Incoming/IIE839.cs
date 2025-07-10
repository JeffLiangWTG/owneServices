using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE839 : IEMCSInboundProvider
	{
		ZString SendingCustomsOffice { get; }
		ZDate IssuanceDate { get; }
		ZString RejectionReasonCode { get; }
		IReadOnlyCollection<IEMCSEvent> RejectedEads { get; }
	}
}
