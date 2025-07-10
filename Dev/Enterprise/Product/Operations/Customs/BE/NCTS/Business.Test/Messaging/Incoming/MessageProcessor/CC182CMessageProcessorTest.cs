using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC182CMessageProcessor))]
	sealed class CC182CMessageProcessorTest : NctsMessageProcessorTestCase<CC182CMessageProcessor, ICC182CDataProvider>
	{
		public void TestValidateC182C_MatchingMRN()
		{
			mockProvider.Setup(x => x.MRN).Returns("22BE04526138290000001");
			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions(() =>
			{
				AssertEquals($"EDIMessage status should be {EDIMessageStatusList.Codes.Discarded}", EDIMessageStatusList.Codes.Discarded, incomingMessage.EM_Status);
				var note = incomingMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ProcessingLog).Single();
				AssertEquals("Note text", "The message with interchange was discarded, because its 'Status at Customs' has already the status WRO.", note.ST_NoteText);
			});
		}

		public void TestIncidentNotCreatedWhenOneExists()
		{
			var existingIncident = header.EnRouteIncidents.AddNew();
			existingIncident.BN_IncidentCode = "1";
			existingIncident.BN_Information = "This is an existing incident";
			existingIncident.BN_EndorsementDate = new ZDateTime(2022, 12, 05);
			existingIncident.BN_EndorsementAuthority = "BE Customs";
			existingIncident.BN_EndorsementPlace = "Antwerp";
			existingIncident.BN_EndorsementCountryCode = "BE";

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var incident = header.EnRouteIncidents.Single();
			CombineAssertions(() =>
			{
				AssertEquals("A new incident is not created when there's an existing incident", existingIncident.PK, incident.PK);
				AssertEquals("Existing incident not updated", "This is an existing incident", incident.BN_Information);
			});
		}

		public void TestContainersNotCreatedWhenContainerIdentificationNumberIsEmpty()
		{
			transportEquipmentProvider.ContainerIdentificationNumber = "";
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var incident = header.EnRouteIncidents.First();
			AssertEquals("No containers created", 0, incident.IncidentContainers.Count);
		}

		public void TestSealsCreatedForIncident()
		{
			transportEquipmentProvider.ContainerIdentificationNumber = "";
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var incident = header.EnRouteIncidents.First();
			AssertContainsExactElementsInAnyOrder(new[] { "1 - seal1", "2 - seal2", "3 - seal3" },
				incident.Seals.Select(x => $"{x.BK_SequenceNumber} - {x.BK_SealNumber}"));
		}

		public void TestProcessMessage()
		{
			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			Factory.Save();

			var incident = header.EnRouteIncidents.First();
			CombineAssertions(() =>
			{
				var movementHeader = header.MovementHeader;
				AssertEquals("Message status", LogicalStatusList.Codes.Accepted, header.EffectiveMessageStatus);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
				AssertEquals("Phase", NctsMovementHeaderTransactionStatusList.Codes.Declaration, movementHeader.BM_Phase);
				AssertEquals("Customs Status", NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, movementHeader.BM_CustomsStatus);
				AssertEquals("Incident Flag", YesNoList.Codes.Yes, header.BH_ExportFlag);
				AssertEquals("N° Incidents", 1, header.EnRouteIncidents.Count);
				AssertEquals("Incident Code", "1", incident.BN_IncidentCode);
				AssertEquals("Incident Information", "The incident is a consequence of another truck leaving a factory", incident.BN_Information);
				AssertEquals("Endorsement Date", new DateTime(2022, 12, 05), incident.BN_EndorsementDate);
				AssertEquals("Endorsement Authority", "BE Customs", incident.BN_EndorsementAuthority);
				AssertEquals("Endorsement Place", "Antwerp", incident.BN_EndorsementPlace);
				AssertEquals("Endorsement Country Code", "BE", incident.BN_EndorsementCountryCode);
				AssertEquals("Location Qualifier", "Z", incident.BN_LocationQualifier);
				AssertEquals("Geolocation", ZGeography.Empty, incident.BN_GeoLocation);
				AssertEquals("Transport type at Departure", "30", incident.BN_TransportAtDepartureType);
				AssertEquals("Transport ID at Departure", "Truck 12345", incident.BN_TransportAtDepartureID);
				AssertEquals("Nationality at departure", "NL", incident.BN_RN_NKTransportAtDepartureIDNationality);
				AssertEquals("Goods Location Address", "Michiganlaan 5", incident.GoodsLocation.Address.Address1);
				AssertEquals("Goods Location Postcode", "2030", incident.GoodsLocation.Address.E2_Postcode);
				AssertEquals("Goods Location City", "Antwerp", incident.GoodsLocation.Address.City);

				var container = incident.IncidentContainers.First();
				AssertEquals("Container Sequence", (ZShort)1, container.BC_SequenceNumber);
				AssertEquals("Container Number", "MSCU1234567", container.BC_ContainerNum);
				AssertEquals("N° Seals", 3, container.TotalSealCount);
				AssertEquals("First Seal Number", "seal1", container.BC_Seal1);
				AssertEquals("Second Seal Number", "seal2", container.BC_Seal2);
				AssertContainsExactElementsInAnyOrder("Additional Seal Numbers", new[] { "seal3" }, container.Seals.Select(x => x.BK_SealNumber));
				AssertEquals("Seals not created for incident", 0, incident.Seals.Count);
				AssertContainsExactElementsInAnyOrder("GoodsReferences", new[] { "1 - 1", "2 - 3" }, container.ItemNumbers.Select(x => $"{x.CY_Order} - {x.CY_Data}"));
				AssertEquals("NctsHeader Logs", 2, header.Logs.GetAllLogs().Count);
				AssertEquals("MovementHeader Logs", 4, movementHeader.Logs.GetAllLogs().Count);
				AssertEquals("Incoming Message Interpretation", "Phase: FIN - Forwarded incident notification<br />Status granted on: 04/12/2022 15:26:39<br />", incomingMessage.EM_MessageInterpretation);

				var log = movementHeader.Logs.Find(x => x.SL_SE_NKEvent == "CES").Single();
				AssertEquals("Only 1 CES-event should have been created", 1, movementHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.CustomsEntryStatus.Code && x.SL_Reference == movementHeader.BM_CustomsStatus).Count());
			});
		}

		protected override string MovementType => NctsMovementType.Codes.Departure;

		protected override bool SupportsSearchByLRN => false;

		protected override bool SupportsSearchByMRN => true;

		protected override Mock<ICC182CDataProvider> MockProvider => mockProvider;

		protected override Mock<CC182CMessageProcessor> MockProcessor => mockProcessor;

		protected override ZString MessageTypeToInclude => BEIncomingMessageTypes.Codes.CC182C;

		protected override string ExpectedMessageFriendlyName => "Forwarded Incident Notification";

		protected override ZString[] PreProcessedOKCustomsStatus => new ZString[] { NCTS5DepartureCustomsStatusList.Codes.MrnAllocated };

		protected override Type ExpectedMessageInterpreterType => typeof(CC182CMessageInterpreter);

		protected override void SetUp()
		{
			mockProvider = new Mock<ICC182CDataProvider>();
			mockProvider.Setup(x => x.MRN).Returns("22BE04526138290000001");
			mockProvider.Setup(x => x.IncidentDateAndTime).Returns(new DateTime(2022, 12, 4, 15, 26, 39));
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
			var transportEquipmentXMLObj = new TransportEquipmentType07
			{
				SequenceNumber = "1",
				ContainerIdentificationNumber = "MSCU1234567",
				Seal = new Collection<SealType04> { seal1, seal2, seal3 },
				GoodsReference = new Collection<GoodsReferenceType01> { goodreference1, goodreference2 },
			};
			var incidentProvider = IncidentXmlProvider.New(new IncidentType03
			{
				Code = "1",
				Text = "The incident is a consequence of another truck leaving a factory",
				Endorsement = new EndorsementType03
				{
					Date = new DateTime(2022, 12, 05),
					Authority = "BE Customs",
					Place = "Antwerp",
					Country = "BE"
				},
				TransportEquipment = new Collection<TransportEquipmentType07> { transportEquipmentXMLObj },
			});
			incidentProvider.Location = LocationXmlProvider.New(new LocationType02());
			incidentProvider.Location.QualifierOfIdentification = "Z";
			incidentProvider.Location.Country = "BE";
			incidentProvider.Location.Address = AddressXmlProvider.New(new AddressType07());
			incidentProvider.Location.Address.StreetAndNumber = "Michiganlaan 5";
			incidentProvider.Location.Address.Postcode = "2030";
			incidentProvider.Location.Address.City = "Antwerp";
			incidentProvider.Transhipment = TranshipmentXmlProvider.New(new TranshipmentType02());
			incidentProvider.Transhipment.TransportMeans = TransportMeansXmlProvider.New(new TransportMeansType02());
			incidentProvider.Transhipment.TransportMeans.IdentificationNumber = "Truck 12345";
			incidentProvider.Transhipment.TransportMeans.TypeOfIdentification = "30";
			incidentProvider.Transhipment.TransportMeans.Nationality = "NL";

			transportEquipmentProvider = incidentProvider.TransportEquipments.First();

			var incidentList = new Collection<IncidentXmlProvider> { incidentProvider };
			mockProvider.Setup(x => x.Incidents).Returns(new ReadOnlyCollection<IncidentXmlProvider>(incidentList));
			mockProcessor = new Mock<CC182CMessageProcessor>(new BatchProcessor.LoggingInformation());

			header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.Company.Country.Code).CE_EntryNum = "22BE04526138290000001";

			base.SetUp();

			Factory.Save();
		}

		Mock<ICC182CDataProvider> mockProvider;
		Mock<CC182CMessageProcessor> mockProcessor;
		TransportEquipmentXmlProvider transportEquipmentProvider;
		NctsHeader header;
	}
}
