using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class KoreaSouthEInvoicingDSBSummaryAppendingRuleControl : RegistryZUserControl
	{
		ZArchitecture.ZGrid ConfigurationGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).BeginInit();
			this.ConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection);
			// 
			// ConfigurationGrid
			// 
			this.ConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDSBSummaryAppendingRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDSBSummaryAppendingRule)(null)).TaxIdPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDSBSummaryAppendingRule)(null)).PostingGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.KoreaSouthEInvoicingDSBSummaryAppendingRule)(null)).Order)));
			this.ConfigurationGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TaxIdPK";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PostingGroup";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Order";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ConfigurationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ConfigurationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ConfigurationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationGrid.GridId = "6B91CD3F-7C53-4704-AFB8-0338BDF9E241";
			this.ConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfigurationGrid.LayoutKey = "ConfigurationGrid";
			this.ConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationGrid.Name = "ConfigurationGrid";
			this.ConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 354, true);
			this.ConfigurationGrid.TabIndex = 3;
			// 
			// KoreaSouthEInvoicingDSBSummaryAppendingRuleControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConfigurationGrid);
			this.Name = "KoreaSouthEInvoicingDSBSummaryAppendingRuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 354, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationGrid)).EndInit();
			this.ConfigurationGrid.ResumeLayout(false);
			this.ConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
