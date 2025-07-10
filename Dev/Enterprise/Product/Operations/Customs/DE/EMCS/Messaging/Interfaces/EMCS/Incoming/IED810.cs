namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED810 : IEmcsDataProvider
	{
		IEMCSEvent ExciseMovementEad { get; }

		string CancellationReasonCode { get; }
	}
}
