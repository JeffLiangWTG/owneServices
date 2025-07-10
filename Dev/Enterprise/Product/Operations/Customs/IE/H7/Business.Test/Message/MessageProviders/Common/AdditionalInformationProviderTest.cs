using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class AdditionalInformationProviderTest : DataProviderTestCase<AdditionalInformationProvider>
	{
		public void TestCode()
		{
			AssertEquals("9002", Provider.Code);
		}

		public void TestText()
		{
			AssertEquals("9002 Desc", Provider.Text);
		}

		protected override AdditionalInformationProvider GetProvider()
		{
			var bill = Factory.New<AsycudaBill>();
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";

			return new AdditionalInformationProvider(new AdditionalInfoSendingObject(bill, null, requestedDocument));
		}
	}
}
