using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrdersDataRegistry))]
	sealed class OrdersDataRegistryTest : RegistryItemSetTestCaseWithFactory<OrdersDataRegistry>
	{
		#region Order Line Quantity Remaining Management

		public void TestOrderLineQtyRemainingManagement()
		{
			AssertEquals("Default value", true, ItemSet.OrderLineQtyRemainingManagement.DefaultValue);
			ItemSet.OrderLineQtyRemainingManagement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("New value", false, ItemSet.OrderLineQtyRemainingManagement.Value);
		}

		#endregion

		#region Automatically Set Order Status

		public void TestAutomaticallySetOrderStatus()
		{
			TestRegistryItem(ItemSet.AutomaticallySetOrderStatus,
				"AutomaticallySetOrderStatus",
				"Orders",
				"Automatically Set Order Status",
				"Switch this registry on to automatically set order with 'order manager lite' status:  CNF, SHP, PRT, and DLV.\r\n\r\nThis setting is not applied when an order is processed through Advanced Order Manager.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				false);
		}

		#endregion

		#region IncludeChangesNotApplied

		public void IncludeChangesNotApplied()
		{
			AssertEquals("Default value", false, ItemSet.IncludeChangesNotApplied.DefaultValue);
			ItemSet.IncludeChangesNotApplied.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("New value", true, ItemSet.IncludeChangesNotApplied.Value);
		}

		#endregion

		#region Order Import Report Type

		public void TestOrderImportReportAttachmentType()
		{
			AssertEquals("Default value", OrgConstants.AttachmentType.PDF, ItemSet.OrderImportReportAttachmentType.DefaultValue);
			ItemSet.OrderImportReportAttachmentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgConstants.AttachmentType.TIF);
			AssertEquals("New value", OrgConstants.AttachmentType.TIF, ItemSet.OrderImportReportAttachmentType.Value);

			Assert(OrdersDataRegistry.Instance.OrderImportReportAttachmentTypeList.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert(OrdersDataRegistry.Instance.OrderImportReportAttachmentTypeList.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert(OrdersDataRegistry.Instance.OrderImportReportAttachmentTypeList.ContainsCode(OrgConstants.AttachmentType.TIF));
		}

		#endregion

		#region Allow Exported Order Lines to be Deleted

		public void TestAllowExportedOrderLinesToBeDeleted()
		{
			AssertEquals("Default value", true, ItemSet.AllowExportedOrderLinesToBeDeleted.DefaultValue);
			ItemSet.AllowExportedOrderLinesToBeDeleted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("New value", false, ItemSet.AllowExportedOrderLinesToBeDeleted.Value);
		}

		#endregion

		#region Order Line Status Field Editable

		public void TestOrderLineStatusEditable()
		{
			AssertEquals("Order Line Status editable by default", true, ItemSet.OrderLineStatusEditable.DefaultValue);
			AssertEquals("Value MUST be cached", false, (ItemSet.OrderLineStatusEditable.Options & RegistryOptions.NotCached) == RegistryOptions.NotCached);

			ItemSet.OrderLineStatusEditable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("New value", false, ItemSet.OrderLineStatusEditable.Value);
		}

		#endregion

		#region Suppress Xml triggers on orders with zero quantity

		public void TestSuppressXmlTriggersOnOrdersWithZeroQuantity()
		{
			AssertEquals("Suppress Xml Triggers on orders with zero quantity by default", false, ItemSet.SuppressXmlTriggersOnOrdersWithZeroQuantity.DefaultValue);
			ItemSet.SuppressXmlTriggersOnOrdersWithZeroQuantity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("New value", true, ItemSet.SuppressXmlTriggersOnOrdersWithZeroQuantity.Value);
		}

		#endregion

		#region ShowOrderMilestones

		public void TestShowOrderMilestonesOnMainScreen()
		{
			TestRegistryItem(ItemSet.ShowOrderMilestonesOnMainScreen,
				"ShowOrderMilestonesOnMainScreen",
				RawDataRegistry.Categories.Orders,
				"Show Order Milestones on Main Order Screen",
				"Show Order Milestones on the first tab of Order Screen.",
				RegistryStorageFlags.System,
				true);
		}

		#endregion

		#region EnableOrderLineReferenceMatching

		public void TestEnableOrderLineReferenceMatching()
		{
			TestRegistryItem(ItemSet.EnableOrderLineReferenceMatching,
				"EnableOrderLineReferenceMatching",
				RawDataRegistry.Categories.Orders,
				"Enable Order Line Reference for matching",
				@"Specify whether Line Reference, an alpha numeric free-text field, can be used for matching order line.

YES – If populated, line reference along with buyer / customer, order number, order split are used to identify and for matching order line. If empty, it is not used for matching and system falls back to existing matching logic which is: buyer / customer, order number, order split number, order line number, and order sub line number.

NO – Line Reference field is not used for matching. It is just like any other non-mandatory free text field on order line.",
				RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				false);
		}

		#endregion

		#region SupportOnly

		public void TestEnableOrderLineShippingTolerance()
		{
			Assert("Enable Order Line Shipping Tolerance off by default", !ItemSet.EnableOrderLineShippingTolerance.DefaultValue);
			Assert("Enable Order Line Shippiner Tolerance is not support-only", 0 == (ItemSet.EnableOrderLineShippingTolerance.Options & RegistryOptions.IsOnlyForSupport));
			ItemSet.EnableOrderLineShippingTolerance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("New value", ItemSet.EnableOrderLineShippingTolerance.Value);
		}

		#endregion

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "EnableOrderLineShippingTolerance";
			}
		}
	}
}
