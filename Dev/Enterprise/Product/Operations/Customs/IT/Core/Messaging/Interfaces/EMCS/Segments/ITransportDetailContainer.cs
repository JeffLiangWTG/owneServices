using System.Collections.Generic;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface ITransportDetailContainer
{
	IEnumerable<ITransportDetails> TransportDetails { get; }
}
