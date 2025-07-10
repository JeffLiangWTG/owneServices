using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonActiveBorderTransportMeansWrapperWithOfficeTest : WrapperHelperTest<NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper>
	{
		public void TestCustomsOfficeAtBorderReferenceNumber()
		{
			AssertEquals("Expected filled CustomsOfficeAtBorderReferenceNumber", "ES009999", wrapper.CustomsOfficeAtBorderReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper(ZString.Empty, ZString.Empty, ZString.Empty, "ES009999", ZString.Empty, 1);
		}

		NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper wrapper;

		protected override NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper GetProvider() => wrapper;
	}
}
