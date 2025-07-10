using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5PreviousG4WrapperTest : WrapperHelperTest<ArrivalNCTS5PreviousG4Wrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		public void TestPreviousG4MRN()
		{
			AssertEquals("Expected filled PreviousG4MRN", "mrncode", wrapper.PreviousG4MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new ArrivalNCTS5PreviousG4Wrapper(1, "mrncode");
		}

		ArrivalNCTS5PreviousG4Wrapper wrapper;

		protected override ArrivalNCTS5PreviousG4Wrapper GetProvider() => wrapper;
	}
}
