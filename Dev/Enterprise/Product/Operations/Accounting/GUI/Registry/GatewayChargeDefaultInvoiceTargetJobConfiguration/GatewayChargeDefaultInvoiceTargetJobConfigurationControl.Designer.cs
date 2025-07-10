namespace Enterprise.Accounting.Registry.GUI
{
	public partial class GatewayChargeDefaultInvoiceTargetJobConfigurationControl
	{

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid)).BeginInit();
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GatewayChargeDefaultInvoiceTargetJobConfigurationCollection);
			// 
			// GatewayChargeDefaultInvoiceTargetJobConfigurationGrid
			// 
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GatewayChargeDefaultInvoiceTargetJobConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GatewayChargeDefaultInvoiceTargetJobConfiguration)(null)).PreviousSendingAgentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GatewayChargeDefaultInvoiceTargetJobConfiguration)(null)).InvoiceTargetJobType)));
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Previous Sending Agent";
			zDropEditColumnStyleInfo1.ColumnName = "PreviousSendingAgentType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.Caption = "Invoice Target Job";
			zDropEditColumnStyleInfo2.ColumnName = "InvoiceTargetJobType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "ConsolDirection";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.ColumnName = "ConsolTransportMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.CopySelectedRowsAllowed = true;
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.GridId = "FE85542C-9922-4B4D-802F-7BD8246633E2";
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.LayoutKey = "GatewayChargeDefaultInvoiceTargetJobConfigurationGrid";
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.Name = "GatewayChargeDefaultInvoiceTargetJobConfigurationGrid";
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.TabIndex = 0;
			// 
			// GatewayChargeDefaultInvoiceTargetJobConfiguration
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid);
			this.Name = "GatewayChargeDefaultInvoiceTargetJobConfiguration";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid)).EndInit();
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.ResumeLayout(false);
			this.GatewayChargeDefaultInvoiceTargetJobConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZGrid GatewayChargeDefaultInvoiceTargetJobConfigurationGrid;
	}
}
