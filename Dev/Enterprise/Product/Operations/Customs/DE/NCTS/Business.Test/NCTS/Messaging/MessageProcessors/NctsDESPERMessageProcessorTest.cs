using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDESPERMessageProcessor))]
	sealed class NctsDESPERMessageProcessorTest : MessageProcessorAbstractTest<NctsDESPERMessageProcessor, AtlasInboundEDIMessage<IDESPER>>
	{
		public void TestLinkedObjectNotFound()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, "NOTORIGINALMSG");

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.Error, message.EM_Status);
				AssertNull(message.EM_LinkedObject);
			});

			dataProviderMock.Verify(m => m.ReferencedMessageIdentifier);
		}

		public void TestGetLinkedObjectFromReference()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals(nctsHeader, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IDESPER)null);

			AssertNoExceptionThrown(() => ProcessMessage(message));

			messageMock.Verify(m => m.DataProvider);
		}

		public void TestStatus()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("ArrivalMovementHeader.BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("ArrivalMovementHeader.BM_Phase", NctsMovementHeaderTransactionStatusList.Codes.Arrival, nctsHeader.ArrivalMovementHeader.BM_Phase);
				AssertEquals("EffectiveMessageStatus", "ACC", nctsHeader.EffectiveMessageStatus);
			});
		}

		public void TestCustomsEntryStatusEvent()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			Factory.Save();

			var stmLog = nctsHeader.ArrivalMovementHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.CustomsEntryStatus).Single();
			CombineAssertions(() =>
			{
				AssertEquals("SL_Reference", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, stmLog.SL_Reference);
			});
		}

		public void TestArrivalMovementHeader()
		{
			dataProviderMock.SetupGet(m => m.InlandModeOfTransport).Returns(ModeOfTransportList.Codes._3_RoadTransport);
			dataProviderMock.SetupGet(m => m.GrossMass).Returns(3m);
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("InlandTransportMode", ModeOfTransportList.Codes._3_RoadTransport, nctsHeader.ArrivalMovementHeader.BM_InlandTransportMode);
				AssertEquals("TotalGrossMassInKilograms", 3m, nctsHeader.ArrivalMovementHeader.TotalGrossMassInKilograms);
			});
		}

		public void TestArrivalMovementHeaderEntryDate()
		{
			dataProviderMock.SetupGet(m => m.DeclarationAcceptanceDate).Returns(new DateTime(2024, 01, 09));
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("BM_EntryDate", new ZDateTime(2024, 01, 09), nctsHeader.ArrivalMovementHeader.BM_EntryDate);
			});
		}

		public void TestMessageIsSetToMessagesTab()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			Assert("Message found", nctsHeader.Messages.Cast<EDIMessage>().Any(m => m.EM_MessageNum == ReferencedMessageIdentifier));
		}

		public void TestMailIsSentToUser()
		{
			var outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var title = "NCTS Arrival Status Update. Response for NCT00000001";
			var subject = $"{title} Ref.: {ReferenceNumber}";
			var bodyMessageTitle = $"<title>{title}</title>";

			var bodyMessageHeader = $@"<strong>NCTS Arrival Status Update. Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=NctsMovementController&BusinessEntityPK={nctsHeader.PK}";
			var bodyMessageSummary = $"Your NCTS Arrival Declaration for Job NCT00000001 has received the Unloading Permission. For details please follow the Link to the Job.";

			var table = new HtmlTableCreator();
			table.WriteRow("MRN", MovementReferenceNumber);
			table.WriteRow("Status Update", "Unloading Permission received");

			AssertEmailForSingleRecipientWithTable("Email Sent", email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, table.ToHtml());
		}

		public void TestArrivalHeaderContainers()
		{
			var transportEquipment1 = Mock.Of<IDESPERTransportEquipment>(m => m.SequenceNumber == 1 && m.ContainerIdentificationNumber == "1");
			var transportEquipment2 = Mock.Of<IDESPERTransportEquipment>(m => m.SequenceNumber == 2 && m.ContainerIdentificationNumber == "2");
			dataProviderMock.SetupGet(m => m.TransportEquipments).Returns(new[] { transportEquipment1, transportEquipment2 });

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Count", 2, nctsHeader.ArrivalHeaderContainers.Count);
			CombineAssertions(() =>
			{
				var arrivalHeaderContainer1 = nctsHeader.ArrivalHeaderContainers[0];
				AssertEquals("UnloadedState 1", NctsUnloadedStateList.Codes.DEC, arrivalHeaderContainer1.BC_UnloadedState);
				AssertEquals("SequenceNumber 1", (ZShort)1, arrivalHeaderContainer1.BC_SequenceNumber);
				AssertEquals("ContainerNum 1", "1", arrivalHeaderContainer1.BC_ContainerNum);

				var arrivalHeaderContainer2 = nctsHeader.ArrivalHeaderContainers[1];
				AssertEquals("UnloadedState 2", NctsUnloadedStateList.Codes.DEC, arrivalHeaderContainer2.BC_UnloadedState);
				AssertEquals("SequenceNumber 2", (ZShort)2, arrivalHeaderContainer2.BC_SequenceNumber);
				AssertEquals("ContainerNum 2", "2", arrivalHeaderContainer2.BC_ContainerNum);
			});
		}

		public void TestArrivalHeaderContainers_ShouldHaveContainerisedMode_WhenContainerIndicatorEquals1()
		{
			dataProviderMock.SetupGet(m => m.ContainerIndicator).Returns(1);

			var transportEquipment = Mock.Of<IDESPERTransportEquipment>(m => m.Seals == Array.Empty<IDESPERTransportEquipmentSeal>());
			dataProviderMock.SetupGet(m => m.TransportEquipments).Returns(new[] { transportEquipment });

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var arrivalHeaderContainer = nctsHeader.ArrivalHeaderContainers[0];
			AssertEquals("Mode", Core.Constants.ContainerModes.Containerised, arrivalHeaderContainer.BC_Mode);
		}

		public void TestArrivalHeaderContainers_ShouldHaveNonContainerisedMode_WhenContainerIndicatorNotEquals1()
		{
			dataProviderMock.SetupGet(m => m.ContainerIndicator).Returns(0);

			var transportEquipment = Mock.Of<IDESPERTransportEquipment>(m => m.Seals == Array.Empty<IDESPERTransportEquipmentSeal>());
			dataProviderMock.SetupGet(m => m.TransportEquipments).Returns(new[] { transportEquipment });

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var arrivalHeaderContainer = nctsHeader.ArrivalHeaderContainers[0];
			AssertEquals("Mode", Core.Constants.ContainerModes.NonContainerised, arrivalHeaderContainer.BC_Mode);
		}

		public void TestSeals()
		{
			var seal1 = Mock.Of<IDESPERTransportEquipmentSeal>(m => m.SequenceNumber == 1 && m.Identifier == "1");
			var transportEquipment1 = Mock.Of<IDESPERTransportEquipment>(m => m.Seals == new[] { seal1 });
			var seal2 = Mock.Of<IDESPERTransportEquipmentSeal>(m => m.SequenceNumber == 2 && m.Identifier == "2");
			var seal3 = Mock.Of<IDESPERTransportEquipmentSeal>(m => m.SequenceNumber == 3 && m.Identifier == "3");
			var transportEquipment2 = Mock.Of<IDESPERTransportEquipment>(m => m.Seals == new[] { seal2, seal3 });
			dataProviderMock.SetupGet(m => m.TransportEquipments).Returns(new[] { transportEquipment1, transportEquipment2 });

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition - ArrivalHeaderContainers Count", 2, nctsHeader.ArrivalHeaderContainers.Count);
			CombineAssertions(() =>
			{
				var arrivalHeaderContainer1 = nctsHeader.ArrivalHeaderContainers[0];
				AssertEquals("ArrivalHeaderContainer1 Seals Count", 1, arrivalHeaderContainer1.Seals.Count);
				var additionalSeal1 = arrivalHeaderContainer1.Seals[0];
				AssertEquals("UnloadingState 1", NctsUnloadedStateList.Codes.DEC, additionalSeal1.BK_UnloadingState);
				AssertEquals("SequenceNumber 1", (ZShort)1, additionalSeal1.BK_SequenceNumber);
				AssertEquals("SealNumber 1", "1", additionalSeal1.BK_SealNumber);

				var arrivalHeaderContainer2 = nctsHeader.ArrivalHeaderContainers[1];
				AssertEquals("ArrivalHeaderContainer2 Seals Count", 2, arrivalHeaderContainer2.Seals.Count);
				var additionalSeal2 = arrivalHeaderContainer2.Seals[0];
				AssertEquals("UnloadingState 2", NctsUnloadedStateList.Codes.DEC, additionalSeal2.BK_UnloadingState);
				AssertEquals("SequenceNumber 2", (ZShort)2, additionalSeal2.BK_SequenceNumber);
				AssertEquals("SealNumber 2", "2", additionalSeal2.BK_SealNumber);
				var additionalSeal3 = arrivalHeaderContainer2.Seals[1];
				AssertEquals("UnloadingState 3", NctsUnloadedStateList.Codes.DEC, additionalSeal3.BK_UnloadingState);
				AssertEquals("SequenceNumber 3", (ZShort)3, additionalSeal3.BK_SequenceNumber);
				AssertEquals("SealNumber 23", "3", additionalSeal3.BK_SealNumber);
			});
		}

		public void TestGoodsReference()
		{
			const int goodsReference1 = 1;
			const int goodsReference2 = 2;

			var transportEquipment1 = Mock.Of<IDESPERTransportEquipment>(m => m.SequenceNumber == 1 && m.GoodsReferences == new[] { goodsReference1 });
			var transportEquipment2 = Mock.Of<IDESPERTransportEquipment>(m => m.SequenceNumber == 2 && m.GoodsReferences == new[] { goodsReference1, goodsReference2 });
			dataProviderMock.SetupGet(m => m.TransportEquipments).Returns(new[] { transportEquipment1, transportEquipment2 });

			var itemPackage1 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 1 && m.Kind == "T1" && m.Quantity == 123 && m.MarksNumber == "No123");
			var itemPackage2 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 2 && m.Kind == "T2" && m.Quantity == 456 && m.MarksNumber == "No456");
			var itemPackage3 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 3 && m.Kind == "T3" && m.Quantity == 789 && m.MarksNumber == "No789");

			var consignmentItem1 = Mock.Of<IDESPERConsignmentItem>(m =>
				m.SequenceNumber == 1 && m.DeclarationGoodsItemNumber == goodsReference1 &&
				m.Packages == new[] { itemPackage1 });
			var consignmentItem2 = Mock.Of<IDESPERConsignmentItem>(m =>
				m.SequenceNumber == 2 && m.DeclarationGoodsItemNumber == goodsReference2 &&
				m.Packages == new[] { itemPackage2, itemPackage3 });

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.SequenceNumber == 1 && m.ConsignmentItems == new[] { consignmentItem1, consignmentItem2 })
			});

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var bill = nctsHeader.Bills.Single();
			AssertEquals("Precondition GoodsItems Count", 2, bill.ArrivalGoodsItems.Count);
			var package1 = bill.ArrivalGoodsItems[0].Packages.Cast<NctsPackage>().Single();
			AssertEquals("Precondition GoodsItem2 Packages Count", 2, bill.ArrivalGoodsItems[1].Packages.Count);
			var package2 = bill.ArrivalGoodsItems[1].Packages[0];
			var package3 = bill.ArrivalGoodsItems[1].Packages[1];
			var package1Containers = package1.ContainersPivot.Containers.OrderBy(x => x.BC_SequenceNumber).ToArray();
			AssertEquals("Precondition ContainersPivots Count", 2, package1Containers.Length);
			CombineAssertions(() =>
			{
				AssertEquals("Precondition Package 1, Container 1: SequenceNumber", (short)1, package1Containers[0].BC_SequenceNumber);
				AssertEquals("Precondition Package 1, Container 2: SequenceNumber", (short)2, package1Containers[1].BC_SequenceNumber);

				var package2Containers = package2.ContainersPivot.Containers.OrderBy(x => x.BC_SequenceNumber).ToArray();
				AssertEquals("Precondition Package 2, Container 2: SequenceNumber", (short)2, package2Containers.Single().BC_SequenceNumber);

				var package3Containers = package3.ContainersPivot.Containers.OrderBy(x => x.BC_SequenceNumber).ToArray();
				AssertEquals("Precondition Package 3, Container 2: SequenceNumber", (short)2, package3Containers.Single().BC_SequenceNumber);
			});
		}

		public void TestArrivalTransportInfos()
		{
			var departureTransportMean1 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 1 && m.TypeOfIdentification == "1" && m.IdentificationNumber == "1" && m.Nationality == "DE");
			var departureTransportMean2 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 2 && m.TypeOfIdentification == "2" && m.IdentificationNumber == "2" && m.Nationality == "CH");
			dataProviderMock.SetupGet(m => m.DepartureTransportMeans).Returns(new[] { departureTransportMean1, departureTransportMean2 });

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Count", 2, nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.Count);
			CombineAssertions(() =>
			{
				var arrivalTransportInfo1 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos[0];
				AssertEquals("TransportState 1", NctsUnloadedStateList.Codes.DEC, arrivalTransportInfo1.TPM_TransportState);
				AssertEquals("SequenceNumber 1", (ZShort)1, arrivalTransportInfo1.TPM_SequenceNumber);
				AssertEquals("TypeOfIdentification 1", "1", arrivalTransportInfo1.TPM_TypeOfIdentification);
				AssertEquals("IdentificationNumber 1", "1", arrivalTransportInfo1.TPM_IdentificationNumber);
				AssertEquals("TransportNationality 1", "DE", arrivalTransportInfo1.TPM_RN_NKTransportNationality);

				var arrivalTransportInfo2 = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos[1];
				AssertEquals("TransportState 2", NctsUnloadedStateList.Codes.DEC, arrivalTransportInfo2.TPM_TransportState);
				AssertEquals("SequenceNumber 2", (ZShort)2, arrivalTransportInfo2.TPM_SequenceNumber);
				AssertEquals("TypeOfIdentification 2", "2", arrivalTransportInfo2.TPM_TypeOfIdentification);
				AssertEquals("IdentificationNumber 2", "2", arrivalTransportInfo2.TPM_IdentificationNumber);
				AssertEquals("TransportNationality 2", "CH", arrivalTransportInfo2.TPM_RN_NKTransportNationality);
			});
		}

		public void TestPreviousDocuments()
		{
			dataProviderMock.SetupGet(m => m.PreviousDocuments).Returns(new[]
			{
				CreateNCTSDocumentMock("Type1", "123",  complementOfInformation: "456"),
				CreateNCTSDocumentMock("Type2", "321", complementOfInformation: "654")
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Count", 2, nctsHeader.PreviousDocuments.Count);
			CombineAssertions(() =>
			{
				var previousDocument1 = nctsHeader.PreviousDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, previousDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", previousDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", previousDocument1.CSI_ReferenceNumber);
				AssertEquals("ReferenceNumber2 1", "456", previousDocument1.CSI_ReferenceNumber2);

				var previousDocument2 = nctsHeader.PreviousDocuments[1];
				AssertEquals("Status 2", NctsUnloadedStateList.Codes.DEC, previousDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", previousDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", previousDocument2.CSI_ReferenceNumber);
				AssertEquals("ReferenceNumber2 2", "654", previousDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestMovementHeader_SupportingDocuments()
		{
			dataProviderMock.SetupGet(m => m.SupportingDocuments).Returns(new[]
			{
				CreateNCTSDocumentMock("Type1", "123", 1, "456"),
				CreateNCTSDocumentMock("Type2", "321", 2, "654")
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var supportingDocuments = nctsHeader.ArrivalMovementHeader.SupportingDocuments;
			AssertEquals("Count", 2, supportingDocuments.Count);
			CombineAssertions(() =>
			{
				var supportingDocument1 = supportingDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("LineNo 1", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("ReferenceNumber2 1", "456", supportingDocument1.CSI_ReferenceNumber2);

				var supportingDocument2 = supportingDocuments[1];
				AssertEquals("Status 2", NctsUnloadedStateList.Codes.DEC, supportingDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", supportingDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("LineNo 2", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("ReferenceNumber2 2", "654", supportingDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestMovementHeader_SupportingDocuments_DocumentLineItemNumberNull()
		{
			dataProviderMock.SetupGet(m => m.SupportingDocuments).Returns(new[]
			{
				CreateNCTSDocumentMock("Type1", "123", null, "456"),
				CreateNCTSDocumentMock("Type2", "321", null, "654")
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);

			var supportingDocuments = nctsHeader.ArrivalMovementHeader.SupportingDocuments;
			AssertEquals("Count", 2, supportingDocuments.Count);
			CombineAssertions(() =>
			{
				var supportingDocument1 = supportingDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("LineNo 1", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("ReferenceNumber2 1", "456", supportingDocument1.CSI_ReferenceNumber2);

				var supportingDocument2 = supportingDocuments[1];
				AssertEquals("Status 2", NctsUnloadedStateList.Codes.DEC, supportingDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", supportingDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("LineNo 2", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("ReferenceNumber2 2", "654", supportingDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestMovementHeader_TransportDocuments()
		{
			var referenceNumber70 = new string('1', 70);
			dataProviderMock.SetupGet(m => m.TransportDocuments).Returns(new[]
			{
				CreateNCTSDocumentMock("Type1", referenceNumber70),
				CreateNCTSDocumentMock("Type2", "321")
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var transportDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToArray();
			AssertEquals("Count", 2, transportDocuments.Length);
			CombineAssertions(() =>
			{
				var transportDocument1 = transportDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, transportDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", transportDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", referenceNumber70, transportDocument1.CSI_ReferenceNumber);

				var transportDocument2 = transportDocuments[1];
				AssertEquals("Status 2", NctsUnloadedStateList.Codes.DEC, transportDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", transportDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", transportDocument2.CSI_ReferenceNumber);
			});
		}

		public void TestMovementHeader_AdditionalReferences()
		{
			dataProviderMock.SetupGet(m => m.AdditionalReferences).Returns(new[]
			{
				CreateNCTSDocumentMock("Type1", "123"),
				CreateNCTSDocumentMock("Type2", "321")
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var additionalReferences = nctsHeader.ArrivalMovementHeader.AdditionalDocuments
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToArray();
			AssertEquals("Count", 2, additionalReferences.Length);
			CombineAssertions(() =>
			{
				var additionalReference1 = additionalReferences[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalReference1.CSI_Status);
				AssertEquals("Code 1", "Type1", additionalReference1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", additionalReference1.CSI_ReferenceNumber);

				var additionalReference2 = additionalReferences[1];
				AssertEquals("Status 2", NctsUnloadedStateList.Codes.DEC, additionalReference2.CSI_Status);
				AssertEquals("Code 2", "Type2", additionalReference2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", additionalReference2.CSI_ReferenceNumber);
			});
		}

		public void TestMovementHeader_AdditionalInformation()
		{
			dataProviderMock.SetupGet(m => m.AdditionalInformations).Returns(new[]
			{
				Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type1" && m.Text == "123"),
				Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type2" && m.Text == "321")
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			var additionalInformation = nctsHeader.ArrivalMovementHeader.AdditionalDocuments
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
			AssertEquals("Count", 2, additionalInformation.Length);
			CombineAssertions(() =>
			{
				var additionalInformation1 = additionalInformation[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalInformation1.CSI_Status);
				AssertEquals("Code 1", "Type1", additionalInformation1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", additionalInformation1.CSI_ReferenceNumber);

				var additionalInformation2 = additionalInformation[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalInformation2.CSI_Status);
				AssertEquals("Code 2", "Type2", additionalInformation2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", additionalInformation2.CSI_ReferenceNumber);
			});
		}

		public void TestHouseConsignments()
		{
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.SequenceNumber == 1 && m.GrossMass == 1),
				Mock.Of<IDESPERHouseConsignment>(m => m.SequenceNumber == 2 && m.GrossMass == 2)
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Count", 2, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			CombineAssertions(() =>
			{
				var movementDetails1 = nctsHeader.ArrivalMovementHeader.MovementDetails[0];
				AssertEquals("UnloadedState 1", NctsUnloadedStateListForHouseConsignment.Codes.DEC, movementDetails1.B9_UnloadedState);
				AssertEquals("SeqNo 1", "1", movementDetails1.B9_SeqNo);
				AssertNotNull("Bill 1", movementDetails1.Bill);
				AssertEquals("Weight 1", 1m, movementDetails1.Bill.B0_Weight);

				var movementDetails2 = nctsHeader.ArrivalMovementHeader.MovementDetails[1];
				AssertEquals("UnloadedState 2", NctsUnloadedStateListForHouseConsignment.Codes.DEC, movementDetails2.B9_UnloadedState);
				AssertEquals("SeqNo 2", "2", movementDetails2.B9_SeqNo);
				AssertNotNull("Bill 2", movementDetails1.Bill);
				AssertEquals("Weight 2", 2m, movementDetails2.Bill.B0_Weight);
			});
		}

		public void TestMovementDetails_Bill_ArrivalTransportInfos()
		{
			var departureTransportMean1 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 1 && m.TypeOfIdentification == "1" && m.IdentificationNumber == "1" && m.Nationality == "DE");
			var departureTransportMean2 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 2 && m.TypeOfIdentification == "2" && m.IdentificationNumber == "2" && m.Nationality == "CH");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.DepartureTransportMeans == new[] { departureTransportMean1, departureTransportMean2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("Count", 2, bill.ArrivalTransportInfos.Count);
			CombineAssertions(() =>
			{
				var arrivalTransportInfo1 = bill.ArrivalTransportInfos[0];
				AssertEquals("TransportState 1", NctsUnloadedStateList.Codes.DEC, arrivalTransportInfo1.TPM_TransportState);
				AssertEquals("SequenceNumber 1", (ZShort)1, arrivalTransportInfo1.TPM_SequenceNumber);
				AssertEquals("TypeOfIdentification 1", "1", arrivalTransportInfo1.TPM_TypeOfIdentification);
				AssertEquals("IdentificationNumber 1", "1", arrivalTransportInfo1.TPM_IdentificationNumber);
				AssertEquals("TransportNationality 1", "DE", arrivalTransportInfo1.TPM_RN_NKTransportNationality);

				var arrivalTransportInfo2 = bill.ArrivalTransportInfos[1];
				AssertEquals("TransportState 2", NctsUnloadedStateList.Codes.DEC, arrivalTransportInfo2.TPM_TransportState);
				AssertEquals("SequenceNumber 2", (ZShort)2, arrivalTransportInfo2.TPM_SequenceNumber);
				AssertEquals("TypeOfIdentification 2", "2", arrivalTransportInfo2.TPM_TypeOfIdentification);
				AssertEquals("IdentificationNumber 2", "2", arrivalTransportInfo2.TPM_IdentificationNumber);
				AssertEquals("TransportNationality 2", "CH", arrivalTransportInfo2.TPM_RN_NKTransportNationality);
			});
		}

		public void TestMovementDetails_Bill_PreviousDocuments()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123", complementOfInformation: "456");
			var document2 = CreateNCTSDocumentMock("Type2", "321", complementOfInformation: "654");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.PreviousDocuments == new[] { document1, document2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("Count", 2, bill.PreviousDocuments.Count);
			CombineAssertions(() =>
			{
				var previousDocument1 = bill.PreviousDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, previousDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", previousDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", previousDocument1.CSI_ReferenceNumber);
				AssertEquals("ReferenceNumber2 1", "456", previousDocument1.CSI_ReferenceNumber2);

				var previousDocument2 = bill.PreviousDocuments[1];
				AssertEquals("Status 2", NctsUnloadedStateList.Codes.DEC, previousDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", previousDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", previousDocument2.CSI_ReferenceNumber);
				AssertEquals("ReferenceNumber2 2", "654", previousDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestMovementDetails_Bill_SupportingDocuments()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123", 1, "456");
			var document2 = CreateNCTSDocumentMock("Type2", "321", 2, "654");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.SupportingDocuments == new[] { document1, document2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("Count", 2, bill.SupportingDocuments.Count);
			CombineAssertions(() =>
			{
				var supportingDocument1 = bill.SupportingDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("LineNo 1", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("ReferenceNumber2 1", "456", supportingDocument1.CSI_ReferenceNumber2);

				var supportingDocument2 = bill.SupportingDocuments[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", supportingDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("LineNo 2", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("ReferenceNumber2 2", "654", supportingDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestMovementDetails_Bill_SupportingDocuments_DocumentLineItemNumberNull()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123", null, "456");
			var document2 = CreateNCTSDocumentMock("Type2", "321", null, "654");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.SupportingDocuments == new[] { document1, document2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			var bill = nctsHeader.ArrivalMovementHeader.MovementDetails[0].Bill;
			AssertEquals("Count", 2, bill.SupportingDocuments.Count);
			CombineAssertions(() =>
			{
				var supportingDocument1 = bill.SupportingDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("LineNo 1", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("ReferenceNumber2 1", "456", supportingDocument1.CSI_ReferenceNumber2);

				var supportingDocument2 = bill.SupportingDocuments[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", supportingDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("LineNo 2", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("ReferenceNumber2 2", "654", supportingDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestMovementDetails_Bill_TransportDocuments()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123");
			var document2 = CreateNCTSDocumentMock("Type2", "321");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.TransportDocuments == new[] { document1, document2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			var transportDocuments = bill.AdditionalDocuments
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToArray();
			AssertEquals("Count", 2, transportDocuments.Length);
			CombineAssertions(() =>
			{
				var transportDocument1 = transportDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, transportDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", transportDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", transportDocument1.CSI_ReferenceNumber);

				var transportDocument2 = transportDocuments[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, transportDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", transportDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", transportDocument2.CSI_ReferenceNumber);
			});
		}

		public void TestMovementDetails_Bill_AdditionalReferences()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123");
			var document2 = CreateNCTSDocumentMock("Type2", "321");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.AdditionalReferences == new[] { document1, document2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			var additionalReferences = bill.AdditionalDocuments
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToArray();
			AssertEquals("Count", 2, additionalReferences.Length);
			CombineAssertions(() =>
			{
				var additionalReference1 = additionalReferences[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalReference1.CSI_Status);
				AssertEquals("Code 1", "Type1", additionalReference1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", additionalReference1.CSI_ReferenceNumber);

				var additionalReference2 = additionalReferences[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalReference2.CSI_Status);
				AssertEquals("Code 2", "Type2", additionalReference2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", additionalReference2.CSI_ReferenceNumber);
			});
		}

		public void TestMovementDetails_Bill_AdditionalInformation()
		{
			var additionalInformationMock1 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type1" && m.Text == "123");
			var additionalInformationMock2 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type2" && m.Text == "321");
			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.AdditionalInformations == new[] { additionalInformationMock1, additionalInformationMock2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			var additionalInformation = bill.AdditionalDocuments
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
			AssertEquals("Count", 2, additionalInformation.Length);
			CombineAssertions(() =>
			{
				var additionalInformation1 = additionalInformation[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalInformation1.CSI_Status);
				AssertEquals("Code 1", "Type1", additionalInformation1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", additionalInformation1.CSI_ReferenceNumber);

				var additionalInformation2 = additionalInformation[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalInformation2.CSI_Status);
				AssertEquals("Code 2", "Type2", additionalInformation2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", additionalInformation2.CSI_ReferenceNumber);
			});
		}

		public void TestHouseConsignment_Items()
		{
			var consignmentItem1 = Mock.Of<IDESPERConsignmentItem>(m =>
				m.SequenceNumber == 1 && m.DeclarationGoodsItemNumber == 1 && m.DescriptionOfGoods == "123" && m.CusCode == "456" && m.HarmonizedSystemSubHeadingCode == "111000" &&
				m.CombinedNomenclatureCode == "22" && m.GrossMass == 11.11m && m.NetMass == 10.01m);
			var consignmentItem2 = Mock.Of<IDESPERConsignmentItem>(m =>
				m.SequenceNumber == 2 && m.DeclarationGoodsItemNumber == 2 && m.DescriptionOfGoods == "321" && m.CusCode == "654" && m.HarmonizedSystemSubHeadingCode == "222000" &&
				m.CombinedNomenclatureCode == "11" && m.GrossMass == 22.22m && m.NetMass == 20.02m);

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m => m.ConsignmentItems == new[] { consignmentItem1, consignmentItem2 })
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("Count", 2, bill.ArrivalGoodsItems.Count);
			CombineAssertions(() =>
			{
				var goodsItem1 = bill.ArrivalGoodsItems[0];
				AssertEquals("UnloadedState 1", NctsUnloadedStateList.Codes.DEC, goodsItem1.BY_UnloadedState);
				AssertEquals("LineNo 1", (short)1, goodsItem1.BY_LineNo);
				AssertEquals("DeclarationGoodsItemNumber 1", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
				AssertEquals("Description 1", "123", goodsItem1.BY_Description);
				AssertEquals("CusC4Number 1", "456", goodsItem1.BY_CusC4Number);
				AssertEquals("FormattedHarmonisedTariff 1", "11100022", goodsItem1.BY_HarmonisedTariff);
				AssertEquals("GrossWeight 1", 11.11m, goodsItem1.BY_GrossWeight);
				AssertEquals("NetWeight 1", 10.01m, goodsItem1.BY_NetWeight);

				var goodsItem2 = bill.ArrivalGoodsItems[1];
				AssertEquals("UnloadedState 2", NctsUnloadedStateList.Codes.DEC, goodsItem2.BY_UnloadedState);
				AssertEquals("LineNo 2", (short)2, goodsItem2.BY_LineNo);
				AssertEquals("DeclarationGoodsItemNumber 2", 2, goodsItem2.BY_DeclarationGoodsItemNumber);
				AssertEquals("Description 2", "321", goodsItem2.BY_Description);
				AssertEquals("CusC4Number 2", "654", goodsItem2.BY_CusC4Number);
				AssertEquals("FormattedHarmonisedTariff 2", "22200011", goodsItem2.BY_HarmonisedTariff);
				AssertEquals("GrossWeight 2", 22.22m, goodsItem2.BY_GrossWeight);
				AssertEquals("NetWeight 2", 20.02m, goodsItem2.BY_NetWeight);
			});
		}

		public void TestHouseConsignment_Items_Packages()
		{
			var itemPackage1 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 1 && m.Kind == "1" && m.Quantity == 123 && m.MarksNumber == "123");
			var itemPackage2 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 2 && m.Kind == "2" && m.Quantity == 456 && m.MarksNumber == "456");

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(mc => mc.ConsignmentItems == new[]
				{
					Mock.Of<IDESPERConsignmentItem>(mci => mci.Packages == new [] { itemPackage1, itemPackage2 })
				})
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("ArrivalGoodsItems Count", 1, bill.ArrivalGoodsItems.Count);
			var goodsItem = bill.ArrivalGoodsItems[0];

			AssertEquals("Count", 2, goodsItem.Packages.Count);
			CombineAssertions(() =>
			{
				var package1 = goodsItem.Packages[0];
				AssertEquals("TypeOfDifference 1", NctsUnloadedStateList.Codes.DEC, package1.B5_TypeOfDifference);
				AssertEquals("SequenceNumber 1", (short)1, package1.B5_SequenceNumber);
				AssertEquals("UnitType 1", "1", package1.B5_UnitType);
				AssertEquals("UnitCount 1", 123, package1.B5_UnitCount);
				AssertEquals("MarksAndNumbers 1", "123", package1.B5_MarksAndNumbers);

				var package2 = goodsItem.Packages[1];
				AssertEquals("TypeOfDifference 2", NctsUnloadedStateList.Codes.DEC, package2.B5_TypeOfDifference);
				AssertEquals("SequenceNumber 2", (short)2, package2.B5_SequenceNumber);
				AssertEquals("UnitType 2", "2", package2.B5_UnitType);
				AssertEquals("UnitCount 2", 456, package2.B5_UnitCount);
				AssertEquals("MarksAndNumbers 2", "456", package2.B5_MarksAndNumbers);
			});
		}

		public void TestHouseConsignment_Items_SupportingDocuments()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123", 1, "456");
			var document2 = CreateNCTSDocumentMock("Type2", "321", 2, "654");

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(mc => mc.ConsignmentItems == new[]
				{
					Mock.Of<IDESPERConsignmentItem>(mci => mci.SupportingDocuments == new [] { document1, document2 })
				})
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("ArrivalGoodsItems Count", 1, bill.ArrivalGoodsItems.Count);
			var goodsItem = bill.ArrivalGoodsItems[0];

			AssertEquals("Count", 2, goodsItem.SupportingDocuments.Count);
			CombineAssertions(() =>
			{
				var supportingDocument1 = goodsItem.SupportingDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("LineNo 1", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("ReferenceNumber2 1", "456", supportingDocument1.CSI_ReferenceNumber2);

				var supportingDocument2 = goodsItem.SupportingDocuments[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", supportingDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("LineNo 2", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("ReferenceNumber2 2", "654", supportingDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestHouseConsignment_Items_SupportingDocuments_DocumentLineItemNumberNull()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123", null, "456");
			var document2 = CreateNCTSDocumentMock("Type2", "321", null, "654");

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(mc => mc.ConsignmentItems == new[]
				{
					Mock.Of<IDESPERConsignmentItem>(mci => mci.SupportingDocuments == new [] { document1, document2 })
				})
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			var goodsItem = nctsHeader.ArrivalMovementHeader.MovementDetails[0].Bill.ArrivalGoodsItems[0];
			AssertEquals("Count", 2, goodsItem.SupportingDocuments.Count);
			CombineAssertions(() =>
			{
				var supportingDocument1 = goodsItem.SupportingDocuments[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("Code 1", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("LineNo 1", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("ReferenceNumber2 1", "456", supportingDocument1.CSI_ReferenceNumber2);

				var supportingDocument2 = goodsItem.SupportingDocuments[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, supportingDocument2.CSI_Status);
				AssertEquals("Code 2", "Type2", supportingDocument2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("LineNo 2", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("ReferenceNumber2 2", "654", supportingDocument2.CSI_ReferenceNumber2);
			});
		}

		public void TestHouseConsignment_Items_AdditionalReferences()
		{
			var document1 = CreateNCTSDocumentMock("Type1", "123");
			var document2 = CreateNCTSDocumentMock("Type2", "321");

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(mc => mc.ConsignmentItems == new[]
				{
					Mock.Of<IDESPERConsignmentItem>(mci => mci.AdditionalReferences == new [] { document1, document2 })
				})
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("ArrivalGoodsItems Count", 1, bill.ArrivalGoodsItems.Count);
			var goodsItem = bill.ArrivalGoodsItems[0];

			var additionalReferences = goodsItem.AdditionalInfos
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToArray();
			AssertEquals("Count", 2, additionalReferences.Length);
			CombineAssertions(() =>
			{
				var additionalReference1 = additionalReferences[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalReference1.CSI_Status);
				AssertEquals("Code 1", "Type1", additionalReference1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", additionalReference1.CSI_ReferenceNumber);

				var additionalReference2 = additionalReferences[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalReference2.CSI_Status);
				AssertEquals("Code 2", "Type2", additionalReference2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", additionalReference2.CSI_ReferenceNumber);
			});
		}

		public void TestHouseConsignment_Items_AdditionalInformation()
		{
			var additionalInformationMock1 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type1" && m.Text == "123");
			var additionalInformationMock2 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type2" && m.Text == "321");

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(mc => mc.ConsignmentItems == new[]
				{
					Mock.Of<IDESPERConsignmentItem>(mci => mci.AdditionalInformations == new [] { additionalInformationMock1, additionalInformationMock2 })
				})
			});
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);

			AssertEquals("Precondition MovementDetails", 1, nctsHeader.ArrivalMovementHeader.MovementDetails.Count);
			var movementDetails = nctsHeader.ArrivalMovementHeader.MovementDetails[0];

			AssertNotNull("Precondition Bill", movementDetails.Bill);
			var bill = movementDetails.Bill;

			AssertEquals("ArrivalGoodsItems Count", 1, bill.ArrivalGoodsItems.Count);
			var goodsItem = bill.ArrivalGoodsItems[0];

			var additionalInformation = goodsItem.AdditionalInfos
				.Where(ad => ad.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
			AssertEquals("Count", 2, additionalInformation.Length);
			CombineAssertions(() =>
			{
				var additionalInformation1 = additionalInformation[0];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalInformation1.CSI_Status);
				AssertEquals("Code 1", "Type1", additionalInformation1.CSI_Code);
				AssertEquals("ReferenceNumber 1", "123", additionalInformation1.CSI_ReferenceNumber);

				var additionalInformation2 = additionalInformation[1];
				AssertEquals("Status 1", NctsUnloadedStateList.Codes.DEC, additionalInformation2.CSI_Status);
				AssertEquals("Code 2", "Type2", additionalInformation2.CSI_Code);
				AssertEquals("ReferenceNumber 2", "321", additionalInformation2.CSI_ReferenceNumber);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		public void TestMapHeader()
		{
			const int goodsReference1 = 1;
			const int goodsReference2 = 2;

			var seal1 = Mock.Of<IDESPERTransportEquipmentSeal>(m => m.SequenceNumber == 1 && m.Identifier == "S1");
			var seal2 = Mock.Of<IDESPERTransportEquipmentSeal>(m => m.SequenceNumber == 2 && m.Identifier == "S2");
			var seal3 = Mock.Of<IDESPERTransportEquipmentSeal>(m => m.SequenceNumber == 3 && m.Identifier == "S3");

			var transportEquipment1 = Mock.Of<IDESPERTransportEquipment>(m =>
				m.SequenceNumber == 1 &&
				m.ContainerIdentificationNumber == "CNT1" &&
				m.GoodsReferences == new[] { goodsReference1, goodsReference2 } &&
				m.Seals == new[] { seal1 });
			var transportEquipment2 = Mock.Of<IDESPERTransportEquipment>(m =>
				m.SequenceNumber == 2 &&
				m.ContainerIdentificationNumber == "CNT2" &&
				m.GoodsReferences == new[] { goodsReference2 } &&
				m.Seals == new[] { seal2, seal3 });
			dataProviderMock.SetupGet(m => m.TransportEquipments).Returns(new[] { transportEquipment1, transportEquipment2 });
			dataProviderMock.SetupGet(m => m.ContainerIndicator).Returns(1);
			dataProviderMock.SetupGet(m => m.InlandModeOfTransport).Returns(ModeOfTransportList.Codes._3_RoadTransport);
			dataProviderMock.SetupGet(m => m.GrossMass).Returns(3m);

			var bmDepartureTransportMean1 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 1 && m.TypeOfIdentification == "1" && m.IdentificationNumber == "DTM1" && m.Nationality == "DE");
			var bmDepartureTransportMean2 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 2 && m.TypeOfIdentification == "2" && m.IdentificationNumber == "DTM2" && m.Nationality == "CH");
			dataProviderMock.SetupGet(m => m.DepartureTransportMeans).Returns(new[] { bmDepartureTransportMean1, bmDepartureTransportMean2 });

			var itemPackage1 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 1 && m.Kind == "T1" && m.Quantity == 123 && m.MarksNumber == "No123");
			var itemPackage2 = Mock.Of<IDESPERPackaging>(m => m.SequenceNumber == 2 && m.Kind == "T2" && m.Quantity == 456 && m.MarksNumber == "No456");

			var bySupportingDocument1 = CreateNCTSDocumentMock("Type1", "123", 1, "456");
			var bySupportingDocument2 = CreateNCTSDocumentMock("Type2", "321", 2, "654");

			var byAdditionalReference1 = CreateNCTSDocumentMock("Type1", "1234");
			var byAdditionalReference2 = CreateNCTSDocumentMock("Type2", "4321");

			var byAdditionalInformation1 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type1" && m.Text == "A123");
			var byAdditionalInformation2 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type2" && m.Text == "A321");

			var consignmentItem1 = Mock.Of<IDESPERConsignmentItem>(m =>
				m.SequenceNumber == 1 && m.DeclarationGoodsItemNumber == 1 && m.DescriptionOfGoods == "123" && m.CusCode == "456" && m.HarmonizedSystemSubHeadingCode == "111000" &&
				m.CombinedNomenclatureCode == "22" && m.GrossMass == 11.11m && m.NetMass == 10.01m &&
				m.Packages == new[] { itemPackage1, itemPackage2 } &&
				m.SupportingDocuments == new[] { bySupportingDocument1, bySupportingDocument2 } &&
				m.AdditionalReferences == new[] { byAdditionalReference1, byAdditionalReference2 } &&
				m.AdditionalInformations == new[] { byAdditionalInformation1, byAdditionalInformation2 }
				);
			var consignmentItem2 = Mock.Of<IDESPERConsignmentItem>(m =>
				m.SequenceNumber == 2 && m.DeclarationGoodsItemNumber == 2 && m.DescriptionOfGoods == "321" && m.CusCode == "654" && m.HarmonizedSystemSubHeadingCode == "222000" &&
				m.CombinedNomenclatureCode == "11" && m.GrossMass == 22.22m && m.NetMass == 20.02m);

			var b0PreviousDocument1 = CreateNCTSDocumentMock("Type1", "P123", complementOfInformation: "456");
			var b0PreviousDocument2 = CreateNCTSDocumentMock("Type2", "P321", complementOfInformation: "654");

			var b0SupportingDocument1 = CreateNCTSDocumentMock("Type1", "S123", 1, "456");
			var b0SupportingDocument2 = CreateNCTSDocumentMock("Type2", "S321", 2, "654");

			var b0TransportDocument1 = CreateNCTSDocumentMock("Type1", "T123");
			var b0TransportDocument2 = CreateNCTSDocumentMock("Type2", "T321");

			var b0AdditionalReference1 = CreateNCTSDocumentMock("Type1", "R123");
			var b0AdditionalReference2 = CreateNCTSDocumentMock("Type2", "R321");

			var b0AdditionalInformation1 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type1" && m.Text == "I123");
			var b0AdditionalInformation2 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type2" && m.Text == "I321");

			var b0DepartureTransportMean1 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 1 && m.TypeOfIdentification == "1" && m.IdentificationNumber == "1" && m.Nationality == "DE");
			var b0DepartureTransportMean2 = Mock.Of<INCTSDepartureTransportMeans>(m =>
				m.SequenceNumber == 2 && m.TypeOfIdentification == "2" && m.IdentificationNumber == "2" && m.Nationality == "CH");

			dataProviderMock.SetupGet(m => m.HouseConsignments).Returns(new[]
			{
				Mock.Of<IDESPERHouseConsignment>(m =>
					m.SequenceNumber == 1 &&
					m.GrossMass == 2.5m &&
					m.ConsignmentItems == new[] { consignmentItem1, consignmentItem2 } &&
					m.DepartureTransportMeans == new[] { b0DepartureTransportMean1, b0DepartureTransportMean2 } &&
					m.PreviousDocuments == new[] { b0PreviousDocument1, b0PreviousDocument2 } &&
					m.SupportingDocuments == new[] { b0SupportingDocument1, b0SupportingDocument2 } &&
					m.TransportDocuments == new[] { b0TransportDocument1, b0TransportDocument2 } &&
					m.AdditionalReferences == new[] { b0AdditionalReference1, b0AdditionalReference2 } &&
					m.AdditionalInformations == new[] { b0AdditionalInformation1, b0AdditionalInformation2 }
					),
				Mock.Of<IDESPERHouseConsignment>(m => m.SequenceNumber == 2 && m.GrossMass == 2)
			});

			var bhPreviousDocument1 = CreateNCTSDocumentMock("Type1", "HP123", complementOfInformation: "456");
			var bhPreviousDocument2 = CreateNCTSDocumentMock("Type2", "HP321", complementOfInformation: "654");
			dataProviderMock.SetupGet(m => m.PreviousDocuments).Returns(new[] { bhPreviousDocument1, bhPreviousDocument2 });

			var bhSupportingDocument1 = CreateNCTSDocumentMock("Type1", "HS123", 1, "456");
			var bhSupportingDocument2 = CreateNCTSDocumentMock("Type2", "HS321", 2, "654");
			dataProviderMock.SetupGet(m => m.SupportingDocuments).Returns(new[] { bhSupportingDocument1, bhSupportingDocument2 });

			var bhTransportDocument1 = CreateNCTSDocumentMock("Type1", "HT123");
			var bhTransportDocument2 = CreateNCTSDocumentMock("Type2", "HT321");
			dataProviderMock.SetupGet(m => m.TransportDocuments).Returns(new[] { bhTransportDocument1, bhTransportDocument2 });

			var bhAdditionalReference1 = CreateNCTSDocumentMock("Type1", "HR123");
			var bhAdditionalReference2 = CreateNCTSDocumentMock("Type2", "HR321");
			dataProviderMock.SetupGet(m => m.AdditionalReferences).Returns(new[] { bhAdditionalReference1, bhAdditionalReference2 });

			var bhAdditionalInformation1 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type1" && m.Text == "HI123");
			var bhAdditionalInformation2 = Mock.Of<INCTSAdditionalInformation>(m => m.Code == "Type2" && m.Text == "HI321");
			dataProviderMock.SetupGet(m => m.AdditionalInformations).Returns(new[] { bhAdditionalInformation1, bhAdditionalInformation2 });

			CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			ProcessMessage(message);
			AssertEquals("Message processed 1st time", EDIMessage.Status.ProcessedOK, message.EM_Status);

			message.EM_Status = EDIMessage.Status.Queued;
			ProcessMessage(message);
			AssertEquals("Message processed 2nd time", EDIMessage.Status.ProcessedOK, message.EM_Status);

			CombineAssertions("Header", () =>
			{
				var prevDocs = nctsHeader.PreviousDocuments;
				AssertEquals("PreviousDocuments.Count", 2, prevDocs.Count);
				var prevDoc = prevDocs[0];
				AssertEquals("prevDoc.CSI_Status", NctsUnloadedStateList.Codes.DEC, prevDoc.CSI_Status);
				AssertEquals("prevDoc.CSI_Code", "Type1", prevDoc.CSI_Code);
				AssertEquals("prevDoc.CSI_ReferenceNumber", "HP123", prevDoc.CSI_ReferenceNumber);
				AssertEquals("prevDoc.CSI_ReferenceNumber2", "456", prevDoc.CSI_ReferenceNumber2);
			});

			CombineAssertions("ArrivalHeaderContainers", () =>
			{
				AssertEquals("Count", 2, nctsHeader.ArrivalHeaderContainers.Count);
				var headerContainer1 = nctsHeader.ArrivalHeaderContainers[0];
				AssertEquals("headerContainer1.BC_UnloadedState", NctsUnloadedStateList.Codes.DEC, headerContainer1.BC_UnloadedState);
				AssertEquals("headerContainer1.BC_SequenceNumber", (ZShort)1, headerContainer1.BC_SequenceNumber);
				AssertEquals("headerContainer1.BC_ContainerNum", "CNT1", headerContainer1.BC_ContainerNum);
				AssertEquals("headerContainer1.BC_Mode", "CNT", headerContainer1.BC_Mode);
				var s1 = (CusSeal)headerContainer1.Seals.Single();
				AssertEquals("s1.BK_UnloadingState", NctsUnloadedStateList.Codes.DEC, s1.BK_UnloadingState);
				AssertEquals("s1.BK_SequenceNumber", (ZShort)1, s1.BK_SequenceNumber);
				AssertEquals("s1.BK_SealNumber", "S1", s1.BK_SealNumber);
			});

			CombineAssertions("ArrivalMovementHeader", () =>
			{
				var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;

				AssertEquals("BM_InlandTransportMode", ModeOfTransportList.Codes._3_RoadTransport, arrivalMovementHeader.BM_InlandTransportMode);
				AssertEquals("BM_GrossWeight", 3m, arrivalMovementHeader.BM_GrossWeight);

				AssertEquals("ArrivalTransportInfos.Count", 2, arrivalMovementHeader.ArrivalTransportInfos.Count);
				var arrivalTransportInfo1 = arrivalMovementHeader.ArrivalTransportInfos[0];
				AssertEquals("arrivalTransportInfo1.TPM_TransportState", NctsUnloadedStateList.Codes.DEC, arrivalTransportInfo1.TPM_TransportState);
				AssertEquals("arrivalTransportInfo1.TPM_SequenceNumber", (ZShort)1, arrivalTransportInfo1.TPM_SequenceNumber);
				AssertEquals("arrivalTransportInfo1.TPM_TypeOfIdentification", "1", arrivalTransportInfo1.TPM_TypeOfIdentification);
				AssertEquals("arrivalTransportInfo1.TPM_IdentificationNumber", "DTM1", arrivalTransportInfo1.TPM_IdentificationNumber);
				AssertEquals("arrivalTransportInfo1.TPM_RN_NKTransportNationality", "DE", arrivalTransportInfo1.TPM_RN_NKTransportNationality);

				var supportingDocs = arrivalMovementHeader.SupportingDocuments;
				AssertEquals("SupportingDocuments.Count", 2, supportingDocs.Count);
				var supportingDoc = supportingDocs[0];
				AssertEquals("supportingDoc.CSI_Status", NctsUnloadedStateList.Codes.DEC, supportingDoc.CSI_Status);
				AssertEquals("supportingDoc.CSI_Code", "Type1", supportingDoc.CSI_Code);
				AssertEquals("supportingDoc.CSI_ReferenceNumber", "HS123", supportingDoc.CSI_ReferenceNumber);
				AssertEquals("supportingDoc.CSI_LineNo", 1, supportingDoc.CSI_LineNo);
				AssertEquals("supportingDoc.CSI_ReferenceNumber2", "456", supportingDoc.CSI_ReferenceNumber2);

				var transportDocs = arrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToArray();
				AssertEquals("TransportDocuments.Count", 2, transportDocs.Length);
				var transportDoc = transportDocs[0];
				AssertEquals("transportDoc.CSI_Status", NctsUnloadedStateList.Codes.DEC, transportDoc.CSI_Status);
				AssertEquals("transportDoc.CSI_Code", "Type1", transportDoc.CSI_Code);
				AssertEquals("transportDoc.CSI_ReferenceNumber", "HT123", transportDoc.CSI_ReferenceNumber);

				var additionalRefs = arrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToArray();
				AssertEquals("AdditionalReference.Count", 2, additionalRefs.Length);
				var additionalRef = additionalRefs[0];
				AssertEquals("additionalRef.CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalRef.CSI_Status);
				AssertEquals("additionalRef.CSI_Code", "Type1", additionalRef.CSI_Code);
				AssertEquals("additionalRef.CSI_ReferenceNumber", "HR123", additionalRef.CSI_ReferenceNumber);

				var additionalInfos = arrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
				AssertEquals("AdditionalInformation.Count", 2, additionalInfos.Length);
				var additionalInfo = additionalInfos[0];
				AssertEquals("additionalInfo.CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInfo.CSI_Status);
				AssertEquals("additionalInfo.CSI_Code", "Type1", additionalInfo.CSI_Code);
				AssertEquals("additionalInfo.CSI_ReferenceNumber", "HI123", additionalInfo.CSI_ReferenceNumber);
			});

			var bills = nctsHeader.Bills;
			var bill1 = bills[0];
			CombineAssertions("Bills", () =>
			{
				AssertEquals("Count", 2, bills.Count);
				AssertEquals("bill1.B0_Weight", 2.5m, bill1.B0_Weight);
				var movementDetail = bill1.MovementDetail;
				AssertEquals("movementDetail.B9_UnloadedState", NctsUnloadedStateListForHouseConsignment.Codes.DEC, movementDetail.B9_UnloadedState);
				AssertEquals("movementDetail.B9_SeqNo", "1", movementDetail.B9_SeqNo);

				var previousDocs = bill1.PreviousDocuments;
				AssertEquals("PreviousDocuments.Count", 2, previousDocs.Count);
				var prevDoc = previousDocs[0];
				AssertEquals("prevDoc.CSI_Status", NctsUnloadedStateList.Codes.DEC, prevDoc.CSI_Status);
				AssertEquals("prevDoc.CSI_Code", "Type1", prevDoc.CSI_Code);
				AssertEquals("prevDoc.CSI_ReferenceNumber", "P123", prevDoc.CSI_ReferenceNumber);
				AssertEquals("prevDoc.CSI_ReferenceNumber2", "456", prevDoc.CSI_ReferenceNumber2);

				var supportingDocs = bill1.SupportingDocuments;
				AssertEquals("SupportingDocuments.Count", 2, supportingDocs.Count);
				var supportingDoc = supportingDocs[0];
				AssertEquals("supportingDoc.CSI_Status", NctsUnloadedStateList.Codes.DEC, supportingDoc.CSI_Status);
				AssertEquals("supportingDoc.CSI_Code", "Type1", supportingDoc.CSI_Code);
				AssertEquals("supportingDoc.CSI_ReferenceNumber", "S123", supportingDoc.CSI_ReferenceNumber);
				AssertEquals("supportingDoc.CSI_LineNo", 1, supportingDoc.CSI_LineNo);
				AssertEquals("supportingDoc.CSI_ReferenceNumber2", "456", supportingDoc.CSI_ReferenceNumber2);

				var transportDocs = bill1.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToArray();
				AssertEquals("TransportDocuments.Count", 2, transportDocs.Length);
				var transportDoc = transportDocs[0];
				AssertEquals("transportDoc.CSI_Status", NctsUnloadedStateList.Codes.DEC, transportDoc.CSI_Status);
				AssertEquals("transportDoc.CSI_Code", "Type1", transportDoc.CSI_Code);
				AssertEquals("transportDoc.CSI_ReferenceNumber", "T123", transportDoc.CSI_ReferenceNumber);

				var additionalRefs = bill1.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToArray();
				AssertEquals("AdditionalReferences.Count", 2, additionalRefs.Length);
				var additionalRef = additionalRefs[0];
				AssertEquals("additionalRef.CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalRef.CSI_Status);
				AssertEquals("additionalRef.CSI_Code", "Type1", additionalRef.CSI_Code);
				AssertEquals("additionalRef.CSI_ReferenceNumber", "R123", additionalRef.CSI_ReferenceNumber);

				var additionalInfos = bill1.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
				AssertEquals("AdditionalInformation.Count", 2, additionalInfos.Length);
				var additionalInfo = additionalInfos[0];
				AssertEquals("additionalInfo.CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInfo.CSI_Status);
				AssertEquals("additionalInfo.CSI_Code", "Type1", additionalInfo.CSI_Code);
				AssertEquals("additionalInfo.CSI_ReferenceNumber", "I123", additionalInfo.CSI_ReferenceNumber);
			});

			CombineAssertions("Goods Items", () =>
			{
				var goodsItems = bill1.ArrivalGoodsItems;
				AssertEquals("goodsItems.Count", 2, goodsItems.Count);
				var goodsItem = goodsItems[0];
				AssertEquals("goodsItem.BY_UnloadedState", NctsUnloadedStateList.Codes.DEC, goodsItem.BY_UnloadedState);
				AssertEquals("goodsItem.BY_LineNo", (ZShort)1, goodsItem.BY_LineNo);
				AssertEquals("goodsItem.BY_DeclarationGoodsItemNumber", 1, goodsItem.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodsItem.BY_Description", "123", goodsItem.BY_Description);
				AssertEquals("goodsItem.BY_CusC4Number", "456", goodsItem.BY_CusC4Number);
				AssertEquals("goodsItem.BY_HarmonisedTariff", "11100022", goodsItem.BY_HarmonisedTariff);
				AssertEquals("goodsItem.BY_GrossWeight", 11.11m, goodsItem.BY_GrossWeight);
				AssertEquals("goodsItem.BY_NetWeight", 10.01m, goodsItem.BY_NetWeight);

				var packages = goodsItem.Packages;
				AssertEquals("packages.Count", 2, packages.Count);
				var package1 = packages[0];
				AssertEquals("package1.B5_TypeOfDifference", NctsUnloadedStateList.Codes.DEC, package1.B5_TypeOfDifference);
				AssertEquals("package1.B5_SequenceNumber", (ZShort)1, package1.B5_SequenceNumber);
				AssertEquals("package1.B5_UnitType", "T1", package1.B5_UnitType);
				AssertEquals("package1.B5_UnitCount", 123, package1.B5_UnitCount);
				AssertEquals("package1.B5_MarksAndNumbers", "No123", package1.B5_MarksAndNumbers);

				var package1Containers = package1.ContainersPivot.Containers.OrderBy(x => x.BC_SequenceNumber).ToArray();
				AssertEquals(".Single()", "CNT1", package1Containers.Single().BC_ContainerNum);

				var supportingDocuments = goodsItem.SupportingDocuments;
				AssertEquals("supportingDocuments.Count", 2, supportingDocuments.Count);
				var supportingDocument1 = supportingDocuments[0];
				AssertEquals("supportingDocument1.CSI_Status", NctsUnloadedStateList.Codes.DEC, supportingDocument1.CSI_Status);
				AssertEquals("supportingDocument1.CSI_Code", "Type1", supportingDocument1.CSI_Code);
				AssertEquals("supportingDocument1.CSI_ReferenceNumber", "123", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("supportingDocument1.CSI_LineNo", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("supportingDocument1.CSI_ReferenceNumber2", "456", supportingDocument1.CSI_ReferenceNumber2);

				var additionalReferences = goodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToArray();
				AssertEquals("additionalReferences.Count", 2, additionalReferences.Length);
				var additionalReference1 = additionalReferences[0];
				AssertEquals("additionalReference1.CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalReference1.CSI_Status);
				AssertEquals("additionalReference1.CSI_Code", "Type1", additionalReference1.CSI_Code);
				AssertEquals("additionalReference1.CSI_ReferenceNumber", "1234", additionalReference1.CSI_ReferenceNumber);

				var additionalInformation = goodsItem.AdditionalInfos.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
				AssertEquals("additionalInformation.Count", 2, additionalInformation.Length);
				var additionalInfo1 = additionalInformation[0];
				AssertEquals("additionalInfo1.CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInfo1.CSI_Status);
				AssertEquals("additionalInfo1.CSI_Code", "Type1", additionalInfo1.CSI_Code);
				AssertEquals("additionalInfo1.CSI_ReferenceNumber", "A123", additionalInfo1.CSI_ReferenceNumber);
			});
		}

		protected override ZString MessageFriendlyName => "NCTS DESPER Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDESPER>> Processor => new NctsDESPERMessageProcessor(logger);

		static INCTSDocument CreateNCTSDocumentMock(string type = "", string referenceNumber = "", int? documentLineItemNumber = 0, string complementOfInformation = "") =>
			Mock.Of<INCTSDocument>(m =>
				m.Type == type &&
				m.ReferenceNumber == referenceNumber &&
				m.DocumentLineItemNumber == documentLineItemNumber &&
				m.ComplementOfInformation == complementOfInformation);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = MovementReferenceNumber;
			nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = ReferenceNumber;

			Factory.Save();

			dataProviderMock = new Mock<IDESPER>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDESPER>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
		}

		Mock<IDESPER> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IDESPER>> messageMock;
		AtlasInboundEDIMessage<IDESPER> message;
		NctsHeader nctsHeader;

		const string ReferencedMessageIdentifier = "19DE265655002905M6";
		const string MovementReferenceNumber = "22DE000000001234E0";
		const string ReferenceNumber = "19DE485154386041M4";
	}
}
