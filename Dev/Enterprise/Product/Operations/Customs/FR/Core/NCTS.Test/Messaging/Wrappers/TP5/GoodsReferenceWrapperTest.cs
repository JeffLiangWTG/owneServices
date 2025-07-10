namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class GoodsReferenceWrapperTest : Customs.Business.Testing.DataProviderTestCase<GoodsReferenceWrapper>
	{
		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("DeclarationGoodsItemNumber should be equal to value entered in parameters.", "1", Provider.DeclarationGoodsItemNumber);
		}

		protected override GoodsReferenceWrapper GetProvider()
		{
			return GoodsReferenceWrapper.New("1");
		}
	}
}
