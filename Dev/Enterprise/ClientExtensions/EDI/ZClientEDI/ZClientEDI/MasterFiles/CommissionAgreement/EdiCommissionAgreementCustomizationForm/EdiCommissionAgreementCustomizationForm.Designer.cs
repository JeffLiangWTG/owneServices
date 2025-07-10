namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EdiCommissionAgreementCustomizationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.customizationTreeControl = new Enterprise.Client.EDI.MasterFiles.GUI.EdiCommissionAgreementCustomizationTreeControl();
			this.LE_EnterpriseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.includedUsagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.topPanelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.customizationTreeControl.SuspendLayout();
			this.includedUsagesGroupBox.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.topPanelGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 539, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreementCustomization);
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.closeButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 510, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 29, true);
			this.bottomPanel.TabIndex = 3;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = ZClientEDI.Res.GetData("7a5d54dd-6f86-4835-aeab-6dbabc876840", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(723, 4, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.closeButton.TabIndex = 1;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// customizationTreeControl
			// 
			this.customizationTreeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customizationTreeControl, ".");
			this.customizationTreeControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customizationTreeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.customizationTreeControl.Name = "customizationTreeControl";
			this.customizationTreeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 425, true);
			this.customizationTreeControl.TabIndex = 0;
			// 
			// LE_EnterpriseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LE_EnterpriseCodeTextBox, "LicenceEnterprise.LE_EnterpriseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreementCustomization)(null)).LicenceEnterprise.LE_EnterpriseCode)));
			this.LE_EnterpriseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 22, true);
			this.LE_EnterpriseCodeTextBox.Name = "LE_EnterpriseCodeTextBox";
			this.LE_EnterpriseCodeTextBox.ReadOnly = true;
			this.LE_EnterpriseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 18, true);
			this.LE_EnterpriseCodeTextBox.TabIndex = 0;
			// 
			// includedUsagesGroupBox
			// 
			this.includedUsagesGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("33ebc367-1bdf-4987-b546-0299f9829355", "Included Usages");
			this.includedUsagesGroupBox.Controls.Add(this.customizationTreeControl);
			this.includedUsagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.includedUsagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 60, true);
			this.includedUsagesGroupBox.Name = "includedUsagesGroupBox";
			this.includedUsagesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.includedUsagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 450, true);
			this.includedUsagesGroupBox.TabIndex = 0;
			this.includedUsagesGroupBox.TabStop = false;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.topPanelGroupBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 60, true);
			this.topPanel.TabIndex = 0;
			// 
			// topPanelGroupBox
			// 
			this.topPanelGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("e2cc954d-b0a9-4c5a-a17c-eedaeab7fe60", "License Info");
			this.topPanelGroupBox.Controls.Add(this.LE_EnterpriseCodeTextBox);
			this.topPanelGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.topPanelGroupBox.Name = "topPanelGroupBox";
			this.topPanelGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.topPanelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 48, true);
			this.topPanelGroupBox.TabIndex = 1;
			this.topPanelGroupBox.TabStop = false;
			// 
			// EdiCommissionAgreementCustomizationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = ZClientEDI.Res.GetData("82b95d6a-4268-417b-964e-9b8db6ef5b68", "Customer Filters");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 563, true);
			this.Controls.Add(this.includedUsagesGroupBox);
			this.Controls.Add(this.bottomPanel);
			this.Controls.Add(this.topPanel);
			this.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreementCustomization);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.Name = "EdiCommissionAgreementCustomizationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.topPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.includedUsagesGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.customizationTreeControl.ResumeLayout(true);
			this.customizationTreeControl.PerformLayout();
			this.includedUsagesGroupBox.ResumeLayout(false);
			this.includedUsagesGroupBox.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.topPanelGroupBox.ResumeLayout(false);
			this.topPanelGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZButton closeButton;
		private EdiCommissionAgreementCustomizationTreeControl customizationTreeControl;
		private ZArchitecture.ZTextBox LE_EnterpriseCodeTextBox;
		private ZArchitecture.GUI.ZGroupBox includedUsagesGroupBox;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.GUI.ZGroupBox topPanelGroupBox;
	}
}