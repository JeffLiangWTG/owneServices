namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GuaranteeCalculationLiabilityAmountForm
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
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DefaultLiabilityAmountButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 166, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj);
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7ae8106c-dcd5-4011-ad58-4f1a4072e812", "OK");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 137, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 5;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("31ddde42-434e-4893-a77d-c6d5184b6cb3", "Cancel");
			this.CancelButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 137, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton2.TabIndex = 6;
			this.CancelButton2.ToolTipCaption = null;
			this.CancelButton2.UseVisualStyleBackColor = true;
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton2_Click);
			// 
			// DefaultLiabilityAmountButton
			// 
			this.DefaultLiabilityAmountButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultLiabilityAmountButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("63A1FFFE-5655-4D87-93BB-5D4BABD409FA", "Default");
			this.DefaultLiabilityAmountButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.DefaultLiabilityAmountButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 137, true);
			this.DefaultLiabilityAmountButton.Name = "DefaultLiabilityAmountButton";
			this.DefaultLiabilityAmountButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DefaultLiabilityAmountButton.TabIndex = 4;
			this.DefaultLiabilityAmountButton.ToolTipCaption = null;
			this.DefaultLiabilityAmountButton.UseVisualStyleBackColor = true;
			this.DefaultLiabilityAmountButton.Click += new System.EventHandler(this.DefaultLiabilityAmountButton_Click);
			// 
			// LayoutPanel
			// 
			this.LayoutPanel.AllowDrop = true;
			this.LayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 13, true);
			this.LayoutPanel.Name = "LayoutPanel";
			this.LayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 118, true);
			this.LayoutPanel.TabIndex = 1;
			// 
			// Phase5GuaranteeCalculationLiabilityAmountForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 190, true);
			this.Controls.Add(this.LayoutPanel);
			this.Controls.Add(this.DefaultLiabilityAmountButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CancelButton2);
			this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 229, true);
			this.Name = "Phase5GuaranteeCalculationLiabilityAmountForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.DefaultLiabilityAmountButton, 0);
			this.Controls.SetChildIndex(this.LayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZButton OkButton;
		private ZArchitecture.GUI.ZButton CancelButton2;
		private ZArchitecture.GUI.ZButton DefaultLiabilityAmountButton;
		private ZArchitecture.GUI.DynamicLayoutPanel LayoutPanel;
	}
}
