namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class SupportContractControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupportDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SupportDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportStartedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupportExpiresDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ContractDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContractDetailsTextBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContractDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportDetailsPanel.SuspendLayout();
			this.SupportDetailsGroupBox.SuspendLayout();
			this.SupportStartedDateEdit.SuspendLayout();
			this.SupportModeDropEdit.SuspendLayout();
			this.SupportExpiresDateEdit.SuspendLayout();
			this.ContractDetailsPanel.SuspendLayout();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.ContractDetailsTextBoxPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader);
			// 
			// SupportDetailsPanel
			// 
			this.SupportDetailsPanel.Controls.Add(this.SupportDetailsGroupBox);
			this.SupportDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SupportDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportDetailsPanel.Name = "SupportDetailsPanel";
			this.SupportDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.SupportDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 106, true);
			this.SupportDetailsPanel.TabIndex = 2;
			// 
			// SupportDetailsGroupBox
			// 
			this.SupportDetailsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|ed6fa1a8-de87-4773-b9b8-8c67a82f9fe2", "Support Details");
			this.SupportDetailsGroupBox.Controls.Add(this.SupportStartedDateEdit);
			this.SupportDetailsGroupBox.Controls.Add(this.SupportModeDropEdit);
			this.SupportDetailsGroupBox.Controls.Add(this.SupportExpiresDateEdit);
			this.SupportDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.SupportDetailsGroupBox.Name = "SupportDetailsGroupBox";
			this.SupportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 97, true);
			this.SupportDetailsGroupBox.TabIndex = 0;
			this.SupportDetailsGroupBox.TabStop = false;
			// 
			// SupportStartedDateEdit
			// 
			this.SupportStartedDateEdit.AllowDrop = true;
			this.SupportStartedDateEdit.AutoCompleteMonthThreshold = 1;
			this.SupportStartedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SupportStartedDateEdit, "LA_SupportStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_SupportStartDate)));
			this.SupportStartedDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|207ae0ba-79fb-4245-bdf6-229fdc5afa98", "Support Started");
			this.SupportStartedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 42, true);
			this.SupportStartedDateEdit.Name = "SupportStartedDateEdit";
			this.SupportStartedDateEdit.TabIndex = 1;
			// 
			// SupportModeDropEdit
			// 
			this.SupportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportModeDropEdit, "LA_SupportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_SupportMode)));
			this.SupportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.SupportModeDropEdit.Name = "SupportModeDropEdit";
			this.SupportModeDropEdit.PreBoundMaxLength = 3;
			this.SupportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 17, true);
			this.SupportModeDropEdit.TabIndex = 0;
			// 
			// SupportExpiresDateEdit
			// 
			this.SupportExpiresDateEdit.AllowDrop = true;
			this.SupportExpiresDateEdit.AutoCompleteMonthThreshold = 1;
			this.SupportExpiresDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SupportExpiresDateEdit, "LA_ContractExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_ContractExpiryDate)));
			this.SupportExpiresDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|1c166cb9-dcba-41f6-8add-a731261ba792", "Support Expires");
			this.SupportExpiresDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 68, true);
			this.SupportExpiresDateEdit.Name = "SupportExpiresDateEdit";
			this.SupportExpiresDateEdit.TabIndex = 2;
			// 
			// ContractDetailsPanel
			// 
			this.ContractDetailsPanel.Controls.Add(this.ContactDetailsGroupBox);
			this.ContractDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContractDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 106, true);
			this.ContractDetailsPanel.Name = "ContractDetailsPanel";
			this.ContractDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ContractDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 415, true);
			this.ContractDetailsPanel.TabIndex = 3;
			// 
			// ContactDetailsGroupBox
			// 
			this.ContactDetailsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|ae74ef0b-58fe-49af-8914-c7e2b3468e71", "Contract Details");
			this.ContactDetailsGroupBox.Controls.Add(this.ContractDetailsTextBoxPanel);
			this.ContactDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 405, true);
			this.ContactDetailsGroupBox.TabIndex = 0;
			this.ContactDetailsGroupBox.TabStop = false;
			// 
			// ContractDetailsTextBoxPanel
			// 
			this.ContractDetailsTextBoxPanel.Controls.Add(this.ContractDetailsTextBox);
			this.ContractDetailsTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContractDetailsTextBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ContractDetailsTextBoxPanel.Name = "ContractDetailsTextBoxPanel";
			this.ContractDetailsTextBoxPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ContractDetailsTextBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 389, true);
			this.ContractDetailsTextBoxPanel.TabIndex = 1;
			// 
			// ContractDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContractDetailsTextBox, "LA_SpecialSupportConditions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ContractDetailsTextBox.CaptionResourceString = null;
			this.ContractDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContractDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContractDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ContractDetailsTextBox.Multiline = true;
			this.ContractDetailsTextBox.Name = "ContractDetailsTextBox";
			this.ContractDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 379, true);
			this.ContractDetailsTextBox.TabIndex = 0;
			// 
			// SupportContractControl
			// 
			this.Controls.Add(this.ContractDetailsPanel);
			this.Controls.Add(this.SupportDetailsPanel);
			this.Name = "SupportContractControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 521, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportDetailsPanel.ResumeLayout(false);
			this.SupportDetailsPanel.PerformLayout();
			this.SupportDetailsGroupBox.ResumeLayout(false);
			this.SupportDetailsGroupBox.PerformLayout();
			this.SupportStartedDateEdit.ResumeLayout(true);
			this.SupportStartedDateEdit.PerformLayout();
			this.SupportModeDropEdit.ResumeLayout(true);
			this.SupportModeDropEdit.PerformLayout();
			this.SupportExpiresDateEdit.ResumeLayout(true);
			this.SupportExpiresDateEdit.PerformLayout();
			this.ContractDetailsPanel.ResumeLayout(false);
			this.ContractDetailsPanel.PerformLayout();
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.ContractDetailsTextBoxPanel.ResumeLayout(false);
			this.ContractDetailsTextBoxPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel SupportDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox SupportDetailsGroupBox;
		private ZArchitecture.GUI.ZDateEdit SupportStartedDateEdit;
		private ZArchitecture.GUI.ZDropEdit SupportModeDropEdit;
		private ZArchitecture.GUI.ZDateEdit SupportExpiresDateEdit;
		private ZArchitecture.GUI.ZPanel ContractDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox ContactDetailsGroupBox;
		private ZArchitecture.GUI.ZPanel ContractDetailsTextBoxPanel;
		private ZArchitecture.ZTextBox ContractDetailsTextBox;
	}
}
