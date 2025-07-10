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
	[TestedType(typeof(ED807MessageProcessor))]
	class ED807MessageProcessorTest : MessageProcessorAbstractTest<ED807MessageProcessor, EmcsInboundEDIMessage<IED807>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED807)null);
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
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761236");
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.INT, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });
			});
		}

		public void TestGenerateEmail()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail(declaration, true, true);
		}

		public void TestGenerateEmail_NoComplementaryInformation()
		{
			eventMock.Setup(m => m.ComplementaryInformation).Returns(ZString.Empty);

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail(declaration, true, true, false);
		}

		public void TestGenerateEmail_NoControlReportNumber()
		{
			dataProviderMock.Setup(m => m.ControlReportNumbers).Returns(Array.Empty<ZString>());

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail(declaration, false, true);
		}

		public void TestGenerateEmail_NoEventReportNumber()
		{
			dataProviderMock.Setup(m => m.EventReportNumbers).Returns(Array.Empty<ZString>());

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail(declaration, true, false);
		}

		public void TestGenerateEmail_NoControlReportNumberAndNoEventReportNumber()
		{
			dataProviderMock.Setup(m => m.ControlReportNumbers).Returns(Array.Empty<ZString>());
			dataProviderMock.Setup(m => m.EventReportNumbers).Returns(Array.Empty<ZString>());

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			AssertEmail(declaration, false, false);
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED807 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED807>> Processor => new ED807MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IED807Event>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");
			eventMock.Setup(m => m.Reason).Returns("4");
			eventMock.Setup(m => m.ComplementaryInformation).Returns("The movement has been interrupted because ...");

			dataProviderMock = new Mock<IED807>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.ExciseMovementEad).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.ControlReportNumbers).Returns(new ZString[] { "46332156", "64831215" });
			dataProviderMock.Setup(m => m.EventReportNumbers).Returns(new ZString[] { "64533189", "2036454D" });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED807>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED807>> messageMock;
		Mock<IED807Event> eventMock;
		EmcsInboundEDIMessage<IED807> message;
		Mock<IED807> dataProviderMock;
		GlbStaff user;

		void AssertEmail(EMCSJobDeclaration job, bool hasControlReportNumber, bool hasEventReportNumber, bool hasComplementaryInformation = true)
		{
			var reference = job.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS Interruption of Movement Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Interruption of Movement Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + job.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} received a notification that the movement has been interrupted. For details please follow the link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ "<tr><td>ARC</td><td>MRN98761234</td></tr>"
									+ "<tr><td>Reason</td><td>4 - Abbruch beantragt im Zuge einer Kontrolle</td></tr>"
									+ (hasComplementaryInformation ? "<tr><td>Complementary Information</td><td>The movement has been interrupted because ...</td></tr>" : "")
									+ "</table>";
			if (hasControlReportNumber)
			{
				bodyMessageTable += "<br />"
									+ "Control Report"
									+ "<br />"
									+ "<br />"
									+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ @"<thead><tr class=""tableheadings""><th>Control Report Number</th></tr></thead>"
									+ "<tr><td>46332156</td></tr>"
									+ "<tr><td>64831215</td></tr>"
									+ "</table>";
			}

			if (hasEventReportNumber)
			{
				bodyMessageTable += "<br />"
									+ "Event Report"
									+ "<br />"
									+ "<br />"
									+ @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
									+ @"<thead><tr class=""tableheadings""><th>Event Report Number</th></tr></thead>"
									+ "<tr><td>64533189</td></tr>"
									+ "<tr><td>2036454D</td></tr>"
									+ "</table>";
			}

			AssertEmailForSingleRecipientWithTable($"Has Control Report Number? {hasControlReportNumber}, Has Event Report Number? {hasControlReportNumber}, Has Complementary Information? {hasComplementaryInformation}:", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}
	}
}
