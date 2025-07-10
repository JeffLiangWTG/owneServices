namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class WarehouseWrapperTest : Customs.Business.Testing.DataProviderTestCase<WarehouseWrapper>
	{
		public void TestIdentifier()
		{
			AssertEquals("Identifier should equal AGC_Number.", "12345", Provider.Identifier);
		}

		public void TestType()
		{
			AssertEquals("Type should equal AGC_Code.", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, Provider.Type);
		}

		protected override WarehouseWrapper GetProvider()
		{
			var authorisationUsage = Factory.New<CusAuthorizationUsage>();
			authorisationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorisationUsage.AGC_Number = "12345";
			return WarehouseWrapper.New(authorisationUsage);
		}
	}
}
