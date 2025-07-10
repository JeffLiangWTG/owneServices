using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<DepartureDocumentWrapper>
	{
		public void TestSource()
		{
			AssertEquals("Expected empty Source", ZString.Empty, wrapper.Source);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new DepartureDocumentWrapper(ZString.Empty, ZString.Empty);
		}
		DepartureDocumentWrapper wrapper;

		protected override DepartureDocumentWrapper GetProvider() => wrapper;
	}
}
