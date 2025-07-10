using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrder))]
	public class WoolworthsOrder_InvoiceOrderLinkTestCase : InvoiceOrderLinkTestCase
	{
		#region Metadata
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Enterprise.Client.Wow.Metadata.WoolworthsOrder);
			}
		}

		#endregion
		public void TestInvoiceOrderMismatchingEvent_EmptyOrderNumber()
		{
			fInvoiceLine.JI_OrderNumber = "";
			fOrder.InvoiceOrderMismatching += new CancelEventHandler(OnInvoiceOrderMismatching);
			fOrder.JD_OrderNumber = "";
			AssertEquals("No link severed as the order number is empty", false, fInvoiceOrderMismatchingFired);
		}

		public void TestHasJobComInvoiceLineLink_WhenDeclarationInactive()
		{
			TestWoolworthsOrder order = Factory.Load<TestWoolworthsOrder>(fOrder.PK);
			AssertEquals("Should have a link to the invoice line initially", true, order.HasJobComInvoiceLineLink);
			fDeclaration.JE_IsCancelled = true;
			AssertEquals("Should not a link now that the declaration has been made inactive", false, order.HasJobComInvoiceLineLink);
		}

		public void TestInvoiceOrderMismatchingEvent_OrderNumber()
		{
			fOrder.InvoiceOrderMismatching += new CancelEventHandler(OnInvoiceOrderMismatching);
			fOrder.JD_OrderNumber = "LinkSevered";
			AssertEquals("Link severed", true, fInvoiceOrderMismatchingFired);
			fInvoiceOrderMismatchingFired = false;
			fOrder.JD_OrderNumber = "AlreadySeveredNoEffect";
			AssertEquals("Link already severed so no event", false, fInvoiceOrderMismatchingFired);
			fOrder.InvoiceOrderMismatching -= new CancelEventHandler(OnInvoiceOrderMismatching);
		}

		public void TestInvoiceOrderMismatchingEvent_Partno()
		{
			fOrder.InvoiceOrderMismatching += new CancelEventHandler(OnInvoiceOrderMismatching);
			fOrderLine.JO_Partno = "LinkSevered";
			AssertEquals("Link severed", true, fInvoiceOrderMismatchingFired);
			fInvoiceOrderMismatchingFired = false;
			fOrderLine.JO_Partno = "AlreadySeveredNoEffect";
			AssertEquals("Link already severed so no event", false, fInvoiceOrderMismatchingFired);
			fOrder.InvoiceOrderMismatching -= new CancelEventHandler(OnInvoiceOrderMismatching);
		}

		public void TestInvoiceOrderMismatchingEvent_PartnoWhenOrderNumberEmpty()
		{
			fOrder.JD_OrderNumber = "";
			fInvoiceLine.JI_OrderNumber = "";
			fOrder.InvoiceOrderMismatching += new CancelEventHandler(OnInvoiceOrderMismatching);
			fOrderLine.JO_Partno = "splaty";
			AssertEquals("Link not severed as order number is empty", false, fInvoiceOrderMismatchingFired);
		}

		public void TestInvoiceOrderMismatchingEvent_DestinationPort()
		{
			fOrder.InvoiceOrderMismatching += new CancelEventHandler(OnInvoiceOrderMismatching);
			fDelivery.J4_RL_NKDestinationPort = "Sevrd";
			AssertEquals("Link severed", true, fInvoiceOrderMismatchingFired);
			fInvoiceOrderMismatchingFired = false;
			fDelivery.J4_RL_NKDestinationPort = "Alred";
			AssertEquals("Link already severed so no event", false, fInvoiceOrderMismatchingFired);
			fOrder.InvoiceOrderMismatching -= new CancelEventHandler(OnInvoiceOrderMismatching);
		}

		public void TestInvoiceOrderMismatchingEvent_WhenDeclarationInactive()
		{
			fOrder.InvoiceOrderMismatching += new CancelEventHandler(OnInvoiceOrderMismatching);
			fDeclaration.JE_IsCancelled = true;
			fOrder.JD_OrderNumber = "LinkSevered";
			AssertEquals("Link not severed because link doesnt exist", false, fInvoiceOrderMismatchingFired);
			fOrder.InvoiceOrderMismatching -= new CancelEventHandler(OnInvoiceOrderMismatching);
		}

		public void TestIndentOrVendorOrderRequestedEvent()
		{
			WoolworthsOrder order = (WoolworthsOrder)GetNewBusinessObject();
			order.JD_OrderNumber = "1234";
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			order.IndentOrVendorOrderRequested += new CancelEventHandler(OnDeclaration_IndentOrVendorOrderRequested);
			AssertEquals("Shouldn't have fired before running test", false, fIndentOrVendorOrderRequestedFired);
			order.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.Order, null);
			AssertEquals("Shouldn't fire for just any old order report", false, fIndentOrVendorOrderRequestedFired);
			order.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.WowIndentOrder, null);
			AssertEquals("Should fire for the Woolworths indent or vendor report", true, fIndentOrVendorOrderRequestedFired);
		}

		public void TestShouldSendToEdiTrack()
		{
			fOrder.JD_OrderStatus = Constants.OrderStatus.Confirmed;
			AssertEquals("Don't send to ediTrack", false, fOrder.ShouldSendToEdiTrack());
			fDeliverContainer.Delete();
			AssertEquals("Don't send to ediTrack", false, fOrder.ShouldSendToEdiTrack());
			fCusContainer.Delete();
			fInvoiceLine.Delete();
			AssertEquals("Send to ediTrack", true, fOrder.ShouldSendToEdiTrack());
		}

		#region Test Classes
		class TestWoolworthsOrder : WoolworthsOrder
		{
			public TestWoolworthsOrder(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new bool HasJobComInvoiceLineLink
			{
				get
				{
					return base.HasJobComInvoiceLineLink;
				}
			}
		}

		#endregion
		#region Implementation
		bool fInvoiceOrderMismatchingFired;
		bool fIndentOrVendorOrderRequestedFired;
		void OnInvoiceOrderMismatching(object sender, CancelEventArgs e)
		{
			fInvoiceOrderMismatchingFired = true;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<Order>();
		}

		void OnDeclaration_IndentOrVendorOrderRequested(object sender, CancelEventArgs e)
		{
			fIndentOrVendorOrderRequestedFired = true;
		}
		#endregion
	}
}
