namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class TransportChargesWrapperTest : Customs.Business.Testing.DataProviderTestCase<TransportChargesWrapper>
	{
		public void TestMethodOfPayment()
		{
			AssertEquals("E", Provider.MethodOfPayment);
		}

		protected override TransportChargesWrapper GetProvider()
		{
			return TransportChargesWrapper.New("E");
		}
	}
}
