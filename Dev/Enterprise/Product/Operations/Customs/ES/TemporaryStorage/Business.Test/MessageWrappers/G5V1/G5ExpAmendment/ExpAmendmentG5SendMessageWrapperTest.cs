using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class ExpAmendmentG5SendMessageWrapperTest : WrapperHelperTest<ExpAmendmentG5SendMessageWrapper>
	{
		protected override ExpAmendmentG5SendMessageWrapper GetProvider() => wrapper;

		public void TestMRN()
		{
			header.MRN = "mrn";
			AssertEquals("Expected filled MRN", "mrn", wrapper.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			wrapper = new ExpAmendmentG5SendMessageWrapper(header, Certificate);
		}

		TemporaryStorageHeader header;
		ExpAmendmentG5SendMessageWrapper wrapper;
	}
}
