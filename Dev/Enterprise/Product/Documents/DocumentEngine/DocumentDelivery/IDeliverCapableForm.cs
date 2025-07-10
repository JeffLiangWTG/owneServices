namespace Enterprise.DocumentEngine.DocumentDelivery
{
	public interface IDeliverCapableForm
	{
		void Deliver(DeliveryInstructions instructions);
		bool IsFormClosed { get; set; }
	}
}
