using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrder))]
	class WoolworthsOrderStatusTransitionsTestCase : InvoiceOrderLinkTestCase
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
		public void TestStatus_Delivered()
		{
			Enterprise.Registry.Business.OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 1;
			Factory.Save();
			AssertEquals("Status should not be set to delivered yet", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);
			fInvoiceLine.JI_InvoiceQuantity = 2;
			fOrderLine.JO_QtyReceived = 2;
			fOrderLine.Order.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Now);
			fCusContainer.JobContainer.JC_ArrivalCartageComplete = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should now be delivered due to declaration delivered date log", Constants.OrderStatus.Delivered, fOrder.JD_OrderStatus);
			fOrderLine.JO_QtyReceived = 0;
			fOrderLine.JO_QtyInvoiced = 0;
			fCusContainer.Delete();
			fDeliverContainer.Delete();
			fOrderLine.Order.UpdateEvent(Events.DeliveryCartageCompleteFinalised, ZDateTimeOffset.Empty);
			fOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			Factory.Save();
			AssertEquals("Status should no longer be delivered as there are no containers", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);
		}

		public void TestStatus_Delivered_NotWhenNoDeclarationDeliveredLog()
		{
			Enterprise.Registry.Business.OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fDeliverContainer.J5_InstoreDate = ZDateTime.Now;
			fOrderLine.JO_Quantity = 2;
			fOrderLine.JO_QtyReceived = 2;
			Factory.Save();
			AssertEquals("Status should not be set to shipped", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);
		}

		public void TestStatus_PartDelivered()
		{
			fOrderLine.JO_QtyInvoiced = 0;
			fOrderLine.JO_QtyReceived = 0;
			Enterprise.Registry.Business.OrdersDataRegistry.Instance.AutomaticallySetOrderStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			fOrderLine.JO_Quantity = 2;
			Factory.Save();
			AssertEquals("Status should not be set to part delivered yet", Constants.OrderStatus.Incomplete, fOrder.JD_OrderStatus);
			fOrderLine.JO_QtyInvoiced = 10;
			fOrderLine.JO_QtyReceived = 0;
			fCusContainer.JobContainer.JC_ArrivalCartageComplete = ZDateTime.Now;
			Factory.Save();
			AssertEquals("Status should now be part delivered", Constants.OrderStatus.PartDelivered, fOrder.JD_OrderStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var order = Factory.NewWithValidTestData<WoolworthsOrder>();
			return order;
		}
	}
}
