using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE905 : IEMCSInboundProvider
	{
		ZString AdministrativeReferenceCode { get; }

		ZString LastReceivedMessageType { get; }

		ZString Status { get; }
	}
}
