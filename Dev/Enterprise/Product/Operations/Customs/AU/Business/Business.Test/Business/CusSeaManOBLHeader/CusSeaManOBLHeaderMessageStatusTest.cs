using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class CusSeaManOBLHeaderMessageStatusTest : TestCaseWithFactory
	{
		#region TestShowDetails

		public void TestShouldShow()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			AssertEquals(true, new CusSeaManOBLHeaderMessageStatus().ShouldShow(billOfLading));
		}

		#endregion

		#region TestGetMessageStatus

		public void TestGetMessageStatus()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_NKLoadPort = "GBLON";
			billOfLading.JS_NKDischargePort = "AUBNE";
			billOfLading.JS_HouseBill = "OB111";
			BillOfLadingContainer container1 = billOfLading.RealContainers.AddNew();
			container1.JC_ContainerNum = "OCLU1111116";
			container1.JC_IsEmptyContainer = false;
			BillOfLadingContainer container2 = billOfLading.RealContainers.AddNew();
			container2.JC_ContainerNum = "OCLU1111117";
			container2.JC_IsEmptyContainer = true;

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "GBLON";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";

			billOfLading.JS_JX = voyage.Sailings[0].PK;

			CusSeaManTranHead importManifest = Factory.New<CusSeaManTranHead>();
			CusSeaManArrivalPort arrivalPort = importManifest.Arrivals.AddNew();
			arrivalPort.BA_RL_NKArrivalPort = "AUBNE";
			CusSeaManOBLHeaderCargoLine cargoLine1 = arrivalPort.CargoLines.AddNew();
			cargoLine1.CargoIdentifier = "OCLU1111116";
			cargoLine1.BO_RL_NKLoadPort = "GBLON";
			cargoLine1.BO_RL_NKDischargePort = "AUBNE";

			Factory.Save();
			CusSeaManOBLHeaderMessageStatus cusSeaManOBLHeaderMessageStatus = new CusSeaManOBLHeaderMessageStatus();

			AssertEquals(ZString.Empty, cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));
			container1.JC_IsEmptyContainer = true;
			AssertEquals("Not Sent", cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));

			EDIMessage message = arrivalPort.Messages.AddNew();
			message.EM_MessageText = "Message";
			message.EM_LinkTable = "CusSeaManArrivalPort";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CST";
			message.EM_MessageSubType = "CST";

			AssertEquals("Message Sent", cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));

			cargoLine1.CargoIdentifier = "OCLU1111115";
			Factory.Save();

			AssertEquals(ZString.Empty, cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));

			CusSeaManOBLHeaderCargoLine cargoLine2 = arrivalPort.CargoLines.AddNew();
			cargoLine2.BO_OceanBill = "OB111";
			cargoLine2.BO_RL_NKLoadPort = "GBLON";
			cargoLine2.BO_RL_NKDischargePort = "AUBNE";
			Factory.Save();

			AssertEquals("Message Sent", cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));

			CusSeaManOBLHeader header = importManifest.OceanBills.AddNew();
			header.BO_OceanBill = "OB111";
			header.BO_RL_NKLoadPort = "GBLON";
			header.BO_RL_NKDischargePort = "AUBNE";
			header.CargoReportStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();

			AssertEquals(CMRBaseStatuses.Descriptions.OriginalAccepted, cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));
		}

		#endregion

		#region TestGetMessageStatusForCoastalMove

		public void TestGetMessageStatusForCoastalMove()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_NKLoadPort = "AUSYD";
			billOfLading.JS_NKDischargePort = "AUBNE";
			billOfLading.JS_HouseBill = "OB111";
			BillOfLadingContainer container1 = billOfLading.RealContainers.AddNew();
			container1.JC_ContainerNum = "OCLU1111116";
			container1.JC_IsEmptyContainer = false;
			BillOfLadingContainer container2 = billOfLading.RealContainers.AddNew();
			container2.JC_ContainerNum = "OCLU1111117";
			container2.JC_IsEmptyContainer = false;

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";

			billOfLading.JS_JX = voyage.Sailings[0].PK;

			CusSeaManTranHead importManifest = Factory.New<CusSeaManTranHead>();
			CusSeaManArrivalPort arrivalPort = importManifest.Arrivals.AddNew();
			arrivalPort.BA_RL_NKArrivalPort = "AUBNE";
			CusSeaManOBLHeaderCargoLine cargoLine1 = arrivalPort.CargoLines.AddNew();
			cargoLine1.CargoIdentifier = "OCLU1111116";
			cargoLine1.BO_RL_NKLoadPort = "AUSYD";
			cargoLine1.BO_RL_NKDischargePort = "AUBNE";

			Factory.Save();
			CusSeaManOBLHeaderMessageStatus cusSeaManOBLHeaderMessageStatus = new CusSeaManOBLHeaderMessageStatus();
			AssertEquals("Not Sent", cusSeaManOBLHeaderMessageStatus.GetMessageStatus(billOfLading));
		}

		#endregion

		#region TestGetCustomsStatus

		public void TestGetCustomsStatus()
		{
			BillOfLading billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_NKLoadPort = "AUSYD";
			billOfLading.JS_NKDischargePort = "AUBNE";
			billOfLading.JS_HouseBill = "OB111";

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";

			billOfLading.JS_JX = voyage.Sailings[0].PK;

			CusSeaManTranHead importManifest = Factory.New<CusSeaManTranHead>();
			CusSeaManOBLHeader header = importManifest.OceanBills.AddNew();
			header.BO_OceanBill = "OB111";
			header.BO_RL_NKLoadPort = "AUSYD";
			header.BO_RL_NKDischargePort = "AUBNE";
			header.ShipmentStatus.Code = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;

			Factory.Save();
			CusSeaManOBLHeaderMessageStatus cusSeaManOBLHeaderMessageStatus = new CusSeaManOBLHeaderMessageStatus();

			AssertEquals(CMRConsolidatedCargoStatuses.Descriptions.HeldCargoIsHeldUnderCustomsControl, cusSeaManOBLHeaderMessageStatus.GetCustomsStatus(billOfLading));
		}

		#endregion

		#region TestGetUserFriendlyStatusMessage

		public void TestGetUserFriendlyStatusMessage()
		{
			BillOfLading billOfLading1 = Factory.New<BillOfLading>();
			billOfLading1.JS_NKLoadPort = "AUSYD";
			billOfLading1.JS_NKDischargePort = "AUPER";
			billOfLading1.JS_HouseBill = "OB111";

			BillOfLading billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_NKLoadPort = "AUBNE";
			billOfLading2.JS_NKDischargePort = "AUPER";
			billOfLading2.JS_HouseBill = "OB222";

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUPER";

			billOfLading1.JS_JX = voyage.Sailings[0].PK;
			billOfLading2.JS_JX = voyage.Sailings[0].PK;

			CusSeaManTranHead importManifest = Factory.New<CusSeaManTranHead>();

			CusSeaManOBLHeader oceanBill1 = importManifest.OceanBills.AddNew();
			CusSeaManOBLHeader oceanBill2 = importManifest.OceanBills.AddNew();
			oceanBill1.BO_OceanBill = "OB111";
			oceanBill1.BO_RL_NKLoadPort = "AUSYD";
			oceanBill1.BO_RL_NKDischargePort = "AUPER";
			oceanBill2.BO_OceanBill = "OB222";
			oceanBill2.BO_RL_NKLoadPort = "AUBNE";
			oceanBill2.BO_RL_NKDischargePort = "AUPER";

			CMRCARSTMessage message = Factory.New<CMRCARSTMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+2HF2 5CBC D7BF:1+8'
DTM+9:20051103122504817010:ZZZ'
FTX+AHN+++CONSOLIDATED STATUS:HELD'
FTX+AHN+++DEPARTURE FROM LAST OVERSEAS PORT:YES'
FTX+AHN+++QUOTED MASTER / OCEAN BILL EXISTS:YES'
FTX+AHN+++IAR ACS CLEARED:YES'
FTX+AHN+++COMPLETE UNDERBOND SERIES APPROVED:N/A'
FTX+AHN+++LCL UNDERBOND SATISFIED:NO'
FTX+AHN+++CARGO NOT A CONSOLIDATION:YES'
FTX+AHN+++RELEASE PREMISE IN DESTINATION:YES'
FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'
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
TDT+20+QA123++11++++9044748::11'
LOC+12+AUSYD::6'
LOC+4+9122P::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:H00001052/SYD1::1'
RFF+MB:1234564'
RFF+AAQ:LCLU99999999'
DOC+1'
PAC+++LCL:67:95'
UNT+34+000001'".Replace("\r\n", "");

			message.EM_LinkedObject = oceanBill2;
			message.EM_LinkUniqueID = oceanBill2.PK;
			message.EM_LinkTable = "CusSeaManOBLHeader";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CRS";
			message.EM_MessageSubType = "CRS";
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			oceanBill2.Messages.Add(message);

			string statusResult = @"CONSOLIDATED STATUS : HELD
COMPLETE UNDERBOND SERIES APPROVED : N/A
LCL UNDERBOND SATISFIED : NO
CARGO REPORT ACS EVALUATED : NO
IMPORT DECLARATIONS MATCHED : N/A
IMPORT DECLARATION ACS EVALUATED : N/A
IMPORT DECLARATION AQIS EVALUATED : N/A
ACS IMPORT DECLARATION EVALUATION COMPLETE : N/A
AQIS IMPORT DECLARATION EVALUATION COMPLETE : N/A
IMPORT DECLARATION PAID : N/A
CARGO REPORT SAC : NO

========================================
Warning: Cargo is not a consolidation.
========================================";
			Factory.Save();

			CusSeaManOBLHeaderMessageStatus cusSeaManOBLHeaderMessageStatus = new CusSeaManOBLHeaderMessageStatus();

			AssertEquals(statusResult, cusSeaManOBLHeaderMessageStatus.GetUserFriendlyStatusMessage(billOfLading2));
			AssertEquals(String.Empty, cusSeaManOBLHeaderMessageStatus.GetUserFriendlyStatusMessage(billOfLading1));
		}

		#endregion
	}
}
