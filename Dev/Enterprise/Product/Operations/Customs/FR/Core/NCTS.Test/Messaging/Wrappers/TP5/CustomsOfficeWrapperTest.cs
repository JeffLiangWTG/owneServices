namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CustomsOfficeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal to value passed to wrapper.", "FR000040", Provider.ReferenceNumber);
		}

		protected override CustomsOfficeWrapper GetProvider()
		{
			return CustomsOfficeWrapper.New("FR000040");
		}
	}
}
