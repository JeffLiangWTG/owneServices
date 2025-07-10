using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting
{
	partial class ConsolRevenueApportionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();
				components?.Dispose();
			}
			base.Dispose(disposing);
		}
		private Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		public Enterprise.ZArchitecture.GUI.ZButton ApportionButton;
		private Enterprise.ZArchitecture.GUI.ZPanel ApportionControlPanel;
		public ConsolRevenueApportionmentUserControl ConsolRevenuesControl;

		#region Designer generated Code

		new void InitializeComponent()
		{
			this.ButtonsPanel = new ZPanel();
			this.CloseButton = new ZButton();
			this.ApportionButton = new ZButton();
			this.ApportionControlPanel = new ZPanel();
			this.ConsolRevenuesControl = new ConsolRevenueApportionmentUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			this.ApportionControlPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 570, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ConsolRevenueMaster);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.CloseButton);
			this.ButtonsPanel.Controls.Add(this.ApportionButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 531, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 39, true);
			this.ButtonsPanel.TabIndex = 7;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(906, 7, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// ApportionButton
			// 
			this.ApportionButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.ApportionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(826, 7, true);
			this.ApportionButton.Name = "ApportionButton";
			this.ApportionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ApportionButton.TabIndex = 6;
			this.ApportionButton.UseVisualStyleBackColor = true;
			// 
			// ApportionControlPanel
			// 
			this.ApportionControlPanel.Controls.Add(this.ConsolRevenuesControl);
			this.ApportionControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApportionControlPanel.Name = "ApportionControlPanel";
			this.ApportionControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 531, true);
			this.ApportionControlPanel.TabIndex = 8;
			// 
			// ConsolRevenuesControl
			// 
			this.BindingSource.SetBindingMember(this.ConsolRevenuesControl, ".");
			this.ConsolRevenuesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolRevenuesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolRevenuesControl.Name = "ConsolRevenuesControl";
			this.ConsolRevenuesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 531, true);
			this.ConsolRevenuesControl.TabIndex = 0;
			// 
			// ConsolRevenueApportionForm
			// 
			this.AcceptButton = this.ApportionButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 594, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolRevenueApportionForm|9f186d6f-5aff-4a95-9b99-db1f47356dd2", "Consol Revenue Apportion", "Consol Revenue Apportion", "Consol Revenue Apportion", "");
			this.Controls.Add(this.ApportionControlPanel);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceType = typeof(ConsolRevenueMaster);
			this.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
			this.IsPostOnly = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(907, 630, true);
			this.Name = "ConsolRevenueApportionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.ApportionControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ApportionControlPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}
