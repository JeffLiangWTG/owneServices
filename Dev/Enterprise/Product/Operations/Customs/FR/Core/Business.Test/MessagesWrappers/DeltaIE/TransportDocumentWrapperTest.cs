using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class TransportDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<TransportDocumentWrapper>
	{
		protected override TransportDocumentWrapper GetProvider()
		{
			var cusSupportingInfo = Factory.NewWithValidTestData<CusSupportingInfo>();
			cusSupportingInfo.CSI_ReferenceNumber = "reference";
			cusSupportingInfo.CSI_Code = PreviousDocumentCodeList.Codes.ZZZ;
			return TransportDocumentWrapper.New(cusSupportingInfo);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be equal to CSI_ReferenceNumber.", "reference", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type should be equal to CSI_Code.", PreviousDocumentCodeList.Codes.ZZZ, Provider.Type);
		}
	}
}
