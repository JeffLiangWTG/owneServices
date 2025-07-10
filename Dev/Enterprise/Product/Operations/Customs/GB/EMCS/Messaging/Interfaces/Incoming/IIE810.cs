using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE810 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }
		ZString CancellationReasonCode { get; }
	}
}
