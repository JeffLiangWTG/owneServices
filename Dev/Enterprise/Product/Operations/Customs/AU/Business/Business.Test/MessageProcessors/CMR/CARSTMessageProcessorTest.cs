using System;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.MessageProcessors.Testing
{
	sealed class CARSTMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestStatusEventsSet()
		{
			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentBranch.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.LCL;
			Transport transport = consol.Transports[0];
			transport.JW_JX = CreateSailing(TestVesselLloyds, TestVoyageNumber).PK;
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = TestContainerNumber;
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = TestHouseBillNumber;

			CusOutturnHeader header = Factory.New<CusOutturnHeader>();
			header.C6_LloydsIMO = TestVesselLloyds;
			header.C6_VoyageNum = TestVoyageNumber;
			header.C6_OutturningPremiseID = "9914N";

			CusUnderbond expectedCargoArrival = Factory.New<CusUnderbond>();
			expectedCargoArrival.C4_ParentID = container.PK;
			expectedCargoArrival.C4_ParentTableCode = JobContainerSchema.Constants.Prefix;
			expectedCargoArrival.C4_DestinationPremiseID = GlbBranch.CurrentBranch.OrgProxy.MainAddress.LocalControlledPremisesID;
			expectedCargoArrival.C4_ModeOfMovement = Enterprise.Core.Constants.TransportModes.Road;
			expectedCargoArrival.C4_C6 = header.PK;

			Factory.Save();
			processor.ProcessMessage(TestCargoStatusMessage);
			AssertEquals(EDIMessage.Status.Received, TestCargoStatusMessage.EM_Status);
			StmALog lastStatusAdviceEvent = shipment.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestCargoLineStatus()
		{
			const string cargoListLineStatusMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+231J I994 1DB5:1+8'
DTM+9:20050831154540243568:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:N/A'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040H0001::1'
RFF+AAQ:TRCU3382910'
RFF+ACC:E'
DOC+1'
PAC+++FCL:67:95'
UNT+16+000001'";

			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			currentBranch.OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
			currentBranch.OrgProxy.OH_IsSeaCTO = true;

			BillOfLading billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_NKLoadPort = "AUSYD";
			billOfLading1.JS_NKDischargePort = "AUPER";
			billOfLading1.JS_HouseBill = "OB111"; // RFF+MB

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUPER";

			billOfLading1.JS_JX = voyage.Sailings[0].PK;

			CusSeaManTranHead importManifest = Factory.New<CusSeaManTranHead>(); // TDT+
			importManifest.BT_VoyageNum = "936";
			importManifest.BT_LloydsIMO = "8811924";

			CusSeaManOBLHeader oceanBill1 = importManifest.OceanBills.AddNew();
			oceanBill1.BO_OceanBill = "OB111";  // RFF+MB
			oceanBill1.BO_RL_NKLoadPort = "AUSYD";
			oceanBill1.BO_RL_NKDischargePort = "AUPER";
			var bill1Detail = oceanBill1.Details.AddNew();
			bill1Detail.BD_ContainerNumber = "112233";
			Factory.Save();

			CusSeaManArrivalPort arrivalPort = importManifest.Arrivals.AddNew();
			arrivalPort.BA_SendersMessageReference = "L5040040H0001";  // RFF+ABO (generated)

			CusSeaManOBLHeaderCargoLine cargoLine1 = arrivalPort.CargoLines.AddNew();
			var cargoLineDetail1 = cargoLine1.Details.AddNew();
			cargoLineDetail1.BD_ContainerNumber = "TRCU000001";

			CusSeaManOBLHeaderCargoLine cargoLine2 = arrivalPort.CargoLines.AddNew();
			CusSeaManOBLDetailCargoLine cargoLineDetail2 = cargoLine2.Details.AddNew();
			cargoLineDetail2.BD_ContainerNumber = "TRCU3382910";  // RFF+AAQ

			CusSeaManOBLHeaderCargoLine cargoLine3 = arrivalPort.CargoLines.AddNew();
			var cargoLineDetail3 = cargoLine3.Details.AddNew();
			cargoLineDetail3.BD_ContainerNumber = "TRCU0000003";

			Factory.Save();

			AssertEquals(cargoLine1.BO_BA, arrivalPort.PK);
			AssertEquals(cargoLine2.BO_BA, arrivalPort.PK);
			AssertEquals(cargoLine3.BO_BA, arrivalPort.PK);
			AssertEquals(cargoLine1.BO_BT, importManifest.PK);
			AssertEquals(cargoLine2.BO_BT, importManifest.PK);
			AssertEquals(cargoLine3.BO_BT, importManifest.PK);

			var testCargoStatusMessage = Factory.New<CMRCARSTMessage>();
			testCargoStatusMessage.EM_MessageText = cargoListLineStatusMessageText.Replace("\r\n", "");

			processor.ProcessMessage(testCargoStatusMessage);
			AssertEquals(EDIMessage.Status.Received, testCargoStatusMessage.EM_Status);

			AssertEquals("message is linked to line from the given senders reference and the container number", cargoLine2, testCargoStatusMessage.EM_LinkedObject);
			AssertEquals("Is set when senders reference and container are supplied", "CLR", cargoLine2.ShipmentStatus.Code); // FTX+AHN+++CONSOLIDATED STATUS
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestCS00405607()
		{
			#region Message Text
			var outgoingMessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+933:::SEACR+S00048638/CMT1:1+9
RFF+PQ:CC
RFF+BH:SHA11
RFF+MB:SHA1
NAD+CN++AMY TEST::123 TEST STREET  SYDNEY 2000 AU
NAD+CZ++ASUS COMPUTER CO LTD::8,168 MEISHENG ROAD  WAI GOA QIAO 2:00131 CN
NAD+VW+41065894724::95
NAD+AH+41065894724::95
TDT+20+2PW++11++++9044748::11
LOC+8+AUSYD::6
LOC+76+CNSHA::6
LOC+12+AUSYD::6
LOC+73+CNSHA::6
LOC+27+CN::5
CNI++:::I
RFF+AAQ:PWSU5577994
GID+1
RFF+ACC:2008
GID+1
RFF+SN:G
GID+1
PAC+1
PAC+++FCL:67:95
PAC+++GENN:121:95
PAC+++PF:185:95
FTX+AAA+++STUFF
MEA+AAE+AAL+KG:1.00
MEA+AAE+G+KG:1.00
MEA+AAE+ABJ+CU:1.00
PCI+28+NM
UNT+32+1
";
			var incomingSCRMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1G4F I64E G906:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:S00048638/CMT1::001'
DTM+310:20160301061858:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'
";
			var incomingCRSMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1ICJ ACJG 906:1+8'
DTM+9:20160301175102888071:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+2PW++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:S00048638/CMT1::1'
RFF+MB:SHA1'
RFF+BH:SHA11'
RFF+AAQ:PWSU5577994'
DOC+1'
PAC+++FCL:67:95'
UNT+17+000001'
";

			#endregion

			var user = Factory.New<GlbStaff>();
			user.GS_LoginName = "TEST USER";
			user.GS_Code = "TSU";
			user.GS_FullName = "TSU";
			user.GS_EmailAddress = "tsu@abc.com";

			Env.Registry.AUCustoms.CargoStatusSendAcknowledgements = Enterprise.Core.Constants.EmailTo.StaffMember;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "SHA1";

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "AALSMEERGRACHT";
			refVessel.RV_LloydsNumber = "9044748";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "2PW";
			transport.JW_Vessel = "AALSMEERGRACHT";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PWSU5577994";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00048638";
			shipment.JS_HouseBill = "SHA11";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			var cargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = cargoSynchroniser.OceanBill;
			oceanBill.CB_PrincipalID = "41065894724";
			oceanBill.CB_ResponsiblePartyID = "41065894724";

			var houseBill = cargoSynchroniser.GetHouseBill(shipment);
			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var outgoingCargoMessage = houseBill.Messages.AddNew(typeof(CMRSEACRRMessage));
			outgoingCargoMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingCargoMessage.EM_MessageType = EDIMessage.ApplicationCodes.CMR;
			outgoingCargoMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingCargoMessage.EM_MessageText = outgoingMessageText.Replace("\r\n", "");
			outgoingCargoMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingCargoMessage.EM_SystemCreateUser = "TSU";

			var incomingSCRMessage = Factory.New<CMRSEACRRMessage>();
			incomingSCRMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingSCRMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingSCRMessage.EM_MessageText = incomingSCRMessageText.Replace("\r\n", "");
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			processor.ProcessMessage(incomingSCRMessage);
			AssertEquals(EDIMessage.Status.Received, incomingSCRMessage.EM_Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Sea Cargo Report - SCR message notification", "Cargo Status Advice - (CARST) Message for Ocean Bill: SHA1 - ORIGINAL ACCEPTED", Env.OutgoingMailManager.EmailsCreated[0].Subject);

			var incomingCRSMesage = Factory.New<CMRCARSTMessage>();
			incomingCRSMesage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingCRSMesage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingCRSMesage.EM_MessageText = incomingCRSMessageText.Replace("\r\n", "");
			Factory.Save();

			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			processor.ProcessMessage(incomingCRSMesage);
			AssertEquals(EDIMessage.Status.Received, incomingCRSMesage.EM_Status);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Sea Cargo Report - CRS message notification", "Cargo Status Advice - (CARST) Message for Ocean Bill: SHA1 - CONSOLIDATED STATUS", Env.OutgoingMailManager.EmailsCreated[1].Subject);
		}

		public void TestMultipleBillsAndContainersPerformance()
		{
			// structure of this test is the same as CARSTMessageProcessorTest.TestCS00405607 but without email sending

			#region Message Text

			var outgoingMessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+933:::SEACR+S00048638/CMT1:1+9
RFF+PQ:CC
RFF+BH:SHA11
RFF+MB:SHA1
NAD+CN++AMY TEST::123 TEST STREET  SYDNEY 2000 AU
NAD+CZ++ASUS COMPUTER CO LTD::8,168 MEISHENG ROAD  WAI GOA QIAO 2:00131 CN
NAD+VW+41065894724::95
NAD+AH+41065894724::95
TDT+20+2PW++11++++9044748::11
LOC+8+AUSYD::6
LOC+76+CNSHA::6
LOC+12+AUSYD::6
LOC+73+CNSHA::6
LOC+27+CN::5
CNI++:::I
RFF+AAQ:PWSU5577994
GID+1
RFF+ACC:2008
GID+1
RFF+SN:G
GID+1
PAC+1
PAC+++FCL:67:95
PAC+++GENN:121:95
PAC+++PF:185:95
FTX+AAA+++STUFF
MEA+AAE+AAL+KG:1.00
MEA+AAE+G+KG:1.00
MEA+AAE+ABJ+CU:1.00
PCI+28+NM
UNT+32+1
";
			var incomingSCRMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEACRR+1G4F I64E G906:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:SEACR'
RFF+AFM:9'
RFF+ABO:S00048638/CMT1::001'
DTM+310:20160301061858:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'
";
			var incomingCRSMessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+1ICJ ACJG 906:1+8'
DTM+9:20160301175102888071:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+2PW++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:S00048638/CMT1::1'
RFF+MB:SHA1'
RFF+BH:SHA11'
RFF+AAQ:PWSU5577994'
DOC+1'
PAC+++FCL:67:95'
UNT+17+000001'
";

			#endregion

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "SHA1";

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Code = "AALSMEERGRACHT";
			refVessel.RV_LloydsNumber = "9044748";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "2PW";
			transport.JW_Vessel = "AALSMEERGRACHT";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PWSU5577994";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00048638";
			shipment.JS_HouseBill = "SHA11";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			var cargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = cargoSynchroniser.OceanBill;
			oceanBill.CB_PrincipalID = "41065894724";
			oceanBill.CB_ResponsiblePartyID = "41065894724";

			var houseBill = cargoSynchroniser.GetHouseBill(shipment);
			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var outgoingCargoMessage = houseBill.Messages.AddNew(typeof(CMRSEACRRMessage));
			outgoingCargoMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingCargoMessage.EM_MessageType = EDIMessage.ApplicationCodes.CMR;
			outgoingCargoMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingCargoMessage.EM_MessageText = outgoingMessageText.Replace("\r\n", "");
			outgoingCargoMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingCargoMessage.EM_SystemCreateUser = "TSU";

			// link to multiple containers
			{
				const int houseBillsCount = 5;
				const int containersCount = 500;
				const int pivotsPerContainer = 2;

				var houseBills = new CusSCAHouse[houseBillsCount];
				for (int i = 0; i < houseBillsCount; i++)
				{
					houseBills[i] = oceanBill.HouseBills.AddNew();
					houseBills[i].CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
				}

				for (int i = 0; i < containersCount; i++)
				{
					CusSCAContainer cont = oceanBill.Containers.AddNew();
					for (int j = 0; j < pivotsPerContainer; j++)
					{
						var pivot = houseBills[(i + j) % houseBillsCount].Pivot.AddNew();
						pivot.CV_CN = cont.PK;
					}
				}
			}

			var incomingSCRMessage = Factory.New<CMRSEACRRMessage>();
			incomingSCRMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingSCRMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingSCRMessage.EM_MessageText = incomingSCRMessageText.Replace("\r\n", "");
			Factory.Save();

			var sw = Stopwatch.StartNew();
			processor.ProcessMessage(incomingSCRMessage);
			AssertLessThan(sw.ElapsedMilliseconds, 10_000);

			AssertEquals(EDIMessage.Status.Received, incomingSCRMessage.EM_Status);

			var incomingCRSMesage = Factory.New<CMRCARSTMessage>();
			incomingCRSMesage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingCRSMesage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingCRSMesage.EM_MessageText = incomingCRSMessageText.Replace("\r\n", "");
			Factory.Save();

			sw.Restart();

			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			processor.ProcessMessage(incomingCRSMesage);
			AssertLessThan(sw.ElapsedMilliseconds, 10_000);

			AssertEquals(EDIMessage.Status.Received, incomingCRSMesage.EM_Status);
		}

		public void TestPopulateConsolidatedEntryConsolidatedCargoStatusWithCARSTMessageResponse()
		{
			#region Message Text

			var incomingCRSMessageTextA = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:YES'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:YES'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:YES'
FTX+AHN+++IMPORT DECLARATION PAID:YES'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+4365++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:CE00000001/CMT::1'
RFF+ABT:AAAGMM44C'
RFF+MB:OB0987123'
RFF+BH:HB4000'
RFF+AAQ:C001'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+35+000001'
";
			var incomingCRSMessageTextB = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3JFD 9FB6 D7B5:1+8'
DTM+9:20050726162613590090:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:YES'
FTX+AHN+++LCL UNDERBOND SATISFIED:YES'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'
FTX+AHN+++IAR AQIS CLEARED:YES'
FTX+AHN+++CARGO REPORT AQIS EVALUATED:YES'
FTX+AHN+++IMPORT DECLARATIONS MATCHED:N/A'
FTX+AHN+++IMPORT DECLARATION ACS EVALUATED:N/A'
FTX+AHN+++IMPORT DECLARATION AQIS EVALUATED:N/A'
FTX+AHN+++ACS EVALUATION COMPLETE:YES'
FTX+AHN+++AQIS CARGO REPORT EVALUATION COMPLETE:YES'
FTX+AHN+++ACS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++AQIS IMPORT DECLARATION EVALUATION COMPLETE:N/A'
FTX+AHN+++IMPORT DECLARATION PAID:N/A'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+4365++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:CE00000001/CMT::1'
RFF+ABT:AAAGMM44C'
RFF+MB:OB0987123'
RFF+BH:HB4001'
RFF+AAQ:C002'
DOC+1'
PAC+++LCL:67:95'
PAC+0000200++BX:185:95'
UNT+35+000001'
";
			#endregion

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			Factory.Save();
			var leadDeclaration = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			leadDeclaration.DisableDefaultPackingInformation = true;
			leadDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			leadDeclaration.JE_IsCancelled = false;
			leadDeclaration.JE_DeclarationReference = "S00048638";
			leadDeclaration.JE_HouseBill = "HB4000";
			leadDeclaration.JE_VoyageFlightNo = "4365";
			leadDeclaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			var masterBill = leadDeclaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "OB0987123";
			var houseBill = leadDeclaration.Bills[0];
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_MasterBill = "OB0987123";
			houseBill.CU_HouseBill = "HB4000";

			var leadDeclarationEntryHeader = leadDeclaration.CustomsEntryHeaders[0];
			leadDeclarationEntryHeader.CH_BGMReference = consolidatedDeclaration.CRD_JobReferenceNumber;
			leadDeclarationEntryHeader.EntryNumber = "AAAGMM44C";

			var container = leadDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "C001";

			var packGroup1 = (PackingGroup)leadDeclaration.Bills[0].PackingGroups.AddNew();
			packGroup1.CR_HouseContainerNumber = 1;
			packGroup1.CR_CU_HouseBill = houseBill.PK;
			packGroup1.CR_CO_Container = container.PK;
			_ = packGroup1.Packages.AddNew();

			var memberDeclaration = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			memberDeclaration.DisableDefaultPackingInformation = true;
			memberDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			memberDeclaration.JE_IsCancelled = false;
			memberDeclaration.JE_DeclarationReference = "S00048639";
			memberDeclaration.JE_HouseBill = "HB4001";
			memberDeclaration.JE_VoyageFlightNo = "4365";
			memberDeclaration.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			var masterBill2 = memberDeclaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_MasterBill = "OB0987123";
			var houseBill2 = memberDeclaration.Bills[0];
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_MasterBill = "OB0987123";
			houseBill2.CU_HouseBill = "HB4001";
			var memberDeclarationEntryHeader = memberDeclaration.CustomsEntryHeaders[0];
			memberDeclarationEntryHeader.CH_BGMReference = "S00048639";
			memberDeclarationEntryHeader.EntryNumber = "AAAGMM44C";

			var container2 = memberDeclaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C002";

			var packGroup2 = (PackingGroup)memberDeclaration.Bills[0].PackingGroups.AddNew();
			packGroup2.CR_HouseContainerNumber = 2;
			packGroup2.CR_CU_HouseBill = houseBill2.PK;
			packGroup2.CR_CO_Container = container2.PK;
			_ = packGroup2.Packages.AddNew();

			var incomingCRSMessage = Factory.New<CMRCARSTMessage>();
			incomingCRSMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingCRSMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingCRSMessage.EM_MessageText = incomingCRSMessageTextA.Replace("CE00000001", consolidatedDeclaration.CRD_JobReferenceNumber).Replace("\r\n", "");
			incomingCRSMessage.EM_MessageNum = "1";
			Factory.Save();

			processor.PreProcessMessage(incomingCRSMessage);
			processor.ProcessMessage(incomingCRSMessage);
			Factory.Save();
			AssertEquals(EDIMessage.Status.Received, incomingCRSMessage.EM_Status);

			AssertEquals("Lead Declaration Message Count", 1, leadDeclaration.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals("Lead Declaration Message Linked object", leadDeclarationEntryHeader, leadDeclaration.CustomsEntryHeaders[0].Messages[0].EM_LinkedObject);
			AssertEquals("Transport Line Message Count", 1, packGroup1.Messages.Count);
			AssertEquals("Transport Line Message Linked object", packGroup1, packGroup1.Messages[0].EM_LinkedObject);
			AssertEquals("Consolidated Declaration Message Count", 1, consolidatedDeclaration.Messages.Count);

			AssertEquals("CLR", packGroup1.CR_CargoStatus);
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine 1", "Y/Y/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine 1", "CLEAR", packGroup1.GetCargoStatusFromLatestMessage());

			var incomingCRSMessage2 = Factory.New<CMRCARSTMessage>();
			incomingCRSMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			incomingCRSMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingCRSMessage2.EM_MessageText = incomingCRSMessageTextB.Replace("CE00000001", consolidatedDeclaration.CRD_JobReferenceNumber).Replace("\r\n", "");
			incomingCRSMessage2.EM_MessageNum = "2";
			Factory.Save();

			processor.PreProcessMessage(incomingCRSMessage2);
			processor.ProcessMessage(incomingCRSMessage2);
			Factory.Save();
			AssertEquals(EDIMessage.Status.Received, incomingCRSMessage2.EM_Status);
			AssertEquals("Member Declaration Message Count", 1, memberDeclaration.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals("Member Declaration Message Linked object", memberDeclarationEntryHeader, memberDeclaration.CustomsEntryHeaders[0].Messages[0].EM_LinkedObject);
			AssertEquals("Transport Line 2 Message Count", 1, packGroup2.Messages.Count);
			AssertEquals("Transport Line 2 Message Linked object", packGroup2, packGroup2.Messages[0].EM_LinkedObject);
			AssertEquals("Consolidated Declaration Message Count", 2, consolidatedDeclaration.Messages.Count);

			AssertEquals("HLD", packGroup2.CR_CargoStatus);
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine 1", "Y/Y/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine 1", "CLEAR", packGroup1.GetCargoStatusFromLatestMessage());
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine 2", "N/Y/N/Y", packGroup2.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine 2", "HELD", packGroup2.GetCargoStatusFromLatestMessage());
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.CARST;

		protected override ZString GetExpectedMessageName() => "Cargo Status Advice - (CARST)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRCARSTMessage);

		protected override void SetUp()
		{
			base.SetUp();
			LoggingInformation logger = new LoggingInformation();
			processor = new CARSTMessageProcessor(logger);
		}
		CARSTMessageProcessor processor;

		const string TestVesselLloyds = "8811924";
		const string TestVoyageNumber = "936";
		const string TestContainerNumber = "TRCU3382910";
		const string TestHouseBillNumber = "DUMMY";
		const string TestCargoStatusText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+231J I994 1DB5:1+8'
DTM+9:20050831154540243568:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+936++11++++8811924::11'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L5040040H0001::1'
RFF+MB:OBL250805001'
RFF+BH:DUMMY'
RFF+AAQ:TRCU3382910'
DOC+1'
PAC+++FCL:67:95'
PAC+0000200++BX:185:95'
UNT+18+000001'";

		CMRCARSTMessage TestCargoStatusMessage
		{
			get
			{
				if (testCargoStatusMessage == null)
				{
					testCargoStatusMessage = Factory.New<CMRCARSTMessage>();
					testCargoStatusMessage.EM_MessageText = TestCargoStatusText.Replace("\r\n", "");
				}
				return testCargoStatusMessage;
			}
		}
		CMRCARSTMessage testCargoStatusMessage;
	}
}
