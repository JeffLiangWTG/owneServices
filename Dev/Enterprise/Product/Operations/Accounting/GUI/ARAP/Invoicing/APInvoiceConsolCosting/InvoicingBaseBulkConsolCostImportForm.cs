using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoicingBaseBulkConsolCostImportForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public InvoicingBaseBulkConsolCostImportForm(InvoicingBaseBulkConsolCostImporter importer)
			: base(importer)
		{
		}

		#region Overrides

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
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
					ConsolCostsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_PlaceOfSupply);
				}

				if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
				{
					ConsolCostsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_SupplyType);
				}

				if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
				{
					ConsolCostsGrid.RemoveFromAvailableColumns(JobConsolCostSchema.Constants.E6_GB_CostTaxBranch);
					ConsolCostsGrid.RemoveFromAvailableColumns(JobConsolCost.Schema.CostTaxBranchName);
				}
			}
		}

		#endregion

		#region Implementation

		new InvoicingBaseBulkConsolCostImporter BusinessEntity
		{
			get { return (InvoicingBaseBulkConsolCostImporter)base.BusinessEntity; }
		}

		void FilterControl_PerformSearch(object sender, EventArgs e)
		{
			BusinessEntity.LoadConsolsCollection();

			if (BusinessEntity.Consols.Cast<InvoicingBaseConsolForImporting>().Any(x => x.ConsolCosts.Count > x.ConsolCostsFilteredByViewingPermission.Count))
			{
				Globals.Message.ShowWarning(ViewingRestrictionOutsideLoginWarningMessage, Caption);
			}
		}

		void FilterGrid_BoundsChanged(object sender, EventArgs e)
		{
			SplitContainer.Bounds = FilterControl.FilteredGrid.Bounds;
		}

		AccountingOnFormFilterControl FilterControl;

		#endregion

		void CancelPostingButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void PostButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.Import();
			Close();
		}

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
			foreach (InvoicingBaseConsolForImporting consol in BusinessEntity.ConsolsFilteredByViewingPermission.ToArray())
			{
				consol.IsSelectedForImport = select;
			}
		}

		protected MultilingualString ViewingRestrictionOutsideLoginWarningMessage
		{
			get { return ResString.GetMultilingualString("ed112fed-4569-44ef-9ba4-b941b342b1e1", "Accruals entered against branch / dept outside your login permission are not listed."); }
		}

		protected MultilingualString Caption
		{
			get { return ResString.GetMultilingualString("eafd3377-3c9f-4893-8f77-38607585d420", "Search Results"); }
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control is SplitContainer && previousControl is AccountingOnFormFilterControl)
				|| (control is AccountingOnFormFilterControl && previousControl is SplitContainer);
		}
	}
}
