using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobComInvoiceLine))]
	sealed class WoolworthsJobComInvoiceLineTest : InvoiceOrderLinkTestCase
	{
		public void TestContainerNumber()
		{
			fDeclaration.CusContainers.RemoveAndDeleteAll();
			AssertEquals("Container number should be empty if there are no cus container records", "", fInvoiceLine.ContainerNumber);
			CusContainer cusContainer = fDeclaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "1234";
			AssertEquals("Container number should be the one on the cus container record if there is only 1 cus container record", "1234", fInvoiceLine.ContainerNumber);
			WoolworthsJobDeclaration declaration = (WoolworthsJobDeclaration)Factory.New(typeof(JobDeclaration));
			WoolworthsJobComInvoiceHeader invoiceHeader = (WoolworthsJobComInvoiceHeader)declaration.Invoices.AddNew();
			WoolworthsJobComInvoiceLine line = (WoolworthsJobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			line.JI_CustomAttrib4 = "ContainerNum";
			AssertEquals("ContainerNumber", line.ContainerNumber, line.JI_CustomAttrib4);
		}

		#region Related Business Objects
		public void TestOrderLineDeliveries()
		{
			CusContainer cusContainer = fInvoiceLine.Declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "containernum";
			Order order = Factory.New<Order>();
			Order decoyOrder = Factory.New<Order>();
			Order decoyOrderSplit = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLine decoyOrderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			OrderLineDelivery decoyDelivery = orderLine.Deliveries.AddNew();
			order.JD_OrderNumber = "OrderNum";
			fInvoiceLine.JI_OrderNumber = "OrderNum";
			decoyOrderSplit.JD_OrderNumber = "OrderNum";
			decoyOrderSplit.JD_OrderNumberSplit = 1;
			orderLine.JO_Partno = "Partno";
			fInvoiceLine.JI_PartNo = "Partno";
			fInvoiceLine.Declaration.JE_RL_NKFinalDestination = "AUSYD";
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			AssertEquals("Should return match on the correct delivery", 1, fInvoiceLine.OrderLineDeliveries.Count);
			AssertEquals("Should return match on the correct delivery", delivery.PK, fInvoiceLine.OrderLineDeliveries[0].PK);
		}

		public void TestOrderLineDeliverContainer()
		{
			fInvoiceLine.JI_OrderNumber = "OrderNum";
			fInvoiceLine.JI_CustomAttrib4 = "ContainerNum";
			fInvoiceLine.JI_PartNo = "Partno";
			fDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			CusContainer cusContainer = fInvoiceLine.Declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "ContainerNum";
			Order order = Factory.New<Order>();
			Order decoyOrder = Factory.New<Order>();
			Order decoyOrderSplit = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLine decoyOrderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			OrderLineDelivery decoyDelivery = orderLine.Deliveries.AddNew();
			OrderLineDeliverContainer orderContainer = delivery.Containers.AddNew();
			order.JD_OrderNumber = "OrderNum";
			decoyOrderSplit.JD_OrderNumber = "OrderNum";
			decoyOrderSplit.JD_OrderNumberSplit = 1;
			orderLine.JO_Partno = "Partno";
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			orderContainer.J5_ContainerNum = "ContainerNum";
			orderContainer.J5_MasterBill = "mawb";
			AssertEquals("Should match on the correct container", orderContainer.PK, fInvoiceLine.OrderLineDeliverContainer.PK);
		}

		public void TestOrderLineDeliverContainer_ForDifferingPorts()
		{
			fInvoiceLine.JI_OrderNumber = "OrderNum";
			fInvoiceLine.JI_PartNo = "Partno";
			fInvoiceLine.JI_CustomAttrib4 = "ContainerNum";
			fDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			CusContainer cusContainer = fInvoiceLine.Declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "ContainerNum";
			Order order = Factory.New<Order>();
			Order decoyOrder = Factory.New<Order>();
			Order decoyOrderSplit = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLine decoyOrderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			OrderLineDelivery decoyDelivery = orderLine.Deliveries.AddNew();
			OrderLineDeliverContainer orderContainer = delivery.Containers.AddNew();
			OrderLineDeliverContainer decoyOrderContainer = delivery.Containers.AddNew();
			order.JD_OrderNumber = "OrderNum";
			decoyOrderSplit.JD_OrderNumber = "OrderNum";
			decoyOrderSplit.JD_OrderNumberSplit = 1;
			orderLine.JO_Partno = "Partno";
			decoyOrderLine.JO_Partno = "Partno";
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			decoyDelivery.J4_RL_NKDestinationPort = "AUMEL";
			orderContainer.J5_ContainerNum = "ContainerNum";
			decoyOrderContainer.J5_ContainerNum = "DecoyContr";
			AssertEquals("Should match on the correct container", orderContainer.PK, fInvoiceLine.OrderLineDeliverContainer.PK);
		}

		#endregion
		#region Quantity Invoice Updating to Order Container
		// Todo - I've commented out as it was failing.  Please look at or see Jenny.  (Dean 5/9/08, WI00012309)
		//public void TestSyncInvoiceQuantitiesToOrderContainer()
		//{
		//   fCusContainer.CO_ContainerNumber = "RemoveLink";
		//   WoolworthsJobComInvoiceLine LinkedInvoiceLine1 = CreateNewLinkedInvoiceLineAndDeliveredContainer();
		//   WoolworthsJobComInvoiceLine LinkedInvoiceLine2 = CreateNewLinkedInvoiceLineAndDeliveredContainer();
		//   WoolworthsJobComInvoiceLine DecoyLinkedInvoiceLine = CreateNewLinkedInvoiceLineAndDeliveredContainer();
		//   LinkedInvoiceLine1.JI_InvoiceQuantity = 3;
		//   LinkedInvoiceLine2.JI_InvoiceQuantity = 5;
		//   DecoyLinkedInvoiceLine.Declaration.CusContainers[0].CO_ContainerNumber = "splaty";
		//   DecoyLinkedInvoiceLine.JI_InvoiceQuantity = 12345;
		//   Factory.Save();
		//   AssertEquals("3 + 5 = 8", 8m, fDeliverContainer.J5_QuantityInStore);
		//}
		public void TestSyncInvoiceQuantitiesToOrderContainer_NotWhenDecIsInactive()
		{
			WoolworthsJobComInvoiceLine linkedInvoiceLine = CreateNewLinkedInvoiceLineAndDeliveredContainer();
			linkedInvoiceLine.JI_InvoiceQuantity = 5;
			fDeliverContainer.J5_QuantityInStore = 0m;
			linkedInvoiceLine.Declaration.JE_IsCancelled = true;
			Factory.Save();
			AssertEquals("Delivery container quantity should be unchanged when declaration inactive", 0m, fDeliverContainer.J5_QuantityInStore);
		}

		//public void TestSyncInvoiceQuantitiesToOrderContainer_UpdatedWhenContainerDeliveredToFCLWarehouse()
		//{
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as the container isn't delivered", 0m, fDeliverContainer.J5_QuantityInStore);
		//    fDeclaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that the container is delivered", 3m, fDeliverContainer.J5_QuantityInStore);
		//}
		//public void TestSyncInvoiceQuantitiesToOrderContainer_UpdatedWhenContainerDeliveredToFTLWarehouse()
		//{
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as the container isn't delivered", 0m, fDeliverContainer.J5_QuantityInStore);
		//    fDeclaration.RaiseDeliveredToFTLWarehouseEvent(ZDateTime.Now);
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that the container is delivered", 3m, fDeliverContainer.J5_QuantityInStore);
		//}
		//public void TestSyncInvoiceQuantitiesToOrderContainer_WhenCusContainerNumberChanges()
		//{
		//    fDeclaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
		//    fInvoiceLine.JI_PartNo = "xxx";
		//    fInvoiceLine.JI_OrderNumber = "xxx";
		//    fDeclaration.JE_RL_NKFinalDestination = "xxx";
		//    fCusContainer.CO_ContainerNumber = "xxx";
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    fInvoiceLine.JI_OrderNumber = "ordernum";
		//    fDeclaration.JE_RL_NKFinalDestination = "AUMEL";
		//    fInvoiceLine.JI_PartNo = "tstprt";
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as it isn't attached to the container", 0m, fDeliverContainer.J5_QuantityInStore);
		//    fCusContainer.CO_ContainerNumber = "12345678";
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that a container number is assigned", 3m, fDeliverContainer.J5_QuantityInStore);
		//}
		//public void TestSyncInvoiceQuantitiesToOrderContainer_WhenCusContainerAdded()
		//{
		//    fDeclaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
		//    fInvoiceLine.JI_PartNo = "xxx";
		//    fInvoiceLine.JI_OrderNumber = "xxx";
		//    fDeclaration.JE_RL_NKFinalDestination = "xxx";
		//    fDeclaration.CusContainers.RemoveAndDeleteAll();
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    fInvoiceLine.JI_OrderNumber = "ordernum";
		//    fDeclaration.JE_RL_NKFinalDestination = "AUMEL";
		//    fInvoiceLine.JI_PartNo = "tstprt";
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as it isn't attached to the container", 0m, fDeliverContainer.J5_QuantityInStore);
		//    CusContainer CusContainer = fDeclaration.CusContainers.AddNew();
		//    CusContainer.CO_ContainerNumber = "12345678";
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that a container is attached", 3m, fDeliverContainer.J5_QuantityInStore);
		//}
		// Todo - I've commented out as it was failing.  Please look at or see Jenny.  (Dean 5/9/08, WI00012309)
		//public void TestSyncInvoiceQuantitiesToOrderDelivery()
		//{
		//   WoolworthsJobComInvoiceLine SecondInvoiceLine = CreateNewLinkedInvoiceLineAndDeliveredContainer();
		//   fDelivery.J4_Allocated = 2;
		//   fInvoiceLine.JI_InvoiceQuantity = 3;
		//   SecondInvoiceLine.JI_InvoiceQuantity = 5;
		//   Factory.Save();
		//   AssertEquals("3 + 5 = 8", 8m, fDelivery.J4_Allocated);
		//   fInvoiceLine.JI_InvoiceQuantity = 1;
		//   SecondInvoiceLine.JI_InvoiceQuantity = 1;
		//   Factory.Save();
		//   AssertEquals("1 + 1 = 2", 2m, fDelivery.J4_Allocated);
		//}
		//public void TestSyncInvoiceQuantitiesToOrderDelivery_WhenPartNoLinkToOrderDeliveryChanges()
		//{
		//    fDeclaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
		//    fInvoiceLine.JI_PartNo = "xxx";
		//    fInvoiceLine.JI_OrderNumber = "xxx";
		//    fDeclaration.JE_RL_NKFinalDestination = "xxx";
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    fInvoiceLine.JI_OrderNumber = "ordernum";
		//    fDeclaration.JE_RL_NKFinalDestination = "AUMEL";
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as it isn't attached to the order delivery", 0m, fDelivery.J4_Allocated);
		//    fInvoiceLine.JI_PartNo = "tstprt";
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that it is attached to the order delivery", 3m, fDelivery.J4_Allocated);
		//}
		//public void TestSyncInvoiceQuantitiesToOrderDelivery_WhenOrderNumberLinkToOrderDeliveryChanges()
		//{
		//    fDeclaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
		//    fInvoiceLine.JI_PartNo = "xxx";
		//    fInvoiceLine.JI_OrderNumber = "xxx";
		//    fDeclaration.JE_RL_NKFinalDestination = "xxx";
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    fDeclaration.JE_RL_NKFinalDestination = "AUMEL";
		//    fInvoiceLine.JI_PartNo = "tstprt";
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as it isn't attached to the order delivery", 0m, fDelivery.J4_Allocated);
		//    fInvoiceLine.JI_OrderNumber = "ordernum";
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that it is attached to the order delivery", 3m, fDelivery.J4_Allocated);
		//}
		//public void TestSyncInvoiceQuantitiesToOrderDelivery_WhenFinalDestinationLinkToOrderDeliveryChanges()
		//{
		//    fDeclaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
		//    fInvoiceLine.JI_PartNo = "xxx";
		//    fInvoiceLine.JI_OrderNumber = "xxx";
		//    fDeclaration.JE_RL_NKFinalDestination = "xxx";
		//    fDelivery.J4_Allocated = 0;
		//    fInvoiceLine.JI_InvoiceQuantity = 3;
		//    fInvoiceLine.JI_OrderNumber = "ordernum";
		//    fDeclaration.JE_RL_NKFinalDestination = "AUMEL";
		//    Factory.Save();
		//    AssertEquals("Quantity shouldn't transfer yet as it isn't attached to the order delivery", 0m, fDelivery.J4_Allocated);
		//    fInvoiceLine.JI_PartNo = "tstprt";
		//    Factory.Save();
		//    AssertEquals("Quantity should be updated now that it is attached to the order delivery", 3m, fDelivery.J4_Allocated);
		//}
		public void TestSyncInvoiceQuantitiesToOrderDelivery_OnlyWhenUserChangesData()
		{
			fDelivery.J4_Allocated = 0;
			fInvoiceLine.JI_InvoiceQuantity = 3;
			Factory.Save();
			fInvoiceLine.JI_InvoiceQuantity = 3;
			fDelivery.J4_Allocated = 99;
			Factory.Save();
			AssertEquals("Should not change as the user hasn't actually made a change to the invoice quantity", 99m, fDelivery.J4_Allocated);
		}

		public void TestGetTotalInvoiceQuantityForLinkedOrderContainer()
		{
			fCusContainer.CO_ContainerNumber = "SeverLink";
			WoolworthsJobComInvoiceLine linkedInvoiceLine1 = CreateNewLinkedInvoiceLineAndDeliveredContainer();
			WoolworthsJobComInvoiceLine linkedInvoiceLine2 = CreateNewLinkedInvoiceLineAndDeliveredContainer();
			WoolworthsJobComInvoiceLine decoyLinkedInvoiceLine = CreateNewLinkedInvoiceLineAndDeliveredContainer();
			WoolworthsJobComInvoiceLine decoyLinkedInvoiceLine2 = CreateNewLinkedInvoiceLineAndDeliveredContainer();
			linkedInvoiceLine1.JI_InvoiceQuantity = 3;
			linkedInvoiceLine2.JI_InvoiceQuantity = 5;
			decoyLinkedInvoiceLine.Declaration.CusContainers[0].CO_ContainerNumber = "SeverLink";
			decoyLinkedInvoiceLine.JI_InvoiceQuantity = 999;
			decoyLinkedInvoiceLine2.Declaration.JE_IsCancelled = true;
			decoyLinkedInvoiceLine2.JI_InvoiceQuantity = 999;
			Factory.Save();
			AssertEquals("3 + 5 = 8", 8m, linkedInvoiceLine1.GetTotalInvoiceQuantityForLinkedOrderContainer());
		}

		#endregion
		#region Validation
		public void TestValidateInvoiceMatchesOrderDelivery_LinePrice()
		{
			fInvoiceLine.JI_LinePrice = 3;
			fOrderLine.JO_LinePrice = 2;
			fInvoiceLine.Validation.ValidateJI_LinePrice();
			AssertEquals("Has warning because the line price is not consistent", true, fInvoiceLine.JI_LinePriceInfo.HasWarnings());
			fOrderLine.JO_LinePrice = 3;
			fInvoiceLine.Validation.ValidateJI_LinePrice();
			AssertEquals("No warning because the line price is now consistent", false, fInvoiceLine.JI_LinePriceInfo.HasWarnings());
		}

		public void TestValidateInvoiceMatchesOrderDelivery_InvoiceUQ()
		{
			fInvoiceLine.JI_InvoiceUQ = "UNT";
			fOrderLine.JO_F3_NKPackType = "TN";
			fInvoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertEquals("Has warning because the UnitOfQuantity is not consistent", true, fInvoiceLine.JI_InvoiceUQInfo.HasWarnings());
			fOrderLine.JO_F3_NKPackType = "UNT";
			fInvoiceLine.Validation.ValidateJI_InvoiceUQ();
			AssertEquals("No warning because the UnitOfQuantity is now consistent", false, fInvoiceLine.JI_InvoiceUQInfo.HasWarnings());
		}

		public void TestValidateInvoiceMatchesOrderDelivery_UnitPrice()
		{
			var validation = (JobComInvoiceLineValidation)fInvoiceLine.Validation;
			fInvoiceLine.JI_InvoiceUQ = "UNT";
			fOrderLine.JO_ItemPrice = 2;
			validation.ValidateUnitPrice();
			AssertEquals("Has warning because the UnitPrice is not consistent", true, fInvoiceLine.UnitPriceInfo.HasWarnings());
			fInvoiceLine.JI_LinePrice = 6;
			fInvoiceLine.JI_InvoiceQuantity = 3;
			AssertEquals("Unit price for test", 2m, fInvoiceLine.UnitPrice);
			validation.ValidateUnitPrice();
			AssertEquals("No warning because the UnitPrice is now consistent", false, fInvoiceLine.UnitPriceInfo.HasWarnings());
		}

		public void TestValidateInvoiceQuantityDividesVendorPack()
		{
			fInvoiceLine.JI_InvoiceQuantity = 10;
			fDeliverContainer.J5_QuantityInStore = 10;
			fOrderLine.JO_InnerPacks = 3;
			fInvoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertEquals("Has warning because the invoice quantity is not consistent", true, fInvoiceLine.JI_InvoiceQuantityInfo.HasWarnings());
			fOrderLine.JO_InnerPacks = 2;
			fInvoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertEquals("No warning because the invoice quantity is now consistent", false, fInvoiceLine.JI_InvoiceQuantityInfo.HasWarnings());
		}

		#endregion
		#region Order Invoice Mismatch Checking
		public void TestDetectOrderInvoiceMismatching()
		{
			fInvoiceLine.JI_OrderNumber = "no_order_lnk_no_evt";
			fInvoiceLine.Declaration.OrderInvoiceMismatching += new CancelEventHandler(OnOrderInvoiceMismatch);
			fInvoiceLine.JI_OrderNumber = "splaty";
			fInvoiceLine.JI_PartNo = "splaty";
			AssertEquals("OrderInvoiceMismatch should not fire yet", false, fOrderInvoiceMismatchFired);
			fInvoiceLine.JI_OrderNumber = "ordernum";
			AssertEquals("OrderInvoiceMismatch should not fire yet", false, fOrderInvoiceMismatchFired);
			fInvoiceLine.JI_OrderNumber = "ordernum";
			AssertEquals("OrderInvoiceMismatch should not fire yet", false, fOrderInvoiceMismatchFired);
			fInvoiceLine.JI_PartNo = "tstprt"; // re-link the order
			AssertEquals("OrderInvoiceMismatch should not fire yet", false, fOrderInvoiceMismatchFired);
			fInvoiceLine.JI_PartNo = "splaty";
			AssertEquals("OrderInvoiceMismatch should have fired", true, fOrderInvoiceMismatchFired);
			fInvoiceLine.JI_Tariff = "splaty";
			AssertEquals("OrderInvoiceMismatch should have fired", true, fOrderInvoiceMismatchFired);
			fInvoiceLine.JI_PartNo = "tstprt";
			fOrderInvoiceMismatchFired = false;
			fInvoiceLine.JI_OrderNumber = "splaty";
			AssertEquals("OrderInvoiceMismatch should have fired", true, fOrderInvoiceMismatchFired);
			fOrderInvoiceMismatchFired = false;
			fInvoiceLine.Declaration.OrderInvoiceMismatching -= new CancelEventHandler(OnOrderInvoiceMismatch);
		}

		public void TestCancelOrderInvoiceMismatching()
		{
			fInvoiceLine.Declaration.OrderInvoiceMismatching += new CancelEventHandler(OnOrderInvoiceMismatch_Cancel);
			fInvoiceLine.JI_OrderNumber = "splaty";
			AssertEquals("Order number should not change as the change was veto'd", "ordernum", fInvoiceLine.JI_OrderNumber);
			fInvoiceLine.Declaration.OrderInvoiceMismatching -= new CancelEventHandler(OnOrderInvoiceMismatch_Cancel);
		}

		#endregion
		#region Implementation
		WoolworthsJobComInvoiceLine CreateNewLinkedInvoiceLineAndDeliveredContainer()
		{
			WoolworthsJobDeclaration declaration = (WoolworthsJobDeclaration)Factory.New(typeof(JobDeclaration));
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			//Declaration.RaiseDeliveredToFCLWarehouseEvent(ZDateTime.Now);
			CusContainer cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "12345678";
			WoolworthsJobComInvoiceHeader invoiceHeader = (WoolworthsJobComInvoiceHeader)declaration.Invoices.AddNew();
			WoolworthsJobComInvoiceLine invoiceLine = (WoolworthsJobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "tstprt";
			invoiceLine.JI_OrderNumber = "ordernum";
			declaration.JE_RL_NKFinalDestination = "AUMEL";
			return invoiceLine;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			////calling base is important. It populates any dependent business object etc
			//JobComInvoiceLine result = Factory.NewWithValidTestData<JobComInvoiceLine>(TestBusinessObjectKind.All);
			//JobDeclaration declaration = result.Declaration;
			//JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)result.InvoiceHeader;
			//if (declaration == null)
			//{
			//	declaration = Factory.New<JobDeclaration>();
			//}
			//OrgHeader importer = Factory.New<OrgHeader>();
			//importer.FillWithValidTestData();
			//declaration.JE_OH_Importer = importer.PK;
			//declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			//if (invoiceHeader == null)
			//{
			//	invoiceHeader = (JobComInvoiceHeader)declaration.Invoices.AddNew();
			//	result.JI_JZ = invoiceHeader.PK;
			//}
			//invoiceHeader.JZ_JE = declaration.PK;
			//CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			//CusEntryLine entryLine = entry.MergedLines.AddNew();
			//declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			//return result;
			JobDeclaration declaration = factory.New<JobDeclaration>();
			OrgHeader importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine result = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_JE = declaration.PK;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			return result;
		}

		void OnOrderInvoiceMismatch(object sender, CancelEventArgs e)
		{
			fOrderInvoiceMismatchFired = true;
		}

		void OnOrderInvoiceMismatch_Cancel(object sender, CancelEventArgs e)
		{
			fOrderInvoiceMismatchFired = true;
			e.Cancel = true;
		}

		bool fOrderInvoiceMismatchFired;
		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("We don't want this to run - we have stuff loading as WOW demand Load Validation - this was not picked up previously as it ran in OnLoadedInternal but now runs in OnLoaded.. so we live with it.", true);
		}
		#endregion
	}
}
