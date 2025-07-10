using CargoWise.ComponentModel;

namespace Enterprise.DocumentEngine.Testing
{
	public class MockDocumentPack : DocumentPack
	{
		public DeliveryInstructions ExpectedDeliveryInstructions;
		public int RunCount;

		protected internal override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
		{
			base.Run(deliveryInstructions);
			RunCount++;
			CallOnAfterReportRun(null);
		}
	}
}
