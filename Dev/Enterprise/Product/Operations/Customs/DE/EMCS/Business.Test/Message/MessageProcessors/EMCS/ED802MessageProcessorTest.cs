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
	[TestedType(typeof(ED802MessageProcessor))]
	class ED802MessageProcessorTest : MessageProcessorAbstractTest<ED802MessageProcessor, EmcsInboundEDIMessage<IED802>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED802)null);
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

		public void TestProcessMessageCore_DeclarationForEADNotFound()
		{
			CombineAssertions(() =>
			{
				eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761236");

				ProcessMessage(message);
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.REM, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });
			});
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail("Generate Email", declaration, true);
		}

		public void TestGenerateEmail_NoReminderInformation()
		{
			dataProviderMock.Setup(m => m.ReminderInformation).Returns(ZString.Empty);
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail("Generate Email", declaration, false);
		}

		void AssertEmail(string testCase, EMCSJobDeclaration declaration, bool hasReminderInformation)
		{
			var reference = declaration.JE_DeclarationReference;
			var subject = $"EMCS Reminder Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Reminder Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $"Your EMCS Declaration for Job {reference} received a reminder message. For details please follow the Link to the Job."
				+ "<br /><br />ARC: MRN98761234"
				+ "<br /><br />Line No.: 1"
				+ "<br /><br />Limit Date Time: 18.05.2020 13:16:05"
				+ (hasReminderInformation ? "<br /><br />Reminder Information: You received this reminder information." : "")
				+ "<br /><br />Reminder Message Type: 1 - Erinnerungsmeldung bei Fristablauf f\u00fcr die \u00c4nderung des Bestimmungsorts";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			AssertEmailForSingleRecipient(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED802 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED802>> Processor => new ED802MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED802>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.ExciseMovement).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.LimitDateTime).Returns(new ZDateTime(2020, 05, 18, 13, 16, 05));
			dataProviderMock.Setup(m => m.ReminderInformation).Returns("You received this reminder information.");
			dataProviderMock.Setup(m => m.ReminderMessageType).Returns(EmcsReminderMessageType.Codes.ChangeOfDestination);

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED802>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED802>> messageMock;
		Mock<IED802> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED802> message;
	}
}
