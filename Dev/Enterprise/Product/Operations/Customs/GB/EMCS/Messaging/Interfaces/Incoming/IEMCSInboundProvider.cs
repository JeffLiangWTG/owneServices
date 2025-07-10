using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IEMCSInboundProvider
	{
		ZString MrnNumber { get; }
		ZString MrnNumberSequenceNumber { get; }
	}
}
