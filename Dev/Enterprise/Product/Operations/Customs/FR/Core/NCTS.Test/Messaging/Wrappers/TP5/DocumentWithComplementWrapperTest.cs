using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class DocumentWithComplementWrapperTest : Customs.Business.Testing.DataProviderTestCase<DocumentWithComplementWrapper>
	{
		public void TestType()
		{
			AssertEquals("Type should be mapped to CSI_Code", "Code", Provider.Type);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals("ComplementOfInformation should be mapped to CSI_ReferenceNumber2", "ReferenceNumber2", Provider.ComplementOfInformation);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be mapped to CSI_ReferenceNumber", "ReferenceNumber", Provider.ReferenceNumber);
		}

		protected override DocumentWithComplementWrapper GetProvider()
		{
			var supportingInfo = Factory.New<CusSupportingInfo>();
			supportingInfo.CSI_Code = "Code";
			supportingInfo.CSI_ReferenceNumber = "ReferenceNumber";
			supportingInfo.CSI_ReferenceNumber2 = "ReferenceNumber2";
			return DocumentWithComplementWrapper.New(supportingInfo);
		}
	}
}
