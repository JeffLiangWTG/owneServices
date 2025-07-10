using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AccCollectionOrderForm : ZForm
	{
		public AccCollectionOrderForm(AccCollectionOrder accCollectionOrder)
			: base(accCollectionOrder)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingControl);
			ControllerID = ControllerIDs.AccCollectionOrder;
			WorkflowTabPage.Initialize(accCollectionOrder);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			CollectionOrder = accCollectionOrder;

			MainTabPage.RunWhenBindingOrFirstShown(((s, e) =>
			{
				AccountBoundGuidFindBox.AllowOverlap(DepositedDateDateEdit);
				zCalcFindBox1.AllowOutsideOfParent();
				zPostingButtonsUserControl1.AllowOutsideOfParent();
			}));
		}

		readonly AccCollectionOrder CollectionOrder;

		internal MenuItem AddTransactionsToOrderMenuItem;
		internal MenuItem RejectOrderMenuItem;
		internal MenuItem CreateReceiptMenuItem;

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (CollectionOrder.IsCancelled || DisplayMode == ODisplayMode.ReadOnly)
			{
				this.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				AddTransactionsToOrderMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccCollectionOrderMenuNames.AddTransactionsToOrderMenuItemName, HandleAddTransactionsToOrder);
				RejectOrderMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccCollectionOrderMenuNames.RejectOrderMenuItemName, HandleRejectOrder);
				CreateReceiptMenuItem = ZFormMenuStrategy.AddActionsMenuItem(this, AccCollectionOrderMenuNames.CreateReceiptMenuItemName, HandleCreateReceipts);
			}
		}

		public override string FormVerb => CollectionOrder.IsCancelled ? FormVerbs.View : base.FormVerb;

		internal void HandleAddTransactionsToOrder(object sender, EventArgs e)
		{
			CollectionOrderBatchHelper.AddTransactionsToOrder(CollectionOrder, Env.Security.CollectionOrderEdit, true);
		}

		internal void HandleRejectOrder(object sender, EventArgs e)
		{
			CollectionOrderBatchHelper.RejectOrder(CollectionOrder, Env.Security.CollectionOrderReject, doSave: false);
		}

		internal void HandleCreateReceipts(object sender, EventArgs e)
		{
			CollectionOrderBatchHelper.CreateReceiptsAndIndividualDepositBatch(CollectionOrder.CollectionBatch, Env.Security.CollectionOrderCreateReceipt);
		}
	}
}

