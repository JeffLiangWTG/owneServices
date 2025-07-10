using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class ReceptionG5SendMessageWrapperTest : WrapperHelperTest<ReceptionG5SendMessageWrapper>
	{
		protected override ReceptionG5SendMessageWrapper GetProvider() => wrapper;

		public void TestMRN()
		{
			header.MRN = "mrn";
			AssertEquals("Expected filled MRN", "mrn", wrapper.MRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			wrapper = new ReceptionG5SendMessageWrapper(header, Certificate);
		}

		TemporaryStorageHeader header;
		ReceptionG5SendMessageWrapper wrapper;
	}
}
