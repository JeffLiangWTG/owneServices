using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED829MessageProcessor))]
	class ED829MessageProcessorTest : MessageProcessorAbstractTest<ED829MessageProcessor, EmcsInboundEDIMessage<IED829>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED829)null);
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

			dataProviderMock.Setup(m => m.ExciseMovementEads).Returns(new[] { eventMock.Object, eventMock2.Object });

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
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.EXP, declaration.JE_EntryStatus);
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

			dataProviderMock.Setup(m => m.ExciseMovementEads).Returns(new[] { eventMock.Object, eventMock2.Object, eventMock3.Object });

			ProcessMessage(message);

			var stmNote = GetStmNote(message);
			AssertMultilineASCIIEquals("EADNumber's for which a Declaration could not be found:\r\nMRN11111111\r\nMRN22222222", stmNote.ST_NoteText);
		}

		public void TestDocumentsAttachedForSingleEAD()
		{
			ProcessMessage(message);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });
		}

		public void TestDocumentsAttachedForMultipleDeclarations()
		{
			var declaration2 = Factory.CreateDeclarationWithEadReference("MRN98761235", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);
			Factory.Save();

			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock.Setup(m => m.ExciseMovementEads).Returns(new[] { eventMock.Object, eventMock2.Object });

			ProcessMessage(message);

			AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration, declaration2 });
		}

		public void TestGenerateEmail()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail("Single", declaration, 1, "MRN98761234");
		}

		public void TestGenerateEmail_Multiple()
		{
			var declaration2 = Factory.CreateDeclarationWithEadReference("MRN98761235", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);
			Factory.Save();

			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock.Setup(m => m.ExciseMovementEads).Returns(new[] { eventMock.Object, eventMock2.Object });

			var lastOutgoingMessage1 = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage1.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage1);
			var lastOutgoingMessage2 = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration2, message.EM_MessageNum);
			lastOutgoingMessage2.EM_SystemCreateUser = user.GS_Code;
			declaration2.Messages.Add(lastOutgoingMessage2);

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEmail("Multiple-Job 1", declaration, 1, "MRN98761234");
				AssertEmail("Multiple-Job 2", declaration2, 1, "MRN98761235");
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			var eventMock2 = new Mock<IEMCSEvent>();
			eventMock2.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761235");
			eventMock2.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock.Setup(m => m.ExciseMovementEads).Returns(new[] { eventMock.Object, eventMock2.Object });

			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "20DE12365485421158E2, MRN98761234, MRN98761235", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED829 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED829>> Processor => new ED829MessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED829>();
			dataProviderMock.Setup(m => m.SendingCustomsOffice).Returns("DE003302");
			dataProviderMock.Setup(m => m.AcceptanceDate).Returns(new ZDate(2020, 4, 30));
			dataProviderMock.Setup(m => m.MRN).Returns("20DE12365485421158E2");
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.ExciseMovementEads).Returns(new[] { eventMock.Object });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED829>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED829>> messageMock;
		Mock<IED829> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED829> message;
		GlbStaff user;

		void AssertEmail(string testCase, EMCSJobDeclaration job, ZInt sequenceNo, ZString arc)
		{
			var reference = job.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS Notification of Accepted Export Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Notification of Accepted Export Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + job.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} received a notification of accepted export. For details please follow the link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Sending Customs Office</td><td>DE003302</td></tr>"
								   + "<tr><td>Date of Acceptance</td><td>30.04.2020</td></tr>"
								   + "<tr><td>MRN</td><td>20DE12365485421158E2</td></tr>"
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
