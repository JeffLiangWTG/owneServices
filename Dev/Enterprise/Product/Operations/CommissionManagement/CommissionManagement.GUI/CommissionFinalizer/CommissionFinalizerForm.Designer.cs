namespace Enterprise.CommissionManagement.GUI
{
	partial class CommissionFinalizerForm
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.filtersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProcessPaymentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FormCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RequestApprovalButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 438, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CommissionFinalizer);
			// 
			// filtersGroupBox
			// 
			this.filtersGroupBox.AutoSize = true;
			this.filtersGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.filtersGroupBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("6f21971a-3f22-419e-b8f7-ae923e56f49e", "Filters");
			this.filtersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filtersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.filtersGroupBox.Name = "filtersGroupBox";
			this.filtersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 405, true);
			this.filtersGroupBox.TabIndex = 0;
			this.filtersGroupBox.TabStop = false;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.ProcessPaymentButton);
			this.bottomPanel.Controls.Add(this.FormCancelButton);
			this.bottomPanel.Controls.Add(this.RequestApprovalButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 408, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 30, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// ProcessPaymentButton
			// 
			this.ProcessPaymentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessPaymentButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("e023e810-6ce7-4058-a8a6-125cb1aa509d", "Process Payment");
			this.ProcessPaymentButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ProcessPaymentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 4, true);
			this.ProcessPaymentButton.Name = "ProcessPaymentButton";
			this.ProcessPaymentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.ProcessPaymentButton.TabIndex = 1;
			this.ProcessPaymentButton.Click += new System.EventHandler(this.ProcessPaymentButton_Click);
			// 
			// FormCancelButton
			// 
			this.FormCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FormCancelButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("9cf1001f-7b70-4bae-9dea-4a9a91238fbc", "Cancel");
			this.FormCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.FormCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(611, 4, true);
			this.FormCancelButton.Name = "FormCancelButton";
			this.FormCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.FormCancelButton.TabIndex = 2;
			this.FormCancelButton.Click += new System.EventHandler(this.FormCancelButton_Click);
			// 
			// RequestApprovalButton
			// 
			this.RequestApprovalButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RequestApprovalButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("e6cc8ed5-2f5a-4eb4-a294-32b67c5c595a", "Request Approval");
			this.RequestApprovalButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 4, true);
			this.RequestApprovalButton.Name = "RequestApprovalButton";
			this.RequestApprovalButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.RequestApprovalButton.TabIndex = 0;
			this.RequestApprovalButton.Click += new System.EventHandler(this.RequestApprovalButton_Click);
			// 
			// CommissionFinalizerForm
			// 
			this.AcceptButton = this.ProcessPaymentButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.FormCancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("9dab55b3-1125-44af-ae29-0f89b522f29b", "Commission Finalizer");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 465, true);
			this.Controls.Add(this.filtersGroupBox);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(CommissionFinalizer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 500, true);
			this.Name = "CommissionFinalizerForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.filtersGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox filtersGroupBox;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		protected ZArchitecture.GUI.ZButton FormCancelButton;
		protected ZArchitecture.GUI.ZButton RequestApprovalButton;
		protected ZArchitecture.GUI.ZButton ProcessPaymentButton;
	}
}
