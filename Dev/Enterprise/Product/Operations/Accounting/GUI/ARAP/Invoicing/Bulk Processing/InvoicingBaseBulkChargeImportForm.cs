using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoicingBaseBulkChargeImportForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public InvoicingBaseBulkChargeImportForm(InvoicingBaseBulkChargeImporter bulkChargeImporterBizObj)
			: base(bulkChargeImporterBizObj)
		{
		}

		#region Overrides

		public override string FormVerb
		{
			get { return String.Empty; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				ZString transactionCurrency = BusinessEntity.ParentInvoice.AH_RX_NKTransactionCurrency;
				ZString localCurrency = BusinessEntity.ParentInvoice.AH_Calc_LocalRXCode;
				SelectedInvoiceCurrencyTotalCalcFindBox.Visible = transactionCurrency != localCurrency;
				InvoiceRateCalcEdit.Visible = transactionCurrency != localCurrency && !BusinessEntity.ParentInvoice.AH_PostedToEFT;

				SplitContainer.Dock = System.Windows.Forms.DockStyle.None;

				JobCollection fakeCollectionForNotUsedGrid = new JobCollection(BusinessEntity.Factory);
				FilterControl = new AccountingOnFormFilterControl(fakeCollectionForNotUsedGrid, BusinessEntity.Filters);
				FilterControl.SetMaxFilterStripPanelHeight(220);
				FilterControl.BackColor = BackColor;
				FilterControl.Dock = System.Windows.Forms.DockStyle.Fill;

				this.Controls.Add(FilterControl);
				this.Controls.SetChildIndex(FilterControl, 0);
				this.Controls.SetChildIndex(this.SplitContainer, 0);
				FilterControl.PerformSearch += new EventHandler<PerformSearchEventArgs>(FilterControl_PerformSearch);
				FilterControl.FilteredGrid.SizeChanged += new EventHandler(FilterGrid_BoundsChanged);
				FilterControl.FilteredGrid.LocationChanged += new EventHandler(FilterGrid_BoundsChanged);
				FilterGrid_BoundsChanged(FilterControl.FilteredGrid, EventArgs.Empty);

				SplitContainer.AllowOverlap(FilterControl);

				if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
				{
					ChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_CostPlaceOfSupply);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					ChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_CostSupplyType);
				}

				if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
				{
					JobsGrid.RemoveFromAvailableColumns(JobHeaderSchema.Constants.JH_GB_TaxBranch);
					ChargesGrid.RemoveFromAvailableColumns(JobChargeSchema.Constants.JR_GB_CostTaxBranch);
				}
			}
		}

		void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			BusinessEntity.LoadJobsCollection();
			if (BusinessEntity.IsChargeHidingApplied())
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion

		#region Implementation

		new InvoicingBaseBulkChargeImporter BusinessEntity
		{
			get { return (InvoicingBaseBulkChargeImporter)base.BusinessEntity; }
		}

		void APInvoiceBulkChargeImporterFiltersControl_LoadCollectionClicked(object sender, EventArgs e)
		{
			BusinessEntity.LoadJobsCollection();
		}

		void PostButton_Click(object sender, EventArgs e)
		{
			try
			{
				BusinessEntity.Import();
				Close();
			}
			catch (CannotGenerateCashAdvanceJournalException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		void CancelPostingButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void FilterGrid_BoundsChanged(object sender, EventArgs e)
		{
			SplitContainer.Bounds = FilterControl.FilteredGrid.Bounds;
		}

		AccountingOnFormFilterControl FilterControl;

		#endregion

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			SelectDeselectAll(true);
		}

		void DeselectAllButton_Click(object sender, EventArgs e)
		{
			SelectDeselectAll(false);
		}

		void SelectDeselectAll(bool select)
		{
			using (BusinessEntity.GetUpdateSelectedLocalTotalSuspender())
			{
				foreach (InvoicingBaseBulkChargeImporterDependentJob job in BusinessEntity.Jobs)
				{
					job.IsSelectedForImport = select;
				}
			}
		}

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage
		{
			get { return ResString.GetMultilingualString("a80c3728-81f4-4181-8c94-c2287dae6f59", "Accruals entered against branch / dept outside your login permission are not listed."); }
		}

		protected MultilingualString Caption
		{
			get { return ResString.GetMultilingualString("4d4a4767-61ad-466d-9e89-7a84f628e740", "Search Results"); }
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control is SplitContainer && previousControl is AccountingOnFormFilterControl)
				|| (control is AccountingOnFormFilterControl && previousControl is SplitContainer);
		}
	}
}
