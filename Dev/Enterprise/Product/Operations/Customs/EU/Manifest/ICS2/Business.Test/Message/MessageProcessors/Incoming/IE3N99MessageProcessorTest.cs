using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N99;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N99MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N99MessageProcessor, Ie3N99Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override string ExpectedEmailSubject => "ICS2 Error Notification for MAN0009999/CommonMasterBill";

		protected override string[] ExpectedEmailBody => new []
		{
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">" +
			"<thead><tr class=\"tableheadings\"><th>Code</th><th>Description</th><th>Technical Error Message</th><th>Message Element Path</th></tr></thead>" +
			"<tr><td>1122</td><td>description1</td><td>technicalErrorMessage1</td><td>messageElementPath1</td></tr>" +
			$"<tr><td>2233</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
			"</table>"
		};

		protected override string EntryNumberTypeToCreate => CusEntryNumberTypes.Standard.LocalReferenceNumber;

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("CusEntryHeader.CH_Status", MessageStatusCodeList.Codes.Error, manifestHeader.AMA_MessageStatus);
		}

		public void TestProcessMessageAndUpdateRequestHeader()
		{
			(var manifestHeader, var incomingMessage) = PrepareForEmailTesting();

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Status = MessageStatusCodeList.Codes.Awaiting;
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFS;

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "B90";
			requestHeader2.EUS_Status = MessageStatusCodeList.Codes.Accepted;
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_RFI;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("Should update its status as its status is RFS.", MessageStatusCodeList.Codes.Error, requestHeader1.EUS_Status);
					AssertEquals("Should not update the status as its status is not RFS.", MessageStatusCodeList.Codes.Accepted, requestHeader2.EUS_Status);
				});
			}
		}

		protected override TestEdiMessage GetIncomingMessage(string localReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N99;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N99 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<functionalReference>{0}</functionalReference>
	<notificationDate>
		<DateTime>1900-01-01T01:01:01+08:00</DateTime>
	</notificationDate>
	<addressedMemberState>
	<country>T1</country>
	</addressedMemberState>
	<representative>
		<identificationNumber>identificationNu1</identificationNumber>
	</representative>
	<transportDocument>
		<documentNumber>documentNumber1</documentNumber>
		<type>Tok1</type>
	</transportDocument>
	<declarant>
		<identificationNumber>identificationNu1</identificationNumber>
	</declarant>
	<customsOfficeOfFirstEntry>
		<referenceNumber>referen1</referenceNumber>
	</customsOfficeOfFirstEntry>
	<error>
		<technicalErrorMessage>technicalErrorMessage1</technicalErrorMessage>
		<description>description1</description>
		<validationCode>1122</validationCode>
		<pointer>
			<messageElementPath>messageElementPath1</messageElementPath>
		</pointer>
	</error>
	<error>
		<validationCode>2233</validationCode>
	</error>
</IE3N99>", localReferenceNumber);

			return incomingMessage;
		}

		protected override IE3N99MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N99MessageProcessor(logger);
		}
	}
}
