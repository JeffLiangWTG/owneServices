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
	[TestedType(typeof(ED840MessageProcessor))]
	class ED840MessageProcessorTest : MessageProcessorAbstractTest<ED840MessageProcessor, EmcsInboundEDIMessage<IED840>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED840)null);
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
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.EVT, declaration.JE_EntryStatus);
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
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);

			var reference = declaration.JE_DeclarationReference;
			var subject = $"EMCS Event Report Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Event Report Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} has received an Event Report. For details please follow the Link to the Job.<br /><br />ARC: MRN98761234";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "EMCS ED840 Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED840>> Processor => new ED840MessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.CreateDeclarationWithEadReference("MRN98761234", "1", EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor);

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN98761234");
			eventMock.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED840>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001005");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.ExciseMovement).Returns(eventMock.Object);

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED840>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}

		EMCSJobDeclaration declaration;
		Mock<IEMCSEvent> eventMock;
		Mock<IED840> dataProviderMock;
		Mock<EmcsInboundEDIMessage<IED840>> messageMock;
		EmcsInboundEDIMessage<IED840> message;
	}
}
