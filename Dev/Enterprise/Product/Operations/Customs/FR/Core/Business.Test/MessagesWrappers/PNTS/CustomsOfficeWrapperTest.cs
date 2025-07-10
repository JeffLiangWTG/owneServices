namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class CustomsOfficeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal cusCodeData CY_Data.", "FR000001", Provider.ReferenceNumber);
		}

		protected override CustomsOfficeWrapper GetProvider()
		{
			var customsOfficeReference = "FR000001";
			return CustomsOfficeWrapper.New(customsOfficeReference);
		}
	}
}
