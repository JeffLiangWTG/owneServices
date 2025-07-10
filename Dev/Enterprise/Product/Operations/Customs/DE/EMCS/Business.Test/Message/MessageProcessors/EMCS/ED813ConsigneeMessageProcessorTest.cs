using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Customs.DE.EMCS.Messaging.Version2_4;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED813ConsigneeMessageProcessor))]
	class ED813ConsigneeMessageProcessorTest : MessageProcessorAbstractTest<ED813ConsigneeMessageProcessor, EmcsInboundEDIMessage<IED813>>
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
			ProcessMessageAndAssertResult(declaration, EntryStatusList.Codes.CHG, EDIMessage.Status.ProcessedOK, ZString.Empty, ExpectedSequenceNumber);
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

		public void TestUpdateDeclarationSequenceNumber_ConsigneeNull()
		{
			ProcessMessage(message);
			AssertEquals("SequenceNumber updated", ExpectedSequenceNumber, declaration.SequenceNumber);
		}

		public void TestUpdateDeclarationSequenceNumber_TraderIdEmpty()
		{
			var consigneeMock = new Mock<IEMCSPartyConsignee>();
			consigneeMock.Setup(m => m.TraderId).Returns(string.Empty);
			consigneeMock.Setup(m => m.Country).Returns(string.Empty);
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			ProcessMessage(message);

			AssertEquals("SequenceNumber updated", ExpectedSequenceNumber, declaration.SequenceNumber);
		}

		public void TestUpdateDeclarationSequenceNumber_TraderIdMatchesImporter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Latvia, "LVTI002", orgAddress.PK);
			Factory.Save();

			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var consigneeMock = new Mock<IEMCSPartyConsignee>();
			consigneeMock.Setup(m => m.TraderId).Returns("LVTI002");
			consigneeMock.Setup(m => m.Country).Returns("LV");
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			ProcessMessage(message);

			AssertEquals("SequenceNumber updated", ExpectedSequenceNumber, declaration.SequenceNumber);
		}

		public void TestUpdateDeclarationSequenceNumber_TraderIdNotMatchImporter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Germany, "DETI000", orgAddress.PK);
			var cusCode2 = orgHeader.CustomsCodes.AddNew();
			cusCode2.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Latvia, "DETI123", orgAddress.PK);
			var cusCode3 = orgHeader.CustomsCodes.AddNew();
			cusCode3.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Core.Constants.CountryCodes.Germany, "DETI123", orgAddress.PK);
			Factory.Save();

			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			var consigneeMock = new Mock<IEMCSPartyConsignee>();
			consigneeMock.Setup(m => m.TraderId).Returns("DETI123");
			consigneeMock.Setup(m => m.Country).Returns("DE");
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			ProcessMessage(message);

			AssertEquals("SequenceNumber not updated", OriginalSequenceNumber, declaration.SequenceNumber);
		}

		public void TestUpdateDeclarationSequenceNumber_NoImporterDocumentaryAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "A1";
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.Latvia, "LVTI002", orgAddress.PK);
			Factory.Save();

			var consigneeMock = new Mock<IEMCSPartyConsignee>();
			consigneeMock.Setup(m => m.TraderId).Returns("LVTI002");
			consigneeMock.Setup(m => m.Country).Returns("LV");
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			ProcessMessage(message);

			AssertEquals("SequenceNumber not updated", OriginalSequenceNumber, declaration.SequenceNumber);
		}

		public void TestUpdateDeclarationJourneyTime()
		{
			declaration.ZG_JourneyTime = "8H";
			ProcessMessage(message);
			AssertEquals("ZG_JourneyTime updated", "2D", declaration.ZG_JourneyTime);
		}

		public void TestUpdateDeclarationJourneyTime_Empty()
		{
			declaration.ZG_JourneyTime = "8H";
			dataProviderMock.Setup(m => m.JourneyTime).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals("ZG_JourneyTime not updated", "8H", declaration.ZG_JourneyTime);
		}

		public void TestUpdateDeclarationTransportMode()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("JE_TransportMode updated", ZString.Empty, declaration.JE_TransportMode);
				AssertEquals("SpecialInstructions cleared", "New instruction", declaration.SpecialInstructions);
			});
		}

		public void TestUpdateDeclarationTransportMode_Zero()
		{
			dataProviderMock.Setup(m => m.TransportModeCode).Returns("0");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("JE_TransportMode updated", Core.Constants.TransportModes.Other, declaration.JE_TransportMode);
				AssertEquals("SpecialInstructions updated", ZString.Empty, declaration.SpecialInstructions);
			});
		}

		public void TestUpdateDeclarationTransportMode_Empty()
		{
			dataProviderMock.Setup(m => m.TransportModeCode).Returns(ZString.Empty);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("JE_TransportMode not updated", Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);
				AssertEquals("SpecialInstructions not updated", "Some instruction", declaration.SpecialInstructions);
			});
		}

		public void TestUpdateDeclarationTransportMode_Invalid()
		{
			dataProviderMock.Setup(m => m.TransportModeCode).Returns("-1");
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.SpecialInstructions = "Some instruction";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("JE_TransportMode not updated", ZString.Empty, declaration.JE_TransportMode);
				AssertEquals("SpecialInstructions updated", "New instruction", declaration.SpecialInstructions);
			});
		}

		public void TestUpdateDeclarationTransportArrangement()
		{
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			ProcessMessage(message);
			AssertEquals("ZG_TransportArrangement updated", EMCSTransportArrangementList.Codes.OwnerOfGoods, declaration.ZG_TransportArrangement);
		}

		public void TestUpdateDeclarationTransportArrangement_Empty()
		{
			dataProviderMock.Setup(m => m.TransportArrangement).Returns(ZString.Empty);
			declaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			ProcessMessage(message);
			AssertEquals("ZG_TransportArrangement not updated", EMCSTransportArrangementList.Codes.Other, declaration.ZG_TransportArrangement);
		}

		public void TestUpdateDeclarationInvoiceNumber()
		{
			declaration.InvoiceNumber = "INV001";
			ProcessMessage(message);
			AssertEquals("InvoiceNumber updated", "1", declaration.InvoiceNumber);
		}

		public void TestUpdateDeclarationInvoiceNumber_Empty()
		{
			dataProviderMock.Setup(m => m.InvoiceNumber).Returns(ZString.Empty);
			declaration.InvoiceNumber = "INV001";
			ProcessMessage(message);
			AssertEquals("InvoiceNumber not updated", "INV001", declaration.InvoiceNumber);
		}

		public void TestUpdateDeclarationInvoiceDate()
		{
			declaration.InvoiceDate = new ZDate(2020, 2, 15);
			ProcessMessage(message);
			AssertEquals("InvoiceDate updated", new ZDate(2020, 7, 25), declaration.InvoiceDate);
		}

		public void TestUpdateDeclarationInvoiceDate_Empty()
		{
			dataProviderMock.Setup(m => m.InvoiceDate).Returns(ZDate.Empty);
			declaration.InvoiceDate = new ZDate(2020, 2, 15);
			ProcessMessage(message);
			AssertEquals("InvoiceDate not updated", new ZDate(2020, 2, 15), declaration.InvoiceDate);
		}

		public void TestUpdateDeclarationDestinationType()
		{
			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			ProcessMessage(message);
			AssertEquals("JE_MessageSubType updated", EMCSDestinationTypeList.Codes.DestinationDirectDelivery, declaration.JE_MessageSubType);
		}

		public void TestUpdateDeclarationGuarantorTypeCode()
		{
			declaration.ZG_GuarantorType = "24";
			ProcessMessage(message);
			AssertEquals("ZG_GuarantorType not updated", "3", declaration.ZG_GuarantorType);
		}

		public void TestUpdateDeclarationGuarantorTypeCode_Empty()
		{
			dataProviderMock.Setup(m => m.GuarantorTypeCode).Returns(ZString.Empty);
			declaration.ZG_GuarantorType = "24";
			ProcessMessage(message);
			AssertEquals("ZG_GuarantorType not updated", "24", declaration.ZG_GuarantorType);
		}

		public void TestUpdateDeclarationGuarantor()
		{
			var guarantorMock = new Mock<IEMCSPartyGuarantor>();
			guarantorMock.Setup(m => m.TraderExciseNumber).Returns("TN001");
			guarantorMock.Setup(m => m.VatNumber).Returns("VN001");
			guarantorMock.Setup(m => m.Language).Returns("DE");
			guarantorMock.Setup(m => m.Name).Returns("Guarantor Party 1");
			guarantorMock.Setup(m => m.Address).Returns("Gp Address 1");
			guarantorMock.Setup(m => m.City).Returns("Berlin");
			guarantorMock.Setup(m => m.Postcode).Returns("0001");
			guarantorMock.Setup(m => m.Country).Returns("DE");
			dataProviderMock.Setup(m => m.Guarantor).Returns(guarantorMock.Object);

			var oldOwnerDocumentaryAddress = declaration.OwnerDocumentaryAddress;
			oldOwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old OwnerDocumentaryAddress deleted", true, oldOwnerDocumentaryAddress.IsDeleted);
				AssertEquals("New OwnerDocumentaryAddress updated", "Gp Address 1", declaration.OwnerDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationGuarantor_GuarantorNull()
		{
			var oldOwnerDocumentaryAddress = declaration.OwnerDocumentaryAddress;
			oldOwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old OwnerDocumentaryAddress deleted", true, oldOwnerDocumentaryAddress.IsDeleted);
				AssertEquals("New OwnerDocumentaryAddress empty", ZString.Empty, declaration.OwnerDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationGuarantor_GuarantorTypeCodeEmpty()
		{
			dataProviderMock.Setup(m => m.GuarantorTypeCode).Returns(ZString.Empty);
			var oldOwnerDocumentaryAddress = declaration.OwnerDocumentaryAddress;
			oldOwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old OwnerDocumentaryAddress not deleted", false, oldOwnerDocumentaryAddress.IsDeleted);
				AssertEquals("Old OwnerDocumentaryAddress not updated", "OwnerDocumentaryAddress Address1", oldOwnerDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationEADNumber()
		{
			declaration.EADNumber = "EAD123";
			ProcessMessage(message);
			AssertEquals("EADNumber updated", "MRN198761234", declaration.EADNumber);
		}

		public void TestUpdateDeclarationDestinationWarehouse()
		{
			var deliveryPlaceMock = new Mock<IEMCSPartyDeliveryPlace>();
			deliveryPlaceMock.Setup(m => m.TraderId).Returns("TI002");
			deliveryPlaceMock.Setup(m => m.Language).Returns("DE");
			deliveryPlaceMock.Setup(m => m.Name).Returns("DeliveryPlace Party 1");
			deliveryPlaceMock.Setup(m => m.Address).Returns("DP Address 1");
			deliveryPlaceMock.Setup(m => m.City).Returns("Berlin");
			deliveryPlaceMock.Setup(m => m.Postcode).Returns("0001");
			deliveryPlaceMock.Setup(m => m.Country).Returns("DE");
			dataProviderMock.Setup(m => m.DeliveryPlace).Returns(deliveryPlaceMock.Object);

			var oldDestinationWarehouseDocumentaryAddress = declaration.DestinationWarehouseDocumentaryAddress;
			oldDestinationWarehouseDocumentaryAddress.Address1 = "DestinationWarehouseDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old DestinationWarehouseDocumentaryAddress deleted", true, oldDestinationWarehouseDocumentaryAddress.IsDeleted);
				AssertEquals("New DestinationWarehouseDocumentaryAddress updated", "DP Address 1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationDestinationWarehouse_Null()
		{
			var oldDestinationWarehouseDocumentaryAddress = declaration.DestinationWarehouseDocumentaryAddress;
			oldDestinationWarehouseDocumentaryAddress.Address1 = "DestinationWarehouseDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old DestinationWarehouseDocumentaryAddress not deleted", false, oldDestinationWarehouseDocumentaryAddress.IsDeleted);
				AssertEquals("Old DestinationWarehouseDocumentaryAddress not updated", "DestinationWarehouseDocumentaryAddress Address1", declaration.DestinationWarehouseDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationCarrierAgent()
		{
			var newTransportArrangerMock = CreatePartyTransporter("VN002", "TransportArranger Party 1", "TAP Address 1");
			dataProviderMock.Setup(m => m.NewTransportArranger).Returns(newTransportArrangerMock);
			var oldCarrierAgentDocumentaryAddress = declaration.CarrierAgentDocumentaryAddress;
			oldCarrierAgentDocumentaryAddress.Address1 = "CarrierAgent Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old CarrierAgentDocumentaryAddress deleted", true, oldCarrierAgentDocumentaryAddress.IsDeleted);
				AssertEquals("New CarrierAgentDocumentaryAddress updated", "TAP Address 1", declaration.CarrierAgentDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationCarrierAgent_TransportArrangerNull()
		{
			var oldCarrierAgentDocumentaryAddress = declaration.CarrierAgentDocumentaryAddress;
			oldCarrierAgentDocumentaryAddress.Address1 = "CarrierAgent Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old CarrierAgentDocumentaryAddress deleted", true, oldCarrierAgentDocumentaryAddress.IsDeleted);
				AssertEquals("New CarrierAgentDocumentaryAddress empty", ZString.Empty, declaration.CarrierAgentDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationCarrierAgent_TransportArrangementEmpty()
		{
			dataProviderMock.Setup(m => m.TransportArrangement).Returns(ZString.Empty);
			var oldCarrierAgentDocumentaryAddress = declaration.CarrierAgentDocumentaryAddress;
			oldCarrierAgentDocumentaryAddress.Address1 = "CarrierAgent Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old CarrierAgentDocumentaryAddress not deleted", false, oldCarrierAgentDocumentaryAddress.IsDeleted);
				AssertEquals("Old CarrierAgentDocumentaryAddress not updated", "CarrierAgent Address1", declaration.CarrierAgentDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationTransporter()
		{
			var newTransporterMock = CreatePartyTransporter("VN003", "NewTransporter Party 1", "TP Address 1");
			dataProviderMock.Setup(m => m.NewTransporter).Returns(newTransporterMock);
			var oldTransporterDocumentaryAddress = declaration.TransporterDocumentaryAddress;
			declaration.TransporterDocumentaryAddress.Address1 = "TransporterDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old TransporterDocumentaryAddress deleted", true, oldTransporterDocumentaryAddress.IsDeleted);
				AssertEquals("New TransporterDocumentaryAddress updated", "TP Address 1", declaration.TransporterDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationTransporter_Null()
		{
			var oldTransporterDocumentaryAddress = declaration.TransporterDocumentaryAddress;
			declaration.TransporterDocumentaryAddress.Address1 = "TransporterDocumentaryAddress Address1";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Old TransporterDocumentaryAddress not deleted", false, oldTransporterDocumentaryAddress.IsDeleted);
				AssertEquals("Old TransporterDocumentaryAddress not updated", "TransporterDocumentaryAddress Address1", declaration.TransporterDocumentaryAddress.Address1);
			});
		}

		public void TestUpdateDeclarationTransportDetails()
		{
			declaration.CusContainers.AddNew().ZG_UnitCode = "3";
			ProcessMessage(message);
			AssertContainsExactElementsInAnyOrder("Existing TransportDetails are deleted and new TransportDetails are created from message", new ZString[] { "1", "2" }, declaration.CusContainers.Cast<EMCSCusContainer>().Select(x => x.ZG_UnitCode));
		}

		public void TestUpdateDeclarationTransportDetails_Empty()
		{
			dataProviderMock.Setup(m => m.TransportDetails).Returns(Array.Empty<ED813TransportDetailsProvider>());
			declaration.CusContainers.AddNew();
			ProcessMessage(message);
			AssertEquals("Existing TransportDetails are not deleted when message doesn't have TransportDetails", 1, declaration.CusContainers.Count);
		}

		protected override ZString MessageFriendlyName => "EMCS ED813 Consignee Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED813>> Processor => new ED813ConsigneeMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			eventMock = new Mock<IEMCSEvent>();
			eventMock.Setup(m => m.AdministrativeReferenceCode).Returns("MRN198761234");
			eventMock.Setup(m => m.SequenceNumber).Returns(ExpectedSequenceNumber);

			declaration = Factory.CreateDeclarationWithEadReference(eventMock.Object.AdministrativeReferenceCode, OriginalSequenceNumber, EMCSEntryTypeList.Codes.Consignee);

			dataProviderMock = new Mock<IED813>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Emb);
			dataProviderMock.Setup(m => m.UpdateEadEsad).Returns(eventMock.Object);
			dataProviderMock.Setup(m => m.NewDestinationCode).Returns(OriginalSequenceNumber);
			dataProviderMock.Setup(m => m.JourneyTime).Returns("2D");
			dataProviderMock.Setup(m => m.TransportModeCode).Returns("9");
			dataProviderMock.Setup(m => m.TransportArrangement).Returns(EMCSTransportArrangementList.Codes.OwnerOfGoods);
			dataProviderMock.Setup(m => m.InvoiceNumber).Returns("1");
			dataProviderMock.Setup(m => m.InvoiceDate).Returns(new ZDate(2020, 7, 25));
			dataProviderMock.Setup(m => m.DestinationTypeCode).Returns(EMCSDestinationTypeList.Codes.DestinationDirectDelivery);
			dataProviderMock.Setup(m => m.GuarantorTypeCode).Returns("3");
			dataProviderMock.Setup(m => m.ComplementaryInfo).Returns("New instruction");

			var transportDetailsMock1 = CreateTransportDetails("1");
			var transportDetailsMock2 = CreateTransportDetails("2");
			dataProviderMock.Setup(m => m.TransportDetails).Returns(new[] { transportDetailsMock1, transportDetailsMock2 });

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED813>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}

		IEMCSPartyTransporter CreatePartyTransporter(string vatNumber, string name, string address)
		{
			var partyTransporterMock = new Mock<IEMCSPartyTransporter>();
			partyTransporterMock.Setup(m => m.VatNumber).Returns(vatNumber);
			partyTransporterMock.Setup(m => m.Language).Returns("DE");
			partyTransporterMock.Setup(m => m.Name).Returns(name);
			partyTransporterMock.Setup(m => m.Address).Returns(address);
			partyTransporterMock.Setup(m => m.City).Returns("Berlin");
			partyTransporterMock.Setup(m => m.Postcode).Returns("0001");
			partyTransporterMock.Setup(m => m.Country).Returns("DE");
			return partyTransporterMock.Object;
		}

		IEMCSTransportDetails CreateTransportDetails(ZString unitCode)
		{
			var transportDetailsMock = new Mock<IEMCSTransportDetails>();
			transportDetailsMock.Setup(m => m.UnitCode).Returns(unitCode);
			transportDetailsMock.Setup(m => m.IdentityOfUnit).Returns("CO000" + unitCode);
			transportDetailsMock.Setup(m => m.CommercialSealIdentification).Returns("SEAL" + unitCode);
			transportDetailsMock.Setup(m => m.ComplementaryInformation).Returns("CI00" + unitCode);
			transportDetailsMock.Setup(m => m.SealInformation).Returns("SI00" + unitCode);
			return transportDetailsMock.Object;
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
