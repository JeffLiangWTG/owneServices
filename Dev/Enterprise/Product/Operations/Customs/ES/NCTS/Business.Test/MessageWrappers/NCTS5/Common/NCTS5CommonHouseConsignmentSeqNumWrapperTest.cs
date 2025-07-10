using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonHouseConsignmentSeqNumWrapperTest : WrapperHelperTest<NCTS5CommonHouseConsignmentSeqNumWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new NCTS5CommonHouseConsignmentSeqNumWrapper(1);
		}
		NCTS5CommonHouseConsignmentSeqNumWrapper wrapper;

		protected override NCTS5CommonHouseConsignmentSeqNumWrapper GetProvider() => wrapper;
	}
}
