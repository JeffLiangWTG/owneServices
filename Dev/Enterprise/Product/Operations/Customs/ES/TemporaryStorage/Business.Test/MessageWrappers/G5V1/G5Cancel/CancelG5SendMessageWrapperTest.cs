using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class CancelG5SendMessageWrapperTest : WrapperHelperTest<CancelG5SendMessageWrapper>
	{
		protected override CancelG5SendMessageWrapper GetProvider() => wrapper;

		public void TestMRN()
		{
			header.MRN = "mrn";
			AssertEquals("Expected filled MRN", "mrn", wrapper.MRN);
		}

		public void TestHeader()
		{
			var header = wrapper.Header;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Header", header);
				AssertSame("Cached Header", wrapper.Header, header);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			wrapper = new CancelG5SendMessageWrapper(header, Certificate);
		}

		TemporaryStorageHeader header;
		CancelG5SendMessageWrapper wrapper;
	}
}
