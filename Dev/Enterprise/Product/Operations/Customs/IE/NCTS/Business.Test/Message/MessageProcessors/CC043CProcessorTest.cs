using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC043C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusSeal = Enterprise.Customs.EU.NCTS.Business.CusSeal;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC043CProcessor))]
	class CC043CProcessorTest : NCTSArrivalMessageProcessorAbstractTest<CC043CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC043CProvider>
	{
		public void TestOverwrite()
		{
			var processor = Processor;
			InterchangeProcessorTestHelper.ContainerIndicator = Flag.Item0;
			InterchangeProcessorTestHelper.GrossMass = 1234m;
			InterchangeProcessorTestHelper.Security = "1";
			var (_, messageAttachee, _, incomingMessage) = CreateSetupData();
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			AssertFieldsUpdated(messageAttachee, 1234m, Core.Constants.ContainerModes.NonContainerised, 2);

			InterchangeProcessorTestHelper.ContainerIndicator = Flag.Item1;
			InterchangeProcessorTestHelper.GrossMass = 1584.5m;
			InterchangeProcessorTestHelper.Security = "0";
			ExpectedB0_SecurityIndicatorFromExport = ZBool.False;
			var incomingMessage2 = CreateNewIncomingMessage();
			processor.PreProcessMessage(incomingMessage2);
			processor.ProcessMessage(incomingMessage2);
			AssertFieldsUpdated(messageAttachee, 1584.5m, Core.Constants.ContainerModes.Containerised, 4);
		}

		ZBool ExpectedB0_SecurityIndicatorFromExport { get; set; } = ZBool.True;

		public void TestContainerIndicator()
		{
			var processor = Processor;
			InterchangeProcessorTestHelper.ContainerIndicator = Flag.Item0;
			var (_, messageAttachee, _, incomingMessage) = CreateSetupData();
			processor.PreProcessMessage(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			var container = messageAttachee.ArrivalHeaderContainers[0];
			AssertEquals("BC_Mode", Core.Constants.ContainerModes.NonContainerised, container.BC_Mode);

			InterchangeProcessorTestHelper.ContainerIndicator = Flag.Item1;
			var incomingMessage2 = CreateNewIncomingMessage();
			processor.PreProcessMessage(incomingMessage2);
			processor.ProcessMessage(incomingMessage2);
			container = messageAttachee.ArrivalHeaderContainers[0];
			AssertEquals("BC_Mode", Core.Constants.ContainerModes.Containerised, container.BC_Mode);
		}

		[TestDate(2024, 5, 22, 19, 0, 0)]
		public void TestGenerationOfCustomsRegistry()
		{
			var orgHeaderDestinationTrader = Factory.New<OrgHeader>();
			orgHeaderDestinationTrader.OH_Code = "ORG001";
			orgHeaderDestinationTrader.OH_FullName = "INTRISNV";

			var (nctsHeader, _, _, incomingMessage) = CreateSetupData();
			nctsHeader.DestinationTrader.OrganisationPK = orgHeaderDestinationTrader.PK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var entryNumber = CusEntryNumber.Load(nctsHeader, CusEntryNumberTypes.EU.CustomsRegistry, Core.Constants.CountryCodes.Ireland);
			CombineAssertions(() =>
			{
				AssertNotNull("A new entry num should have been created on the NCTS header", entryNumber);
				AssertEquals("The category of the entry num should be CUS", "CUS", entryNumber.CE_Category);
				AssertEquals("The number of the entry num should be 21", nctsHeader.LocalReferenceNumber, entryNumber.CE_EntryNum);
				AssertEquals("The reference of the entry num should be TA-ORG001", "TA-ORG001", entryNumber.CE_EntryLineReference);
				AssertEquals("The reference of the entry num should be 20-07-2023", new ZDateTime(2024, 5, 22, 19, 0, 0), entryNumber.CE_IssueDate);
			});
		}

		protected override void AssertProcessResultCore(NctsHeader nctsHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Unloading Permission (IE043) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
			AssertMessageInterpretation(incomingMessage, $@"An Unloading Permission (IE043) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Customs Office of Destination (Actual)</td><td>RNALPHN9</td></tr><tr><td>Trader at Destination </td><td>IN043</td></tr><tr><td>Address</td><td>123 WHERE ST, CITY, IE</td></tr></table>");
			AssertFieldsUpdated(nctsHeader, 1584.5m, Core.Constants.ContainerModes.Containerised, 2);
		}

		#region Assert Fields Updated

		void AssertFieldsUpdated(NctsHeader nctsHeader, decimal expectedGrossWeight, string expectedContainerMode, int expectedEnRouteIncidents)
		{
			AssertEquals("BM_GrossWeight", expectedGrossWeight, nctsHeader.ArrivalMovementHeader.TotalGrossMassInKilograms);

			AssertEquals("Arrival Header Containers", 2, nctsHeader.ArrivalHeaderContainers.Count);
			var container1 = nctsHeader.ArrivalHeaderContainers[0];
			var container2 = nctsHeader.ArrivalHeaderContainers[1];
			AssertContainer(container1, (ZShort)1, expectedContainerMode, "1234");
			AssertContainer(container2, (ZShort)2, expectedContainerMode, "5678");
			AssertEquals("BM_SealQty, Container1 + Container2", (ZShort)4 + 1, nctsHeader.ArrivalMovementHeader.BM_SealQty);

			AssertEquals("Container 1 Seals", 4, container1.Seals.Count);
			AssertEquals("Container 2 Seals", 1, container2.Seals.Count);
			AssertSeal(container1.Seals[0], (ZShort)1, "1111");
			AssertSeal(container1.Seals[1], (ZShort)2, "1222");
			AssertSeal(container1.Seals[2], (ZShort)3, "1333");
			AssertSeal(container1.Seals[3], (ZShort)4, "1444");
			AssertSeal(container2.Seals[0], (ZShort)1, "2111");

			AssertEquals("Arrival Transport Infos", 2, nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.Count);
			AssertTransportInfo(nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos[0], (ZShort)1, "21", "111", "IE");
			AssertTransportInfo(nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos[1], (ZShort)2, "41", "222", "XI");

			AssertEquals("Supporting Documents", 3, nctsHeader.ArrivalMovementHeader.SupportingDocuments.Count);
			AssertSupportingDocument(nctsHeader.ArrivalMovementHeader.SupportingDocuments[0], CusSupportingInfoTypeList.Codes.SupportingDocument, (ZShort)1, "XYZA", "111", "112");
			AssertSupportingDocument(nctsHeader.ArrivalMovementHeader.SupportingDocuments[1], CusSupportingInfoTypeList.Codes.SupportingDocument, (ZShort)2, "ABCA", "222", "122");
			AssertSupportingDocument(nctsHeader.ArrivalMovementHeader.SupportingDocuments[2], CusSupportingInfoTypeList.Codes.SupportingDocument, (ZShort)3, "DEFA", "333", "132");

			AssertEquals("Transport Documents + Additional References + Additional Information", 2 + 2 + 2, nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Count);
			AssertAdditionalDocument(nctsHeader.ArrivalMovementHeader.AdditionalDocuments[0], CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument, (ZShort)1, "PQRB", "234");
			AssertAdditionalDocument(nctsHeader.ArrivalMovementHeader.AdditionalDocuments[1], CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.TransportDocument, (ZShort)2, "YZAB", "678");

			AssertAdditionalDocument(nctsHeader.ArrivalMovementHeader.AdditionalDocuments[2], CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalReference, (ZShort)1, "REFC", "456");
			AssertAdditionalDocument(nctsHeader.ArrivalMovementHeader.AdditionalDocuments[3], CusSupportingInfoTypeList.Codes.AdditionalInfo, AdditionalInfoSubTypeList.Codes.AdditionalReference, (ZShort)2, "REGC", "789");

			AssertEquals("Incidents", expectedEnRouteIncidents, nctsHeader.EnRouteIncidents.Count);
			AssertIncident(nctsHeader.EnRouteIncidents[0], "2", "Test", new ZDate(2023, 8, 22), "Authority", "Meath", Core.Constants.CountryCodes.Ireland, "U", "IEROS", "IE");
			AssertIncident(nctsHeader.EnRouteIncidents[1], "4", "Extra", new ZDate(2023, 8, 23), "Authority 2", "Dublin", Core.Constants.CountryCodes.Ireland, "U", "IEDUB", "IE");
			AssertConsignment(nctsHeader);
		}

		void AssertContainer(NctsArrivalHeaderContainer container, ZShort expectedSequenceNumber, ZString expectedContainerMode, ZString expectedContainerNumber)
		{
			AssertEquals("Container BC_UnloadedState", NctsUnloadedStateList.Codes.DEC, container.BC_UnloadedState);
			AssertEquals("Container BC_ParentTableCode", "BH", container.BC_ParentTableCode);
			AssertEquals("Container BC_SequenceNumber", expectedSequenceNumber, container.BC_SequenceNumber);
			AssertEquals("Container BC_Mode", expectedContainerMode, container.BC_Mode);
			AssertEquals("Container BC_ContainerNum", expectedContainerNumber, container.BC_ContainerNum);
		}

		void AssertSeal(CusSeal seal, ZShort expectedSequenceNumber, ZString expectedSealNumber)
		{
			AssertEquals("Seal BK_UnloadingState", NctsUnloadedStateList.Codes.DEC, seal.BK_UnloadingState);
			AssertEquals("Seal BK_ParentTableCode", "BC", seal.BK_ParentTableCode);
			AssertEquals("Seal BK_SequenceNumber", expectedSequenceNumber, seal.BK_SequenceNumber);
			AssertEquals("Seal BK_SealNumber", expectedSealNumber, seal.BK_SealNumber);
		}

		void AssertTransportInfo(ArrivalCusTransportMeans transportInfo, ZShort expectedSequenceNumber, ZString expectedType, ZString expectedIdentificationNumber, ZString expectedNationality)
		{
			AssertEquals("Transport Info TPM_TransportState", NctsUnloadedStateList.Codes.DEC, transportInfo.TPM_TransportState);
			AssertEquals("Transport Info TPM_ParentTable", "BM", transportInfo.TPM_ParentTableCode);
			AssertEquals("Transport Info TPM_SequenceNumber", expectedSequenceNumber, transportInfo.TPM_SequenceNumber);
			AssertEquals("Transport Info TPM_TypeOfIdentification", expectedType, transportInfo.TPM_TypeOfIdentification);
			AssertEquals("Transport Info TPM_IdentificationNumber", expectedIdentificationNumber, transportInfo.TPM_IdentificationNumber);
			AssertEquals("Transport Info TPM_RN_NKTransportNationality", expectedNationality, transportInfo.TPM_RN_NKTransportNationality);
		}

		void AssertSupportingDocument(NctsSupportingDocument supportingDocument, ZString expectedType, ZShort expectedSequenceNumber, ZString expectedCode, ZString expectedReferenceNumber, ZString expectedReferenceNumber2)
		{
			AssertEquals("Supporting Document CSI_Status", NctsUnloadedStateList.Codes.DEC, supportingDocument.CSI_Status);
			AssertEquals("Supporting Document CSI_Type", expectedType, supportingDocument.CSI_Type);
			AssertEquals("Supporting Document CSI_ParentTableCode", "BM", supportingDocument.CSI_ParentTableCode);
			AssertEquals("Supporting Document CSI_LineNo", expectedSequenceNumber, supportingDocument.CSI_LineNo);
			AssertEquals("Supporting Document CSI_Code", expectedCode, supportingDocument.CSI_Code);
			AssertEquals("Supporting Document CSI_ReferenceNumber", expectedReferenceNumber, supportingDocument.CSI_ReferenceNumber);
			AssertEquals("Supporting Document CSI_ReferenceNumber2", expectedReferenceNumber2, supportingDocument.CSI_ReferenceNumber2);
		}

		void AssertAdditionalDocument(NctsAdditionalInfo additionalDocument, ZString expectedType, ZString expectedSubType, ZShort expectedSequenceNumber, ZString expectedCode, ZString expectedReferenceNumber)
		{
			AssertEquals("Additional Document CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalDocument.CSI_Status);
			AssertEquals("Additional Document CSI_Type", expectedType, additionalDocument.CSI_Type);
			AssertEquals("Additional Document CSI_SubType", expectedSubType, additionalDocument.CSI_SubType);
			AssertEquals("Additional Document CSI_ParentTableCode", "BM", additionalDocument.CSI_ParentTableCode);
			AssertEquals("Additional Document CSI_LineNo", expectedSequenceNumber, additionalDocument.CSI_LineNo);
			AssertEquals("Additional Document CSI_Code", expectedCode, additionalDocument.CSI_Code);
			AssertEquals("Additional Document CSI_ReferenceNumber", expectedReferenceNumber, additionalDocument.CSI_ReferenceNumber);
		}

		void AssertIncident(EnRouteIncident incident, ZString expectedCode, ZString expectedText, ZDate expectedDate, ZString expectedAuthority, ZString expectedPlace, ZString expectedEndorsementCountry, ZString expectedLocationQualifier, ZString expectedEventPlace, ZString expectedIncidentCountry)
		{
			AssertEquals("Incident BN_IncidentCode", expectedCode, incident.BN_IncidentCode);
			AssertEquals("Incident BN_Information", expectedText, incident.BN_Information);

			AssertEquals("Incident BN_CustomsStatus", "CUS", incident.BN_CustomsStatus);
			AssertEquals("Incident BN_EndorsementDate", expectedDate, incident.BN_EndorsementDate);
			AssertEquals("Incident BN_EndorsementAuthority", expectedAuthority, incident.BN_EndorsementAuthority);
			AssertEquals("Incident BN_EndorsementPlace", expectedPlace, incident.BN_EndorsementPlace);
			AssertEquals("Incident BN_EndorsementCountryCode", expectedEndorsementCountry, incident.BN_EndorsementCountryCode);

			AssertEquals("Incident BN_LocationQualifier", expectedLocationQualifier, incident.BN_LocationQualifier);
			AssertEquals("Incident BN_EventPlace", expectedEventPlace, incident.BN_EventPlace);
			AssertEquals("Incident BN_EventCountryCode", expectedIncidentCountry, incident.BN_EventCountryCode);
		}

		void AssertConsignment(NctsHeader header)
		{
			AssertEquals("Consignment Inland mode of transport", "2", header.ArrivalMovementHeader.BM_InlandTransportMode);
			var additionalInformations = header.ArrivalMovementHeader.AdditionalDocuments
				.Where(x => x.CSI_Status == NctsUnloadedStateList.Codes.DEC
				&& x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
				&& x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList();
			AssertEquals("Consignment Additional information count", 2, additionalInformations.Count);
			AssertConsignment_AdditionalInformation(additionalInformations[0], "30300", "ABC");
			AssertConsignment_AdditionalInformation(additionalInformations[1], "30600", "DEF");

			var previousDocs = header.PreviousDocuments
				.Where(x => x.CSI_Status == NctsUnloadedStateList.Codes.DEC
				&& x.CSI_Type == CusSupportingInfoTypeList.Codes.PreviousDocument
				&& x.CSI_ParentTableCode == "BH").ToList();
			AssertEquals("Consignment Previous document count", 2, previousDocs.Count);
			AssertConsignment_PreviousDocs(previousDocs[0], "A001", "123456", "Information test 1");
			AssertConsignment_PreviousDocs(previousDocs[1], "A004", "987654", "Information test 2");

			var houseConsignments = header.Bills;
			AssertEquals("House Consignment count", 1, houseConsignments.Count);
			AssertHouseConsignment(houseConsignments[0]);
		}

		void AssertConsignment_AdditionalInformation(NctsAdditionalInfo additionalInfo, ZString expectedCode, ZString expectedText)
		{
			AssertEquals("Consignment Additional Information CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInfo.CSI_Status);
			AssertEquals("Consignment Additional Information CSI_Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalInfo.CSI_Type);
			AssertEquals("Consignment Additional Information CSI_SubType", AdditionalInfoSubTypeList.Codes.AdditionalInformation, additionalInfo.CSI_SubType);
			AssertEquals("Consignment Additional Information CSI_Code", expectedCode, additionalInfo.CSI_Code);
			AssertEquals("Consignment Additional Information CSI_ReferenceNumber", expectedText, additionalInfo.CSI_ReferenceNumber);
		}

		void AssertConsignment_PreviousDocs(PreviousDocument additionalInfo, ZString expectedCode, ZString expectedReference, ZString expectedReference2)
		{
			AssertEquals("Consignment Previous Document CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInfo.CSI_Status);
			AssertEquals("Consignment Previous Document CSI_Type", CusSupportingInfoTypeList.Codes.PreviousDocument, additionalInfo.CSI_Type);
			AssertEquals("Consignment Previous Document CSI_Code", expectedCode, additionalInfo.CSI_Code);
			AssertEquals("Consignment Previous Document CSI_ReferenceNumber", expectedReference, additionalInfo.CSI_ReferenceNumber);
			AssertEquals("Consignment Previous Document CSI_ReferenceNumber2", expectedReference2, additionalInfo.CSI_ReferenceNumber2);
		}

		void AssertHouseConsignment(EU.NCTS.Business.NctsBill houseConsignment)
		{
			AssertEquals("House Consignment Un State", NctsUnloadedStateList.Codes.DEC, houseConsignment.MovementDetail.B9_UnloadedState);
			AssertEquals("House Consignment Sequence Number", "1", houseConsignment.MovementDetail.B9_SeqNo);
			AssertEquals("House Consignment Gross mass", 1250.75m, houseConsignment.B0_Weight);

			AssertEquals("House Consignment Security indicator", ExpectedB0_SecurityIndicatorFromExport, houseConsignment.B0_SecurityIndicatorFromExport);

			var transportDocs = houseConsignment.AdditionalDocuments.Where(x =>
			x.CSI_Status == NctsUnloadedStateList.Codes.DEC
			&& x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
			&& x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToList();
			AssertEquals("House Consignment Transport Document Count", 2, transportDocs.Count);
			AssertHouseConsignment_TransportDoc(transportDocs[0], "1", "PQRD", "234");
			AssertHouseConsignment_TransportDoc(transportDocs[1], "2", "YZAD", "678");

			var additionalRefs = houseConsignment.AdditionalDocuments.Where(x =>
			x.CSI_Status == NctsUnloadedStateList.Codes.DEC
			&& x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
			&& x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToList();
			AssertEquals("House Consignment Additional References Count", 2, additionalRefs.Count);
			AssertHouseConsignment_AdditionalReferences(additionalRefs[0], "1", "REFE", "456");
			AssertHouseConsignment_AdditionalReferences(additionalRefs[1], "2", "REGE", "789");

			AssertHouseConsignment_Consignor(houseConsignment.MovementDetail.ConsignorDocAddress);
			AssertHouseConsignment_Consignee(houseConsignment.MovementDetail.ConsigneeDocAddress);

			AssertEquals("House Consignment Supporting Document Count", 2, houseConsignment.SupportingDocuments.Count);
			AssertHouseConsignment_SupportingDocument(houseConsignment.SupportingDocuments[0], 1, "PQRD", "234", "2222");
			AssertHouseConsignment_SupportingDocument(houseConsignment.SupportingDocuments[1], 2, "YZAD", "678", "4444");

			AssertEquals("House Consignment Departure Transport Means Count", 2, houseConsignment.DepartureTransportInfos.Count);
			AssertHouseConsignment_DepartureTransportMeans(houseConsignment.DepartureTransportInfos[0], 1, "11", "111", "IE");
			AssertHouseConsignment_DepartureTransportMeans(houseConsignment.DepartureTransportInfos[1], 2, "20", "777", "GB");

			var consignmentItems = houseConsignment.ArrivalGoodsItems;
			AssertConsignmentItems(consignmentItems);
		}

		void AssertHouseConsignment_TransportDoc(AdditionalInfo additionalInfo, ZString expectedSequenceNo, ZString expectedType, ZString expectedReferenceNo)
		{
			AssertEquals("House Consignment Transport Document CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInfo.CSI_Status);
			AssertEquals("House Consignment Transport Document CSI_Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalInfo.CSI_Type);
			AssertEquals("House Consignment Transport Document CSI_SubType", AdditionalInfoSubTypeList.Codes.TransportDocument, additionalInfo.CSI_SubType);
			AssertEquals("House Consignment Transport Document CSI_ParentTableCode", "B0", additionalInfo.CSI_ParentTableCode);
			AssertEquals("House Consignment Transport Document CSI_LineNo", ZInt.Parse(expectedSequenceNo), additionalInfo.CSI_LineNo);
			AssertEquals("House Consignment Transport Document CSI_Code", expectedType, additionalInfo.CSI_Code);
			AssertEquals("House Consignment Transport Document CSI_ReferenceNumber", expectedReferenceNo, additionalInfo.CSI_ReferenceNumber);
		}

		void AssertHouseConsignment_AdditionalReferences(AdditionalInfo additionalReference, ZString expectedSequenceNo, ZString expectedType, ZString expectedReferenceNo)
		{
			AssertEquals("House Consignment Additional Reference CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalReference.CSI_Status);
			AssertEquals("House Consignment Additional Reference CSI_Type", CusSupportingInfoTypeList.Codes.AdditionalInfo, additionalReference.CSI_Type);
			AssertEquals("House Consignment Additional Reference CSI_SubType", AdditionalInfoSubTypeList.Codes.AdditionalReference, additionalReference.CSI_SubType);
			AssertEquals("House Consignment Additional Reference CSI_ParentTableCode", "B0", additionalReference.CSI_ParentTableCode);
			AssertEquals("House Consignment Additional Reference CSI_LineNo", ZInt.Parse(expectedSequenceNo), additionalReference.CSI_LineNo);
			AssertEquals("House Consignment Additional Reference CSI_Code", expectedType, additionalReference.CSI_Code);
			AssertEquals("House Consignment Additional Reference CSI_ReferenceNumber", expectedReferenceNo, additionalReference.CSI_ReferenceNumber);
		}

		void AssertHouseConsignment_Consignor(JobDocAddress consignor)
		{
			AssertEquals("House Consignment Consignor Address Type", DocAddressTypes.Codes.ConsignorDocumentaryAddress, consignor.E2_AddressType);
			AssertEquals("House Consignment Consignor Parent Table", "B9", consignor.E2_ParentTableCode);
			AssertEquals("House Consignment Consignor Address Override", true, consignor.E2_AddressOverride);
			AssertEquals("House Consignment Consignor Identification Number", "C002", consignor.GetEuIdentificationNumber());
			AssertEquals("House Consignment Consignor Identification Number", "C002", consignor.E2_GovRegNum);
			AssertEquals("House Consignment Consignor Identification Type", "EOR", consignor.E2_GovRegNumType);
			AssertEquals("House Consignment Consignor Name", "Consignor Name", consignor.CompanyName);
			AssertEquals("House Consignment Consignor Address Street and Number", "567 WHERE ST", consignor.Address1);
			AssertEquals("House Consignment Consignor Address City", "Dublin", consignor.City);
			AssertEquals("House Consignment Consignor Address Postcode", "D1 234", consignor.Postcode);
			AssertEquals("House Consignment Consignor Address Country", "IE", consignor.Country.Code);
			AssertEquals("House Consignment Consignor Address Country", "VAD", consignor.E2_ValidationStatus);
		}

		void AssertHouseConsignment_Consignee(JobDocAddress consignee)
		{
			AssertEquals("House Consignment Consignee Address Type", DocAddressTypes.Codes.ConsigneeAddress, consignee.E2_AddressType);
			AssertEquals("House Consignment Consignee Parent Table", "B9", consignee.E2_ParentTableCode);
			AssertEquals("House Consignment Consignee Address Override", true, consignee.E2_AddressOverride);
			AssertEquals("House Consignment Consignee Identification Number", "C001", consignee.GetEuIdentificationNumber());
			AssertEquals("House Consignment Consignee Identification Number", "C001", consignee.E2_GovRegNum);
			AssertEquals("House Consignment Consignee Identification Type", "EOR", consignee.E2_GovRegNumType);
			AssertEquals("House Consignment Consignee Name", "Consignee Name", consignee.CompanyName);
			AssertEquals("House Consignment Consignee Address Street and Number", "89 WHERE ST", consignee.Address1);
			AssertEquals("House Consignment Consignee Address City", "Dublin", consignee.City);
			AssertEquals("House Consignment Consignee Address Postcode", "D1 289", consignee.Postcode);
			AssertEquals("House Consignment Consignee Address Country", "IE", consignee.Country.Code);
			AssertEquals("House Consignment Consignee Address Country", "VAD", consignee.E2_ValidationStatus);
		}

		void AssertHouseConsignment_SupportingDocument(NctsSupportingDocument supportingDoc, ZInt expectedSequenceNo, ZString expectedType, ZString expectedReferenceNo, ZString expectedReferenceNo2)
		{
			AssertEquals("House Consignment Supporting Document CSI_Status", NctsUnloadedStateList.Codes.DEC, supportingDoc.CSI_Status);
			AssertEquals("House Consignment Supporting Document CSI_Type", CusSupportingInfoTypeList.Codes.SupportingDocument, supportingDoc.CSI_Type);
			AssertEquals("House Consignment Supporting Document CSI_SubType", ZString.Empty, supportingDoc.CSI_SubType);
			AssertEquals("House Consignment Supporting Document CSI_ParentTableCode", "B0", supportingDoc.CSI_ParentTableCode);
			AssertEquals("House Consignment Supporting Document CSI_LineNo", expectedSequenceNo, supportingDoc.CSI_LineNo);
			AssertEquals("House Consignment Supporting Document CSI_Code", expectedType, supportingDoc.CSI_Code);
			AssertEquals("House Consignment Supporting Document CSI_ReferenceNumber", expectedReferenceNo, supportingDoc.CSI_ReferenceNumber);
			AssertEquals("House Consignment Supporting Document CSI_ReferenceNumber2", expectedReferenceNo2, supportingDoc.CSI_ReferenceNumber2);
		}

		void AssertHouseConsignment_DepartureTransportMeans(DepartureCusTransportMeans departureTransportMeans, ZInt expectedSequenceNo, ZString expectedTypeOfId, ZString expectedIdNo, ZString expectedNationality)
		{
			AssertEquals("House Consignment Departure Transport Means TPM_ParentTableCode", "B0", departureTransportMeans.TPM_ParentTableCode);
			AssertEquals("House Consignment Departure Transport Means TPM_SequenceNumber", expectedSequenceNo, departureTransportMeans.TPM_SequenceNumber);
			AssertEquals("House Consignment Departure Transport Means TPM_TypeOfIdentification", expectedTypeOfId, departureTransportMeans.TPM_TypeOfIdentification);
			AssertEquals("House Consignment Departure Transport Means TPM_IdentificationNumber", expectedIdNo, departureTransportMeans.TPM_IdentificationNumber);
			AssertEquals("House Consignment Departure Transport Means TPM_RN_NKTransportNationality", expectedNationality, departureTransportMeans.TPM_RN_NKTransportNationality);
		}

		void AssertConsignmentItems(INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> consignmentItems)
		{
			AssertEquals("Consignment Item count", 1, consignmentItems.Count);
			var item = consignmentItems[0];
			AssertEquals("Consignment Item Un State", NctsUnloadedStateList.Codes.DEC, item.BY_UnloadedState);
			AssertEquals("Consignment Item Goods item number", (ZShort)1, item.BY_LineNo);
			AssertEquals("Consignment Item Declaration goods item number", (ZInt)1, item.BY_DeclarationGoodsItemNumber);
			AssertEquals("Consignment Item Description of goods", "Treated Timber", item.BY_Description);
			AssertEquals("Consignment Item CUS Code", "0018113-5", item.BY_CusC4Number);
			AssertEquals("Consignment Item Declaration Type", "A1", item.BY_Type);
			AssertEquals("Consignment Item Country of Destination", "IE", item.BY_RN_NKCountryOfDestination);
			AssertEquals("Consignment Item BY_HarmonisedTariff", "101023A1", item.BY_HarmonisedTariff);
			AssertEquals("Consignment Item Gross Weight", 1250.75m, item.BY_GrossWeight);
			AssertEquals("Consignment Item Nett weight", 1200m, item.BY_NetWeight);

			AssertEquals("Consignment Item Packaging Count", 2, item.Packages.Count);
			AssertEquals("Consignment Item Package[0] Sequence Number", (ZShort)1, item.Packages[0].B5_SequenceNumber);
			AssertEquals("Consignment Item Package[0] Un State", NctsUnloadedStateList.Codes.DEC, item.Packages[0].B5_TypeOfDifference);
			AssertEquals("Consignment Item Package[0] No of Packages", (ZLong)1, item.Packages[0].B5_UnitCount);
			AssertEquals("Consignment Item Package[0] Shipping marks", "PT", item.Packages[0].B5_UnitType);
			AssertEquals("Consignment Item Package[0] type", "1", item.Packages[0].B5_MarksAndNumbers);

			AssertEquals("Consignment Item Package[1] Sequence Number", (ZShort)2, item.Packages[1].B5_SequenceNumber);
			AssertEquals("Consignment Item Package[1] Un State", NctsUnloadedStateList.Codes.DEC, item.Packages[1].B5_TypeOfDifference);
			AssertEquals("Consignment Item Package[1] No of Packages", (ZLong)4, item.Packages[1].B5_UnitCount);
			AssertEquals("Consignment Item Package[1] Shipping marks", "CT", item.Packages[1].B5_UnitType);
			AssertEquals("Consignment Item Package[1] type", "MARK", item.Packages[1].B5_MarksAndNumbers);

			AssertEquals("Consignment Item Supporting Doc Count", 2, item.SupportingDocuments.Count);
			AssertEquals("Consignment Item Supporting Document[0] CSI_Status", NctsUnloadedStateList.Codes.DEC, item.SupportingDocuments[0].CSI_Status);
			AssertEquals("Consignment Item Supporting Document[0] CSI_LineNo", 1, item.SupportingDocuments[0].CSI_LineNo);
			AssertEquals("Consignment Item Supporting Document[0] CSI_Code", "AB12", item.SupportingDocuments[0].CSI_Code);
			AssertEquals("Consignment Item Supporting Document[0] CSI_ReferenceNumber", "AA1", item.SupportingDocuments[0].CSI_ReferenceNumber);
			AssertEquals("Consignment Item Supporting Document[0] CSI_ReferenceNumber2", "Test 123", item.SupportingDocuments[0].CSI_ReferenceNumber2);

			AssertEquals("Consignment Item Supporting Document[1] CSI_Status", NctsUnloadedStateList.Codes.DEC, item.SupportingDocuments[1].CSI_Status);
			AssertEquals("Consignment Item Supporting Document[1] CSI_LineNo", 2, item.SupportingDocuments[1].CSI_LineNo);
			AssertEquals("Consignment Item Supporting Document[1] CSI_Code", "CD34", item.SupportingDocuments[1].CSI_Code);
			AssertEquals("Consignment Item Supporting Document[1] CSI_ReferenceNumber", "BB2", item.SupportingDocuments[1].CSI_ReferenceNumber);
			AssertEquals("Consignment Item Supporting Document[1] CSI_ReferenceNumber2", "Test 456", item.SupportingDocuments[1].CSI_ReferenceNumber2);

			var transportDocs = item.AdditionalInfos.Where(x =>
			x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
			&& x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ToList();
			AssertEquals("Consignment Item Transport Doc Count", 1, transportDocs.Count);
			var doc = transportDocs[0];
			AssertEquals("Consignment Item Transport Doc CSI_Status", NctsUnloadedStateList.Codes.DEC, doc.CSI_Status);
			AssertEquals("Consignment Item Transport Doc CSI_LineNo", 1, doc.CSI_LineNo);
			AssertEquals("Consignment Item Transport Doc CSI_Code", "MO44", doc.CSI_Code);
			AssertEquals("Consignment Item Transport Doc CSI_ReferenceNumber", "DEFG9876", doc.CSI_ReferenceNumber);

			var additionalReferences = item.AdditionalInfos.Where(x =>
			x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
			&& x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ToList();
			AssertEquals("Consignment Item Additional Reference Count", 2, additionalReferences.Count);
			AssertEquals("Consignment Item Additional Reference[0] CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalReferences[0].CSI_Status);
			AssertEquals("Consignment Item Additional Reference[0] CSI_LineNo", 1, additionalReferences[0].CSI_LineNo);
			AssertEquals("Consignment Item Additional Reference[0] CSI_Code", "N380", additionalReferences[0].CSI_Code);
			AssertEquals("Consignment Item Additional Reference[0] CSI_ReferenceNumber", "TEST6666", additionalReferences[0].CSI_ReferenceNumber);
			AssertEquals("Consignment Item Additional Reference[1] CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalReferences[1].CSI_Status);
			AssertEquals("Consignment Item Additional Reference[1] CSI_LineNo", 2, additionalReferences[1].CSI_LineNo);
			AssertEquals("Consignment Item Additional Reference[1] CSI_Code", "ZZ46", additionalReferences[1].CSI_Code);
			AssertEquals("Consignment Item Additional Reference[1] CSI_ReferenceNumber", "TEST7788", additionalReferences[1].CSI_ReferenceNumber);

			var additionalInformation = item.AdditionalInfos.Where(x =>
			x.CSI_Type == CusSupportingInfoTypeList.Codes.AdditionalInfo
			&& x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToList();
			AssertEquals("Consignment Item Additional Information Count", 2, additionalInformation.Count);
			AssertEquals("Consignment Item Additional Information[0] CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInformation[0].CSI_Status);
			AssertEquals("Consignment Item Additional Information[0] CSI_Code", "TEST1", additionalInformation[0].CSI_Code);
			AssertEquals("Consignment Item Additional Information[0] CSI_Description", "Sample", additionalInformation[0].CSI_Description);
			AssertEquals("Consignment Item Additional Information[1] CSI_Status", NctsUnloadedStateList.Codes.DEC, additionalInformation[1].CSI_Status);
			AssertEquals("Consignment Item Additional Information[1] CSI_Code", "TEST2", additionalInformation[1].CSI_Code);
			AssertEquals("Consignment Item Additional Information[1] CSI_Description", "Example", additionalInformation[1].CSI_Description);
		}

		#endregion

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE043;

		protected override ZString MessageFriendlyName => "CC043C: UNLOADING PERMISSION";

		protected override CC043CProcessor Processor => new CC043CProcessor(logger, typeof(Cc043CType));

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC043CText("21IEDUB11A782454R2");
	}
}
