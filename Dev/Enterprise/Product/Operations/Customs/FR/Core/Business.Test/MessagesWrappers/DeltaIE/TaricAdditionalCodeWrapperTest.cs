using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class TaricAdditionalCodeWrapperTest : DataProviderTestCase<TaricAdditionalCodeWrapper>
	{
		public void TestTaricAdditionalCode()
		{
			AssertEquals("TaricAdditionalCode should equal to code passed as parameter.", "XXX", Provider.TaricAdditionalCode);
		}

		protected override TaricAdditionalCodeWrapper GetProvider()
		{
			var code = "XXX";

			return TaricAdditionalCodeWrapper.New(code);
		}
	}
}
