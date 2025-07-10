using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class CartageAdviceRunDocsTest : BaseRunDocumentsTest
	{
		public CartageAdviceRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get { return fBusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return fBusinessContext; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		#region Shipment Cartage Advice

		#region With Receipt

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceWithReceiptForFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceWithReceiptForNotFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceWithReceiptForFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceWithReceiptForNotFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Without Receipt

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceForFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceForNotFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceForFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestShipmentCartageAdviceForNotFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Combined Cartage Advice

		#region With Receipt

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceWithReceiptForFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceWithReceiptForNotFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceWithReceiptForFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceWithReceiptForNotFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Without Receipt

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceForFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceForNotFCLImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Arrival");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceForFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";

			var packLine1 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCombinedShipmentCartageAdviceForNotFCLExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");

			FilterForMenuItem = filter;
			fBusinessContext = BusinessContext.Shipment;
			fBusinessObject = shipment;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#endregion

		#endregion

		#region CFS Cartage Advice

		[ExpectNoExceptions]
		public void TestCFSCartageAdviceForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Arrival");
			fFilterForMenuItem = filter;
			fBusinessObject = Factory.New(typeof(CFSContainer));
			fBusinessContext = BusinessContext.CFSContainerRego;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCFSCartageAdviceForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Departure");
			fFilterForMenuItem = filter;
			fBusinessObject = Factory.New(typeof(CFSContainer));
			fBusinessContext = BusinessContext.CFSContainerRego;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCFSCartageAdviceWithReceiptForImport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice With Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Arrival");
			fFilterForMenuItem = filter;
			fBusinessObject = Factory.New(typeof(CFSContainer));
			fBusinessContext = BusinessContext.CFSContainerRego;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCFSCartageAdviceWithReceiptForExport()
		{
			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice With Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.StartsWith, "Departure");
			fFilterForMenuItem = filter;
			fBusinessObject = Factory.New(typeof(CFSContainer));
			fBusinessContext = BusinessContext.CFSContainerRego;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Booking Cartage Advice

		#region With Receipt

		[ExpectNoExceptions]
		public void TestBookingCartageAdviceWithReceiptForFCLExport()
		{
			var booking = Factory.New<CommonShipment>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "Container";

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");

			FilterForMenuItem = filter;
			fBusinessObject = booking;
			fBusinessContext = BusinessContext.QuotedBooking;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBookingCartageAdviceWithReceiptForNotFCLExport()
		{
			var booking = Factory.New<CommonShipment>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice with Receipt");

			FilterForMenuItem = filter;
			fBusinessObject = booking;
			fBusinessContext = BusinessContext.QuotedBooking;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Without Receipt

		[ExpectNoExceptions]
		public void TestBookingCartageAdviceForFCLExport()
		{
			CommonShipment booking = Factory.New<CommonShipment>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "Container";

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			FilterForMenuItem = filter;
			fBusinessObject = booking;
			fBusinessContext = BusinessContext.QuotedBooking;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestBookingCartageAdviceForNotFCLExport()
		{
			CommonShipment booking = Factory.New<CommonShipment>();
			booking.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cartage Advice");

			FilterForMenuItem = filter;
			fBusinessObject = booking;
			fBusinessContext = BusinessContext.QuotedBooking;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#endregion

		#region Cartage Cartage Advice

		#region Cartage Cartage Advice

		#region With Receipt

		[ExpectNoExceptions]
		public void TestCartageCartageAdviceWithReceiptForFCLExport()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.FillWithValidTestData();

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Cartage Advice With Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Equal, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageCartageAdviceWithReceiptForNotFCLExport()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Cartage Advice With Receipt");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Equal, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Without Receipt

		[ExpectNoExceptions]
		public void TestCartageCartageAdviceForFCLExport()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.FillWithValidTestData();

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Equal, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageCartageAdviceForNotFCLExport()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Cartage Advice");
			filter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Equal, "\"<UseDocBuilderFreightDocs>\" != \"Y\"");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#endregion

		#region Cartage Combined Cartage Advice

		#region With Receipt

		[ExpectNoExceptions]
		public void TestCartageCombinedCartageAdviceWithReceiptForFCLExport()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.FillWithValidTestData();

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageCombinedCartageAdviceWithReceiptForNotFCLExport()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice with Receipt");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Without Receipt

		[ExpectNoExceptions]
		public void TestCartageCombinedCartageAdviceForFCLExport()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.FillWithValidTestData();

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCartageCombinedCartageAdviceForNotFCLExport()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;

			ZQuery filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Combined Cartage Advice");

			FilterForMenuItem = filter;
			fBusinessObject = cartage;
			fBusinessContext = BusinessContext.Cartage;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#endregion

		#endregion

		#region ContainerLeg Cartage Advice

		#region ContainerLeg Cartage Advice

		#region With Receipt

		[ExpectNoExceptions]
		public void TestContainerLegCartageAdviceWithReceipt()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.FillWithValidTestData();

			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg containerLeg = move.CartageLegs.AddNew();
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Cartage Advice With Receipt");

			fBusinessObject = containerLeg;
			fBusinessContext = BusinessContext.ContainerLeg;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#region Without Receipt

		[ExpectNoExceptions]
		public void TestContainerLegCartageAdvice()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.FillWithValidTestData();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.FillWithValidTestData();

			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg containerLeg = move.CartageLegs.AddNew();
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Cartage Advice");

			fBusinessObject = containerLeg;
			fBusinessContext = BusinessContext.ContainerLeg;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		ZQuery fFilterForMenuItem;
		BusinessObject fBusinessObject;
		BusinessContext fBusinessContext;

		#endregion
	}
}
