namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class GoodsReferenceWrapperTest : Customs.Business.Testing.DataProviderTestCase<GoodsReferenceWrapper>
	{
		protected override GoodsReferenceWrapper GetProvider()
		{
			return GoodsReferenceWrapper.New("1");
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("DeclarationGoodsItemNumber should be equal to value entered in parameters.", "1", Provider.DeclarationGoodsItemNumber);
		}
	}
}
