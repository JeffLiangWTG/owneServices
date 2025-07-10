using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsJobDeclaration))]
	public class WoolworthsJobDeclarationTest : InvoiceOrderLinkTestCase
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Enterprise.Metadata.Business.AUJobDeclaration);
			}
		}

		#endregion
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestDetectOrderInvoiceMismatching()
		{
			fDeclaration.JE_RL_NKFinalDestination = "splat";
			fInvoiceLine.Declaration.OrderInvoiceMismatching += new CancelEventHandler(OnOrderInvoiceMismatch);
			fDeclaration.JE_RL_NKFinalDestination = "spla2";
			AssertEquals("OrderInvoiceMismatch should not fire yet", false, fOrderInvoiceMismatchFired);
			fDeclaration.JE_RL_NKFinalDestination = "AUMEL"; // rejoin to order
			AssertEquals("OrderInvoiceMismatch should not fire yet", false, fOrderInvoiceMismatchFired);
			fDeclaration.JE_RL_NKFinalDestination = "splat";
			AssertEquals("OrderInvoiceMismatch should fire", true, fOrderInvoiceMismatchFired);
			fInvoiceLine.Declaration.OrderInvoiceMismatching -= new CancelEventHandler(OnOrderInvoiceMismatch);
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CusContainer cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "1234";
			Factory.Save();
			declaration.Delete();
			Factory.Save();
		}

		public void TestLoadDeclarationDontLoadLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			BusinessObjectFactory retrievingFactory = new BusinessObjectFactory();
			JobDeclaration retrievedDeclaration = retrievingFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Shouldn't automatically load invoice lines when loading declaration", false, retrievedDeclaration.Invoices[0].IsInvoiceLinesLoaded());
		}

		public void TestMarkOrderContainersAsDelivered_WhenContainerDeliveredLogCreated()
		{
			fDeliverContainer.J5_ContainerNum = "container";
			fCusContainer.CO_ContainerNumber = "container";
			fInvoiceLine.JI_InvoiceQuantity = 10;
			fOrderLine.JO_Quantity = 10;
			fOrderLine.JO_QtyReceived = 0;
			fDeliverContainer.J5_QuantityInvoiced = 10;
			fDeliverContainer.J5_QuantityInStore = 0;
			StmALog containerUnpackLog = fDeclaration.Logs.AddNew(Events.ContainerUnpack);
			AssertEquals("Container should not yet be marked as delivered", true, fDeliverContainer.J5_QuantityInvoiced != fDeliverContainer.J5_QuantityInStore);
			AssertEquals("Order line should not yet be marked as delivered", true, fOrderLine.JO_Quantity != fOrderLine.JO_QtyReceived);
			Factory.Save();
			AssertEquals("Container should be marked as delivered now", fDeliverContainer.J5_QuantityInvoiced, fDeliverContainer.J5_QuantityInStore);
			AssertEquals("Order line should be marked as delivered now, the delivered quantities should cascade from container->delivery->orderline", fOrderLine.JO_Quantity, fOrderLine.JO_QtyReceived);
		}

		public void TestMarkOrderContainerAsDelivered_NotIfDecInactive()
		{
			fDeliverContainer.J5_ContainerNum = "container";
			fCusContainer.CO_ContainerNumber = "container";
			fInvoiceLine.JI_InvoiceQuantity = 10;
			fOrderLine.JO_Quantity = 10;
			fOrderLine.JO_QtyReceived = 0;
			fDeliverContainer.J5_QuantityInvoiced = 10;
			fDeliverContainer.J5_QuantityInStore = 0;
			StmALog containerUnpackLog = fDeclaration.Logs.AddNew(Events.ContainerUnpack);
			fDeclaration.JE_IsCancelled = true;
			Factory.Save();
			AssertEquals("Container should NOT be marked as delivered because the dec is inactive", 0m, fDeliverContainer.J5_QuantityInStore);
		}

		public void TestOnFactorySavingBeforeTransactionCoreDoNotPopulateToJP_DeliveryCartageAdvised()
		{
			WoolworthsJobDeclaration declaration = Factory.New<WoolworthsJobDeclaration>();
			WoolworthsCusContainer cusContainer = (WoolworthsCusContainer)declaration.CusContainers.AddNew();
			ZDateTime currentDateTime = ZDateTime.Now;
			cusContainer.JobContainer.JC_ArrivalCartageAdvised = currentDateTime;
			declaration.HasChanges = true;
			declaration.JE_MessageType = "IMP";
			Factory.Save();
			AssertEquals(true, declaration.DocsAndCartage.JP_DeliveryCartageAdvised.IsEmpty && declaration.DocsAndCartage.JP_DeliveryCartageAdvised != currentDateTime);
		}

		#region Implementation
		protected bool fOrderInvoiceMismatchFired;
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>();
		}

		protected void OnOrderInvoiceMismatch(object sender, CancelEventArgs e)
		{
			fOrderInvoiceMismatchFired = true;
		}
		#endregion
	}
}
