using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED803MessageProcessor))]
	class ED803MessageProcessorTest : MessageProcessorAbstractTest<ED803MessageProcessor, EmcsInboundEDIMessage<IED803>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED803)null);
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
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.DIV, declaration.JE_EntryStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
		}

		public void TestDocumentsAttached()
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

		public void TestGenerateEmail()
		{
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail("Generated Email ", declaration, true);
		}

		public void TestGenerateEmail_NoDownstreamARC()
		{
			dataProviderMock.Setup(m => m.DownstreamARCs).Returns(Array.Empty<ZString>());

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail("Generated Email without Downstream ARC", declaration, false);
		}

		void AssertEmail(string testCase, EMCSJobDeclaration jobDeclaration, bool hasDownstreamARCs)
		{
			var reference = jobDeclaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			var subject = $"EMCS Notification of diverted e-AD Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Notification of diverted e-AD Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + jobDeclaration.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} received a notification of diverted e-AD. For details please follow the link to the Job.<br /><br />"
									+ "Notification Date/Time: 19.05.2020 10:30<br />"
									+ "Notification Type: 2 - Split<br />"
									+ "ARC: MRN98761234<br />"
									+ "Sequence Number: 1<br />";
			if (hasDownstreamARCs)
			{
				var bodyMessageTable = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">"
									+ "<thead><tr class=\"tableheadings\"><th>Downstream ARC</th></tr></thead>"
									+ "<tr><td>20DE66421598431563461</td></tr>"
									+ "<tr><td>20DE64915434650054634</td></tr>"
									+ "</table>";
				AssertEmailForSingleRecipientWithTable(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			}
			else
			{
				AssertEmailForSingleRecipient(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			}
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED803 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED803>> Processor => new ED803MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED803>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.ExciseMovementEad).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.NotificationDateTime).Returns(new ZDateTime(2020, 5, 19, 10, 30, 0));
			dataProviderMock.Setup(m => m.NotificationType).Returns(DataProviderHelpers.XmlEnumToString(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED803BBodyNotificationOfDivertedEadExciseNotificationNotificationType.Item2));
			dataProviderMock.Setup(m => m.DownstreamARCs).Returns(new ZString[] { "20DE66421598431563461", "20DE64915434650054634" });
			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED803>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			message = messageMock.Object;
			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED803>> messageMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED803> message;
		Mock<IED803> dataProviderMock;
		GlbStaff user;
	}
}
