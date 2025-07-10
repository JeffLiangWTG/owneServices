using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class SealCommonWrapperTest : WrapperHelperTest<SealCommonWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestSealNumber()
		{
			AssertEquals("Expected filled SealNumber", "Seal1", wrapper.SealNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new SealCommonWrapper(1, "Seal1");
		}

		SealCommonWrapper wrapper;

		protected override SealCommonWrapper GetProvider() => wrapper;
	}
}
