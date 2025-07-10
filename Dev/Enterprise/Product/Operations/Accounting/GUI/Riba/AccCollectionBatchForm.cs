using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AccCollectionBatchForm : ZForm, IButtonDeleteTextOverride, IDoDisplayModeDeleteOverride, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public AccCollectionBatchForm(AccCollectionBatch batchBizO)
			: base(batchBizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingControl);
			ControllerID = ControllerIDs.AccCollectionBatch;
			WorkflowTabPage.Initialize(batchBizO);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			BottomPanel.AllowOverlap(MainTabControl);
			WorkflowTabPage.AllowOutsideOfParent();

			MainTabPage.RunWhenBindingOrFirstShown(((s, e) =>
			{
				OrderLinesGrid.AllowOverlap(OrdersGrid);
			}));
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == BankAccountBoundGuidFindBox && previousControl == BatchTypeDropEdit;
		}

		AccCollectionBatch Batch => BusinessEntity as AccCollectionBatch;

		string IButtonDeleteTextOverride.DeleteButtonText => Enterprise.Accounting.GUI.Res.GetString("AccCollectionBatchForm|830C91EC-8156-4bf5-81D1-87F91AE146E1", "Cancel");

		internal MenuItem AddTransactionsToOrderMenuItem;
		internal MenuItem RejectOrderMenuItem;
		ZDropEdit CollectionFileFormatDropEdit;
		ZDropEdit BatchTypeDropEdit;
		internal MenuItem CreateReceiptMenuItem;

		MultilingualString PrintMenuItemName
		{
			get { return ResString.GetMultilingualString("AccCollectionBatchForm|2F511EAE-4352-4499-9196-1C13F0168F65", "Print"); }
		}
		internal MenuItem PrintMenuItem;

		internal void HandleAddTransactionsToOrder(object sender, EventArgs e)
		{
			var orders = OrdersGrid.SelectedElements.OfType<AccCollectionOrder>();
			if (orders == null || orders.Count() != 1)
			{
				Globals.Message.ShowError(CollectionOrderBatchHelper.SelectOrderFirstErrorMessage);
			}
			else
			{
				CollectionOrderBatchHelper.AddTransactionsToOrder(orders.First(), Env.Security.CollectionBatchEdit, orders.First().IncludeInBatch);
			}
		}

		internal void HandleRejectOrder(object sender, EventArgs e)
		{
			var orders = OrdersGrid.SelectedElements.OfType<AccCollectionOrder>();
			if (orders == null || orders.Count() != 1)
			{
				Globals.Message.ShowError(CollectionOrderBatchHelper.SelectOrderFirstErrorMessage);
			}
			else
			{
				CollectionOrderBatchHelper.RejectOrder(orders.First(), Env.Security.CollectionOrderReject, doSave: false);
			}
		}

		internal void HandlePrint(object sender, EventArgs e)
		{
			var orders = OrdersGrid.SelectedElements.OfType<AccCollectionOrder>();
			if (orders == null || !orders.Any())
			{
				Globals.Message.ShowError(CollectionOrderBatchHelper.SelectAtLeastOneOrderFirstErrorMessage);
			}
			else
			{
				CollectionOrderBatchHelper.PrintCollectionOrders(orders.ToArray(), Env.Security.CollectionOrderPrint);
			}
		}

		internal void HandleCreateReceipts(object sender, EventArgs e)
		{
			if (!Batch.IsInDatabase || Batch.HasChanges)
			{
				Globals.Message.ShowInformation(Enterprise.Accounting.GUI.Res.GetString("d93a3d47-4300-4088-9573-52bc03fcb6b3", "Please save this batch before trying to create receipts and deposit batch."));
			}
			else
			{
				var orders = OrdersGrid.SelectedElements.OfType<AccCollectionOrder>();
				CollectionOrderBatchHelper.CreateReceipts(orders, Env.Security.CollectionOrderCreateReceipt);
				this.OrderLinesGrid.Refresh();
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded menu text")]
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (DisplayMode == ODisplayMode.Delete || DisplayMode == ODisplayMode.ReadOnly)
			{
				this.SetReadOnlyIncludingChildren(true);
			}
			else if (DisplayMode == ODisplayMode.Browse)
			{
				OrdersGrid.ContextMenu.MenuItems.Add(0, AddTransactionsToOrderMenuItem = new ZMenuItem(AccountingConstants.AccCollectionOrderMenuNames.AddTransactionsToOrderMenuItemName, new EventHandler(HandleAddTransactionsToOrder)));
				MenuItem deleteMenu = OrdersGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Delete");
				if (deleteMenu != null)
				{
					OrdersGrid.ContextMenu.MenuItems.Remove(deleteMenu);
				}
				OrdersGrid.ContextMenu.MenuItems.Add(1, RejectOrderMenuItem = new ZMenuItem(AccountingConstants.AccCollectionOrderMenuNames.RejectOrderMenuItemName, new EventHandler(HandleRejectOrder)));
				OrdersGrid.ContextMenu.MenuItems.Add(2, CreateReceiptMenuItem = new ZMenuItem(AccountingConstants.AccCollectionOrderMenuNames.CreateReceiptMenuItemName, new EventHandler(HandleCreateReceipts)));
				OrdersGrid.ContextMenu.MenuItems.Add(3, PrintMenuItem = new ZMenuItem(PrintMenuItemName, new EventHandler(HandlePrint)));
			}
			else if (DisplayMode == ODisplayMode.New)
			{
				var reloadMenuItem = Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.FileReloadMenuItemName];
				if (reloadMenuItem != null)
				{
					Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems.Remove(reloadMenuItem);
				}
			}
		}

		public override string FormVerb
		{
			get { return DisplayMode == ODisplayMode.Delete ? ((IButtonDeleteTextOverride)this).DeleteButtonText : base.FormVerb; }
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			string question = Enterprise.Accounting.GUI.Res.GetString("afc012ad-683b-4fd7-924e-3920b56efc7f", "You are about to cancel this batch. Do you want to proceed?");
			return Globals.Message.Show(question, Enterprise.Accounting.GUI.Res.GetString("1d7f6caf-f6b3-4039-b4b0-a58424d35a01", "Cancel Batch Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			ContinueWithDelete result = ContinueWithDelete.No;
			Batch.RunPreSaveValidation();
			if (Batch.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				result = base.ShowPreDeleteDialogs();
			}
			return result;
		}

		void IDoDisplayModeDeleteOverride.DoDisplayModeDelete()
		{
			ZFormStrategy.DoDisplayModeDelete(this);
			PostingControl.CloseButton.Text = ZFormPostingButtonsStrategy.DefaultCloseButtonText;
		}

		protected override void DeleteCore()
		{
			if (Batch != null)
			{
				Batch.ACB_IsCancelled = true;
				foreach (AccCollectionOrder order in Batch.CollectionOrders)
				{
					order.CollectionOrderLines.DeleteAll();
				}
				Batch.CollectionOrders.DeleteAll();
				Batch.ACB_TotalAmount = 0;
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var fileGeneratorDataValidation = ObjectFactory.Get<IAccountingDependencyFactory>().GetCollectionBatchFileGeneratorProvider(Batch, Env.Instance)?.GetValidation();
			if (fileGeneratorDataValidation != null)
			{
				var errorMessage = fileGeneratorDataValidation.GetPreSaveValidationMessage();
				if (!errorMessage.IsEmpty)
				{
					Globals.Message.ShowError(errorMessage);
					return ContinueWithSave.No;
				}
			}
			return base.ValidateAndSave();
		}

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}
	}
}

