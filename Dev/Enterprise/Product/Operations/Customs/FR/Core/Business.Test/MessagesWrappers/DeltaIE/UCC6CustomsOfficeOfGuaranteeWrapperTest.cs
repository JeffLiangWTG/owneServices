using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	internal class UCC6CustomsOfficeOfGuaranteeWrapperTest : DataProviderTestCase<UCC6CustomsOfficeOfGuaranteeWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("CustomsOfficeOfGuarantee should be empty", string.Empty, Provider.ReferenceNumber);
		}

		protected override UCC6CustomsOfficeOfGuaranteeWrapper GetProvider()
		{
			return UCC6CustomsOfficeOfGuaranteeWrapper.New();
		}
	}
}
