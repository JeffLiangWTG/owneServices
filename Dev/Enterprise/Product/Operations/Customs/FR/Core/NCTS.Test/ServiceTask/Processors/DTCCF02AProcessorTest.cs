using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CCF02A;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using NctsMessageStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMessageStatusList;
using NctsMovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTCCF02AProcessorTest : DTBaseProcessorTest<Ccf02AType>
	{
		protected override IEnumerable<DTMessageProcessorTestCase> DTMessageProcessorTestCases
		{
			get
			{
				var expectedNewMessageInterpretation = @"
<p>(TOM)</p>
<p>AND</p>
<p>JERRY</p>
<p>New {1} status: {2}</p>
<p>New detailed {1} status: {0}</p>";
				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "ANTICIPEE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.DeclarationAccepted,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Anticipated,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Anticipated), "departure", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.DeclarationAccepted))
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "VALIDEE_ANTICIPEE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.DeclarationMrnAllocated))
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "DEMANDE_RECTIF"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.AmendmentRequestAcknowledge,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.AmendmentRequestAcknowledge), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "DEMANDE_INVALID"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.CancellationRequestAcknowledge,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.CancellationRequestAcknowledge), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "GARANTIE_SOUS_ENRG"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.PendingGuaranteesRegistration,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.PendingGuaranteesRegistration), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "ATTENTE_GARANTIE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.PendingGuarantee,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.PendingGuarantee), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "GARANTIE_INVALIDE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid))
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "NON_LIB_POUR_TRANS"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.GoodsNotReleasedForTransit,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.GoodsNotReleasedForTransit))
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "NOTIF_ARRIVEE_DEST"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "RECOUVR_RECOMMANDE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "RECHERCHE_ENGAGEE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "INFO_RECOUVREMENT"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "SOUS_CONTROLE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.GoodsUnderCustomsControl))
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "APUREE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "departure", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "AVIS_ANT_ARRIV_DEM"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = ZString.Empty,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.PreArrivalNotificationRequest,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.PreArrivalNotificationRequest), "arrival", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "LIBEREE_DEST"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = ZString.Empty,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.ReleasedAtDestination,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.ReleasedAtDestination), "arrival", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "ATTENTE_RESOL_DIFF"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = ZString.Empty,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "arrival", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "APUREE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = ZString.Empty,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "arrival", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "NOTIF_ARRIVEE_DEST"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = ZString.Empty,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.ArrivalNotification,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.ArrivalNotification), "arrival", "Unknown")
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Arrival,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "SOUS_CONTROLE"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewArrivalStatus = NctsTransitStatusList.Codes.GoodsUnderCustomsControl,
					ExpectedNewDetailedArrivalStatus = NctsDetailedStatusList.Codes.Unknown,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.Unknown), "arrival", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.GoodsUnderCustomsControl))
				};

				yield return new DTMessageProcessorTestCase
				{
					SetUpHeader = header => header.BH_HeaderType = NctsMovementType.Codes.Departure,
					IncomingMessageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "NOTIF_EMBARQUEMENT"),
					ExpectedNewMessageStatus = NctsMessageStatusList.Codes.Ok,
					ExpectedNewDepartureStatus = ZString.Empty,
					ExpectedNewDetailedDepartureStatus = NctsDetailedStatusList.Codes.BoardingNotification,
					ExpectedNewMessageInterpretation = string.Format(expectedNewMessageInterpretation, new NctsDetailedStatusList().GetDescriptionFromCode(NctsDetailedStatusList.Codes.BoardingNotification), "departure", new NctsTransitStatusList().GetDescriptionFromCode(NctsTransitStatusList.Codes.Unknown))
				};
			}
		}

		public void TestStaNOT2Anticipee_FrontiereIntelligente_TadWithWatermarkCreated()
		{
			// Coming from country code GB, IE and XI.
			// Departure or Transit office has attribute IsIntelligentBorder.
			// Transport mode is Truck or Sea.

			using (MockProductEnvironment())
			{
				var messageText = frNctsReponseHelper.GetEmbeddedResourceFile("DTF02A_MESSAGE_TEMPLATE.xml").Replace("{StaNOT2}", "ANTICIPEE");

				var header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_1");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				header.PortOfDispatch = "GB101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, true);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_1");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
				header.PortOfDispatch = "GB101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, false);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_1");
				header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				header.ArrivalMovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
				header.PortOfDispatch = "GB101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, false);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_2");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "GB101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, true);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_3");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "IE101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, true);

				var northernIreland = CreateNewOrGetExistingRefCountryStates("HH", RefUNLOCO.Regions.NorthernIreland, "AAA", Core.Constants.CountryCodes.UnitedKingdom);
				CreateNewOrGetExistingRefUNLOCO("XI102", "GB").RL_RW = northernIreland.PK;
				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_4");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "XI102";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, true);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_5");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "GB101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, true);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_6");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "GB102";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "FR202", ZDateTime.Empty);
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "FR203", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR202", "BBBB" } });
				ActAndAssert(header, true);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_5");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "FR101";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR201", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR201", "AAAA" } });
				ActAndAssert(header, false);

				header = frNctsReponseHelper.SetupMessagesForTest(Factory, NctsMovementType.Codes.Departure, messageText, initialDeclarationStatus, true, NctsMessageStatusList.Codes.DepartureDeclarationSent, jobNo: "BH_6");
				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				header.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				header.PortOfDispatch = "FR102";
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "FR202", ZDateTime.Empty);
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "FR203", ZDateTime.Empty);
				CreateIntelligentBorders(new Dictionary<string, string> { { "FR202", "BBBB" } });
				ActAndAssert(header, false);
			}

			IDisposable MockProductEnvironment()
			{
				var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var savedDbType = keyForTest.DatabaseTypeForTest;
				keyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				return new DisposableAction(() =>
				{
					keyForTest.DatabaseTypeForTest = savedDbType;
				});
			}

			void ActAndAssert(NctsHeader header, bool expectPrinted)
			{
				Factory.Save();
				var serviceLogger = new TestServiceLogger();
				var processor = new FRNctsResponseMessageProcessor(serviceLogger);

				var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
				AssertNull("Prerequisite: No print jobs are created for entry.", printJob);

				processor.ExecuteBatch();

				var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, header.PK));
				AssertEquals("There should be a print job created for entry.", expectPrinted ? 1 : 0, printJobs.Length);

				if (expectPrinted)
				{
					AssertContains("Provisoire - Temporary", printJobs[0].SP_WatermarkText);
				}
			}

			RefUNLOCO CreateNewOrGetExistingRefUNLOCO(ZString code, ZString countryCode)
			{
				var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
				if (result == null)
				{
					result = Factory.New<RefUNLOCO>();
					result.RL_Code = code;
					result.RL_RN_NKCountryCode = countryCode;
				}
				return result;
			}

			RefCountryStates CreateNewOrGetExistingRefCountryStates(ZString code, ZString regionName, ZString description, ZString countryCode)
			{
				var result = new RefCountryStates.Loader(Factory).LoadRefCountryStatesFromCode(code, countryCode);
				if (result == null)
				{
					result = Factory.NewWithValidTestData<RefCountryStates>();
					result.RW_Code = code;
					result.RW_RegionName = regionName;
					result.RW_Description = description;
					result.RW_RN_NKCountryCode = countryCode;
				}
				return result;
			}

			void CreateIntelligentBorders(Dictionary<string, string> codeAndDescriptions)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
				foreach (var codeAndDescription in codeAndDescriptions)
				{
					var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, codeAndDescription.Key, codeAndDescription.Value, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, "IsIntelligentBorder", "Y");
				}
			}
		}
	}
}
