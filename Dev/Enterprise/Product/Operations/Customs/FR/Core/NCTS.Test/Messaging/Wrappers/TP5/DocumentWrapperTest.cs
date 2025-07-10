using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class DocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<DocumentWrapper>
	{
		public void TestType()
		{
			AssertEquals("Type should be mapped to CSI_Code.", "Code", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be mapped to CSI_ReferenceNumber.", "ReferenceNumber", Provider.ReferenceNumber);
		}

		protected override DocumentWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			return DocumentWrapper.New(supportingInfo);
		}
	}
}
