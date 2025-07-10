using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED813ConsignorMessageProcessor))]
	class ED813ConsignorMessageProcessorTest : MessageProcessorAbstractTest<ED813ConsignorMessageProcessor, EmcsInboundEDIMessage<IED813>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED813)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject_BranchDoesNotMatch()
		{
			message.EM_GB = Factory.New<GlbCompany>().Branches.AddNew().PK;
			ProcessMessage(message);
			AssertNull(message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			ProcessMessageAndAssertResult(declaration, EntryStatusList.Codes.CHG, EDIMessage.Status.ProcessedOK, EDIMessage.Status.Received, ExpectedSequenceNumber);
		}

		public void TestProcessMessageCore_DeclarationForEADNotFound()
		{
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("IncorrectReferenceCode");
			ProcessMessageAndAssertResult(null, ZString.Empty, EDIMessage.Status.Error, ZString.Empty, OriginalSequenceNumber);
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
			var subject = $"EMCS Change of Destination Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS Change of Destination Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your EMCS Declaration for Job {reference} received a notification of a changed destination. For details please follow the link to the Job.<br /><br />ARC: MRN198761234<br /><br />New Destination Code: 1 Destination - Tax Warehouse";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipient(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN198761234", message.GetLogbookRegistrationNumber());
		}

		public void TestDeclarationNotUpdated()
		{
			declaration.ZG_JourneyTime = "8H";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			declaration.InvoiceNumber = "INV001";
			declaration.InvoiceDate = new ZDate(2020, 2, 15);
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			declaration.ZG_GuarantorType = "24";
			declaration.EADNumber = "EAD123";
			declaration.CarrierAgentDocumentaryAddress.Address1 = "CarrierAgent Address1";
			declaration.OwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			declaration.DestinationWarehouseDocumentaryAddress.Address1 = "DestinationWarehouseDocumentaryAddress Address1";
			declaration.TransporterDocumentaryAddress.Address1 = "TransporterDocumentaryAddress Address1";
			declaration.CusContainers.AddNew().ZG_UnitCode = "3";

			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("ZG_JourneyTime", "8H", declaration.ZG_JourneyTime);
				AssertEquals("JE_TransportMode", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
				AssertEquals("SpecialInstructions", "Some instruction", declaration.SpecialInstructions);
				AssertEquals("ZG_TransportArrangement", EMCSTransportArrangementList.Codes.Other, declaration.ZG_TransportArrangement);
				AssertEquals("InvoiceNumber", "INV001", declaration.InvoiceNumber);
				AssertEquals("InvoiceDate", new ZDate(2020, 2, 15), declaration.InvoiceDate);
				AssertEquals("JE_MessageSubType", EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee, declaration.JE_MessageSubType);
				AssertEquals("ZG_GuarantorType", "24", declaration.ZG_GuarantorType);
				AssertEquals("EADNumber", "EAD123", declaration.EADNumber);
				AssertEquals("CarrierAgentDocumentaryAddress", "CarrierAgent Address1", declaration.CarrierAgentDocumentaryAddress.Address1);
				AssertEquals("OwnerDocumentaryAddress", "OwnerDocumentaryAddress Address1", declaration.OwnerDocumentaryAddress.Address1);
				AssertEquals("DestinationWarehouseDocumentaryAddress", "DestinationWarehouseDocumentaryAddress Address1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
				AssertEquals("TransporterDocumentaryAddress", "TransporterDocumentaryAddress Address1", declaration.TransporterDocumentaryAddress.Address1);
				AssertEquals("TransportDetails", "3", string.Join(", ", declaration.CusContainers.Cast<EMCSCusContainer>().Select(x => x.ZG_UnitCode)));
			});
		}

		protected override ZString MessageFriendlyName => "EMCS ED813 Consignor Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED813>> Processor => new ED813ConsignorMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN198761234");
			eventMock.Setup(m => m.SequenceNumber).Returns(ExpectedSequenceNumber);

			declaration = Factory.CreateDeclarationWithEadReference(eventMock.Object.AdministrativeReferenceCode, OriginalSequenceNumber, EMCSEntryTypeList.Codes.Consignor);
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;

			dataProviderMock = new Mock<IED813>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.UpdateEadEsad).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.NewDestinationCode).Returns(OriginalSequenceNumber);

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED813>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}

		void ProcessMessageAndAssertResult(BusinessObject expectedLinkedObject, ZString expectedEntryStatus, ZString expectedEmStatus, ZString expectedDeclarationMessageStatus, ZString expectedSequenceNumber)
		{
			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Linked Object", expectedLinkedObject, message.EM_LinkedObject);
				AssertEquals("Entry Status", expectedEntryStatus, declaration.JE_EntryStatus);
				AssertEquals("Message Status", expectedEmStatus, message.EM_Status);
				AssertEquals("Declaration - Message Status", expectedDeclarationMessageStatus, declaration.JE_MessageStatus);
				AssertEquals("Declaration - Sequence number updated", expectedSequenceNumber, declaration.SequenceNumber);
			});
		}

		const string ExpectedSequenceNumber = "2";
		const string OriginalSequenceNumber = "1";

		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED813>> messageMock;
		Mock<IED813> dataProviderMock;
		Mock<IEMCSEvent> eventMock;
		EmcsInboundEDIMessage<IED813> message;
	}
}
