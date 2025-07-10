namespace Enterprise.Customs.KR.GUI
{
	partial class FinalPriceExtensionRequestNewForm
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
		private new void InitializeComponent()
		{
            this.finalPriceExtensionRequestNewUserControl = new Enterprise.Customs.KR.GUI.FinalPriceExtensionRequestNewUserControl();
            this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.SearchButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.finalPriceExtensionRequestNewUserControl.SuspendLayout();
            this.ButtonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 498, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader);
            // 
            // finalPriceExtensionRequestNewUserControl
            // 
            this.finalPriceExtensionRequestNewUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.finalPriceExtensionRequestNewUserControl, ".");
            this.finalPriceExtensionRequestNewUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.finalPriceExtensionRequestNewUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.finalPriceExtensionRequestNewUserControl.Name = "finalPriceExtensionRequestNewUserControl";
            this.finalPriceExtensionRequestNewUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 464, true);
            this.finalPriceExtensionRequestNewUserControl.TabIndex = 0;
            // 
            // ButtonsPanel
            // 
            this.ButtonsPanel.Controls.Add(this.SearchButton);
            this.ButtonsPanel.Controls.Add(this.SendButton);
            this.ButtonsPanel.Controls.Add(this.CancelButtonX);
            this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 464, true);
            this.ButtonsPanel.Name = "ButtonsPanel";
            this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 34, true);
            this.ButtonsPanel.TabIndex = 1;
            // 
            // SearchButton
            // 
            this.SearchButton.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3fda5fd5-47e8-40a2-979f-d8ab41dca957", "Import Declaration Search");
            this.SearchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 26, true);
            this.SearchButton.TabIndex = 2;
            this.SearchButton.ToolTipCaption = null;
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // SendButton
            // 
            this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendButton.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("D550DBFC-6596-4852-8A75-3242A61A3D32", "Send");
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(670, 4, true);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 26, true);
            this.SendButton.TabIndex = 2;
            this.SendButton.ToolTipCaption = null;
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // CancelButtonX
            // 
            this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButtonX.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4054B9B9-3369-469A-AAE9-ADE277C2C4C0", "Cancel");
            this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 4, true);
            this.CancelButtonX.Name = "CancelButtonX";
            this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 26, true);
            this.CancelButtonX.TabIndex = 3;
            this.CancelButtonX.ToolTipCaption = null;
            this.CancelButtonX.UseVisualStyleBackColor = true;
            // 
            // FinalPriceExtensionRequestNewForm
            // 
            this.AcceptButton = this.SendButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.CancelButtonX;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 522, true);
            this.Controls.Add(this.finalPriceExtensionRequestNewUserControl);
            this.Controls.Add(this.ButtonsPanel);
            this.DataSourceType = typeof(Enterprise.Customs.KR.Business.FinalPriceReportByDateExtensionHeader);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 559, true);
            this.Name = "FinalPriceExtensionRequestNewForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "FinalPriceExtensionRequestForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.ButtonsPanel, 0);
            this.Controls.SetChildIndex(this.finalPriceExtensionRequestNewUserControl, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.finalPriceExtensionRequestNewUserControl.ResumeLayout(true);
            this.finalPriceExtensionRequestNewUserControl.PerformLayout();
            this.ButtonsPanel.ResumeLayout(false);
            this.ButtonsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private FinalPriceExtensionRequestNewUserControl finalPriceExtensionRequestNewUserControl;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton CancelButtonX;
		private ZArchitecture.GUI.ZButton SearchButton;
	}
}
