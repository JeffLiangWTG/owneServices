using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class EX583AdditionalInformationProviderTest : DataProviderTestCase<EX583AdditionalInformationProvider>
	{
		public void TestCode()
		{
			AssertEquals("Code (DocumentType)", "9002", Provider.Code);
		}

		public void TestText()
		{
			sendingObject.DocumentInformation = "Document Information";
			AssertEquals("Text (DocumentComplementaryInformation)", "Document Information", Provider.Text);
		}

		protected override EX583AdditionalInformationProvider GetProvider() => new EX583AdditionalInformationProvider(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			sendingObject = new AdditionalInfoSendingObject("9002");
		}

		AdditionalInfoSendingObject sendingObject;
	}
}
