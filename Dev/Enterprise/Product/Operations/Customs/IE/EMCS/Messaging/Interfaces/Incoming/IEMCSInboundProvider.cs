using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IEMCSInboundProvider
	{
		ZString MrnNumber { get; }
		ZString MrnNumberSequenceNumber { get; }
	}
}
