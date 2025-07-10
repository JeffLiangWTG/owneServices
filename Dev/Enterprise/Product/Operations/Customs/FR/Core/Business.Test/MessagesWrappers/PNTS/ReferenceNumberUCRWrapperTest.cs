using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class ReferenceNumberUCRWrapperTest : Customs.Business.Testing.DataProviderTestCase<ReferenceNumberUCRWrapper>
	{
		public void TestReferenceNumberUCRProperty()
		{
			AssertEquals("ReferenceNumberUCRProperty should equal ABL_UCRNumber", "UCRNumber", Provider.ReferenceNumberUCRProperty);
		}

		protected override ReferenceNumberUCRWrapper GetProvider()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_UCRNumber = "UCRNumber";
			return ReferenceNumberUCRWrapper.New(bill.ABL_UCRNumber);
		}
	}
}
