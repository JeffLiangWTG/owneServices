using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CommonDocumentSequenceNumberWrapperTest : WrapperHelperTest<CommonDocumentSequenceNumberWrapper>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new CommonDocumentSequenceNumberWrapper(ZString.Empty, ZString.Empty, 1);
		}
		CommonDocumentSequenceNumberWrapper wrapper;

		protected override CommonDocumentSequenceNumberWrapper GetProvider() => wrapper;
	}
}
