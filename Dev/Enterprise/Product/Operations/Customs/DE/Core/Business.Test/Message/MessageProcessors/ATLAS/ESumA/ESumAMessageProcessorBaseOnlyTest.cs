using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.ESumA.Testing
{
	[TestedType(typeof(ESumaMessageProcessorForTest))]
	sealed class ESumAMessageProcessorBaseOnlyTest : MessageProcessorAbstractTest<ESumaMessageProcessorForTest, AtlasInboundEDIMessage<IESumADataProvider>>
	{
		public void TestProcessMessage_Successfully()
		{
			MockAttachedDocument();
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, message.EM_GB);
				AssertEquals("EM_Status = PRS", AtlasEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertNull("No StmNote", GetStmNote(message));
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.First();
				Assert("Email has document attached", email.Attachments.Cast<AttachmentDef>().Any(x => x.DisplayName == "file1.pdf"));
			});
		}

		public void TestLinkedObject()
		{
			ProcessMessage(message);
			AssertNull(message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IESumADataProvider)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestEmail()
		{
			MockAttachedDocument();
			ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var subject = "ESumA Declaration Message Status Response";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = ZString.Empty;
			var bodyMessageSummary = @"A response message has been received from Customs.<br />Shown below is a summary of relevant information received in the message.<br />
<br />
MRN: REFERENCENUMBER</br>Local Reference: LOCALREFERENCENUMBER</br></br>For details please see attached PDF-Report.";

			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient(ZString.Empty
					, email
					, "bob@builder.com"
					, subject
					, bodyMessageTitle
					, bodyMessageHeader
					, bodyMessageSummary);
				var attachment = email.Attachments.Cast<AttachmentDef>().Last();
				AssertEquals("Email FileName", "file1.pdf", attachment.DisplayName);
				AssertEquals("Email ImageData", Convert.FromBase64String("XXX="), attachment.Data);
			});
		}

		public void TestStmNoteForMissingOrganisation()
		{
			orgHeader.Delete();
			ProcessMessage(message);
			var stmNote = GetStmNote(message);
			AssertEquals(GetStmNoteText("Organization with EORINumber: 'GR123456789'"), stmNote.ST_NoteText);
		}

		public void TestStmNoteForMissingPremissesAddress()
		{
			orgHeader.MainAddress.Delete();
			ProcessMessage(message);
			var stmNote = GetStmNote(message);
			AssertEquals(GetStmNoteText("premisses address for Organization-Code: 'ORGTEST' and EORIBranch: '0000'"), stmNote.ST_NoteText);
		}

		public void TestStmNoteForMissingEMailAddress()
		{
			orgHeader.MainAddress.OA_Email = ZString.Empty;
			ProcessMessage(message);
			var stmNote = GetStmNote(message);
			AssertEquals(GetStmNoteText("recipients Email-Address for Organization-Code: 'ORGTEST', Address: 'TESTSTRASSE 1, MAINZ'"), stmNote.ST_NoteText);
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(message);
			AssertEquals("REFERENCENUMBER", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "ESumA ENSSTA Message Processor";

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>> Processor => new ESumAENSSTAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.GetOrgHeaderWithEoriNumberAndEORIBranch("ORGTEST", "123456789", Core.Constants.CountryCodes.Greece, "0000");
			orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Email = "bob@builder.com";
			orgAddress.OA_Address1 = "TESTSTRASSE 1";
			orgAddress.OA_City = "MAINZ";
			dataProviderMock = new Mock<IESumADataProvider>();
			dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns("GR123456789");
			dataProviderMock.Setup(m => m.InterchangeRecipientSubsidiaryNumber).Returns("0000");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("LOCALREFERENCENUMBER");
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("REFERENCENUMBER");
			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IESumADataProvider>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}
		AtlasInboundEDIMessage<IESumADataProvider> message;
		OrgHeader orgHeader;
		OrgAddress orgAddress;
		Mock<AtlasInboundEDIMessage<IESumADataProvider>> messageMock;
		Mock<IESumADataProvider> dataProviderMock;

		ZString GetStmNoteText(ZString reason) => $"Couldn't send Email due to missing or invalid {reason}.";

		void MockAttachedDocument()
		{
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
		}
	}

	class ESumaMessageProcessorForTest : ESumAMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>, IESumADataProvider>
	{
		public ESumaMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "ESumA Message Processor For Test";
	}
}
