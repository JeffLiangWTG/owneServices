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
	[TestedType(typeof(ED810MessageProcessor))]
	class ED810MessageProcessorTest : MessageProcessorAbstractTest<ED810MessageProcessor, EmcsInboundEDIMessage<IED810>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED810)null);
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

		public void TestProcessMessageCore_MessageProcessed()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.CAN, declaration.JE_EntryStatus);
				AssertEquals("Declaration - Message Status", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("Message - Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestProcessMessage_MessageGroupIsEmb()
		{
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignee;
			Factory.Save();
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Emb);

			ProcessMessage(message);
			CombineAssertions(EmcsMessageSubTypeList.Codes.Emb, () =>
			{
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.CAN, declaration.JE_EntryStatus);
				AssertEquals("Declaration - Message Status", ZString.Empty, declaration.JE_MessageStatus);
				AssertEquals("Message - Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
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

			var reference = declaration.JE_DeclarationReference;
			var subject = $"EMCS Cancellation of e-AD Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Cancellation of e-AD Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $"Your EMCS Declaration for Job {reference} has been canceled. For details please follow the link to the Job.<br /><br />ARC: MRN98765432<br /><br />Cancellation Reason: Sonstiger";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98765432", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED810 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED810>> Processor => new ED810MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98765432", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98765432");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED810>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.ExciseMovementEad).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.CancellationReasonCode).Returns("0");

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED810>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED810>> messageMock;
		Mock<IED810> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED810> message;
	}
}
