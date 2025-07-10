namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE840 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }
	}
}
