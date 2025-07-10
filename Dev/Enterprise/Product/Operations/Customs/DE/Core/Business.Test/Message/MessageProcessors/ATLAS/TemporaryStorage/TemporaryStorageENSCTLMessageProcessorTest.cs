using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageENSCTLMessageProcessor))]
	sealed class TemporaryStorageENSCTLMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageENSCTLMessageProcessor, AtlasInboundEDIMessage<IENSCTL>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IENSCTL)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestSendEmailNotification_Successfully()
		{
			var orgHeader = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("ORG", "123456", Core.Constants.CountryCodes.Germany, "0000");
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Email = ExpectedEmailRecipientsMailAddress;

			var attachments = new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			};
			messageMock.Setup(m => m.AttachedDocuments).Returns(attachments);
			ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.First();
			var subject = "EKS Declaration Message Status Response";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageSummary = "For details please see attached PDF-Report.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
								   + $"<tr><td>Reference</td><td>{TransportDocumentNumber}</td></tr>"
								   + "</table>";
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipientWithTable("Email content", email, ExpectedEmailRecipientsMailAddress, subject, bodyMessageTitle, ZString.Empty, bodyMessageSummary, bodyMessageTable);
				Assert("Email has document attached", email.Attachments.Cast<AttachmentDef>().Any(x => x.DisplayName == "file1.pdf"));
				AssertEquals("MessageStatus", AtlasEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertNull("No Message Note", message.GetNote("Could not send Notification Email"));
				AssertEquals("LogbookRegistrationNumber", MRN, message.GetLogbookRegistrationNumber());
			});
		}

		public void TestSendEmailNotification_NotSuccessfully()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("No eMail created", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("MessageStatus", AtlasEDIMessage.Status.Discarded, message.EM_Status);
				AssertEquals("Message Note", "Could not send Notification Email due to non matching Data to obtain Organization, Address, Email-Address.", message.GetNote("Could not send Notification Email"));
				AssertEquals("LogbookRegistrationNumber", MRN, message.GetLogbookRegistrationNumber());
			});
		}

		protected override ZString MessageFriendlyName => "Temporary Storage ENSCTL Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IENSCTL>> Processor => new TemporaryStorageENSCTLMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();
			dataProviderMock = new Mock<IENSCTL>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("ENSCTL123456");
			dataProviderMock.Setup(m => m.MessageRecipientIdentificationNumber).Returns("DE123456");
			dataProviderMock.Setup(m => m.MessageRecipientSubsidiaryNumber).Returns("0000");
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			dataProviderMock.Setup(m => m.TransportDocumentNumber).Returns(TransportDocumentNumber);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IENSCTL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		Mock<IENSCTL> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IENSCTL>> messageMock;
		AtlasInboundEDIMessage<IENSCTL> message;
		const string MRN = "23DE586601055987B7";
		const string TransportDocumentNumber = "654321";
		const string ExpectedEmailRecipientsMailAddress = "bob@builder.de";
	}
}
