using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class AirMessageExporterTest : AirOceanMessageExporterTestCase
	{
		public void TestExportValidationTypeToUse()
		{
			AirMessageExporter exporter = new AirMessageExporter(Consol, NotificationBuffer);
			AssertEquals("Should use JXC air validation", JXCExportValidationType.Air, exporter.ExportValidationTypeToUse);
			exporter = new AirMessageExporter(PreShipmentWrapper, NotificationBuffer);
			AssertEquals("Should use JXC air validation", JXCExportValidationType.Air, exporter.ExportValidationTypeToUse);
		}

		#region PreShipment
		protected override void PreparePreShipment()
		{
			PreShipmentWrapper.SendingForwarderPK = SendingForwarder.PK;
			PreShipmentWrapper.ReceivingForwarderPK = ReceivingForwarder.PK;
			PreShipmentWrapper.Shipment.JS_RL_NKDestination = "IDJKT";
			PreShipmentWrapper.Shipment.JS_HouseBill = "HB 10293";
			PreShipmentWrapper.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			PreShipmentWrapper.Shipment.JS_BookingReference = "1235";
			PreShipmentWrapper.Shipment.JS_MarksAndNumbers = "HAHAHAHA";
			ExportAWBHeader aWBHeader = PreShipmentWrapper.Shipment.AWBHeader;
			PopulateExportAWBRateLine(aWBHeader.AWBRateLines[0], "20", 111m);
			PopulateExportAWBRateLine(aWBHeader.AWBRateLines[1], "100", 10.20m);
			AddNewExportAWBOtherCharges(aWBHeader, Core.Constants.AWB.ChargeCodes.AC, "", "TEST", 200m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect);
		}

		protected override void PrepareDummyExporterForTestExportedMessage_PreShipment()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "IDJKT";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITROM", "ITMIL");
			MessageLine[] lines = new MessageLine[7];
			ShipmentExportAWBHeader shipmentAWBHeader = PreShipmentWrapper.Shipment.AWBHeader as ShipmentExportAWBHeader;
			lines[0] = new HAWBLine(PreShipmentWrapper, shipmentAWBHeader, HouseLevelRecordType.PreShipment);
			lines[1] = new REFRLine("1235", REFRLine.ReferenceFrom.Shipper);
			lines[2] = new REFRLine("HB 10293", REFRLine.ReferenceFrom.Consignee);
			lines[3] = new FBDNLine(shipmentAWBHeader, shipmentAWBHeader.AWBRateLines[0]);
			lines[4] = new FBDNLine(shipmentAWBHeader, shipmentAWBHeader.AWBRateLines[1]);
			lines[5] = new OTHRLine(shipmentAWBHeader, shipmentAWBHeader.AWBOtherCharges[0]);
			lines[6] = new SHMKLine(PreShipmentWrapper.Shipment);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("HB 10293", lines) };
		}

		#endregion
		#region Standard Consol
		protected override void PrepareStandardConsol()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Consol.JK_MasterBillNum = "08112345678";
			Consol.JK_RL_NKDischargePort = "USATL";
			Consol.SetDefaultReceivingForwarderAddress(ReceivingForwarder);
			Consol.SetDefaultSendingForwarderAddress(SendingForwarder);
			PopulateExportAWBRateLine(Consol.AWBHeader.AWBRateLines[0], "100", 1.5m);
			AddNewExportAWBOtherCharges(Consol.AWBHeader, Core.Constants.AWB.ChargeCodes.AT, Core.Constants.AWB.EntitlementCode.Agent, "Something", 200m, "");
			AddNewExportAWBOtherCharges(Consol.AWBHeader, Core.Constants.AWB.ChargeCodes.MY, Core.Constants.AWB.EntitlementCode.Carrier, "Something else", 199.56m, "");
			JASForwardingShipment shipment1 = (JASForwardingShipment)Consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment1.JS_HouseBill = "HB11";
			shipment1.JS_BookingReference = "BHB11";
			PopulateExportAWBRateLine(shipment1.AWBHeader.AWBRateLines[0], "1", 20m);
			PopulateExportAWBRateLine(shipment1.AWBHeader.AWBRateLines[1], "2", 23.45m);
			JASForwardingShipment shipment2 = (JASForwardingShipment)Consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment2.JS_HouseBill = "HB12";
			shipment2.JS_MarksAndNumbers = "MN12";
			shipment2.AWBHeader.AWBRateLines[0].Clear();
			AddNewExportAWBOtherCharges(shipment2.AWBHeader, Core.Constants.AWB.ChargeCodes.AW, "", "Something2", 0m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect);
		}

		protected override void PrepareDummyExporterForTestExportedMessage_StandardConsol()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "USATL";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITROM", "ITMIL");
			MessageLine[] lines = new MessageLine[13];
			ConsolExportAWBHeader consolAWBHeader = Consol.AWBHeader as ConsolExportAWBHeader;
			lines[0] = new MAWBLine(consolAWBHeader);
			lines[1] = new OTHRLine(consolAWBHeader, consolAWBHeader.AWBOtherCharges[0]);
			lines[2] = new OTHRLine(consolAWBHeader, consolAWBHeader.AWBOtherCharges[1]);
			lines[3] = new FBDNLine(consolAWBHeader, consolAWBHeader.AWBRateLines[0]);
			lines[4] = new HAWBLine(Consol, Consol.Shipments[0].AWBHeader as ShipmentExportAWBHeader);
			lines[5] = new REFRLine(Consol.Shipments[0].JS_BookingReference, REFRLine.ReferenceFrom.Shipper);
			lines[6] = new REFRLine(Consol.Shipments[0].JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			lines[7] = new FBDNLine(Consol.Shipments[0].AWBHeader, Consol.Shipments[0].AWBHeader.AWBRateLines[0]);
			lines[8] = new FBDNLine(Consol.Shipments[0].AWBHeader, Consol.Shipments[0].AWBHeader.AWBRateLines[1]);
			lines[9] = new HAWBLine(Consol, Consol.Shipments[1].AWBHeader as ShipmentExportAWBHeader);
			lines[10] = new REFRLine(Consol.Shipments[1].JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			lines[11] = new OTHRLine(Consol.Shipments[1].AWBHeader, Consol.Shipments[1].AWBHeader.AWBOtherCharges[0]);
			lines[12] = new SHMKLine((JASForwardingShipment)Consol.Shipments[1]);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("08112345678.081", lines) };
		}

		#endregion
		#region Co-Load Consol
		protected override void PrepareDummyExporterForTestExportedMessage_CoLoadConsol()
		{
			ExpectedMessageExporter.HeaderData.FreightDest = "USATL";
			ExpectedMessageExporter.HeaderData.SetSendingForwarder("AUSYD", "AUCOR");
			ExpectedMessageExporter.HeaderData.SetDestinationForwarder("ITROM", "ITMIL");
			MessageLine[] lines1 = new MessageLine[5];
			lines1[0] = new HAWBLine(Consol, Consol.Shipments[0].AWBHeader as ShipmentExportAWBHeader, HouseLevelRecordType.CoLoad);
			lines1[1] = new REFRLine(Consol.Shipments[0].JS_BookingReference, REFRLine.ReferenceFrom.Shipper);
			lines1[2] = new REFRLine(Consol.Shipments[0].JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			lines1[3] = new FBDNLine(Consol.Shipments[0].AWBHeader, Consol.Shipments[0].AWBHeader.AWBRateLines[0]);
			lines1[4] = new FBDNLine(Consol.Shipments[0].AWBHeader, Consol.Shipments[0].AWBHeader.AWBRateLines[1]);
			MessageLine[] lines2 = new MessageLine[4];
			lines2[0] = new HAWBLine(Consol, Consol.Shipments[1].AWBHeader as ShipmentExportAWBHeader, HouseLevelRecordType.CoLoad);
			lines2[1] = new REFRLine(Consol.Shipments[1].JS_HouseBill, REFRLine.ReferenceFrom.Consignee);
			lines2[2] = new OTHRLine(Consol.Shipments[1].AWBHeader, Consol.Shipments[1].AWBHeader.AWBOtherCharges[0]);
			lines2[3] = new SHMKLine((JASForwardingShipment)Consol.Shipments[1]);
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[] { new JXCMessageExporter.MessageFileNameAndContents("08112345678_1.081", lines1), new JXCMessageExporter.MessageFileNameAndContents("08112345678_2.081", lines2) };
		}

		#endregion
		protected override AirOceanMessageExporter GetNewAirOceanMessageExporter(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			return new AirMessageExporter(consol, notificationSubscriber);
		}

		protected override AirOceanMessageExporter GetNewAirOceanMessageExporter(PreShipmentWrapper preShipment, INotifications notificationSubscriber)
		{
			return new AirMessageExporter(preShipment, notificationSubscriber);
		}

		void PopulateExportAWBRateLine(ExportAWBRateLine rateLine, ZString noOfPieces, ZDecimal total)
		{
			rateLine.ER_NoOfPiecesOrRCP = noOfPieces;
			rateLine.ER_Total = total;
		}

		void AddNewExportAWBOtherCharges(ExportAWBHeader aWBHeader, ZString chargeCode, ZString entitlementCode, ZString description, ZDecimal amount, ZString pPDCLT)
		{
			ExportAWBOtherCharges aWBOtherCharges = aWBHeader.AWBOtherCharges.AddNew();
			aWBOtherCharges.EO_ChargeCode = chargeCode;
			aWBOtherCharges.EO_EntitlementCode = entitlementCode;
			aWBOtherCharges.EO_ChargeDescription = description;
			aWBOtherCharges.EO_Amount = amount;
			aWBOtherCharges.EO_PPDCLT = pPDCLT;
		}
	}
}
