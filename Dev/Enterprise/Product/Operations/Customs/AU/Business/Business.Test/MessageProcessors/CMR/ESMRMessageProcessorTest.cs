using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ESMRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessESMRClearResponse()
		{
			SetupConsol("C00001208");
			AssertESMLinesEstablished();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearMessage.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
				@"Consol #: C00001208

Status: CLEAR
Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.

Status of Lines:
Line: 0001
	Reference: S00001234
	CAN: AAAACNPKX
	Status: CLEAR

";
			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));
			AssertEquals("Should have a CLO log because we got a clear status", "CLO", consol.Logs.GetAllLogs()[consol.Logs.GetAllLogs().Count - 1].SL_Reference);

			AUCusEntryNumber permit = wrapper.GetPermit();
			AssertEquals("PermitNumber", "AAAACNPMT", permit.CE_EntryNum);

			int lineSequence = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				lineSequence++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", lineSequence, wrappedShipment.ESMLineNumber);
				AssertEquals("ESMNumberType should have been converted to manifested", true, wrappedShipment.HasSubManifestLineNumber);
			}
		}

		public void TestProcessESMRFindsOutgoingMessageOnConsol()
		{
			SetupConsol("C00001208");
			AssertESMLinesEstablished();

			var branch1 = GlbCompany.CurrentCompany.Branches.AddNew();
			var branch2 = GlbCompany.CurrentCompany.Branches.AddNew();

			outgoingMessage.EM_GB = branch1.PK;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(-1);
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageText = "I Am The Message You Are Looking For";

			var otherOutgoingMessage = consol.Messages.AddNew();
			otherOutgoingMessage.EM_GB = branch2.PK;
			otherOutgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.UniversalDataMessaging;
			otherOutgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			otherOutgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			otherOutgoingMessage.EM_Status = EDIMessage.Status.Sent;
			otherOutgoingMessage.EM_MessageText = "This Is Not The Message You Are Looking For";

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearMessage.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			incomingMessage.EM_GB = ZGuid.Empty;

			processor.ProcessMessage(incomingMessage);
			AssertEquals(outgoingMessage.EM_GB, incomingMessage.EM_GB);

			var attachment = processor.SentReport.Attachments.Cast<AttachmentDef>().FirstOrDefault(a => a.DisplayName == CMRMessageResponseProcessor.OutgoingMessageAttachmentFilename);
			var text = Encoding.ASCII.GetString(attachment.Data);
			AssertEquals("I Am The Message You Are Looking For", text);
		}

		public void TestProcessESMRClearExpiredResponse()
		{
			SetupConsol("C00001208");
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00001236";
			var wrappedShipment = new FreightShipmentWrapper(shipment3, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(3);
			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_UniqueConsignRef = "S00001237";
			wrappedShipment = new FreightShipmentWrapper(shipment4, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(4);
			var shipment5 = consol.Shipments.AddNew();
			shipment5.JS_UniqueConsignRef = "S00001238";
			wrappedShipment = new FreightShipmentWrapper(shipment5, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(5);
			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment6, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(6);
			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment7, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(7);
			var shipment8 = consol.Shipments.AddNew();
			shipment8.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment8, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(8);
			var shipment9 = consol.Shipments.AddNew();
			shipment9.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment9, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(9);
			var shipment10 = consol.Shipments.AddNew();
			shipment10.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment10, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(10);
			var shipment11 = consol.Shipments.AddNew();
			shipment11.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment11, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(11);

			AssertESMLinesEstablished();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearExpired.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
				@"Consol #: C00001208

Status: CLEAR - EXPIRED:REPORTED
Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH. CUSTOMS DOES NOT EXPECT FURTHER QUOTATION OF THE RELATED CAN.

Status of Lines:
Line: 0001
	Reference: S00001234
	CAN: ACLTXJFMX
	Status: CLEAR

Line: 0002
	Reference: S00001234
	CAN: EXLV
	Status: CLEAR

Line: 0003
	Reference: S00001234
	CAN: ACLXCYAFM
	Status: CLEAR

Line: 0004
	Reference: S00001234
	CAN: ACLW4MTXH
	Status: CLEAR

Line: 0005
	Reference: S00001234
	CAN: ACLYT3X7K
	Status: CLEAR

Line: 0006
	Reference: S00001234
	CAN: ACLY4KHKY
	Status: CLEAR

Line: 0007
	Reference: S00001234
	CAN: ACLX4WMN7
	Status: CLEAR

Line: 0008
	Reference: S00001234
	CAN: ACLYNPJAR
	Status: CLEAR

Line: 0009
	Reference: S00001234
	CAN: ACLX9J4RF
	Status: CLEAR

Line: 0010
	Reference: S00001234
	CAN: ACLYFTJLG
	Status: CLEAR

Line: 0011
	Reference: S00001234
	CAN: ACLYMFTJF
	Status: CLEAR

";
			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			AUCusEntryNumber permit = wrapper.GetPermit();
			AssertEquals("PermitNumber", "ACLYTNA3E", permit.CE_EntryNum);

			int lineSequence = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				lineSequence++;
				wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", lineSequence, wrappedShipment.ESMLineNumber);
				AssertEquals("ESMNumberType should have been converted to manifested", true, wrappedShipment.HasSubManifestLineNumber);
			}

			AssertEquals("Should have been 11 ESMNumberType converted to manifested", 11, lineSequence);
		}

		public void TestProcessESMRRejectedResponse()
		{
			SetupConsol("C00001208");
			AssertESMLinesEstablished();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRRejectionMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
				@"Consol #: C00001208

Status: REJECTED
Status Description: THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.


Errors:
	TOTAL NUMBER OF PACKAGES MUST BE PROVIDED WHERE MODE OF TRANSPORT IS SEA AND TOTAL NUMBER OF CONTAINERS IS 0.";

			AssertEquals("Should not have CLO log because we didn't get a clear", 0, consol.Logs.GetAllLogs().Count);
			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Rejected, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));
			foreach (CommonShipment shipment in consol.Shipments)
			{
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertNull("ESMMainfestLineSequence should have been deleted", wrappedShipment.ESMMainfestLineSequence);
			}
		}

		public void TestProcessESMRErrorExpiredResponse()
		{
			SetupConsol("C00007502");
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00048174";
			var wrappedShipment = new FreightShipmentWrapper(shipment3, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(3);
			wrappedShipment.UpdatePreliminaryLineNumberToManifestedLineNumber();

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_UniqueConsignRef = "S00048277";
			wrappedShipment = new FreightShipmentWrapper(shipment4, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(4);
			wrappedShipment.UpdatePreliminaryLineNumberToManifestedLineNumber();

			var shipment5 = consol.Shipments.AddNew();
			wrappedShipment = new FreightShipmentWrapper(shipment5, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(5);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_UniqueConsignRef = "S00048290";
			wrappedShipment = new FreightShipmentWrapper(shipment6, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(6);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_UniqueConsignRef = "S00048313";
			wrappedShipment = new FreightShipmentWrapper(shipment7, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(7);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment8 = consol.Shipments.AddNew();
			shipment8.JS_UniqueConsignRef = "S00048326";
			wrappedShipment = new FreightShipmentWrapper(shipment8, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(8);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment9 = consol.Shipments.AddNew();
			wrappedShipment = new FreightShipmentWrapper(shipment9, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(9);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment10 = consol.Shipments.AddNew();
			shipment10.JS_UniqueConsignRef = "S00048453";
			wrappedShipment = new FreightShipmentWrapper(shipment10, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(10);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment11 = consol.Shipments.AddNew();
			shipment11.JS_UniqueConsignRef = "S00048454";
			wrappedShipment = new FreightShipmentWrapper(shipment11, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(11);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment12 = consol.Shipments.AddNew();
			wrappedShipment = new FreightShipmentWrapper(shipment12, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(12);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			var shipment13 = consol.Shipments.AddNew();
			shipment13.JS_UniqueConsignRef = "REBATE48922";
			wrappedShipment = new FreightShipmentWrapper(shipment13, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(13);

			var shipment14 = consol.Shipments.AddNew();
			shipment14.JS_UniqueConsignRef = "TRAN48923";
			wrappedShipment = new FreightShipmentWrapper(shipment14, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(14);
			AssertESMLines(14);

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRErrorExpired.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(10);
			incomingMessage.EM_LinkUniqueID = consol.PK;
			incomingMessage.EM_LinkTable = "JobConsol";
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
				@"Consol #: C00007502

Status: ERROR - EXPIRED:REPORTED VALIDATION
Status Description: THE GOODS COVERED BY THE DOCUMENT CANNOT BE DEALT WITH. CUSTOMS DOES NOT EXPECT FURTHER QUOTATION OF THE RELATED CAN. THE LODGED OR AMENDED INFORMATION HAS NOT PASSED THE REQUIRED EDITS.


Errors:
	AN EXPORT DOCUMENT STATUS OF A QUOTED CUSTOMS AUTHORITY NUMBER HAS RESULTED IN LINE LINENUM=000000000000002 BEING IN ERROR (	Reference: S00001234
)

Status of Lines:
Line: 0001
	Reference: S00001234
	CAN: EXPE
	Status: CLEAR

Line: 0002
	Reference: S00001234
	CAN: ACMHHNLL9
	Status: ERROR
	Status Description: AN EXPORT DOCUMENT STATUS OF A QUOTED CUSTOMS AUTHORITY NUMBER HAS RESULTED IN LINE LINENUM=000000000000002 BEING IN ERROR

Line: 0003
	Reference: S00001234
	CAN: ACMH6E4YL
	Status: CLEAR

Line: 0004
	Reference: S00001234
	CAN: ACMJNRHA4
	Status: CLEAR

Line: 0006
	Reference: S00001234
	CAN: EXPE
	Status: CLEAR

Line: 0007
	Reference: S00001234
	CAN: ACMKFCXLF
	Status: CLEAR

Line: 0008
	Reference: S00001234
	CAN: ACMJXL7RG
	Status: CLEAR

Line: 0010
	Reference: S00001234
	CAN: ACMG96MJE
	Status: CLEAR

Line: 0011
	Reference: S00001234
	CAN: ACMKEJ9XW
	Status: CLEAR

Line: 0013
	Reference: S00001234
	CAN: EXLV
	Status: CLEAR

Line: 0014
	Reference: S00001234
	CAN: EXLV
	Status: CLEAR

";

			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertESMLines(14);
		}

		public void TestProcessESMRErrorResponse()
		{
			SetupConsol("C00001208");
			AssertESMLinesEstablished();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRErrorMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
				@"Consol #: C00001208

Status: ERROR - VALIDATION
Status Description: THE GOODS COVERED BY THE DOCUMENT CANNOT BE DEALT WITH. THE LODGED OR AMENDED INFORMATION HAS NOT PASSED THE REQUIRED EDITS.

Status of Lines:
Line: 0001
	Reference: S00001234
	CAN: AAAACNPKX
	Status: ERROR
	Status Description: AN EXPORT DOCUMENT STATUS OF A QUOTED CUSTOMS AUTHORITY NUMBER HAS RESULTED IN LINE LINENUM=000000000000001 BEING IN ERROR.

";

			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Errors, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));
		}

		public void TestProcessESMRWithdrawnResponse()
		{
			SetupConsol("C00001208");
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRWithdrawnMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Status", Constants.CMRConsolStatus.Withdrawn, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));
			AssertEquals("CAN", ZString.Empty, wrapper.CAN);
			AssertEquals(0, processor.ErrorEmailSendCount);
			AssertEquals(1, processor.AcknowledgementEmailSendCount);
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				var wrappedShipment = new FreightShipmentWrapper(freightShipment, consol);
				AssertEquals("Shipment - Should have had SubManifestLineNumber data deleted", null, wrappedShipment.ESMMainfestLineSequence);
			}
		}

		public void TestProcessExportManifestHeader()
		{
			SetupHeader();
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearMessage.txt")).Replace("\r\n", "").Replace("C00001208", "K00001234");
			processor.ProcessMessage(incomingMessage);

			AssertEquals("CAN", "AAAACNPMT", header.ED_CAN);
			AssertEquals("DocumentStatus", CMR3CharDocumentStatus.Clear.Code, header.ED_DocumentStatus);
			AssertEquals("DocumentStatus", ZString.Empty, header.ED_DocumentStatusConditions);

			AssertEquals("DocumentStatus", CMR3CharDocumentStatus.Clear.Code, header.Lines[0].EL_DocumentStatus);
			AssertEquals("DocumentStatus", ZString.Empty, header.Lines[0].EL_DocumentStatusConditions);
		}

		public void TestProcessESMResponseEmailSubjectLine()
		{
			SetupConsol("C00001208");
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUADL";
			consol.JK_RL_NKDischargePort = "NZAKL";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUADL";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_VoyageFlight = "PW234";
			transport.JW_ETD = new ZDateTime(2015, 07, 06);
			consol.JK_MasterBillNum = "081-00394857";

			incomingMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+342E H5B2 5FB5:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00001208/CMT1::003'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC4H9CM'DOC+1'RFF+TN:AAAC4H9FN'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TN:AAAC4H77M'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TN:AAAC4H9AW'CST+0003'FTX+AHN+++CLEAR'DOC+1'RFF+TN:AAAC4H9GE'CST+0004'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'CNT+5:0005'UNT+30+000001'";
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Subject should be a short value not the full detail description", "Export Sub Manifest Response(ESMR) Message for Consol #: C00001208 - CLEAR", processor.SentReport.Subject);
		}

		public void TestProcessResponseCausingException()
		{
			SetupConsol("C00026910");
			AssertESMLinesEstablished();
			incomingMessage.EM_MessageText = ResponseCausingException;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));
			AssertEquals("Should have a CLO log because we got a clear status", "CLO", consol.Logs.GetAllLogs()[consol.Logs.GetAllLogs().Count - 1].SL_Reference);

			AUCusEntryNumber permit = wrapper.GetPermit();
			AssertEquals("PermitNumber", "AAAC3FWKY", permit.CE_EntryNum);
		}

		public void TestESMMessageReturned_ThenUpdateAllHVLVConsignmentsExportCustomsClearanceStatus()
		{
			SetupConsol("C00001208");

			var shipmentA = consol.Shipments.AddNew();
			var shipmentB = consol.Shipments.AddNew();
			shipmentA.JS_HouseBill = "HBA";
			shipmentB.JS_HouseBill = "HBB";
			shipmentA.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipmentB.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var consignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			var consignment2 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			var consignment3 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			var consignment4 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());

			consignment1.HVC_JS_ManifestedOnShipment = shipmentA.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipmentA.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipmentB.PK;
			consignment4.HVC_JS_ManifestedOnShipment = shipmentB.PK;

			PrepareRefCusCodeList(Factory, new[] { (Constants.CMRConsolStatus.Clear, "COVID-19 go away!", "HLD") });

			Factory.Save();

			CombineAssertions("Precondition for consignment export clearance status: ", () =>
			{
				AssertEquals("consignment1", ZString.Empty, consignment1.HVC_ExportCustomsClearanceStatus);
				AssertEquals("consignment2", ZString.Empty, consignment2.HVC_ExportCustomsClearanceStatus);
				AssertEquals("consignment3", ZString.Empty, consignment3.HVC_ExportCustomsClearanceStatus);
				AssertEquals("consignment4", ZString.Empty, consignment4.HVC_ExportCustomsClearanceStatus);
			});

			incomingMessage.EM_LinkUniqueID = consol.PK;
			incomingMessage.EM_LinkTable = "JobConsol";
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearMessage.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			consol.Messages.Add(incomingMessage);
			processor.ProcessMessage(incomingMessage);

			AssertEquals("Precondition: ", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			CombineAssertions("All consignment's export customs clearance status should be clear", () =>
			{
				AssertEquals("consignment1", Constants.CMRConsolStatus.Clear, consignment1.HVC_ExportCustomsClearanceStatus);
				AssertEquals("consignment2", Constants.CMRConsolStatus.Clear, consignment2.HVC_ExportCustomsClearanceStatus);
				AssertEquals("consignment3", Constants.CMRConsolStatus.Clear, consignment3.HVC_ExportCustomsClearanceStatus);
				AssertEquals("consignment4", Constants.CMRConsolStatus.Clear, consignment4.HVC_ExportCustomsClearanceStatus);
			});
		}

		public void TestProcessESMRWithdrawalRejectionResponse()
		{
			SetupConsol("C00001208");
			wrapper = new FreightConsolWrapper(consol);
			wrapper.UpdatePreliminaryLinesToManifestedLines();

			incomingMessage.EM_LinkUniqueID = consol.PK;
			incomingMessage.EM_LinkTable = "JobConsol";
			incomingMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+3HGD 522H B805:001+11'FTX+AHN+++REJECTED:THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::005'RFF+ACW:ESM'RFF+AFM:4'ERP+::0000'ERC+XM1034::95'FTX+AAO+++REPORT REJECTED?: SUB-MANIFEST @ IS REVOKED AND CANNOT BE FURTHER PROCESSED'CNT+55:01'UNT+12+000001'";
			consol.Messages.Add(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Status", Constants.CMRConsolStatus.Rejected, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				var wrappedShipment = new FreightShipmentWrapper(freightShipment, consol);
				AssertEquals("ESMNumberType manifested should still be present", true, wrappedShipment.HasSubManifestLineNumber);
			}
		}

		public void TestProcessESMRClearResponseAfterDeletingShipments()
		{
			SetupConsol("C00001208");
			AssertESMLinesEstablished();
			int shipmentSequenceUpdated = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				shipmentSequenceUpdated++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", true, wrappedShipment.IsPreliminaryNumberType);
			}
			AssertEquals("shipmentSequenceUpdated", 2, shipmentSequenceUpdated);

			var shipment1 = consol.Shipments[0];
			var shipment2 = consol.Shipments[1];
			consol.Shipments.Remove(shipment2);
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearMessage.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			ZString expectedResult =
				@"Consol #: C00001208

Status: CLEAR
Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.

Status of Lines:
Line: 0001
	Reference: S00001234
	CAN: AAAACNPKX
	Status: CLEAR

";
			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			shipmentSequenceUpdated = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				shipmentSequenceUpdated++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", shipmentSequenceUpdated, wrappedShipment.ESMLineNumber);
				AssertEquals("ESMNumberType should have been converted to manifested", true, wrappedShipment.HasSubManifestLineNumber);
			}

			AssertEquals("shipmentSequenceUpdated", 1, shipmentSequenceUpdated);

			var wrappedShipmentPostUpdate = new FreightShipmentWrapper(shipment2, consol);
			AssertEquals("shipment2 ESMNumberType should have been converted to manifested even though this shipment no longer attached to the Consol", true, wrappedShipmentPostUpdate.HasSubManifestLineNumber);
		}

		public void TestProcessESMRClearWithDeletedLineAfterDetachingThatShipment()
		{
			SetupConsol("C00027267");
			var shipment1 = consol.Shipments[0];
			shipment1.JS_UniqueConsignRef = "S00048125";
			var shipment2 = consol.Shipments[1];
			shipment2.JS_UniqueConsignRef = "S00048126";
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00048127";
			var wrappedFreightShipment = new FreightShipmentWrapper(shipment3, consol);
			wrappedFreightShipment.CreatePreliminaryManifestLineNumber(3);
			Factory.Save();

			AssertESMLinesEstablished();
			int shipmentSequenceUpdated = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				shipmentSequenceUpdated++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", true, wrappedShipment.IsPreliminaryNumberType);
			}
			AssertEquals("shipmentSequenceUpdated", 3, shipmentSequenceUpdated);

			consol.Shipments.Remove(shipment3);
			incomingMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+230H 72HI A3G5:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00027267/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC4MN7P'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			consol.Messages.Add(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertContains("Report", "Consol #: C00027267", processor.SentReport.Body);
			AssertContains("Report details", "Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.", processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			shipmentSequenceUpdated = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				shipmentSequenceUpdated++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", shipmentSequenceUpdated, wrappedShipment.ESMLineNumber);
				AssertEquals("ESMNumberType should have been converted to manifested", true, wrappedShipment.HasSubManifestLineNumber);
			}

			AssertEquals("shipmentSequenceUpdated", 2, shipmentSequenceUpdated);
			var wrappedShipment3PostUpdate = new FreightShipmentWrapper(shipment3, consol);
			AssertEquals("ESMLineNumber", 3, wrappedShipment3PostUpdate.ESMLineNumber);
			AssertEquals("This ESMNumberType should also have been converted to manifested even though the shipment has been detached from the Consol", true, wrappedShipment3PostUpdate.HasSubManifestLineNumber);

			// send & response should still recognise previous detached consignment that now needs to be deleted at Customs.
			outgoingMessage = (CMRESMMessage)consol.Messages.AddNew(typeof(CMRESMMessage));
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1).AddMinutes(10);
			wrappedShipment3PostUpdate.UpdateManifestedLineNumberToPreliminaryDeletedLine();

			incomingMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+10BF C42B F3G5:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00027267/CMT1::002'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC4MN7P'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000001'";
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1).AddMinutes(15);
			consol.Messages.Add(incomingMessage);
			processor.ProcessMessage(incomingMessage);
			Factory.Save();

			AssertContains("Report", "Consol #: C00027267", processor.SentReport.Body);
			AssertContains("Report details", "Status Description: THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.", processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			shipmentSequenceUpdated = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				shipmentSequenceUpdated++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", shipmentSequenceUpdated, wrappedShipment.ESMLineNumber);
				AssertEquals("ESMNumberType should have been converted to manifested", true, wrappedShipment.HasSubManifestLineNumber);
			}

			AssertEquals("shipmentSequenceUpdated", 2, shipmentSequenceUpdated);
			var wrappedShipment3PostSecondSend = new FreightShipmentWrapper(shipment3, consol);
			AssertEquals("This ESMNumberType should now show as deleted as the shipment has been detached from the Consol & subsequent message sent to Customs", false, wrappedShipment3PostUpdate.HasSubManifestLineNumber);
			AssertEquals("This ESMNumberType should be a deleted ESM line", true, wrappedShipment3PostSecondSend.IsDeletedNumberType);
		}

		public void TestProcessESMRErrorValidationResponse()
		{
			SetupConsol("C00061638");
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00001236";
			var wrappedShipment = new FreightShipmentWrapper(shipment3, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(21);
			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_UniqueConsignRef = "S00001237";
			wrappedShipment = new FreightShipmentWrapper(shipment4, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(22);
			var shipment5 = consol.Shipments.AddNew();
			shipment5.JS_UniqueConsignRef = "S00001238";
			wrappedShipment = new FreightShipmentWrapper(shipment5, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(23);
			var shipment6 = consol.Shipments.AddNew();
			shipment6.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment6, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(24);
			var shipment7 = consol.Shipments.AddNew();
			shipment7.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment7, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(25);
			var shipment8 = consol.Shipments.AddNew();
			shipment8.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment8, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(26);
			var shipment9 = consol.Shipments.AddNew();
			shipment9.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment9, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(27);
			var shipment10 = consol.Shipments.AddNew();
			shipment10.JS_UniqueConsignRef = "S00001239";
			wrappedShipment = new FreightShipmentWrapper(shipment10, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(28);

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMErrorValidation.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
				@"Consol #: C00061638

Status: ERROR - VALIDATION
Status Description: THE GOODS COVERED BY THE DOCUMENT CANNOT BE DEALT WITH. THE LODGED OR AMENDED INFORMATION HAS NOT PASSED THE REQUIRED EDITS.


Errors:
	LINENUM=000000000000022 IS TRYING TO QUOTE EDN=ACMYLXYHF THAT HAS ALREADY BEEN TALLY EXPIRED (	Reference: S00001234
)

Status of Lines:
Line: 0001
	Reference: S00001234
	CAN: ACMYXJLAR
	Status: CLEAR

Line: 0002
	Reference: S00001234
	CAN: ACMW7JYYR
	Status: CLEAR

Line: 0021
	Reference: S00001234
	CAN: ACLTXJFMX
	Status: CLEAR

Line: 0022
	Reference: S00001234
	CAN: ACMYLXYHF
	Status: ERROR
	Status Description: LINENUM=000000000000022 IS TRYING TO QUOTE EDN=ACMYLXYHF THAT HAS ALREADY BEEN TALLY EXPIRED

Line: 0023
	Reference: S00001234
	CAN: EXLV
	Status: CLEAR

Line: 0024
	Reference: S00001234
	CAN: ACMYXCTTJ
	Status: CLEAR

Line: 0025
	Reference: S00001234
	CAN: ACMYXAWTM
	Status: CLEAR

Line: 0026
	Reference: S00001234
	CAN: ACMYXGWGF
	Status: CLEAR

Line: 0027
	Reference: S00001234
	CAN: EXLV
	Status: CLEAR

Line: 0028
	Reference: S00001234
	CAN: EXPE
	Status: CLEAR";
			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
			AssertEquals("Status", Constants.CMRConsolStatus.Errors, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			AUCusEntryNumber permit = wrapper.GetPermit();
			AssertEquals("PermitNumber", "ACMMHG9WX", permit.CE_EntryNum);

			int lineSequence = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				lineSequence++;
				wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMLineNumber", lineSequence, wrappedShipment.ESMLineNumber);
				AssertEquals("ESMNumberType should have been converted to manifested", true, wrappedShipment.HasSubManifestLineNumber);
				if (lineSequence == 2)
				{
					lineSequence = 20; // 18 shipments have been removed from this test message for convenience
				}
			}

			AssertEquals("Should have been 10 ESMNumberType converted to manifested", 10, (lineSequence - 18));
		}

		protected override ZString GetExpectedMessageCode() => "ESM";

		protected override ZString GetExpectedMessageName() => "Export Sub Manifest Response(ESMR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			processor = new ESMRMessageProcessorTestHelper(logger);
		}

		protected override Type IncomingMessageType => typeof(CMRESMRMessage);

		void AssertESMLinesEstablished()
		{
			int lineSequence = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				lineSequence++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				AssertEquals("ESMNumberType", consol.JK_UniqueConsignRef, wrappedShipment.ESMPreliminaryLineSequence.CY_Data);
				AssertEquals("Line Number", lineSequence, wrappedShipment.ESMPreliminaryLineSequence.CY_Order);
			}
		}

		void AssertESMLines(int expectedESMLines)
		{
			int lineSequence = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				lineSequence++;
				var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
				if (wrappedShipment.ESMMainfestLineSequence != null)
				{
					AssertEquals("ESMNumberType", consol.JK_UniqueConsignRef, wrappedShipment.ESMMainfestLineSequence.CY_Data);
					AssertEquals("Line Number", lineSequence, wrappedShipment.ESMMainfestLineSequence.CY_Order);
				}
				else if (wrappedShipment.ESMPreliminaryLineSequence != null)
				{
					AssertEquals("ESMNumberType", consol.JK_UniqueConsignRef, wrappedShipment.ESMPreliminaryLineSequence.CY_Data);
					AssertEquals("Line Number", lineSequence, wrappedShipment.ESMPreliminaryLineSequence.CY_Order);
				}
				else if (wrappedShipment.ESMDeletedLineSequence != null)
				{
					AssertEquals("ESMNumberType", consol.JK_UniqueConsignRef, wrappedShipment.ESMDeletedLineSequence.CY_Data);
					AssertEquals("Line Number", lineSequence, wrappedShipment.ESMDeletedLineSequence.CY_Order);
				}
				else if (wrappedShipment.ESMPreliminaryDeletedLineSequence != null)
				{
					AssertEquals("ESMNumberType", consol.JK_UniqueConsignRef, wrappedShipment.ESMPreliminaryDeletedLineSequence.CY_Data);
					AssertEquals("Line Number", lineSequence, wrappedShipment.ESMPreliminaryDeletedLineSequence.CY_Order);
				}
			}

			AssertEquals("expectedESMLines", expectedESMLines, lineSequence);
		}

		static void PrepareRefCusCodeList(BusinessObjectFactory factory,
		IEnumerable<(string code, string description, string releaseStatus)> exportCusCodes)
		{
			var refDataGrouping = factory.New<RefDataGrouping>();
			refDataGrouping.ZZZ_DataGrouping = Core.Constants.CountryCodes.Australia;
			refDataGrouping.ZZZ_Description = nameof(Core.Constants.CountryCodes.Australia);

			if (exportCusCodes != null)
			{
				var attributeName = SetUpCodeTypeAndAttributeName(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus);
				AddCusCodeCode(exportCusCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, attributeName);
			}

			string SetUpCodeTypeAndAttributeName(string codeType)
			{
				var refCodeType = factory.New<Universal.RefCusCodeType>();
				refCodeType.ZZK_CodeType = codeType;
				refCodeType.ZZK_Description = "Customs Status";
				refCodeType.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Australia;

				var attributeName = factory.New<Universal.RefCusCodeListAttributeName>();
				attributeName.ZXE_ZZK_NKCodeType = refCodeType.ZZK_CodeType;
				attributeName.ZXE_ZZZ_NKDataGrouping = refDataGrouping.ZZZ_DataGrouping;
				attributeName.ZXE_Name = "EcommerceReleaseStatus";

				return attributeName.ZXE_Name;
			}

			void AddCusCodeCode(IEnumerable<(string code, string description, string releaseStatus)> cusCodes, string codeType, string attributeName)
			{
				foreach (var (code, description, releaseStatus) in cusCodes)
				{
					var refCusCode = factory.New<Universal.RefCusCodeList>();
					refCusCode.ZZD_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Australia;
					refCusCode.ZZD_Code = code;
					refCusCode.ZZD_ZZK_NKCodeType = codeType;
					refCusCode.ZZD_StartDate = ZDate.BrettsBirthday;
					refCusCode.ZZD_EndDate = ZDate.Today.AddMonths(1);
					refCusCode.ZZD_Description = description;

					var releaseStatusAttribute = refCusCode.Attributes.AddNew();
					releaseStatusAttribute.ZZE_ZXE_NKName = attributeName;
					releaseStatusAttribute.ZZE_Value = releaseStatus;
				}
			}

			factory.Save();
		}

		void SetupConsol(string consolID)
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolID;
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001234";
			var wrappedShipment = new FreightShipmentWrapper(shipment1, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(1);
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001235";
			wrappedShipment = new FreightShipmentWrapper(shipment2, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(2);

			wrapper = new FreightConsolWrapper(consol);
			outgoingMessage = (CMRESMMessage)consol.Messages.AddNew(typeof(CMRESMMessage));
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
		}

		void SetupHeader()
		{
			header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_BGMReference = "K00001234";
			header.Lines.AddNew().EL_LineNo = (short)1;
		}

		ExportCustomsManifestHeader header;
		ForwardingConsol consol;
		ESMRMessageProcessorTestHelper processor;
		FreightConsolWrapper wrapper;

		const string ResponseCausingException = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+1J14 5A43 72GE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026910/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3FWKY'DOC+1'RFF+TN:AAAC3FWHN'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TN:AAAC3FWJK'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000001'";
		sealed class ESMRMessageProcessorTestHelper : ESMRMessageProcessor
		{
			public ESMRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
