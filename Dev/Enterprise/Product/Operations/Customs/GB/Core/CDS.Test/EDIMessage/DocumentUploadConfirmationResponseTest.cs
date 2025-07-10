using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class DocumentUploadConfirmationResponseTest : TestCaseWithFactory
	{
		public void TestMessageDataObjectType()
		{
			var message = Factory.New<CDSDocumentUploadConfirmationResponse>();
			AssertType<DocumentUploadConfirmationResponse>(message.MessageDataObject);
		}

		public void TestMessageDataObject()
		{
			var message = Factory.New<CDSDocumentUploadConfirmationResponse>();
			message.EM_MessageText = @"<Root xmlns=""hmrc:fileupload"">
  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
  <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
  <Outcome>SUCCESS</Outcome>
  <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
</Root>";
			AssertEquals("CDS Entry Document - 2GB427168118378-B60004374.pdf", message.MessageDataObject.FileName);
			AssertEquals(true, message.MessageDataObject.IsSuccess);

			message = Factory.New<CDSDocumentUploadConfirmationResponse>();
			message.EM_MessageText = @"<Root xmlns=""hmrc:fileupload"">
  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
  <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
  <Outcome>FAIL</Outcome>
  <Details>Unable to connect to the server</Details>
</Root>";
			AssertEquals("CDS Entry Document - 2GB427168118378-B60004374.pdf", message.MessageDataObject.FileName);
			AssertEquals(false, message.MessageDataObject.IsSuccess);
		}
	}
}
