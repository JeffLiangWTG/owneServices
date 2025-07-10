using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonActiveBorderTransportMeansWrapperTest : WrapperHelperTest<NCTS5CommonActiveBorderTransportMeansWrapper>
	{
		public void TestConveyanceReferenceNumber()
		{
			AssertEquals("Expected filled ConveyanceReferenceNumber", "reference", wrapper.ConveyanceReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapper = new NCTS5CommonActiveBorderTransportMeansWrapper(ZString.Empty, ZString.Empty, ZString.Empty, "reference", 1);
		}

		NCTS5CommonActiveBorderTransportMeansWrapper wrapper;

		protected override NCTS5CommonActiveBorderTransportMeansWrapper GetProvider() => wrapper;
	}
}
