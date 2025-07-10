using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PayableOrder
{
	public partial class AccPayableOrderForm : ZForm
	{
		public AccPayableOrderForm()
		{
			InitializeComponent();
			OrdersUserControl.Inner = NewOrdersUserControl();
		}

		protected AccPayableOrderUserControlDecider OrdersUserControl;

		public AccPayableOrderForm(AccPayableOrderHeader bO)
			: base(bO)
		{
			InitializeComponent();
			Order = bO;
			Order.OnDispositionChanged += delegate { order_DispositionChanged(); };

			OrdersUserControl.Inner = NewOrdersUserControl();
			OrdersUserControl.DockInside(OrdersTabPage);

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			DataContext = Constants.DataContext.PayableOrder;
			MinimumSize = Size;
			SetupLayout();
			SetupActionsMenu();

			WorkflowTabPage.Initialize(Order);
			Order.WorkflowItems.Load();

			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			SecurityOverrideProviderSource.Get(BusinessEntity).Provider = new PayableOrderSecurityOverrideProvider(bO);
			Order.ShowApprovalMessage += new AccPayableOrderHeader.ApprovalMessageEventHandler(Order_ShowApprovalMessage);
		}

		void order_DispositionChanged()
		{
			if (Order.APH_Disposition == Core.Constants.PayableOrderDisposition.Complete)
			{
				if (splitOrderMenuItem != null)
				{
					splitOrderMenuItem.Enabled = false;
				}

				if (approveMenuItem != null)
				{
					approveMenuItem.Enabled = false;
				}

				if (printAndBookMenuItem != null)
				{
					printAndBookMenuItem.Enabled = false;
				}

				if (postAPInvoiceMenuItem != null)
				{
					postAPInvoiceMenuItem.Enabled = false;
				}
			}
		}

		void Order_ShowApprovalMessage(AccPayableOrderHeader.ApprovalMessage approvalMessage)
		{
			string message = "";

			switch (approvalMessage)
			{
				case AccPayableOrderHeader.ApprovalMessage.AlreadyApproved:
					message = Res.GetString("988464a1-3863-4cba-8ce6-4e0d6525027e", "This order has been already approved");
					break;

				case AccPayableOrderHeader.ApprovalMessage.NotReadyForApproval:
					message = Res.GetString("5c95c1b5-c6f6-4c77-9db0-33201d0ac8ab",
@"This Purchase Order's disposition is not 'Pending Approval' due to one of the below mentioned reasons:  
* No order lines created for this order or
* No changes made to order line values of existing order lines that have already been approved");
					break;
			}

			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.Show(message);
			}
		}

		public readonly AccPayableOrderHeader Order;

		protected virtual AccPayableOrderUserControl NewOrdersUserControl()
		{
			return new AccPayableOrderUserControl();
		}

		public override string FormCaption
		{
			get { return Res.GetString("PayableOrderForm|FormCaption", "Order") + " " + Order.APH_OrderNumberAndSplit; }
		}

		void SetupLayout()
		{
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes &&
				this.DisplayMode != ODisplayMode.Delete &&
				Order.ShouldPromptSplitOnSave)
			{
				switch (ShowConfirmationForSplittingOrder())
				{
					// split the order
					case OrderSplitDialogResult.SplitOrder:
						CreateOrder(AccPayableOrderHeader.CreateOrderType.Split);
						break;
					case OrderSplitDialogResult.CreateNewOrder:
						CreateOrder(AccPayableOrderHeader.CreateOrderType.New);
						break;
					case OrderSplitDialogResult.No: /* do nothing */
						break;
				}
			}
			return result;
		}

		#region Order Split

		void OnSplitOrder_Click(object sender, EventArgs e)
		{
			string errorMessage = this.Order.CanSplitOrder();
			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.ShowError(errorMessage);
			}
			else
			{
				CreateOrder(AccPayableOrderHeader.CreateOrderType.Split);
			}
		}

		OrderSplitDialogResult ShowConfirmationForSplittingOrder()
		{
			using (OrderSplitMessageBox msgBox = new OrderSplitMessageBox())
			{
				ZFormModaliser.ShowDialogWithoutDispose(msgBox);
				return msgBox.OrderSplitDialogResult;
			}
		}

		void CreateOrder(AccPayableOrderHeader.CreateOrderType splitType)
		{
			((IOrdersModule)ZModuleFactory.Instance.Create(ModuleIDs.AccPayableOrder)).ShowFormForSplit(Order, splitType);
		}

		#endregion

		#region Actions Menu

		ZMenuItem splitOrderMenuItem;
		ZMenuItem approveMenuItem;
		ZMenuItem printAndBookMenuItem;
		ZMenuItem postAPInvoiceMenuItem;

		void SetupActionsMenu()
		{
			var shouldDisableMenu = Order.APH_Disposition == Core.Constants.PayableOrderDisposition.Complete || Order.IsCancelled;

			splitOrderMenuItem = new ZMenuItem(ResString.GetMultilingualString("APOrderManager.Orders.Actions.SplitOrder", "Split Order"), new EventHandler(OnSplitOrder_Click));
			splitOrderMenuItem.Enabled = !shouldDisableMenu;
			ActionsMenuItem.MenuItems.Add(splitOrderMenuItem);

			approveMenuItem = new ZMenuItem(ResString.GetMultilingualString("APOrderManager.Orders.Actions.Approve", "Approve"), new EventHandler(OnApproveOrder_Click));
			approveMenuItem.Enabled = !shouldDisableMenu;
			ActionsMenuItem.MenuItems.Add(approveMenuItem);

			printAndBookMenuItem = new ZMenuItem(ResString.GetMultilingualString("APOrderManager.Orders.Actions.PlaceOrder", "Place Order"), new EventHandler(OnPrintAndBookOrder_Click));
			printAndBookMenuItem.Enabled = !shouldDisableMenu;
			ActionsMenuItem.MenuItems.Add(printAndBookMenuItem);

			postAPInvoiceMenuItem = new ZMenuItem(ResString.GetMultilingualString("APOrderManager.Orders.Actions.PostAPInvoice", "Post Accounts Payable Invoice"), new EventHandler(OnPostAPInvoice_Click));
			postAPInvoiceMenuItem.Enabled = !shouldDisableMenu;
			ActionsMenuItem.MenuItems.Add(postAPInvoiceMenuItem);
		}

		void OnApproveOrder_Click(object sender, EventArgs e)
		{
			Order.RunPreSaveValidation();
			if (Order.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				var registryRestrictionMessage = Order.HasRegistryRestrictions();
				if (!string.IsNullOrEmpty(registryRestrictionMessage))
				{
					Globals.Message.Show(registryRestrictionMessage);
				}
				else if (Order.Approve())
				{
					Globals.Message.Show(Res.GetString("7ef96cf4-d74a-4550-b0d0-9880c7e7b7bb", "Order has been successfully approved."));
				}
			}
		}

		void OnPrintAndBookOrder_Click(object sender, EventArgs e)
		{
			if (Order.HasChanges)
			{
				Globals.Message.Show(Res.GetString("c3eaea7d-8676-48f2-98c7-ad35d31a73bb", "Please save your order first before print and book."));
				return;
			}

			string cannotPrintMessage = Res.GetString("a3edd202-8f63-45b1-ba64-869152d9e7c4",
@"The Booking Request may not be delivered to the supplier due to one of the below mentioned reasons:
* The order has not been approved.
* No changes were made to existing order lines that have already been booked.");
			string checkEDocsMessage = Res.GetString("6b2e1d98-e638-47df-8097-4922d9973589", "A copy may be re-printed directly from eDocs.");

			if (Order.APH_Disposition == Core.Constants.PayableOrderDisposition.OrderIncomplete
				|| Order.APH_Disposition == Core.Constants.PayableOrderDisposition.PendingApproval)
			{
				Globals.Message.Show(cannotPrintMessage);
			}
			else if (Order.APH_Disposition == Core.Constants.PayableOrderDisposition.OrderToBePlaced)
			{
				PrintBookingRequest();
			}
			else
			{
				if (Order.IsBooked)
				{
					Globals.Message.Show(cannotPrintMessage + "\r\n" + checkEDocsMessage);
				}
				else
				{
					PrintBookingRequest();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter values should not be translated, do not use Res.GetString()")]
		void PrintBookingRequest()
		{
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(Order);
			documentCommands.Load();
			ZQuery menuFilter = new ZQuery(StmMenuItemSchema.SU_MenuName,"Purchase Order");
			menuFilter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, ZBool.True);
			BusinessObject[] result = documentCommands.Find(menuFilter);
			DocumentCommand menu;
			if (result.Length == 1)
			{
				menu = (DocumentCommand)result[0];
				try
				{
					DocumentPack docPack = new DocumentPack(menu, Order, null, null, true);
					DocumentPrintSet task = new DocumentPrintSet(menu);
					task.Add(docPack);

					DeliveryInstructionDestination deliveryStatus = task.Run(Env.Security.None);
					if (deliveryStatus == DeliveryInstructionDestination.TakenFromContact
						|| deliveryStatus == DeliveryInstructionDestination.Print)
					{
						Order.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.Booked, "Booking request document delivered.");
						if (!Order.IsBookingConfirmed)
						{
							Order.APH_Disposition = Core.Constants.PayableOrderDisposition.PendingConfirmation;
						}
						Order.Logs.Factory.Save();
					}

					//menu.Parent = Order;
					//DocumentRunner documentRunner = new DocumentRunner(this, ModuleIDs.AccPayableOrder);
					//documentRunner.Run(menu);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("40544133-e872-47a7-aaba-04e2c6448149", "Could not find Booking Request document menu."));
			}
		}

		void OnPostAPInvoice_Click(object sender, EventArgs e)
		{
			var canBePosted = Order.GetErrorMessageForNotAbleToPost();
			if (string.IsNullOrEmpty(canBePosted))
			{
				PostOrder();
			}
			else
			{
				Globals.Message.Show(canBePosted);
			}
		}

		void PostOrder()
		{
			if (Order.Supplier == null || Order.APH_InvoiceNumber.IsEmpty || Order.APH_InvoiceDate.IsEmpty || Order.APH_GoodsDescription.IsEmpty || Order.APH_DueDate.IsEmpty)
			{
				Globals.Message.Show(Res.GetString("cbf45a42-dae9-4b3d-b925-ac794ed53478", "The following fields must be entered.\r\n- Supplier, Invoice Number, Invoice Date, Goods Description and Due Date"));
			}
			else if (Order.APH_Calc_TotalAmount < Order.APH_Calc_TotalInvoicedPrice)
			{
				Globals.Message.Show(Res.GetString("c607aea7-4a9c-4246-b691-3d709f0f1fa8", "The Invoiced amount may not be more than the approved Order Line Price"));
			}
			else
			{
				var invoiceFromOrder = Order.PopulateInvoiceFromOrder();
				var controller = AccountingControllerCreator.GetNewController(invoiceFromOrder);
				controller.ShowFormForNewEntity(invoiceFromOrder);
			}
		}

		#endregion
	}
}
