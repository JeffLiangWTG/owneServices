using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AdditionalInfoSendingObjectProviderTest : DataProviderTestCase<AdditionalInfoSendingObjectProvider>
	{
		public void TestCode()
		{
			AssertEquals("DocumentType", "9002", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("DocumentComplementaryInformation", "Document Information", Provider.Text);
		}

		protected override AdditionalInfoSendingObjectProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var requestedDocument = entryHeader.EntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var sendingObject = new AdditionalInfoSendingObject(entryHeader, null, requestedDocument);
			sendingObject.DocumentInformation = "Document Information";
			provider = new AdditionalInfoSendingObjectProvider(sendingObject);
		}
		AdditionalInfoSendingObjectProvider provider;
	}
}
