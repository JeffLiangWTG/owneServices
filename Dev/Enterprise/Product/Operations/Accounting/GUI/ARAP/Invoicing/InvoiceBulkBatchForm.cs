using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceBulkBatchForm : InvoiceBatchForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public InvoiceBulkBatchForm()
		{
		}

		public InvoiceBulkBatchForm(InvoiceBulkBatch batchHeader)
			: base(batchHeader)
		{
			JobTypeCheckedListBox.ColumnWidth = 335;
		}

		new InvoiceBulkBatch BusinessEntity
		{
			get { return base.BusinessEntity as InvoiceBulkBatch; }
		}

		#region Overrides		

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.Controls.Remove(BatchInvoiceLinesGrid);
			InitializeComponent();
		}

		protected override void InitializePostingStrategy()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButton, CloseButton, null);
		}

		protected override void LoadLines(ZQuery query)
		{
			InvoicingBase[] linesFetched = LineFactory.Load<InvoicingBase>(query);
			BusinessEntity.ClearBatches();
			BusinessEntity.CreateBatches(linesFetched);
			NotificationLabel.ForeColor = System.Drawing.Color.Black;
			NotificationLabel.Text = Res.GetString("InvoiceBulkBatchForm|15094ED0-3928-4414-B4DD-E8CC9E812E0A", "Found {0} records that match your criteria. Create {1} Invoice Batches",
												linesFetched.Length, BusinessEntity.InvoiceBatchHeaders.Count);
		}

		protected override void FilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			base.FilterControl_ClearButtonClicked(sender, e);
			if (BusinessEntity != null)
			{
				BusinessEntity.ClearBatches();
			}
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = ContinueWithSave.No;
			if (BusinessEntity.InvoiceBatchHeaders.Count > 0)
			{
				result = base.ValidateAndSave();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("a3e5d462-e5f2-4e44-b5c7-4ecd38c475d4", "There are not Invoice Batches for posting."), FormCaption);
			}
			return result;
		}

		protected override void PrintBatches()
		{
			if (Globals.Message.Show(Res.GetString("4a263aca-2bea-480e-9122-376312b24b92", "Do you want to print all Invoice Batches?"), Res.GetString("43c3bfbd-eb8c-484d-94df-2d192808cf47", "Invoice Bulk Batch"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.PK, BusinessEntity.InvoiceBatchHeaders.GetPKs());
				InvoiceBatchHeaderPrintTask printTask = new InvoiceBatchHeaderPrintTask(filter);
				printTask.Run();
			}
		}

		protected override void InvoiceBatchForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			base.InvoiceBatchForm_DisplayModeChanged(sender, e);
			if (e.FromMode == ODisplayMode.New && e.ToMode == ODisplayMode.Browse)
			{
				BatchInvoicesGrid.RemoveAction = RemoveAction.NoRemovePossible;
			}
		}

		#endregion
	}
}
