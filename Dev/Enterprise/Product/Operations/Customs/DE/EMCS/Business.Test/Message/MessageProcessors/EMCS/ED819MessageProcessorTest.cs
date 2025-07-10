using System;
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
	[TestedType(typeof(ED819MessageProcessor))]
	class ED819MessageProcessorTest : MessageProcessorAbstractTest<ED819MessageProcessor, EmcsInboundEDIMessage<IED819>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED819)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_DeclarationForEADNotFound()
		{
			CombineAssertions(() =>
			{
				eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN87654321");

				ProcessMessage(message);
				AssertNull(message.EM_LinkedObject);
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Declaration - Message Status", ZString.Empty, declaration.JE_MessageStatus);
				AssertEquals("Message - Status", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });
			});
		}

		public void TestProcessMessageCore_MessageGroupIsEME()
		{
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
			Factory.Save();
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.ALT, declaration.JE_EntryStatus);
				AssertEquals("Message Status", ZString.Empty, declaration.JE_MessageStatus);
			});
		}

		public void TestGenerateEmail()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail("No Reasons", declaration, ZString.Empty);
		}

		public void TestGenerateEmail_Rejected()
		{
			var reasonMock1 = new Mock<IED819Reason>();
			reasonMock1.Setup(m => m.ReasonCode).Returns("0");
			reasonMock1.Setup(m => m.ComplementaryInformation).Returns("Test first information");

			var reasonMock2 = new Mock<IED819Reason>();
			reasonMock2.Setup(m => m.ReasonCode).Returns("3");
			reasonMock2.Setup(m => m.ComplementaryInformation).Returns("Test second information");

			dataProviderMock.Setup(m => m.AlertOrRejectionReasons).Returns(new IED819Reason[] { reasonMock1.Object, reasonMock2.Object });

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings"">"
		+ "<th>Reason</th><th>Additional Information</th></tr></thead>"
		+ $"<tr><td>0 - Other</td><td>Test first information</td></tr>"
		+ $"<tr><td>3 - The quantity(ies) does(do) not match with the order</td><td>Test second information</td></tr></table>";

			AssertEmail("No Reasons", declaration, bodyMessageTable);
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98765432", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED819 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED819>> Processor => new ED819MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98765432", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98765432");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED819>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Emb);
			dataProviderMock.Setup(m => m.ExciseMovement).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.AlertOrRejectionReasons).Returns(Array.Empty<IED819Reason>());

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED819>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED819>> messageMock;
		Mock<IED819> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED819> message;
		GlbStaff user;

		void AssertEmail(string testCase, EMCSJobDeclaration job, ZString bodyMessageTable)
		{
			var reference = job.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS Notification of Alert or Rejection Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Notification of Alert or Rejection Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + job.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} received an Alert or Rejection. For details please follow the link to the Job.";
			AssertEmailForSingleRecipientWithTable(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}
	}
}
