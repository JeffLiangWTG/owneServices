namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class AccessCodeWrapperTest : Customs.Business.Testing.DataProviderTestCase<AccessCodeWrapper>
	{
		public void TestAccessCode()
		{
			AssertEquals("AccessCode should be AccessCodeForTest", "AccessCodeForTest", Provider.AccessCode);
		}

		protected override AccessCodeWrapper GetProvider()
		{
			return AccessCodeWrapper.New("AccessCodeForTest");
		}
	}
}
