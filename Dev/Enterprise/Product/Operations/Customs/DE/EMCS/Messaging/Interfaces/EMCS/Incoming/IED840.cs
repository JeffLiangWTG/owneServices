namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED840 : IEmcsDataProvider
	{
		IEMCSEvent ExciseMovement { get; } 
	}
}
