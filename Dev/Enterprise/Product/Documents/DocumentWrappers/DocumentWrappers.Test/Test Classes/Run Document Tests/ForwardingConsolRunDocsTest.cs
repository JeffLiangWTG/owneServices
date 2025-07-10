using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class ForwardingConsolRunDocsTest : BaseRunDocumentsTest
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Consol; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestConsolNotes()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Consol Notes");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAgentDepartureNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Agent Departure Notice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAgentInstruction()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Agents Instruction");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNoticeAndChargeSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Arrival Notice and Charge Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestArrivalNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Arrival Notice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBookingConfirmation()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Booking Confirmation");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCargoLoadList()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cargo Load List");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCargoManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cargo Manifest");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageAdviceWithReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice With Receipt");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestChargeSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Charge Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCoLoadInterimReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Co-Load Interim Receipt");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestImportContainerManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Container Manifest");
			FilterForMenuItem.AddToFilter(StmMenuItemSchema.SU_DocumentDirection, "ARV");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportContainerManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Container Manifest");
			FilterForMenuItem.AddToFilter(StmMenuItemSchema.SU_DocumentDirection, "DEP");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestConsolCoverSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Consol Cover Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDelayAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delay Alert");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDocumentsAvailableNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Documents Available Notice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDeliveryOrder()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Delivery Order");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportReceivalAdvice()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Receival Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "AUNOTBLK");
			FilterForMenuItem = filter;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportReceivalAdviceForBBK()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Receival Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "AUBLK");
			FilterForMenuItem = filter;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestForwardingInstructionDetailed()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Instruction Detailed");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestForwardingInstructionStandard()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Forwarding Instruction Standard");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLetterToOverseasAgent()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Letter To Overseas Agent");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestManifestLandscapeForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Manifest Standard (Landscape)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsClientSpecific, SQLComparisonOperator.Equal, "N");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestManifestLandscapeDetailedForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Manifest Detailed (Landscape)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsClientSpecific, SQLComparisonOperator.Equal, "N");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestManifestLandscapeForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Manifest Standard (Landscape)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsClientSpecific, SQLComparisonOperator.Equal, "N");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestManifestLandscapeDetailedForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Manifest Detailed (Landscape)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsClientSpecific, SQLComparisonOperator.Equal, "N");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestManifestPortraitForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Manifest (Portrait)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestManifestPortraitForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Manifest (Portrait)");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestMYInwardManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "MY Inward Manifest");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestOutturnReport()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Outturn Report");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestPreAlert()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Pre-Alert");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipperDepartureNotice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipper Departure Notice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShippingAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shipping Advice");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportSummaryManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Summary Manifest");
			FilterForMenuItem.AddToFilter(new ZQuery(StmMenuItemSchema.SU_DocumentDirection, DocumentEngineCore.DocumentSupport.DocumentDirection.DEP));
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestImportSummaryManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Summary Manifest");
			FilterForMenuItem.AddToFilter(new ZQuery(StmMenuItemSchema.SU_DocumentDirection, DocumentEngineCore.DocumentSupport.DocumentDirection.ARV));
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestAWBBarcodeLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "AWB Barcode Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLaserHAWB()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Laser HAWB");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLaserMAWB()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Laser MAWB");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNeutralHAWB()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Neutral HAWB");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestNeutralMAWB()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Neutral MAWB");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTranshipmentManifest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Transhipment List");
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
		public void TestUltimateConsignorNotification()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Ultimate Consignor Notification");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestImportCargoLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Import Cargo Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForProfitShare()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Profit Share");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTimeSlotRequest()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Time Slot Request");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForMissingDocuments()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Missing Documents");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestDockReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Dock Receipt");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#region Icelandic docs

		[ExpectNoExceptions]
		public void TestAirFreightPreManifestDeparture()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Airfreight Pre-Manifest");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			FilterForMenuItem = filter;
			RunDocument();
		}

		public void TestAirFreightPreManifestArrival()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Airfreight Pre-Manifest");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestUppskiptingarbeidniDeparture()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Uppskiptingarbeidni");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			FilterForMenuItem = filter;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestUppskiptingarbeidniArrival()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Uppskiptingarbeidni");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");
			FilterForMenuItem = filter;
			RunDocument();
		}

		#endregion

		#region Implementation

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				return consol;
			}
		}

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
