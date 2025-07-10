using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonGoodsReferenceWrapperTest : WrapperHelperTest<NCTS5CommonGoodsReferenceWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("Expected filled DeclarationGoodsItemNumber", "2", wrapper.DeclarationGoodsItemNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new NCTS5CommonGoodsReferenceWrapper(1, 2);
		}

		NCTS5CommonGoodsReferenceWrapper wrapper;

		protected override NCTS5CommonGoodsReferenceWrapper GetProvider() => wrapper;
	}
}
