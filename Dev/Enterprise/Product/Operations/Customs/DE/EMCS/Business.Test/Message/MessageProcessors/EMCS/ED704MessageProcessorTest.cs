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
	[TestedType(typeof(ED704MessageProcessor))]
	class ED704MessageProcessorTest : MessageProcessorAbstractTest<ED704MessageProcessor, EmcsInboundEDIMessage<IED704>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED704)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			CombineAssertions(() =>
			{
				declaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REM;
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.REM, declaration.JE_EntryStatus);
				AssertEquals("Declaration Message Status", EDIMessage.Status.Rejected, declaration.JE_MessageStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });
			});
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			originalMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(message);

			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS Submission Rejected Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Submission Rejected Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} has been rejected. For details please follow the Link to the Job."
										+ "<br />"
										+ "<br />"
										+ "ARC: MRN98761234"
										+ "<br />"
										+ "<br />"
										+ "Local Reference Number: B000222547896254786321"
										+ "<br />"
										+ "<br />"
										+ "Error Details:";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ @"<thead><tr class=""tableheadings""><th>Error Number</th><th>Line Number</th><th>Column Number</th><th>Error Type</th><th>Error Reason</th></tr></thead>"
									+ "<tr><td>COM-00001</td><td>1</td><td>2</td><td>26 - Identische Nachricht liegt bereits vor</td><td>Identische Nachricht liegt bereits vor</td></tr>"
									+ "<tr><td>COM-00002</td><td>2</td><td>3</td><td>90 - Unbekannter Referenzcode (ARC)</td><td>Unbekannter Referenzcode (ARC)</td></tr>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookLocalReferenceNumber", "B000222547896254786321", message.GetLogbookLocalReferenceNumber());
		}

		public void TestPopulateLogbookRegistrationNumber_ARC()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegistrationNumber_CorrelationIdentifier()
		{
			dataProviderMock.Setup(m => m.AdministrativeReferenceCode).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", correlationIdentifier, message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED704 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED704>> Processor => new ED704MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			originalMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, correlationIdentifier);

			var errorMock1 = new Mock<IEMCSError>();
			errorMock1.Setup(m => m.ErrorNumber).Returns("COM-00001");
			errorMock1.Setup(m => m.LineNumber).Returns("1");
			errorMock1.Setup(m => m.ColumnNumber).Returns("2");
			errorMock1.Setup(m => m.ErrorType).Returns("26");
			errorMock1.Setup(m => m.ErrorReason).Returns("Identische Nachricht liegt bereits vor");

			var errorMock2 = new Mock<IEMCSError>();
			errorMock2.Setup(m => m.ErrorNumber).Returns("COM-00002");
			errorMock2.Setup(m => m.LineNumber).Returns("2");
			errorMock2.Setup(m => m.ColumnNumber).Returns("3");
			errorMock2.Setup(m => m.ErrorType).Returns("90");
			errorMock2.Setup(m => m.ErrorReason).Returns("Unbekannter Referenzcode (ARC)");

			dataProviderMock = new Mock<IED704>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.CorrelationIdentifier).Returns(correlationIdentifier);
			dataProviderMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("B000222547896254786321");
			dataProviderMock.Setup(m => m.Errors).Returns(new IEMCSError[] { errorMock1.Object, errorMock2.Object });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED704>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED704>> messageMock;
		Mock<IED704> dataProviderMock;
		EmcsInboundEDIMessage<IED704> message;
		EDIMessage originalMessage;
		const string correlationIdentifier = "DE90003480001011";
	}
}
