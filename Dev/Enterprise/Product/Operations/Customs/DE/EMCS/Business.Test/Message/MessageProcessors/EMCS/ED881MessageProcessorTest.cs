using System.Linq;
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
	[TestedType(typeof(ED881MessageProcessor))]
	class ED881MessageProcessorTest : MessageProcessorAbstractTest<ED881MessageProcessor, EmcsInboundEDIMessage<IED881>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED881)null);
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
				eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN87654321");

				ProcessMessage(message);
				AssertNull(message.EM_LinkedObject);
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Declaration - Message Status", ZString.Empty, declaration.JE_MessageStatus);
				AssertEquals("Message - Status", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed_RequestAccepted()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.MAN, declaration.JE_EntryStatus);
				AssertEquals("Message - Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed_RequestRejected()
		{
			dataProviderMock.Setup(m => m.RequestAccepted).Returns(ZBool.False);
			dataProviderMock.Setup(m => m.RejectionReason).Returns("1");
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Empty Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message - Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestGenerateEmail_Accepted()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail(true, ZString.Empty);
		}

		public void TestGenerateEmail_Rejected()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			dataProviderMock.Setup(m => m.RequestAccepted).Returns(ZBool.False);
			dataProviderMock.Setup(m => m.RejectionReason).Returns("1");

			ProcessMessage(message);
			AssertEmail(false, "<br /><br />Manual Closure Rejection Reason: 1 - Vorgelegter Nachweis rechtfertigt keine manuelle Erledigung");
		}

		public void TestGenerateEmail_Rejected_RejectedReason_0()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			dataProviderMock.Setup(m => m.RequestAccepted).Returns(ZBool.False);
			dataProviderMock.Setup(m => m.RejectionReason).Returns("0");
			dataProviderMock.Setup(m => m.RejectionComplement).Returns("Another reason for rejection");

			ProcessMessage(message);
			var rejectionInformation = "<br /><br />Manual Closure Rejection Reason: 0 - Andere"
				+ "<br /><br />Rejection Complement: Another reason for rejection";
			AssertEmail(false, rejectionInformation);
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "20DE12365485421158E2", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED881 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED881>> Processor => new ED881MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("20DE12365485421158E2", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("20DE12365485421158E2");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED881>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.MessageSender).Returns("DE003302");
			dataProviderMock.Setup(m => m.RequestAccepted).Returns(ZBool.True);
			dataProviderMock.Setup(m => m.ResponseAttributes).Returns(eventMock.Object);

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED881>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED881>> messageMock;
		Mock<IED881> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED881> message;
		GlbStaff user;

		void AssertEmail(ZBool accepted, ZString optionalRejectionInformation)
		{
			var acceptedRejected = accepted ? "Accepted" : "Rejected";

			var reference = declaration.JE_DeclarationReference;
			var subject = $"EMCS Manual Closure {acceptedRejected} Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>EMCS Manual Closure {acceptedRejected} Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = (accepted ? $"Your EMCS Declaration for Job {reference} has been closed manually. For details please follow the link to the Job." : $"Your request for a Manual Closure of Job {reference} has been rejected. For details please follow the link to the Job.")
				+ "<br /><br />Sending Customs Office: DE003302"
				+ "<br /><br />MRN: 20DE12365485421158E2"
				+ "<br /><br />Sequence Number: 1"
				+ optionalRejectionInformation;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			});
		}
	}
}
