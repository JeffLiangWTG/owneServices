using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class OrdersDataRegistry : RegistryItemSet
	{
		#region Construction

		OrdersDataRegistry()
		{
		}

		public static OrdersDataRegistry Instance
		{
			get { return fInstance ?? (fInstance = new OrdersDataRegistry()); }
		}

		[ThreadStatic]
		static OrdersDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Orders_AdvancedOrderManager { get { return CombineCategories(Orders, ResString.GetMultilingualString("55e40253-8fbc-4c28-ad18-28e55801acbd", "Advanced Order Manager")); } }
		}

		#endregion

		#region Order Line Quantity Remaining Management

		public BooleanRegistryItem OrderLineQtyRemainingManagement
		{
			get
			{
				return GetItem("OrderLineQtyRemainingManagement", () => new BooleanRegistryItem(
					"OrderLineQtyRemainingManagement",
					RawDataRegistry.Categories.Orders,
					ResString.GetMultilingualString("9d874f1c-9e2a-4148-9fd7-a48301ee1e77", "Order Line Quantity Remaining Management"),
					ResString.GetMultilingualString("b9cfa5e8-90da-4da6-85ac-94349b589119", @"This registry item configures whether the Quantity Remaining field is based on Quantity Received or Quantity Invoiced.

Set this value to 'Yes' to use Quantity Received to calculate remaining quantity and update Quantity Received when you change Quantity Invoiced.
Set this value to 'No' to use Quantity Invoiced to calculate remaining quantity."),
					RegistryStorageFlags.System,
					true));
			}
		}

		#endregion

		#region Automatically Set Order Status

		public BooleanRegistryItem AutomaticallySetOrderStatus
		{
			get => GetItem("AutomaticallySetOrderStatus", delegate
				{
					return new BooleanRegistryItem(
						"AutomaticallySetOrderStatus",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("ac0b3856-1687-4aa0-a538-1b42bf040fa0", "Automatically Set Order Status"),
						ResString.GetMultilingualString("9199bc50-3f0f-4dc0-8fdc-8aa2de1abf85", @"Switch this registry on to automatically set order with 'order manager lite' status:  CNF, SHP, PRT, and DLV.

This setting is not applied when an order is processed through Advanced Order Manager."),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
		}

		#endregion

		#region Delivery Status for External Cartage

		public BooleanRegistryItem AutoSetDeliveredForOrdersForExternalCartage
		{
			get
			{
				return GetItem("AutoSetDeliveredForOrdersForExternalCartage", delegate
				{
					return new BooleanRegistryItem(
						"AutoSetDeliveredForOrdersForExternalCartage",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("863dbabe-9374-497f-8742-15040413e69d", "External Port Transport sets order to Delivered"),
						ResString.GetMultilingualString("c9858139-cc4f-4f03-8b19-7c3c543f6dc7", "Automatically updates order status to Delivered if the delivery Local Transport company on the attached shipment/declaration is an external/third party Local Transport company."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Order Import Report Attachment Type

		ICodeDescriptionPairListProvider OrderImportReportAttachmentTypeListProvider
		{
			get
			{
				if (orderImportReportAttachmentTypeListProvider == null)
				{
					orderImportReportAttachmentTypeListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var orderImportReportTypeList = new CodeDescriptionPairList();
						orderImportReportTypeList.AddPair(OrgConstants.AttachmentType.XLS, ResString.GetMultilingualString("359344d7-24e5-42f9-9c43-fb993dad1c49", "Microsoft Excel Spreadsheet"));
						orderImportReportTypeList.AddPair(OrgConstants.AttachmentType.PDF, ResString.GetMultilingualString("44344f99-22e9-47ab-ad37-f0f0ca55ebea", "Portable Document Format"));
						orderImportReportTypeList.AddPair(OrgConstants.AttachmentType.TIF, ResString.GetMultilingualString("a09e07cc-162a-44ec-bdc9-a62110ba9bb5", "Tagged Image File"));
						orderImportReportTypeList.DefaultCode = OrgConstants.AttachmentType.PDF;
						return orderImportReportTypeList;
					});
				}
				return orderImportReportAttachmentTypeListProvider;
			}
		}
		ICodeDescriptionPairListProvider orderImportReportAttachmentTypeListProvider;

		public CodeDescriptionPairList OrderImportReportAttachmentTypeList
		{
			get
			{
				if (orderImportReportAttachmentTypeList == null)
				{
					orderImportReportAttachmentTypeList = OrderImportReportAttachmentTypeListProvider.CodeDescriptionPairList;
				}
				return orderImportReportAttachmentTypeList;
			}
		}
		CodeDescriptionPairList orderImportReportAttachmentTypeList;

		public CodePairRegistryItem OrderImportReportAttachmentType
		{
			get
			{
				return GetItem("OrderImportReportAttachmentType", delegate
			   {
				   return new CodePairRegistryItem(
					   "OrderImportReportAttachmentType",
					   RawDataRegistry.Categories.Orders,
					   ResString.GetMultilingualString("e2242a6c-909c-4647-9054-f03e5b9c55db", "Order Import Report Attachment Type"),
					   ResString.GetMultilingualString("c9c678c7-5d10-46e3-9963-130368c6c482", "Choose attachment type for 'Order Import Report'"),
					   OrderImportReportAttachmentTypeListProvider,
					   RegistryStorageFlags.System,
					   RegistryOptions.PreserveTestValue,
					   OrderImportReportAttachmentTypeList.DefaultCode);
			   });
			}
		}

		#endregion

		#region Allow Exported Order Lines to be Deleted

		public BooleanRegistryItem AllowExportedOrderLinesToBeDeleted
		{
			get
			{
				return GetItem("AllowExportedOrderLinesToBeDeleted", delegate
				{
					return new BooleanRegistryItem(
						"AllowExportedOrderLinesToBeDeleted",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("30d67de4-4354-46ab-a89c-e1980fd68938", "Allow Exported Order Lines To Be Deleted"),
						ResString.GetMultilingualString("e9994c59-9c0d-4571-a050-141c10043c67", "Specifies whether order lines on a customer exported order can be deleted."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Order Line Status Field Editable

		public BooleanRegistryItem OrderLineStatusEditable
		{
			get
			{
				return GetItem("OrderLineStatusEditable", delegate
				{
					return new BooleanRegistryItem(
						"OrderLineStatusEditable",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("b072d2bf-1114-40f5-9c3e-b76c4f851993", "Order Line Status field editable"),
						ResString.GetMultilingualString("b6290e5b-bd5e-4f00-b337-a9192ade78c6", "Allow user to edit the Order Line Status field."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Suppress xml triggers on orders with zero quantity

		public BooleanRegistryItem SuppressXmlTriggersOnOrdersWithZeroQuantity
		{
			get
			{
				return GetItem("SuppressXmlTriggersOnOrdersWithZeroQuantity", delegate
				{
					return new BooleanRegistryItem(
						"SuppressXmlTriggersOnOrdersWithZeroQuantity",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("9c242fa4-0588-4442-8ca4-50473d4d1991", "Suppress XML triggers on orders with zero quantity"),
						ResString.GetMultilingualString("14b3504f-180c-4fad-bd00-0f582a62f5cb", "Suppress XML Messaging triggers for order and shipment pre-advice triggers with zero quantity. Order Confirmation Number field trigger will still run regardless of quantity."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Include changes not applied

		public BooleanRegistryItem IncludeChangesNotApplied
		{
			get
			{
				return GetItem("IncludeChangesNotApplied", delegate
				{
					return new BooleanRegistryItem(
						"IncludeChangesNotApplied",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("dafd4717-bdd4-4ff8-b050-e7ab58f3e765", "Include Changes Not Applied"),
						ResString.GetMultilingualString("24a8d147-352c-48fb-82e5-ad4439663599", "Switch on to include changes not applied in order import report."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region ShowOrderMilestones

		public BooleanRegistryItem ShowOrderMilestonesOnMainScreen
		{
			get
			{
				return GetItem("ShowOrderMilestonesOnMainScreen", delegate
				{
					return new BooleanRegistryItem(
						"ShowOrderMilestonesOnMainScreen",
						RawDataRegistry.Categories.Orders,
						ResString.GetMultilingualString("c55fabd5-69a7-4ea6-9118-162d313100a0", "Show Order Milestones on Main Order Screen"),
						ResString.GetMultilingualString("7f4744b1-c31c-48fd-8a45-f4faeb005010", "Show Order Milestones on the first tab of Order Screen."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Enable Order Line Reference Matching

		public BooleanRegistryItem EnableOrderLineReferenceMatching => GetItem(
			"EnableOrderLineReferenceMatching",
			() => new BooleanRegistryItem(
				"EnableOrderLineReferenceMatching",
				RawDataRegistry.Categories.Orders,
				ResString.GetMultilingualString("d3322f80-30e7-43f5-b913-2457d912e5aa", "Enable Order Line Reference for matching"),
				ResString.GetMultilingualString("8bc75690-b378-49ec-bd22-52483262d676", @"Specify whether Line Reference, an alpha numeric free-text field, can be used for matching order line.

YES – If populated, line reference along with buyer / customer, order number, order split are used to identify and for matching order line. If empty, it is not used for matching and system falls back to existing matching logic which is: buyer / customer, order number, order split number, order line number, and order sub line number.

NO – Line Reference field is not used for matching. It is just like any other non-mandatory free text field on order line."),
				RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
				false
			)
		);

		#endregion

		#region Enable Order Line Shipping Tolerance

		public BooleanRegistryItem EnableOrderLineShippingTolerance => GetItem(
			"EnableOrderLineShippingTolerance",
			() => new BooleanRegistryItem(
				"EnableOrderLineShippingTolerance",
				RawDataRegistry.Categories.Orders,
				ResString.GetMultilingualString("dad43a84-cd5c-4b44-b5be-e076c59528e4", "Enable Order Line Shipping Tolerance entry"),
				ResString.GetMultilingualString("e1ae8bab-b3bd-4863-a4d7-b60cd647a42e", "This enables the Shipping Tolerance Tab on the Order Line"),
				RegistryStorageFlags.System,
				false
			)
		);

		#endregion
	}
}
