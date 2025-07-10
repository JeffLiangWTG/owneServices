using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(IENCTS043MessageProcessor))]
	sealed class IENCTS043MessageProcessorTest : NctsMessageProcessorTestCase<IENCTS043MessageProcessor, ICC043CAndIENCTS043CDataProvider>
	{
		public void TestProcessMessage()
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;

			var container1 = header.ArrivalHeaderContainers.AddNew();
			container1.BC_ContainerNum = "1111";
			container1.BC_Seal1 = "SEAL1";
			var container2 = header.ArrivalHeaderContainers.AddNew();
			container2.BC_ContainerNum = "2222";
			container2.BC_Seal1 = "SEAL2";

			Extensions.CreateMovementReferenceNumber(header, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			var messageDataProvider = Processor.GetMessageDataProvider(incomingMessage);
			var containerisedTransportEquipmentProvider = messageDataProvider.Consignment.TransportEquipments.First();

			Assert_Header(header, NctsTransitStatusList.Codes.Unknown);
			Assert_Consignment(header);
			Assert_HouseConsignment(header);
			Assert_ConsignmentItem(header);
			AssertContainerAndGoodsItems(header, containerisedTransportEquipmentProvider);
		}

		public void TestDiscardedForExistingCC043C()
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;

			var cc043cMessage = Factory.New<BEMessage>();
			cc043cMessage.EM_MessageSubType = "043";
			cc043cMessage.EM_Status = "PRS";
			header.Messages.Add(cc043cMessage);

			Extensions.CreateMovementReferenceNumber(header, "22BE000000000012J1");
			Factory.Save();
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMessage status should be {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
				var note = incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single();
				AssertContains("Note text", "The message is discarded because it found an NCTS declaration with MRN 22BE000000000012J1 that already received a CC043C Message. (Interchange Number:, Number:, Type:NCT); message status set to DISCARDED.", note.ST_NoteText);
			});
		}

		public void TestFindParentOfMessage()
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE000000000012J1");
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = arrivalHeader.ArrivalMovementHeader;
			movementHeader.BM_CustomsStatus = "ULR";
			movementHeader.BM_Phase = "044";
			arrivalHeader.EffectiveMessageStatus = "SNT";
			Extensions.CreateMovementReferenceNumber(arrivalHeader, "22BE000000000012J1");

			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			Extensions.CreateMovementReferenceNumber(departureHeader, "22BE000000000012J1");

			Factory.Save();

			var processor = new IENCTS043MessageProcessorForTest(new LoggingInformation());
			var parentHeader = (NctsHeader)processor.FindParentOfMessageExposed(incomingMessage, mockProvider.Object);

			AssertEquals("We should have found the arrival header", true, parentHeader.IsArrivalMovement);
		}

		void Assert_Header(NctsHeader header, string customsStatus)
		{
			var movementHeader = header.ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Message status", LogicalStatusList.Codes.Sent, header.EffectiveMessageStatus);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("BM_CustomsStatus", customsStatus, movementHeader.BM_CustomsStatus);
				AssertEquals("DeclarationType", "A", movementHeader.BM_InBondEntryType);
				AssertEquals("DeclarationAcceptanceDate", new ZDateTime(2022, 4, 1, 12, 34, 56), movementHeader.BM_EntryDate);
				AssertEquals("Security", "NON", movementHeader.BM_TypeOfSecurity);
				AssertEquals("ReducedDatasetIndicator", ZBool.True, movementHeader.BM_ReducedDatasetIndicator);
				AssertEquals((short)4, movementHeader.BM_SealQty);

				AssertContains("Incoming Message Interpretation", "Unloading Permission<br />Status granted on:", incomingMessage.EM_MessageInterpretation);
			});
		}

		void Assert_Consignment(NctsHeader header)
		{
			var movementHeader = header.ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Consignment - country of destination", "CH", movementHeader.BM_RL_NKDestinationPort);
				AssertEquals("Consignment - inland mode of transport", "1", movementHeader.BM_InlandTransportMode);
				AssertEquals("Consignment - gross", ZDecimal.Zero, movementHeader.BM_GrossWeight);

				var consignor = header.Consignor;
				AssertEquals("Consignor", true, consignor.IsEmpty);
				var consignee = header.Consignee;
				AssertEquals("Consignee", true, consignee.IsEmpty);

				AssertEquals("Number of transport equipments", 2, header.ArrivalHeaderContainers.Count);
				AssertContainerAndSeals(header.ArrivalHeaderContainers[0], "MSCU1234567", NctsUnloadedStateList.Codes.DEC, Core.Constants.ContainerModes.Containerised, 1, "seal1", "seal2", "seal3");
				AssertContainerAndSeals(header.ArrivalHeaderContainers[1], ZString.Empty, NctsUnloadedStateList.Codes.DEC, Core.Constants.ContainerModes.NonContainerised, 2, "seal1");

				AssertEquals("Number of transport means", 2, movementHeader.ArrivalTransportInfos.Count);
				var departureTransportMeans1 = movementHeader.ArrivalTransportInfos.First();
				AssertEquals("Transport state of the first transport means", "DEC", departureTransportMeans1.TPM_TransportState);
				AssertEquals("Sequence number of the first transport means", new ZShort(1), departureTransportMeans1.TPM_SequenceNumber);
				AssertEquals("Identification type of the first transport means", "99", departureTransportMeans1.TPM_TypeOfIdentification);
				AssertEquals("Identification number of the first transport means", "Trailer 784p", departureTransportMeans1.TPM_IdentificationNumber);
				AssertEquals("Nationality of the first transport means", "DE", departureTransportMeans1.TPM_RN_NKTransportNationality);

				var departureTransportMeans2 = movementHeader.ArrivalTransportInfos.Last();
				AssertEquals("Transport state of the second transport means", "DEC", departureTransportMeans2.TPM_TransportState);
				AssertEquals("Sequence number of the second transport means", new ZShort(2), departureTransportMeans2.TPM_SequenceNumber);
				AssertEquals("Identification type of the second transport means", "10", departureTransportMeans2.TPM_TypeOfIdentification);
				AssertEquals("Identification number of the second transport means", "Truck 553", departureTransportMeans2.TPM_IdentificationNumber);
				AssertEquals("Nationality of the second transport means", "BE", departureTransportMeans2.TPM_RN_NKTransportNationality);

				AssertEquals("Number of previous documents", 2, movementHeader.Header.PreviousDocuments.Count);
				var previousDocument1 = (CusSupportingInfo)movementHeader.Header.PreviousDocuments.First();
				AssertEquals("Sequence number of the first previous document", 1, previousDocument1.CSI_LineNo);
				AssertEquals("Type of the first previous document", "N705", previousDocument1.CSI_Code);
				AssertEquals("Reference number of the first previous document", "waybill 7895666", previousDocument1.CSI_ReferenceNumber);
				AssertEquals("Reference number 2 of the first previous document", "no further info for house consignment", previousDocument1.CSI_ReferenceNumber2);
				AssertEquals("Status of the first previous document", "DEC", previousDocument1.CSI_Status);

				var previousDocument2 = (CusSupportingInfo)movementHeader.Header.PreviousDocuments.Last();
				AssertEquals("Sequence number of the second previous document", 2, previousDocument2.CSI_LineNo);
				AssertEquals("Type of the second previous document", "N706", previousDocument2.CSI_Code);
				AssertEquals("Reference number of the second previous document", "waybill 12345", previousDocument2.CSI_ReferenceNumber);
				AssertEquals("Reference number 2 of the second previous document", "there is further info for house consignment", previousDocument2.CSI_ReferenceNumber2);
				AssertEquals("Status of the second previous document", "DEC", previousDocument2.CSI_Status);

				AssertEquals("Number of supporting documents", 2, movementHeader.SupportingDocuments.Count);
				var supportingDocument1 = (CusSupportingInfo)movementHeader.SupportingDocuments.First();
				AssertEquals("Sequence number of the first supporting document", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("Type of the first supporting document", "N951", supportingDocument1.CSI_Code);
				AssertEquals("Reference number of the first supporting document", "TIF house", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("Reference number 2 of the first supporting document", "test house supporting doc", supportingDocument1.CSI_ReferenceNumber2);
				AssertEquals("Status of the first supporting document", "DEC", supportingDocument1.CSI_Status);

				var supportingDocument2 = (CusSupportingInfo)movementHeader.SupportingDocuments.Last();
				AssertEquals("Sequence number of the second supporting document", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("Type of the second supporting document", "N952", supportingDocument2.CSI_Code);
				AssertEquals("Reference number of the second supporting document", "TIF away", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("Reference number 2 of the second supporting document", "test away supporting doc", supportingDocument2.CSI_ReferenceNumber2);
				AssertEquals("Status of the second supporting document", "DEC", supportingDocument2.CSI_Status);

				var transportDocuments = movementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				var additionalInfos = movementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "INF");
				var additionalReferences = movementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");

				AssertEquals("Number of transport documents", 2, transportDocuments.Count());
				AssertEquals("Number of additional infos", 2, additionalInfos.Count());
				AssertEquals("Number of additional references", 2, additionalReferences.Count());

				var transportDocument1 = transportDocuments.First();
				AssertEquals("Sequence number of the first transport document", 1, transportDocument1.CSI_LineNo);
				AssertEquals("Type of the first transport document", "N271", transportDocument1.CSI_Code);
				AssertEquals("Reference number of the first transport document", "Packing 1", transportDocument1.CSI_ReferenceNumber);
				AssertEquals("Status of the first transport document", "DEC", transportDocument1.CSI_Status);

				var transportDocument2 = transportDocuments.Last();
				AssertEquals("Sequence number of the second transport document", 2, transportDocument2.CSI_LineNo);
				AssertEquals("Type of the second transport document", "N272", transportDocument2.CSI_Code);
				AssertEquals("Reference number of the second transport document", "Packing 2", transportDocument2.CSI_ReferenceNumber);
				AssertEquals("Status of the second transport document", "DEC", transportDocument2.CSI_Status);

				var additionalInfos1 = additionalInfos.First();
				AssertEquals("Sequence number of the first additional info", 1, additionalInfos1.CSI_LineNo);
				AssertEquals("Type of the first additional info", "20300", additionalInfos1.CSI_Code);
				AssertEquals("Description of the first additional info", "this house 1 does not exist in any export declaration", additionalInfos1.CSI_Description);
				AssertEquals("Status of the first additional info", "DEC", additionalInfos1.CSI_Status);

				var additionalInfos2 = additionalInfos.Last();
				AssertEquals("Sequence number of the second additional info", 2, additionalInfos2.CSI_LineNo);
				AssertEquals("Type of the second additional info", "20301", additionalInfos2.CSI_Code);
				AssertEquals("Description of the second additional info", "this house 2 does not exist in any export declaration", additionalInfos2.CSI_Description);
				AssertEquals("Status of the second additional info", "DEC", additionalInfos2.CSI_Status);

				var additionalReference1 = additionalReferences.First();
				AssertEquals("Sequence number of the first additional reference", 1, additionalReference1.CSI_LineNo);
				AssertEquals("Type of the first additional reference", "Y022", additionalReference1.CSI_Code);
				AssertEquals("Reference of the first additional reference", "AEO 478900 House 1", additionalReference1.CSI_ReferenceNumber);
				AssertEquals("Status of the first additional reference", "DEC", additionalReference1.CSI_Status);

				var additionalReference2 = additionalReferences.Last();
				AssertEquals("Sequence number of the second additional reference", 2, additionalReference2.CSI_LineNo);
				AssertEquals("Type of the second additional reference", "Y023", additionalReference2.CSI_Code);
				AssertEquals("Reference of the second additional reference", "AEO 478900 House 2", additionalReference2.CSI_ReferenceNumber);
				AssertEquals("Status of the second additional reference", "DEC", additionalReference2.CSI_Status);

				AssertEquals("Incident Flag", YesNoList.Codes.Yes, header.BH_ExportFlag);
				AssertEquals("N° Incidents", 1, header.EnRouteIncidents.Count);
				var incident = header.EnRouteIncidents[0];
				AssertEquals("Incident Code", "1", incident.BN_IncidentCode);
				AssertEquals("Incident Information", "The incident is a consequence of another truck leaving a factory", incident.BN_Information);
				AssertEquals("Created By", "CUS", incident.BN_CustomsStatus);
				AssertEquals("Endorsement Date", new DateTime(2022, 12, 05), incident.BN_EndorsementDate);
				AssertEquals("Endorsement Authority", "BE Customs", incident.BN_EndorsementAuthority);
				AssertEquals("Endorsement Place", "Antwerp", incident.BN_EndorsementPlace);
				AssertEquals("Endorsement Country Code", "BE", incident.BN_EndorsementCountryCode);
				AssertEquals("Location Qualifier", "Z", incident.BN_LocationQualifier);
				var geoLocation = ZGeography.CreatePoint((double)incident.GoodsLocation.Address.E2_Longitude, (double)incident.GoodsLocation.Address.E2_Latitude);
				AssertEquals("Geolocation", geoLocation, incident.BN_GeoLocation);
				AssertEquals("Transport type at Departure", "30", incident.BN_TransportAtDepartureType);
				AssertEquals("Transport ID at Departure", "Truck 12345", incident.BN_TransportAtDepartureID);
				AssertEquals("Nationality at departure", "NL", incident.BN_RN_NKTransportAtDepartureIDNationality);
				AssertEquals("Goods Location Address", "Michiganlaan 5", incident.GoodsLocation.Address.Address1);
				AssertEquals("Goods Location Postcode", "2030", incident.GoodsLocation.Address.E2_Postcode);
				AssertEquals("Goods Location City", "Antwerp", incident.GoodsLocation.Address.City);
				AssertEquals("UNLocode", "BEANR", incident.GoodsLocation.Unlocode);
				AssertEquals("Latitude", 51.1252563, (double)incident.GoodsLocation.Address.E2_Latitude);
				AssertEquals("Longitude", 4.2563235, (double)incident.GoodsLocation.Address.E2_Longitude);

				var container = incident.IncidentContainers.First();
				AssertEquals("Container Sequence", (ZShort)1, container.BC_SequenceNumber);
				AssertEquals("Container Number", "MSCU1234567", container.BC_ContainerNum);
				AssertEquals("N° Seals", 3, container.TotalSealCount);
				AssertEquals("First Seal Number", "seal1", container.BC_Seal1);
				AssertEquals("Second Seal Number", "seal2", container.BC_Seal2);
				AssertContainsExactElementsInAnyOrder("Additional Seal Numbers", new[] { "seal3" }, container.Seals.Select(x => x.BK_SealNumber));
				AssertEquals("Seals not created for incident", 0, incident.Seals.Count);
			});
		}

		void AssertContainerAndGoodsItems(NctsHeader header, TransportEquipmentXmlProvider containerisedTransportEquipmentProvider)
		{
			var incident = header.EnRouteIncidents[0];
			var container = incident.IncidentContainers.First();
			var goodsReferencesList = containerisedTransportEquipmentProvider.GoodsReferences.ToList();

			AssertEquals(containerisedTransportEquipmentProvider.GoodsReferences.Count, container.ItemNumbers.Count);
			for (var index = 0; index < goodsReferencesList.Count; index++)
			{
				var linkedItem = container.ItemNumbers[index];
				AssertEquals(ZShort.Parse(goodsReferencesList[index].SequenceNumber), linkedItem.CY_Order);
				AssertEquals(goodsReferencesList[index].DeclarationGoodsItemNumber, linkedItem.CY_Data);
			}
		}

		void AssertContainerAndSeals(NctsArrivalHeaderContainer container, ZString containerNum, ZString unloadedState, ZString mode, ZShort containerSequence, params ZString[] seals)
		{
			AssertEquals(containerNum, container.BC_ContainerNum);
			AssertEquals(unloadedState, container.BC_UnloadedState);
			AssertEquals(mode, container.BC_Mode);
			AssertEquals(containerSequence, container.BC_SequenceNumber);
			AssertEquals(seals[0], container.BC_Seal1);
			AssertEquals(seals[0], container.BC_Seal2);

			AssertEquals(seals.Length, container.Seals.Count);
			for (var index = 0; index < seals.Length; index++)
			{
				var actualSeal = container.Seals[index];
				AssertEquals(seals[index], actualSeal.BK_SealNumber);
				AssertEquals((short)index + 1, actualSeal.BK_SequenceNumber);
				AssertEquals(unloadedState, actualSeal.BK_UnloadingState);
			}
		}

		void Assert_HouseConsignment(NctsHeader header)
		{
			var bill = header.Bills.Last();
			var movementDetail = bill.MovementDetail;
			CombineAssertions(() =>
			{
				AssertEquals("Customs Status", "", header.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("1. Link to move detail", ZGuid.Empty, movementDetail.B9_B9_InBondMoveDetail);
				AssertEquals("1. Sequence number", "1", movementDetail.B9_SeqNo);
				AssertEquals("1. Gross mass", new ZDecimal(1000), bill.B0_Weight);
				AssertEquals("1. Gross mass unit", "KG", bill.B0_WeightUQ);
				AssertEquals("1. Security indicator", false, bill.B0_SecurityIndicatorFromExport);

				var consignor = movementDetail.ConsignorDocAddress;
				AssertEquals("1. Consignor Identification number", "BE0452806999", consignor.E2_GovRegNum);
				AssertEquals("1. Consignor Name", "Jordens", consignor.E2_CompanyName);
				AssertEquals("1. Consignor Street and number", "Blijkerijstraat 14", consignor.Address1);
				AssertEquals("1. Consignor Postcode", "2390", consignor.Postcode);
				AssertEquals("1. Consignor City", "Oostmalle", consignor.City);
				AssertEquals("1. Consignor Country", "BE", consignor.Country.Code);

				var consignee = movementDetail.ConsigneeDocAddress;
				AssertEquals("1. Consignee Identification number", "BE0452806814", consignee.E2_GovRegNum);
				AssertEquals("1. Consignee Name", "Intris", consignee.E2_CompanyName);
				AssertEquals("1. Consignee Street and number", "Wapenstilstandlaan 47", consignee.Address1);
				AssertEquals("1. Consignee Postcode", "2600", consignee.Postcode);
				AssertEquals("1. Consignee City", "Berchem", consignee.City);
				AssertEquals("1. Consignee Country", "BE", consignee.Country.Code);

				AssertEquals("1. Number of transport means", 2, bill.ArrivalTransportInfos.Count);
				var departureTransportMeans1 = bill.ArrivalTransportInfos.First();
				AssertEquals("1. Transport state of the first transport means", "DEC", departureTransportMeans1.TPM_TransportState);
				AssertEquals("1. Sequence number of the first transport means", new ZShort(1), departureTransportMeans1.TPM_SequenceNumber);
				AssertEquals("1. Identification type of the first transport means", "99", departureTransportMeans1.TPM_TypeOfIdentification);
				AssertEquals("1. Identification number of the first transport means", "Trailer 784p", departureTransportMeans1.TPM_IdentificationNumber);
				AssertEquals("1. Nationality of the first transport means", "DE", departureTransportMeans1.TPM_RN_NKTransportNationality);

				var departureTransportMeans2 = bill.ArrivalTransportInfos.Last();
				AssertEquals("1. Transport state of the second transport means", "DEC", departureTransportMeans2.TPM_TransportState);
				AssertEquals("1. Sequence number of the second transport means", new ZShort(2), departureTransportMeans2.TPM_SequenceNumber);
				AssertEquals("1. Identification type of the second transport means", "10", departureTransportMeans2.TPM_TypeOfIdentification);
				AssertEquals("1. Identification number of the second transport means", "Truck 553", departureTransportMeans2.TPM_IdentificationNumber);
				AssertEquals("1. Nationality of the second transport means", "BE", departureTransportMeans2.TPM_RN_NKTransportNationality);

				AssertEquals("1. Number of previous documents", 2, bill.PreviousDocuments.Count);
				var previousDocument1 = (CusSupportingInfo)bill.PreviousDocuments.First();
				AssertEquals("1. Sequence number of the first previous document", 1, previousDocument1.CSI_LineNo);
				AssertEquals("1. Type of the first previous document", "N705", previousDocument1.CSI_Code);
				AssertEquals("1. Reference number of the first previous document", "waybill 7895666", previousDocument1.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the first previous document", "no further info for house consignment", previousDocument1.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the first previous document", "DEC", previousDocument1.CSI_Status);

				var previousDocument2 = (CusSupportingInfo)bill.PreviousDocuments.Last();
				AssertEquals("1. Sequence number of the second previous document", 2, previousDocument2.CSI_LineNo);
				AssertEquals("1. Type of the second previous document", "N706", previousDocument2.CSI_Code);
				AssertEquals("1. Reference number of the second previous document", "waybill 12345", previousDocument2.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the second previous document", "there is further info for house consignment", previousDocument2.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the second previous document", "DEC", previousDocument2.CSI_Status);

				AssertEquals("1. Number of supporting documents", 2, bill.SupportingDocuments.Count);
				var supportingDocument1 = (CusSupportingInfo)bill.SupportingDocuments.First();
				AssertEquals("1. Sequence number of the first supporting document", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("1. Type of the first supporting document", "N951", supportingDocument1.CSI_Code);
				AssertEquals("1. Reference number of the first supporting document", "TIF house", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the first supporting document", "test house supporting doc", supportingDocument1.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the first supporting document", "DEC", supportingDocument1.CSI_Status);

				var supportingDocument2 = (CusSupportingInfo)bill.SupportingDocuments.Last();
				AssertEquals("1. Sequence number of the second supporting document", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("1. Type of the second supporting document", "N952", supportingDocument2.CSI_Code);
				AssertEquals("1. Reference number of the second supporting document", "TIF away", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the second supporting document", "test away supporting doc", supportingDocument2.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the second supporting document", "DEC", supportingDocument2.CSI_Status);

				var transportDocuments = bill.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				var additionalInfos = bill.AdditionalDocuments.Where(x => x.CSI_SubType == "INF");
				var additionalReferences = bill.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");

				AssertEquals("1. Number of transport documents", 2, transportDocuments.Count());
				AssertEquals("1. Number of additional infos", 2, additionalInfos.Count());
				AssertEquals("1. Number of additional references", 2, additionalReferences.Count());

				var transportDocument1 = transportDocuments.First();
				AssertEquals("1. Sequence number of the first transport document", 1, transportDocument1.CSI_LineNo);
				AssertEquals("1. Type of the first transport document", "N271", transportDocument1.CSI_Code);
				AssertEquals("1. Reference number of the first transport document", "Packing 1", transportDocument1.CSI_ReferenceNumber);
				AssertEquals("1. Status of the first transport document", "DEC", transportDocument1.CSI_Status);

				var transportDocument2 = transportDocuments.Last();
				AssertEquals("1. Sequence number of the second transport document", 2, transportDocument2.CSI_LineNo);
				AssertEquals("1. Type of the second transport document", "N272", transportDocument2.CSI_Code);
				AssertEquals("1. Reference number of the second transport document", "Packing 2", transportDocument2.CSI_ReferenceNumber);
				AssertEquals("1. Status of the second transport document", "DEC", transportDocument2.CSI_Status);

				var additionalInfos1 = additionalInfos.First();
				AssertEquals("1. Sequence number of the first additional info", 1, additionalInfos1.CSI_LineNo);
				AssertEquals("1. Type of the first additional info", "20300", additionalInfos1.CSI_Code);
				AssertEquals("1. Description of the first additional info", "this house 1 does not exist in any export declaration", additionalInfos1.CSI_Description);
				AssertEquals("1. Status of the first additional info", "DEC", additionalInfos1.CSI_Status);

				var additionalInfos2 = additionalInfos.Last();
				AssertEquals("1. Sequence number of the second additional info", 2, additionalInfos2.CSI_LineNo);
				AssertEquals("1. Type of the second additional info", "20301", additionalInfos2.CSI_Code);
				AssertEquals("1. Description of the second additional info", "this house 2 does not exist in any export declaration", additionalInfos2.CSI_Description);
				AssertEquals("1. Status of the second additional info", "DEC", additionalInfos2.CSI_Status);

				var additionalReference1 = additionalReferences.First();
				AssertEquals("1. Sequence number of the first additional reference", 1, additionalReference1.CSI_LineNo);
				AssertEquals("1. Type of the first additional reference", "Y022", additionalReference1.CSI_Code);
				AssertEquals("1. Reference of the first additional reference", "AEO 478900 House 1", additionalReference1.CSI_ReferenceNumber);
				AssertEquals("1. Status of the first additional reference", "DEC", additionalReference1.CSI_Status);

				var additionalReference2 = additionalReferences.Last();
				AssertEquals("1. Sequence number of the second additional reference", 2, additionalReference2.CSI_LineNo);
				AssertEquals("1. Type of the second additional reference", "Y023", additionalReference2.CSI_Code);
				AssertEquals("1. Reference of the second additional reference", "AEO 478900 House 2", additionalReference2.CSI_ReferenceNumber);
				AssertEquals("1. Status of the second additional reference", "DEC", additionalReference2.CSI_Status);
			});
		}

		void Assert_ConsignmentItem(NctsHeader header)
		{
			var bill = header.Bills.Last();
			var goodItem = bill.ArrivalGoodsItems.Last();
			CombineAssertions(() =>
			{
				var consignee = goodItem.ConsigneeDocAddress;
				AssertEquals("1. Consignee", true, consignee.IsEmpty);

				AssertEquals("1. Number of previous documents", 2, goodItem.PreviousDocuments.Count);
				var previousDocument1 = (CusSupportingInfo)goodItem.PreviousDocuments.First();
				AssertEquals("1. Sequence number of the first previous document", 1, previousDocument1.CSI_LineNo);
				AssertEquals("1. Type of the first previous document", "N705", previousDocument1.CSI_Code);
				AssertEquals("1. Reference number of the first previous document", "waybill 7895666", previousDocument1.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the first previous document", "no further info for house consignment", previousDocument1.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the first previous document", "DEC", previousDocument1.CSI_Status);

				var previousDocument2 = (CusSupportingInfo)goodItem.PreviousDocuments.Last();
				AssertEquals("1. Sequence number of the second previous document", 2, previousDocument2.CSI_LineNo);
				AssertEquals("1. Type of the second previous document", "N706", previousDocument2.CSI_Code);
				AssertEquals("1. Reference number of the second previous document", "waybill 12345", previousDocument2.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the second previous document", "there is further info for house consignment", previousDocument2.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the second previous document", "DEC", previousDocument2.CSI_Status);

				AssertEquals("1. Number of supporting documents", 2, goodItem.SupportingDocuments.Count);
				var supportingDocument1 = (CusSupportingInfo)goodItem.SupportingDocuments.First();
				AssertEquals("1. Sequence number of the first supporting document", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("1. Type of the first supporting document", "N951", supportingDocument1.CSI_Code);
				AssertEquals("1. Reference number of the first supporting document", "TIF house", supportingDocument1.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the first supporting document", "test house supporting doc", supportingDocument1.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the first supporting document", "DEC", supportingDocument1.CSI_Status);

				var supportingDocument2 = (CusSupportingInfo)goodItem.SupportingDocuments.Last();
				AssertEquals("1. Sequence number of the second supporting document", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("1. Type of the second supporting document", "N952", supportingDocument2.CSI_Code);
				AssertEquals("1. Reference number of the second supporting document", "TIF away", supportingDocument2.CSI_ReferenceNumber);
				AssertEquals("1. Reference number 2 of the second supporting document", "test away supporting doc", supportingDocument2.CSI_ReferenceNumber2);
				AssertEquals("1. Status of the second supporting document", "DEC", supportingDocument2.CSI_Status);

				var transportDocuments = goodItem.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				var additionalInfos = goodItem.AdditionalInfos.Where(x => x.CSI_SubType == "INF");
				var additionalReferences = goodItem.AdditionalInfos.Where(x => x.CSI_SubType == "REF");

				AssertEquals("1. Number of transport documents", 2, transportDocuments.Count());
				AssertEquals("1. Number of additional infos", 2, additionalInfos.Count());
				AssertEquals("1. Number of additional references", 2, additionalReferences.Count());

				var transportDocument1 = transportDocuments.First();
				AssertEquals("1. Sequence number of the first transport document", 1, transportDocument1.CSI_LineNo);
				AssertEquals("1. Type of the first transport document", "N271", transportDocument1.CSI_Code);
				AssertEquals("1. Reference number of the first transport document", "Packing 1", transportDocument1.CSI_ReferenceNumber);
				AssertEquals("1. Status of the first transport document", "DEC", transportDocument1.CSI_Status);

				var transportDocument2 = transportDocuments.Last();
				AssertEquals("1. Sequence number of the second transport document", 2, transportDocument2.CSI_LineNo);
				AssertEquals("1. Type of the second transport document", "N272", transportDocument2.CSI_Code);
				AssertEquals("1. Reference number of the second transport document", "Packing 2", transportDocument2.CSI_ReferenceNumber);
				AssertEquals("1. Status of the second transport document", "DEC", transportDocument2.CSI_Status);

				var additionalInfos1 = additionalInfos.First();
				AssertEquals("1. Sequence number of the first additional info", 1, additionalInfos1.CSI_LineNo);
				AssertEquals("1. Type of the first additional info", "20300", additionalInfos1.CSI_Code);
				AssertEquals("1. Description of the first additional info", "this house 1 does not exist in any export declaration", additionalInfos1.CSI_Description);
				AssertEquals("1. Status of the first additional info", "DEC", additionalInfos1.CSI_Status);

				var additionalInfos2 = additionalInfos.Last();
				AssertEquals("1. Sequence number of the second additional info", 2, additionalInfos2.CSI_LineNo);
				AssertEquals("1. Type of the second additional info", "20301", additionalInfos2.CSI_Code);
				AssertEquals("1. Description of the second additional info", "this house 2 does not exist in any export declaration", additionalInfos2.CSI_Description);
				AssertEquals("1. Status of the second additional info", "DEC", additionalInfos2.CSI_Status);

				var additionalReference1 = additionalReferences.First();
				AssertEquals("1. Sequence number of the first additional reference", 1, additionalReference1.CSI_LineNo);
				AssertEquals("1. Type of the first additional reference", "Y022", additionalReference1.CSI_Code);
				AssertEquals("1. Reference of the first additional reference", "AEO 478900 House 1", additionalReference1.CSI_ReferenceNumber);
				AssertEquals("1. Status of the first additional reference", "DEC", additionalReference1.CSI_Status);

				var additionalReference2 = additionalReferences.Last();
				AssertEquals("1. Sequence number of the second additional reference", 2, additionalReference2.CSI_LineNo);
				AssertEquals("1. Type of the second additional reference", "Y023", additionalReference2.CSI_Code);
				AssertEquals("1. Reference of the second additional reference", "AEO 478900 House 2", additionalReference2.CSI_ReferenceNumber);
				AssertEquals("1. Status of the second additional reference", "DEC", additionalReference2.CSI_Status);

				var package = goodItem.Packages.First();
				AssertEquals(new ZShort(1), package.B5_SequenceNumber);
				AssertEquals("1", package.B5_UnitType);
				AssertEquals(1, package.B5_UnitCount);
				AssertEquals("marks", package.B5_MarksAndNumbers);
				AssertEquals(NctsUnloadedStateList.Codes.DEC, package.B5_TypeOfDifference);

				AssertEquals("Link container - Container Number", "MSCU1234567", package.ContainersSelected.First());

				AssertEquals("desc", goodItem.BY_Description);
				AssertEquals("cuscode", goodItem.BY_CusC4Number);
				AssertEquals("commcode", goodItem.BY_HarmonisedTariff);
				AssertEquals(new ZDecimal(200), goodItem.BY_GrossWeight);
				AssertEquals(new ZDecimal(150), goodItem.BY_NetWeight);

				var dangerous = goodItem.UNDGs.First();
				AssertEquals("X", dangerous.DI_DG_NKSubs);
			});
		}

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC043CAndIENCTS043CDataProvider> MockProvider => mockProvider ?? (mockProvider = new Mock<ICC043CAndIENCTS043CDataProvider>());
		Mock<ICC043CAndIENCTS043CDataProvider> mockProvider;

		protected override Mock<IENCTS043MessageProcessor> MockProcessor => mockProcessor ?? (mockProcessor = new Mock<IENCTS043MessageProcessor>(new LoggingInformation()));
		Mock<IENCTS043MessageProcessor> mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.IENCTS043;

		protected override string ExpectedMessageFriendlyName => BEIncomingMessageTypes.Descriptions.IENCTS043;

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NctsTransitStatusList.Codes.Unknown };

		protected override ZString[] PreProcessedOKPhase => new ZString[] { NctsMovementHeaderTransactionStatusList.Codes.Arrival };

		protected override ZString[] PreProcessOKMessageStatus => new ZString[] { LogicalStatusList.Codes.Sent };

		protected override Type ExpectedMessageInterpreterType => typeof(IENCTS043MessageInterpreter);

		protected override string MovementType => NctsMovementType.Codes.Arrival;

		protected override void SetUp()
		{
			base.SetUp();
			var consignmentProvider = ConsignmentXmlProvider.New(new ConsignmentType05());
			consignmentProvider.CountryOfDestination = "CH";
			consignmentProvider.InlandModeOfTransport = "1";
			consignmentProvider.GrossMass = ZDecimal.Zero;
			consignmentProvider.ContainerIndicator = ZBool.True;

			var consignorAddress = AddressXmlProvider.New(new AddressType07());
			consignorAddress.StreetAndNumber = "Blijkerijstraat 14";
			consignorAddress.Postcode = "2390";
			consignorAddress.City = "Oostmalle";
			consignorAddress.Country = "BE";

			var consigneeAddressXMLObj = new AddressType09()
			{
				StreetAndNumber = "Wapenstilstandlaan 47",
				Postcode = "2600",
				City = "Berchem",
				Country = "BE",
			};
			var consigneeAddress = AddressXmlProvider.New(consigneeAddressXMLObj);

			var consignor = PartyXmlProvider.New(new ConsigneeType04
			{
				IdentificationNumber = "BE0452806999",
				Name = "Jordens",
			});
			consignor.Address = consignorAddress;

			var consignee = PartyXmlProvider.New(new ConsignorType05
			{
				IdentificationNumber = "BE0452806814",
				Name = "Intris",
			});
			consignee.Address = consigneeAddress;

			consignmentProvider.Consignor = consignor;
			consignmentProvider.Consignee = consignee;

			var seal1 = new SealType04()
			{
				SequenceNumber = "1",
				Identifier = "seal1",
			};

			var seal2 = new SealType04()
			{
				SequenceNumber = "2",
				Identifier = "seal2",
			};
			var seal3 = new SealType04()
			{
				SequenceNumber = "3",
				Identifier = "seal3",
			};
			var goodreference1 = new GoodsReferenceType01()
			{
				SequenceNumber = "1",
				DeclarationGoodsItemNumber = "1",
			};
			var goodreference2 = new GoodsReferenceType01()
			{
				SequenceNumber = "2",
				DeclarationGoodsItemNumber = "3",
			};
			var transportEquipment1 = new TransportEquipmentType07()
			{
				SequenceNumber = "1",
				ContainerIdentificationNumber = "MSCU1234567",
				Seal = new Collection<SealType04> { seal1, seal2, seal3 },
				GoodsReference = new Collection<GoodsReferenceType01> { goodreference1, goodreference2 }
			};
			var containerisedTransportEquipmentProvider = TransportEquipmentXmlProvider.New(transportEquipment1);

			var nonContainerisedTransportEquipmentProvider = TransportEquipmentXmlProvider.New(new TransportEquipmentType07()
			{
				SequenceNumber = "2",
				Seal = new Collection<SealType04> { seal1 }
			});

			var transportEquipmentList = new Collection<TransportEquipmentXmlProvider> { containerisedTransportEquipmentProvider, nonContainerisedTransportEquipmentProvider };
			consignmentProvider.TransportEquipments = new ReadOnlyCollection<TransportEquipmentXmlProvider>(transportEquipmentList);

			var transportMeans1 = new DepartureTransportMeansType02()
			{
				SequenceNumber = "1",
				IdentificationNumber = "Trailer 784p",
				TypeOfIdentification = "99",
				Nationality = "DE",
			};
			var transportMeans2 = new DepartureTransportMeansType02()
			{
				SequenceNumber = "2",
				IdentificationNumber = "Truck 553",
				TypeOfIdentification = "10",
				Nationality = "BE",
			};
			var departureTransportMeans1 = TransportMeansXmlProvider.New(transportMeans1);
			var departureTransportMeans2 = TransportMeansXmlProvider.New(transportMeans2);

			consignmentProvider.TransportMeans = new Collection<TransportMeansXmlProvider> { departureTransportMeans1, departureTransportMeans2 };

			var previousDocumentXMLObj1 = new PreviousDocumentType04()
			{
				SequenceNumber = "1",
				Type = "N705",
				ReferenceNumber = "waybill 7895666",
				ComplementOfInformation = "no further info for house consignment",
			};
			var previousDocumentXMLObj2 = new PreviousDocumentType04()
			{
				SequenceNumber = "2",
				Type = "N706",
				ReferenceNumber = "waybill 12345",
				ComplementOfInformation = "there is further info for house consignment",
			};
			var previousDocumentXMLObj3 = new PreviousDocumentType07()
			{
				SequenceNumber = "1",
				Type = "N705",
				ReferenceNumber = "waybill 7895666",
				ComplementOfInformation = "no further info for house consignment",
			};
			var previousDocumentXMLObj4 = new PreviousDocumentType07()
			{
				SequenceNumber = "2",
				Type = "N706",
				ReferenceNumber = "waybill 12345",
				ComplementOfInformation = "there is further info for house consignment",
			};
			var previousDocument1 = DocumentXmlProvider.New(previousDocumentXMLObj1);
			var previousDocument2 = DocumentXmlProvider.New(previousDocumentXMLObj2);

			var supportingDocumentXMLObj1 = new SupportingDocumentType02()
			{
				SequenceNumber = "1",
				Type = "N951",
				ReferenceNumber = "TIF house",
				ComplementOfInformation = "test house supporting doc",
			};
			var supportingDocumentXMLObj2 = new SupportingDocumentType02()
			{
				SequenceNumber = "2",
				Type = "N952",
				ReferenceNumber = "TIF away",
				ComplementOfInformation = "test away supporting doc",
			};
			var supportingDocument1 = DocumentXmlProvider.New(supportingDocumentXMLObj1);
			var supportingDocument2 = DocumentXmlProvider.New(supportingDocumentXMLObj2);

			var transportDocumentXMLObj1 = new TransportDocumentType02()
			{
				SequenceNumber = "1",
				Type = "N271",
				ReferenceNumber = "Packing 1",
			};
			var transportDocumentXMLObj2 = new TransportDocumentType02()
			{
				SequenceNumber = "2",
				Type = "N272",
				ReferenceNumber = "Packing 2",
			};
			var transportDocument1 = DocumentXmlProvider.New(transportDocumentXMLObj1);
			var transportDocument2 = DocumentXmlProvider.New(transportDocumentXMLObj2);

			var additionalReferenceXMLObj1 = new AdditionalReferenceType02()
			{
				SequenceNumber = "1",
				Type = "Y022",
				ReferenceNumber = "AEO 478900 House 1",
			};
			var additionalReferenceXMLObj2 = new AdditionalReferenceType02()
			{
				SequenceNumber = "2",
				Type = "Y023",
				ReferenceNumber = "AEO 478900 House 2",
			};
			var additionalReferenceXMLObj3 = new AdditionalReferenceType03()
			{
				SequenceNumber = "1",
				Type = "Y022",
				ReferenceNumber = "AEO 478900 House 1",
			};
			var additionalReferenceXMLObj4 = new AdditionalReferenceType03()
			{
				SequenceNumber = "2",
				Type = "Y023",
				ReferenceNumber = "AEO 478900 House 2",
			};
			var additionalReference1 = DocumentXmlProvider.New(additionalReferenceXMLObj1);
			var additionalReference2 = DocumentXmlProvider.New(additionalReferenceXMLObj2);

			var additionalInformationXMLObj1 = new AdditionalInformationType02()
			{
				SequenceNumber = "1",
				Code = "20300",
				Text = "this house 1 does not exist in any export declaration",
			};
			var additionalInformationXMLObj2 = new AdditionalInformationType02()
			{
				SequenceNumber = "2",
				Code = "20301",
				Text = "this house 2 does not exist in any export declaration",
			};
			var additionalInformation1 = DocumentXmlProvider.New(additionalInformationXMLObj1);
			var additionalInformation2 = DocumentXmlProvider.New(additionalInformationXMLObj2);

			consignmentProvider.PreviousDocuments = new Collection<DocumentXmlProvider> { previousDocument1, previousDocument2 };
			consignmentProvider.SupportingDocuments = new Collection<DocumentXmlProvider> { supportingDocument1, supportingDocument2 };
			consignmentProvider.TransportDocuments = new Collection<DocumentXmlProvider> { transportDocument1, transportDocument2 };
			consignmentProvider.AdditionalReference = new Collection<DocumentXmlProvider> { additionalReference1, additionalReference2 };
			consignmentProvider.AdditionalInformation = new Collection<DocumentXmlProvider> { additionalInformation1, additionalInformation2 };

			var incidentProvider = IncidentXmlProvider.New(new IncidentType03()
			{
				TransportEquipment = new Collection<TransportEquipmentType07> { transportEquipment1 }
			});

			incidentProvider.Code = "1";
			incidentProvider.Text = "The incident is a consequence of another truck leaving a factory";
			incidentProvider.Endorsement = EndorsementXmlProvider.New(new EndorsementType03
			{
				Date = new DateTime(2022, 12, 05),
				Authority = "BE Customs",
				Place = "Antwerp",
				Country = "BE"
			});
			incidentProvider.Location = LocationXmlProvider.New(new LocationType02());
			incidentProvider.Location.QualifierOfIdentification = "Z";
			incidentProvider.Location.Country = "BE";
			incidentProvider.Location.Address = AddressXmlProvider.New(new AddressType07());
			incidentProvider.Location.Address.StreetAndNumber = "Michiganlaan 5";
			incidentProvider.Location.Address.Postcode = "2030";
			incidentProvider.Location.Address.City = "Antwerp";
			incidentProvider.Location.UNLocode = "BEANR";
			incidentProvider.Location.GNSS = GNSSXmlProvider.New(new GnssType());
			incidentProvider.Location.GNSS.Latitude = "51.1252563";
			incidentProvider.Location.GNSS.Longitude = "4.2563235";
			incidentProvider.Transhipment = TranshipmentXmlProvider.New(new TranshipmentType02());
			incidentProvider.Transhipment.TransportMeans = TransportMeansXmlProvider.New(new TransportMeansType02());
			incidentProvider.Transhipment.TransportMeans.IdentificationNumber = "Truck 12345";
			incidentProvider.Transhipment.TransportMeans.TypeOfIdentification = "30";
			incidentProvider.Transhipment.TransportMeans.Nationality = "NL";
			consignmentProvider.Incidents = new Collection<IncidentXmlProvider> { incidentProvider };
			var commodity = new CommodityType08();
			commodity.CommodityCode = new CommodityCodeType05();
			commodity.CommodityCode.HarmonizedSystemSubHeadingCode = "comm";
			commodity.CommodityCode.CombinedNomenclatureCode = "code";
			commodity.CusCode = "cuscode";
			commodity.DescriptionOfGoods = "desc";
			var dangerousGoods = new DangerousGoodsType01();
			dangerousGoods.SequenceNumber = "1";
			dangerousGoods.UnNumber = "X";
			commodity.DangerousGoods = new Collection<DangerousGoodsType01> { dangerousGoods };
			commodity.GoodsMeasure = new GoodsMeasureType03();
			commodity.GoodsMeasure.NetMass = 150;
			commodity.GoodsMeasure.GrossMass = 200;
			var commodityProvider = CargoWise.Customs.BE.MessageContracts.MessageProviders.CommodityProvider.New(commodity);
			var consignmentItem = new ConsignmentItemType04()
			{
				DeclarationGoodsItemNumber = "1",
				GoodsItemNumber = "1",
				CountryOfDestination = "BE",
				DeclarationType = "X",
				Packaging = new Collection<PackagingType02> { new PackagingType02 { NumberOfPackages = "1", SequenceNumber = "1", ShippingMarks = "marks", TypeOfPackages = "1" } },
				Consignee = new ConsigneeType03
				{
					IdentificationNumber = "BE0452806814",
					Name = "Intris",
					Address = consigneeAddressXMLObj,
				},
				Commodity = commodity,
				PreviousDocument = new Collection<PreviousDocumentType04> { previousDocumentXMLObj1, previousDocumentXMLObj2 },
				SupportingDocument = new Collection<SupportingDocumentType02> { supportingDocumentXMLObj1, supportingDocumentXMLObj2 },
				TransportDocument = new Collection<TransportDocumentType02> { transportDocumentXMLObj1, transportDocumentXMLObj2 },
				AdditionalInformation = new Collection<AdditionalInformationType02> { additionalInformationXMLObj1, additionalInformationXMLObj2 },
				AdditionalReference = new Collection<AdditionalReferenceType02> { additionalReferenceXMLObj1, additionalReferenceXMLObj2 }
			};

			var houseConsignmentXmlProvider1 = HouseConsignmentXmlProvider.New(new HouseConsignmentType04()
			{
				DepartureTransportMeans = new Collection<DepartureTransportMeansType02> { transportMeans1, transportMeans2 },
				PreviousDocument = new Collection<PreviousDocumentType07> { previousDocumentXMLObj3, previousDocumentXMLObj4 },
				SupportingDocument = new Collection<SupportingDocumentType02> { supportingDocumentXMLObj1, supportingDocumentXMLObj2 },
				TransportDocument = new Collection<TransportDocumentType02> { transportDocumentXMLObj1, transportDocumentXMLObj2 },
				AdditionalInformation = new Collection<AdditionalInformationType02> { additionalInformationXMLObj1, additionalInformationXMLObj2 },
				AdditionalReference = new Collection<AdditionalReferenceType03> { additionalReferenceXMLObj3, additionalReferenceXMLObj4 },
				ConsignmentItem = new Collection<ConsignmentItemType04> { consignmentItem }
			});
			houseConsignmentXmlProvider1.SequenceNumber = 1;
			houseConsignmentXmlProvider1.GrossMass = 1000;
			houseConsignmentXmlProvider1.SecurityIndicatorFromExportDeclaration = "0";
			houseConsignmentXmlProvider1.Consignor = consignor;
			houseConsignmentXmlProvider1.Consignee = consignee;
			consignmentProvider.HouseConsignments = new Collection<HouseConsignmentXmlProvider> { houseConsignmentXmlProvider1 };

			mockProvider.Setup(x => x.Consignment).Returns(consignmentProvider);
			mockProvider.Setup(x => x.DeclarationAcceptanceDate).Returns(new DateTime(2022, 4, 1, 12, 34, 56));
			mockProvider.Setup(x => x.DeclarationType).Returns("A");
			mockProvider.Setup(x => x.Security).Returns("NON");
			mockProvider.Setup(x => x.ReducedDatasetIndicator).Returns(ZBool.True);

			Factory.Save();
		}
	}

	class IENCTS043MessageProcessorForTest : IENCTS043MessageProcessor
	{
		public IENCTS043MessageProcessorForTest(LoggingInformation logger) : base(logger) { }

		public BusinessObject FindParentOfMessageExposed(BEMessage message, ICC043CAndIENCTS043CDataProvider messageDataProvider) => base.FindParentOfMessage(message, messageDataProvider);
	}
}
