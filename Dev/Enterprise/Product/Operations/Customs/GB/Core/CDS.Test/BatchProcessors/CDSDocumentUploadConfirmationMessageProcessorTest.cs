using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDocumentUploadConfirmationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessSuccessResponse_JobDeclaration()
		{
			var declaration = SetUp_Declaration();
			AssertProcessSuccessResponse(declaration, expectEmailSent: true);
		}

		public void TestProcessSuccessResponse_AsycudaBill()
		{
			var bill = SetUp_AsycudaBill();
			AssertProcessSuccessResponse(bill, expectEmailSent: false);
		}

		void AssertProcessSuccessResponse(IMessageAttachee messageAttachee, bool expectEmailSent)
		{
			var response = CreateResponseMessage(true);
			messageAttachee.Messages.Add(response);
			new CDSDocumentUploadConfirmationMessageProcessor(new LoggingInformation()).ProcessMessage(response);
			Factory.Save();

			AssertEquals("eHubId", "f127bdc2b97b4305aaba8026dc82949a", response.EM_ApplicationReference);
			AssertEquals(messageAttachee, response.EM_LinkedObject);
			AssertEquals(EDIMessage.Status.ProcessedOK, response.EM_Status);
			StmALog log = default;
			AssertNoExceptionThrown(() => log = messageAttachee.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentDeliveredCode).Single());
			var expectedLogReference = "Delivered successfully to CDS: CDS Entry Document - 2GB427168118378-B60004374.pdf|Job Number: B00000999|Details: Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.";
			AssertEquals("Reference", expectedLogReference, log.SL_Reference);

			if (expectEmailSent)
			{
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(user.GS_EmailAddress, email.Recipients[0].Email);
				AssertEquals("Document Upload Status (CDS Entry Document - 2GB427168118378-B60004374.pdf)|Job Number: B00000999", email.Subject);
				AssertEquals(@"Delivered successfully to CDS: CDS Entry Document - 2GB427168118378-B60004374.pdf
Job Number: B00000999
Details: Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer."
					, email.Body);
			}
			else
			{
				AssertEquals("No emails should be created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestProcessFailResponse_JobDeclaration()
		{
			var declaration = SetUp_Declaration();
			TestProcessFailResponse(declaration, expectEmailSent: true);
		}

		public void TestProcessFailResponse_AsycudaBill()
		{
			var bill = SetUp_AsycudaBill();
			TestProcessFailResponse(bill, expectEmailSent: false);
		}

		public void TestProcessFailResponse(IMessageAttachee messageAttachee, bool expectEmailSent)
		{
			var response = CreateResponseMessage(false);
			messageAttachee.Messages.Add(response);
			new CDSDocumentUploadConfirmationMessageProcessor(new LoggingInformation()).ProcessMessage(response);
			Factory.Save();

			AssertEquals(messageAttachee, response.EM_LinkedObject);
			AssertEquals(EDIMessage.Status.ProcessedOK, response.EM_Status);

			StmALog log = default;
			AssertNoExceptionThrown(() => log = messageAttachee.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentUnallocatedCode).Single());
			var expectedLogReference = "Failure to deliver to CDS: CDS Entry Document - 2GB427168118378-B60004374.pdf|Job Number: B00000999|Details: Unable to connect to the server";
			AssertEquals("Reference", expectedLogReference, log.SL_Reference);

			if (expectEmailSent)
			{
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(user.GS_EmailAddress, email.Recipients[0].Email);
				AssertEquals("Document Upload Status (CDS Entry Document - 2GB427168118378-B60004374.pdf)|Job Number: B00000999", email.Subject);
				AssertEquals(@"Failure to deliver to CDS: CDS Entry Document - 2GB427168118378-B60004374.pdf
Job Number: B00000999
Details: Unable to connect to the server", email.Body);
			}
			else
			{
				AssertEquals("No emails should be created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestProcessRejectedResponse_JobDeclaration()
		{
			var declaration = SetUp_Declaration();
			TestProcessRejectedResponse(declaration, expectEmailSent: true);
		}

		public void TestProcessRejectedResponse_AsycudaBill()
		{
			var bill = SetUp_AsycudaBill();
			TestProcessRejectedResponse(bill, expectEmailSent: false);
		}

		void TestProcessRejectedResponse(IMessageAttachee messageAttachee, bool expectEmailSent)
		{
			var response = CreateRejectedResponseMessage();
			messageAttachee.Messages.Add(response);
			new CDSDocumentUploadConfirmationMessageProcessor(new LoggingInformation()).ProcessMessage(response);
			Factory.Save();

			AssertEquals(messageAttachee, response.EM_LinkedObject);
			AssertEquals(EDIMessage.Status.ProcessedOK, response.EM_Status);

			StmALog log = default;
			AssertNoExceptionThrown(() => log = messageAttachee.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentUnallocatedCode).Single());
			var expectedLogReference = "Failure to deliver to CDS: |Job Number: B00000999|Details: Some rejected error message";
			AssertEquals("Reference", expectedLogReference, log.SL_Reference);

			if (expectEmailSent)
			{
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(user.GS_EmailAddress, email.Recipients[0].Email);
				AssertEquals("Document Upload Status ()|Job Number: B00000999", email.Subject);
				AssertEquals(@"Failure to deliver to CDS: 
Job Number: B00000999
Details: Some rejected error message", email.Body);
			}
			else
			{
				AssertEquals("No emails should be created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		CDSDocumentUploadConfirmationResponse CreateRejectedResponseMessage()
		{
			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			inboundInterchange.EI_Status = EDIInterchange.Status.Queued;
			inboundInterchange.EI_From = "eHub";
			inboundInterchange.EI_To = "WTG";
			inboundInterchange.EI_SessionGUID = new ZGuid("f127bdc2-b97b-4305-aaba-8026dc82949a");
			inboundInterchange.EI_BodyText = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</s0:ConversationID>
    <s0:eHubTrackingId>f127bdc2-b97b-4305-aaba-8026dc82949a</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <Root xmlns=""hmrc:fileupload"">
      <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
      <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
      <FileName></FileName>
      <Outcome>REJECTED</Outcome>
      <Details>Some rejected error message</Details>
    </Root>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";

			var message = Factory.New<CDSDocumentUploadConfirmationResponse>();
			message.EM_ApplicationReference = new ZString("f127bdc2-b97b-4305-aaba-8026dc82949a").KeepAlphanumericCharacters();
			message.EM_MessageText = @"<Root xmlns=""hmrc:fileupload"">
				  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
				  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
				  <FileName></FileName>
				  <Outcome>REJECTED</Outcome>
				  <Details>Some rejected error message</Details>
				</Root>";

			message.EM_EI = inboundInterchange.PK;
			message.EM_SystemCreateUser = "ABC";
			return message;
		}

		CDSDocumentUploadConfirmationResponse CreateResponseMessage(bool isSentSuccess)
		{
			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			inboundInterchange.EI_Status = EDIInterchange.Status.Queued;
			inboundInterchange.EI_From = "eHub";
			inboundInterchange.EI_To = "WTG";
			inboundInterchange.EI_SessionGUID = new ZGuid("f127bdc2-b97b-4305-aaba-8026dc82949a");
			if (isSentSuccess)
			{
				inboundInterchange.EI_BodyText = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</s0:ConversationID>
    <s0:eHubTrackingId>f127bdc2-b97b-4305-aaba-8026dc82949a</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <Root xmlns=""hmrc:fileupload"">
      <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
      <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
      <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
      <Outcome>SUCCESS</Outcome>
      <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
    </Root>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			}
			else
			{
				inboundInterchange.EI_BodyText = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader>
    <s0:ConversationID>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</s0:ConversationID>
    <s0:eHubTrackingId>f127bdc2-b97b-4305-aaba-8026dc82949a</s0:eHubTrackingId>
  </s0:ResponseHeader>
  <s0:ResponseBody>
    <Root xmlns=""hmrc:fileupload"">
      <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
      <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
      <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
      <Outcome>FAIL</Outcome>
      <Details>Unable to connect to the server</Details>
    </Root>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
			}

			var message = Factory.New<CDSDocumentUploadConfirmationResponse>();
			message.EM_ApplicationReference = new ZString("f127bdc2-b97b-4305-aaba-8026dc82949a").KeepAlphanumericCharacters();
			if (isSentSuccess)
			{
				message.EM_MessageText = @"<Root xmlns=""hmrc:fileupload"">
  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
  <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
  <Outcome>SUCCESS</Outcome>
  <Details>Thank you for submitting your documents. Typical clearance times are 2 hours for air and 3 hours for maritime declarations. During busy periods wait times may be longer.</Details>
</Root>";
			}
			else
			{
				message.EM_MessageText = @"<Root xmlns=""hmrc:fileupload"">
  <FileReference>622bc0f8-ef14-48dd-b1ca-0dbdeea96c24</FileReference>
  <BatchId>91dd2f2f-56cb-425b-a6fe-abd3ce8e7adc</BatchId>
  <FileName>CDS Entry Document - 2GB427168118378-B60004374.pdf</FileName>
  <Outcome>FAIL</Outcome>
  <Details>Unable to connect to the server</Details>
</Root>";
			}
			message.EM_EI = inboundInterchange.PK;
			message.EM_SystemCreateUser = "ABC";
			return message;
		}

		const string JobNumber = "B00000999";
		GlbStaff user;

		JobDeclaration SetUp_Declaration()
		{
			var outgoingMessage = SetupUserAndOutgoingMessage();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = JobNumber;
			declaration.Messages.Add(outgoingMessage);
			return declaration;
		}

		AsycudaBill SetUp_AsycudaBill()
		{
			var outgoingMessage = SetupUserAndOutgoingMessage();

			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			bill.ABL_BillNumber = JobNumber;
			bill.Messages.Add(outgoingMessage);
			return bill;
		}

		CDSEDIMessage SetupUserAndOutgoingMessage()
		{
			user = Factory.New<GlbStaff>();
			user.GS_Code = "ABC";
			user.GS_LoginName = "ABC";
			user.GS_EmailAddress = "abc@abc.com";
			user.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "1";
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_SessionGUID = new ZGuid("f127bdc2-b97b-4305-aaba-8026dc82949a");

			var outgoingMessage = Factory.New<CDSEDIMessage>();
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateUser = "ABC";

			return outgoingMessage;
		}
	}
}
