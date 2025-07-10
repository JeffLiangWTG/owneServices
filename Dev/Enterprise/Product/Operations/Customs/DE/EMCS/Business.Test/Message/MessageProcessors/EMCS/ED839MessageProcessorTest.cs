using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED839MessageProcessor))]
	class ED839MessageProcessorTest : MessageProcessorAbstractTest<ED839MessageProcessor, EmcsInboundEDIMessage<IED839>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED839)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_BranchDoesNotMatch()
		{
			message.EM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;
			ProcessMessage(message);
			AssertNull(message.EM_LinkedObject);
		}

		public void TestGetLinkedObject_MultipleMRN()
		{
			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock.Setup(m => m.RejectedEads).Returns(new IEMCSEvent[] { eventMock.Object, eventMock2.Object });

			ProcessMessage(message);
			AssertNull("Message contains multiple MRNs", message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_DeclarationForEADNotFound()
		{
			CombineAssertions(() =>
			{
				eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761236");

				ProcessMessage(message);
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.Discarded, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.ERJ, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestNoteForDeclarationsNotFoundFromEADNumbers()
		{
			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN11111111");
			eventMock2.Setup(m => m.SequenceNumber).Returns("1");

			var eventMock3 = new Mock<IEMCSEvent>();
			eventMock3.Setup(m => m.AdministrativeReferenceCode).Returns("MRN22222222");
			eventMock3.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock.Setup(m => m.RejectedEads).Returns(new IEMCSEvent[] { eventMock.Object, eventMock2.Object, eventMock3.Object });

			ProcessMessage(message);

			var stmNote = GetStmNote(message);
			AssertMultilineASCIIEquals("EADNumber's for which a Declaration could not be found:\r\nMRN11111111\r\nMRN22222222", stmNote.ST_NoteText);
		}

		public void TestDocumentsAttachedForSingleEAD()
		{
			var attachedDocuments = new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			};

			messageMock.Setup(m => m.AttachedDocuments).Returns(attachedDocuments);

			CombineAssertions(() =>
			{
				var docManagerSupport = (IDocManagerSupport)declaration;
				AssertEquals("No eDocs to start with", 0, docManagerSupport.DocManagerInfo.AllEDocs.Count);

				ProcessMessage(message);

				AssertEquals("eDoc attached only once", 1, docManagerSupport.DocManagerInfo.AllEDocs.Count);
				attachedDocuments[0].ImageData.Dispose();
			});
		}

		public void TestDocumentsAttachedForMultipleDeclarations()
		{
			var declaration2 = Factory.CreateDeclarationWithEadReference("MRN98761235", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);
			Factory.Save();

			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock.Setup(m => m.RejectedEads).Returns(new IEMCSEvent[] { eventMock.Object, eventMock2.Object });

			var attachedDocuments = new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			};

			messageMock.Setup(m => m.AttachedDocuments).Returns(attachedDocuments);

			CombineAssertions(() =>
			{
				var docManagerSupportDec1 = (IDocManagerSupport)declaration;
				var docManagerSupportDec2 = (IDocManagerSupport)declaration2;
				AssertEquals("Dec1 no eDocs", 0, docManagerSupportDec1.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Dec2 no eDocs", 0, docManagerSupportDec2.DocManagerInfo.AllEDocs.Count);

				ProcessMessage(message);

				AssertEquals("Dec1 attached Doc", 1, docManagerSupportDec1.DocManagerInfo.AllEDocs.Count);
				AssertEquals("Dec2 attached Doc", 1, docManagerSupportDec2.DocManagerInfo.AllEDocs.Count);
				attachedDocuments[0].ImageData.Dispose();
			});
		}

		public void TestGenerateEmail()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail("Single", declaration, 1, "MRN98761234");
		}

		public void TestGenerateEmail_NoMRN()
		{
			dataProviderMock.Setup(m => m.MRN).Returns(ZString.Empty);

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail("No MRN", declaration, 1, "MRN98761234", false);
		}

		public void TestGenerateEmail_Multiple()
		{
			var declaration2 = Factory.CreateDeclarationWithEadReference("MRN98761235", "2", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);
			Factory.Save();

			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("2");

			dataProviderMock.Setup(m => m.RejectedEads).Returns(new[] { eventMock.Object, eventMock2.Object });

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			var lastOutgoingMessage2 = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration2, message.EM_MessageNum);
			lastOutgoingMessage2.EM_SystemCreateUser = user.GS_Code;
			declaration2.Messages.Add(lastOutgoingMessage2);

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEmail("Multiple-Job 1", declaration, 1, "MRN98761234");
				AssertEmail("Multiple-Job 2", declaration2, 2, "MRN98761235");
			});
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookLocalReferenceNumber", "B000222547896254786321", message.GetLogbookLocalReferenceNumber());
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("2");

			dataProviderMock.Setup(m => m.RejectedEads).Returns(new[] { eventMock.Object, eventMock2.Object });

			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "20DE12365485421158E2, MRN98761234, MRN98761235", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED839 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED839>> Processor => new ED839MessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED839>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.RejectedEads).Returns(new IEMCSEvent[] { eventMock.Object });
			dataProviderMock.Setup(m => m.SendingCustomsOffice).Returns("DE003302");
			dataProviderMock.Setup(m => m.IssuanceDate).Returns(new ZDate(2020, 01, 09));
			dataProviderMock.Setup(m => m.MRN).Returns("20DE12365485421158E2");
			dataProviderMock.Setup(m => m.RejectionReasonCode).Returns("1");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("B000222547896254786321");

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED839>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED839>> messageMock;
		Mock<IED839> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED839> message;
		GlbStaff user;

		void AssertEmail(string testCase, EMCSJobDeclaration job, ZInt sequenceNo, ZString arc, bool hasMRN = true)
		{
			var reference = job.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS e-AD rejected Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS e-AD rejected Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + job.PK;
			var bodyMessageSummary = $"Your EMCS Declaration for Job {reference} was rejected by customs. For details please follow the link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Sending Customs Office</td><td>DE003302</td></tr>"
								   + "<tr><td>Date of Issuance</td><td>09.01.2020</td></tr>"
								   + (hasMRN ? "<tr><td>MRN</td><td>20DE12365485421158E2</td></tr>" : "")
								   + "<tr><td>Rejection Reason</td><td>1 - Registriernr. Einheitspapier Einfuhr nicht gefunden</td></tr>"
								   + "</table>"
								   + "<br />"
								   + @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + @"<thead><tr class=""tableheadings""><th>Sequence No.</th><th>ARC</th></tr></thead>"
								   + $"<tr><td>{sequenceNo}</td><td>{arc}</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}
	}
}
