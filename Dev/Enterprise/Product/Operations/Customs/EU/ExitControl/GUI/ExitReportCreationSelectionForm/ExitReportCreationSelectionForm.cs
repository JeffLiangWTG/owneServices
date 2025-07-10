using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public partial class ExitReportCreationSelectionForm : ZChildForm
	{
		public ExitReportCreationSelectionForm(CusExitConsignment consignment, CusExitReport report)
			: base(consignment)
		{
			this.report = report;
			MinimizeBox = false;
			PrePopulate();
		}
		readonly CusExitReport report;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				setupReportDataDisposable?.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ItemsGrid.AfterBind += ItemsGrid_AfterBind;
		}

		void ItemsGrid_AfterBind(object sender, EventArgs e)
		{
			var listManager = ItemsGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentItemChanged += ItemsGrid_ListManager_CurrentItemChanged;
				ItemsGrid_ListManager_CurrentItemChanged(null, null);
			}
		}

		void ItemsGrid_ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			if (ItemsGrid.ListManager?.GetCurrent() is CusExitConsignmentItem cusExitConsignmentItem)
			{
				PackingDetailsGrid.SetDataBinding(cusExitConsignmentItem, nameof(CusExitConsignmentItem.CusExitConsignmentPackagePivots));
			}
		}

		public new CusExitConsignment BusinessEntity => (CusExitConsignment)base.BusinessEntity;

		void PrePopulate()
		{
			setupReportDataDisposable = ReportManager.SetupReportData(BusinessEntity, report);
		}
		IDisposable setupReportDataDisposable;

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			SelectAll();
		}

		void SelectAll()
		{
			foreach (var item in BusinessEntity.CusExitConsignmentItems)
			{
				item.CCI_Calc_ShouldReportItem = true;
				foreach (var pivot in item.CusExitConsignmentPackagePivots)
				{
					var package = pivot.Package;
					if (package != null)
					{
						package.CXP_Calc_ShouldReportItem = true;
					}
				}
			}
			ItemsGrid.Refresh();
			PackingDetailsGrid.Refresh();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ClearAllButton_Click(object sender, EventArgs e)
		{
			foreach (var item in BusinessEntity.CusExitConsignmentItems)
			{
				item.CCI_Calc_ShouldReportItem = false;
				foreach (var pivot in item.CusExitConsignmentPackagePivots)
				{
					if (pivot.Package is CusExitConsignmentPackage package)
					{
						package.CXP_Calc_ShouldReportItem = false;
					}
				}
			}
			ItemsGrid.Refresh();
			PackingDetailsGrid.Refresh();
		}
	}
}
