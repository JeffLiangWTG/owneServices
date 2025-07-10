using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class ForwardingShipmentRunDocsTest : BaseRunDocumentsTest
	{
		public override BusinessObject GetBusinessObject
		{
			get
			{
				if (fBusinessObject == null)
				{
					var shipmentObject = Factory.New<ForwardingShipment>();
					shipmentObject.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipmentObject.Consols.AddNew();
					shipmentObject.JS_HouseBillOfLadingType = "FIA";
					shipmentObject.CoLoadShipments.AddNew();

					return shipmentObject;
				}
				else
				{
					return fBusinessObject;
				}
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestAgentsInstruction()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Agents Instruction");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNoticeByColoadBills()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Arrival Notice by Coload House Bills");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTimeSlotRequestForFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Time Slot Request");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			fBusinessObject = shipment;
			FilterForMenuItem = filter;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTimeSlotRequestForNotFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Time Slot Request");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			fBusinessObject = shipment;
			FilterForMenuItem = filter;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTimeSlotRequestForFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Time Slot Request");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			fBusinessObject = shipment;
			FilterForMenuItem = filter;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNoticeAndChargeSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Arrival Notice and Charge Sheet");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNoticeAndChargeSheetChina()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "China Arrival Notice and Charge Sheet");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Arrival Notice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNoticeChina()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "China Arrival Notice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBookingConfirmation()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Booking Confirmation");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestChargeSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Charge Sheet");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestColoadMasterManifestForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Master Shipment Manifest");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestColoadMasterManifestForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Master Shipment Manifest");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCoverSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDelayAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delay Alert");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDeliveryLabels_LabelPrinter()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delivery Labels (label printer)");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDeliveryLabels_SideBySide()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delivery Labels (side by side)");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDeliveryOrder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delivery Order");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDisbursementNote()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Disbursement Note");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDocumentsAvailableNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Documents Available Notice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShiLianDan()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shi Lian Dan");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEntryPrintLandscape()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Entry Print (Landscape)");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestForwardersCertificateOfReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarders Certificate of Receipt");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestOutturnReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Outturn Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestPenangDeliveryOrder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Penang Delivery Order");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestPreAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pre-Alert");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestSendElectronicOriginalBillOfLading()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Send Electronic Original Bill of Lading");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentNotes()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipment Notes");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipperDepartureNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipper Departure Notice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShippingAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Advice");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShippingOrderCFS()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Order - CFS");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShippingOrderConsignor()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Order - Consignor");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTranshipmentCartageAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Transhipment Cartage Advice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLaserHAWB()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Laser HAWB");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLaserHAWBWithFollowOn()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Laser HAWB with Follow on Page");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLetterOfGuarantee()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Letter of Guarantee");
			ZString menuFilterString = "\"<CurrentCompany.Country.Code>\"==\"SG\"&&(\"<TransportMode>\"==\"SEA\"||\"<TransportMode>\"==\"RAI\")";
			filter.AddToFilter(StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Equal, menuFilterString);
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLetterOfIndemnity()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Letter of Indemnity");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNeutralHAWB()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Neutral HAWB");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNorthPortPKDPDeliveryOrder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "NorthPort PKDP Delivery Order");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNorthPortDeliveryOrder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "NorthPort Delivery Order");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestWestPortDeliveryOrder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "WestPort Delivery Order");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNorthWestPortDeliveryOrderFollowOn()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "North/WestPort Document Continuation");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNorthPortContainerShippingNote()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "NorthPort Container Shipping Note");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAirExportCargoWeightsAndMeasurementReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Air Cargo Weight and Measurement Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestSeaExportCargoWeightsAndMeasurementReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Sea Cargo Weight and Measurement Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestInterimReceiptCFS()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Interim Receipt - CFS");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestInterimReceiptConsignor()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Interim Receipt - Consignor");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForMissingDocuments()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request For Missing Documents");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestUltimateConsigneeNotification()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Ultimate Consignee Notification");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForCollectCharges()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request For Collect Charges");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestUltimateConsignorNotification()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Ultimate Consignor Notification");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestHazardousCargoLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Hazardous Cargo Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestFreightLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Freight Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTranshipmentFreightLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Transhipment Freight Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestContainerDetentionReminder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Container Detention Reminder");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestContainerLiabilityStatement()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Container Liability Statement");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Service");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForProfitShare()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Profit Share");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#region Icelandic docs

		[ExpectNoExceptions]
		public void TestKennitolubreytingDeparture()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Kennitolubreyting");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestKennitolubreytingArrival()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Kennitolubreyting");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestUmskraningDeparture()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Umskraning");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestUmskraningArrival()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Umskraning");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			FilterForMenuItem = filter;
			RunDocument();
		}

		#endregion

		#region German docs

		[ExpectNoExceptions]
		public void TestFOBAndienung()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "FOB - Andienung");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAusfuhrbescheinigung()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Ausfuhrbescheinigung (Export Cert.)");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestVersicherungsanmeldung()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Versicherungsanmeldung");
			RunDocument();
		}

		#endregion

		#region Customs Dec Documents On Shipment
		[ExpectNoExceptions]
		public void TestStandardLandedCosting()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Landed Costing - Old");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEFTRequest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "EFT Request");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportAuthority()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Authority");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCommercialInvoice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Commercial Invoice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestSummaryCommercialInvoice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Summary Commercial Invoice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestContainerList()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Container List");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportPackingList()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Packing List");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportLetter()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Letter");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCertificateOfOrigin()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Certificate Of Origin");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCertificateOfInsurace()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Certificate Of Insurance");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBeneficiaryCertificate()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Beneficiary Certificate");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShippersLetterOfInstructions()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shippers Letter Of Instruction");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDocumentaryCollectionForm()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Documentary Collection Form");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBankDraft()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bank Draft");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestWorksheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Worksheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestWorksheet_WithDeclaration()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_JS = GetBusinessObject.PK;

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Worksheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAuthorisationForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Authorization for Service");
			RunDocument();
		}

		#endregion

		#region South Africa docs

		[ExpectNoExceptions]
		public void TestApplicationForRefundDA63()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Application For Refund (DA63)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "MSGBKRCTYAPP=EXPZABLT");
			FilterForMenuItem = filter;
			RunDocument();
		}

		#endregion

		#region Implementation

		ZQuery fFilterForMenuItem;
		BusinessObject fBusinessObject;

		#endregion
	}
}
