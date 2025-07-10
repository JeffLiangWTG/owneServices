using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	public class MockDeliveryInstructions : DeliveryInstructions
	{
		public int NotificationCount;
		public ZString LastHandler;

		public MockDeliveryInstructions()
		{
		}

		public override DocDeliveryPrintDetails PrinterDelivery
		{
			get
			{
				if (fPrintDelivery == null)
				{
					fPrintDelivery = new MockPrinterDelivery(Factory);
				}

				return fPrintDelivery;
			}
		}

		MockPrinterDelivery fPrintDelivery;
	}
}
