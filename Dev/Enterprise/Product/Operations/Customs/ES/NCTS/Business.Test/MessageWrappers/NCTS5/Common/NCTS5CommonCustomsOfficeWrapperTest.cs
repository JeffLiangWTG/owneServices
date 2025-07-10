using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonCustomsOfficeWrapperTest : WrapperHelperTest<NCTS5CommonCustomsOfficeWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Expected filled ReferenceNumber", "ES009999", wrapper.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new NCTS5CommonCustomsOfficeWrapper("ES009999", 1);
		}

		NCTS5CommonCustomsOfficeWrapper wrapper;

		protected override NCTS5CommonCustomsOfficeWrapper GetProvider() => wrapper;
	}
}
