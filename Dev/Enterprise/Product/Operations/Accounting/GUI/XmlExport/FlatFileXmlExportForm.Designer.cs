namespace Enterprise.Accounting.GUI.XmlExport
{
	public partial class FlatFileXmlExportForm
	{
		new void InitializeComponent()
		{
			this.TransactionNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).BeginInit();
			this.NewExportBatchGroupBox.SuspendLayout();
			this.ExportExistingBatchGroupBox.SuspendLayout();
			this.DatesGroupBox.SuspendLayout();
			this.PeriodsGroupBox.SuspendLayout();
			this.TransactionTypesGroupBox.SuspendLayout();
			this.ExcludeTransactionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DepartmentModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BranchModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.JobModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrganisationModuleButtonGrid
			// 
			// 
			// 
			// 
			this.OrganisationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrganisationModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrganisationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.OrganisationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrganisationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.OrganisationModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.OrganisationModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.OrganisationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 51, true);
			this.OrganisationModuleButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// DeparmentModuleButtonGrid
			// 
			// 
			// 
			// 
			this.DepartmentModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DepartmentModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DepartmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DepartmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DepartmentModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.DepartmentModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DepartmentModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.DepartmentModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DepartmentModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DepartmentModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.DepartmentModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.DepartmentModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 52, true);
			this.DepartmentModuleButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// BranchModuleButtonGrid
			// 
			// 
			// 
			// 
			this.BranchModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.BranchModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BranchModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.BranchModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BranchModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.BranchModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.BranchModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.BranchModuleButtonGrid.InnerGrid.Name = "Grid";
			this.BranchModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.BranchModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.BranchModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.BranchModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 51, true);
			this.BranchModuleButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// JobModuleButtonGrid
			// 
			// 
			// 
			// 
			this.JobModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.JobModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.JobModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.JobModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.JobModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.JobModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.JobModuleButtonGrid.InnerGrid.Name = "Grid";
			this.JobModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.JobModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.JobModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.JobModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 51, true);
			this.JobModuleButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 618, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 22, true);
			// 
			// FlatFileXmlExportForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 640, true);
			this.Name = "FlatFileXmlExportForm";
			this.TransactionNumbersGroupBox.ResumeLayout(false);
			this.TransactionNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).EndInit();
			this.NewExportBatchGroupBox.ResumeLayout(false);
			this.ExportExistingBatchGroupBox.ResumeLayout(false);
			this.ExportExistingBatchGroupBox.PerformLayout();
			this.DatesGroupBox.ResumeLayout(false);
			this.PeriodsGroupBox.ResumeLayout(false);
			this.PeriodsGroupBox.PerformLayout();
			this.TransactionTypesGroupBox.ResumeLayout(false);
			this.ExcludeTransactionsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DepartmentModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BranchModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.JobModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

	}
}
