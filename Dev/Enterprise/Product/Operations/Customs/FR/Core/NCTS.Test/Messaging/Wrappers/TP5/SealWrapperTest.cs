namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class SealWrapperTest : Customs.Business.Testing.DataProviderTestCase<SealWrapper>
	{
		public void TestIdentifier()
		{
			AssertEquals("Wrapper Identifier should equal Seal identifier.", "SEAL1", Provider.Identifier);
		}

		protected override SealWrapper GetProvider()
		{
			var seal = Factory.New<EU.NCTS.Business.CusSeal>();
			seal.BK_SealNumber = "SEAL1";
			return SealWrapper.New("SEAL1");
		}
	}
}
