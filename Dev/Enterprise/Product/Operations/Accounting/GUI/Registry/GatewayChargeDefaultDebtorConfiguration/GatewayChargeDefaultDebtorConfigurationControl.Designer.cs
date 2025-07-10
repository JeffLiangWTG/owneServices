namespace Enterprise.Accounting.Registry.GUI
{
	public partial class GatewayChargeDefaultDebtorConfigurationControl
	{

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.GatewayChargeDefaultDebtorConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GatewayChargeDefaultDebtorConfigurationGrid)).BeginInit();
			this.GatewayChargeDefaultDebtorConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfigurationCollection);
			// 
			// GatewayChargeDefaultDebtorConfigurationGrid
			// 
			this.GatewayChargeDefaultDebtorConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GatewayChargeDefaultDebtorConfigurationGrid, ".");
			this.GatewayChargeDefaultDebtorConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.ChargeGroup);
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.ConsolPaymentTerm);
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.RelatedJob);
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.PreviousSendingAgent);
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo5.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.Debtor);
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.ConsolDirection);
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = nameof(Enterprise.MasterFiles.Business.GatewayChargeDefaultDebtorConfiguration.ConsolTransportMode);
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.GatewayChargeDefaultDebtorConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.GatewayChargeDefaultDebtorConfigurationGrid.CopySelectedRowsAllowed = true;
			this.GatewayChargeDefaultDebtorConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GatewayChargeDefaultDebtorConfigurationGrid.GridId = "1439708F-B6C5-411D-ACB5-79EFA48AB096";
			this.GatewayChargeDefaultDebtorConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GatewayChargeDefaultDebtorConfigurationGrid.LayoutKey = "GatewayChargeDefaultDebtorConfigurationGrid";
			this.GatewayChargeDefaultDebtorConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GatewayChargeDefaultDebtorConfigurationGrid.Name = "GatewayChargeDefaultDebtorConfigurationGrid";
			this.GatewayChargeDefaultDebtorConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.GatewayChargeDefaultDebtorConfigurationGrid.TabIndex = 0;
			// 
			// GatewayChargeDefaultDebtorConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.GatewayChargeDefaultDebtorConfigurationGrid);
			this.Name = "GatewayChargeDefaultDebtorConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GatewayChargeDefaultDebtorConfigurationGrid)).EndInit();
			this.GatewayChargeDefaultDebtorConfigurationGrid.ResumeLayout(false);
			this.GatewayChargeDefaultDebtorConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZGrid GatewayChargeDefaultDebtorConfigurationGrid;
	}
}
