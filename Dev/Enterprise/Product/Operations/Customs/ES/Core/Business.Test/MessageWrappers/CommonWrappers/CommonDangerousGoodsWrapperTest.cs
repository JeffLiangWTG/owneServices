using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommonDangerousGoodsWrapperTest : WrapperHelperTest<CommonDangerousGoodsWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestUNDangerousCode()
		{
			AssertEquals("Expected filled UNDangerousCode", "1110", wrapper.UNDangerousCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new CommonDangerousGoodsWrapper(1, "1110");
		}

		CommonDangerousGoodsWrapper wrapper;

		protected override CommonDangerousGoodsWrapper GetProvider() => wrapper;
	}
}
