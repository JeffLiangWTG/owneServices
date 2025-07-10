namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class SupportingDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentWrapper>
	{
		public void TestType()
		{
			AssertEquals("Type should be mapped to CSI_Code.", "Code", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should be mapped to CSI_ReferenceNumber.", "ReferenceNumber", Provider.ReferenceNumber);
		}

		public void TestDocumentLineItemNumber()
		{
			AssertEquals("DocumentLineItemNumber should be mapped to CSI_ItemNumber.", 99, Provider.DocumentLineItemNumber);
		}

		public void TestComplementOfInformation()
		{
			AssertEquals("ComplementOfInformation should be mapped to CSI_ReferenceNumber2.", "ReferenceNumber2", Provider.ComplementOfInformation);
		}

		protected override SupportingDocumentWrapper GetProvider()
		{
			var supportingDocument = Factory.New<EU.NCTS.Business.NctsSupportingDocument>();
			supportingDocument.CSI_Code = "Code";
			supportingDocument.CSI_ReferenceNumber = "ReferenceNumber";
			supportingDocument.CSI_ItemNumber = 99;
			supportingDocument.CSI_ReferenceNumber2 = "ReferenceNumber2";
			return SupportingDocumentWrapper.New(supportingDocument);
		}
	}
}
