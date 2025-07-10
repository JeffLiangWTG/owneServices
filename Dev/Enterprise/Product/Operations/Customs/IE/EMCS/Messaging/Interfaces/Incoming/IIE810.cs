namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE810 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }

		string CancellationReasonCode { get; }
	}
}
