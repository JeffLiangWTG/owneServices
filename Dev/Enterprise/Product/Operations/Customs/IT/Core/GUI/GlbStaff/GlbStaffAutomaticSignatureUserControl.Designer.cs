namespace Enterprise.Customs.IT.GUI;

partial class GlbStaffAutomaticSignatureUserControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.IsConfigurationActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.DelegateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.UserTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.FiscalUserTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ClearAutomaticSignatureButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.AddAutomaticSignatureButton = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.DelegateDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.GlbStaffWrapper);
		// 
		// IsConfigurationActiveCheckBox
		// 
		this.BindingSource.SetBindingMember(this.IsConfigurationActiveCheckBox, "AutomaticSignaturePasswordCollection.IsConfigurationActive");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.AutomaticSignatureExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).AutomaticSignaturePasswordCollection)).SyncRoot)).IsConfigurationActive)));
		this.IsConfigurationActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 31, true);
		this.IsConfigurationActiveCheckBox.Name = "IsConfigurationActiveCheckBox";
		this.IsConfigurationActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
		this.IsConfigurationActiveCheckBox.TabIndex = 0;
		// 
		// DelegateDropEdit
		// 
		this.DelegateDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.DelegateDropEdit, "AutomaticSignaturePasswordCollection.GP_MailBoxID");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.AutomaticSignatureExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).AutomaticSignaturePasswordCollection)).SyncRoot)).GP_MailBoxID)));
		this.DelegateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 57, true);
		this.DelegateDropEdit.Name = "DelegateDropEdit";
		this.DelegateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
		this.DelegateDropEdit.TabIndex = 1;
		// 
		// UserTextBox
		// 
		this.BindingSource.SetBindingMember(this.UserTextBox, "AutomaticSignaturePasswordCollection.GP_UserID");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.AutomaticSignatureExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).AutomaticSignaturePasswordCollection)).SyncRoot)).GP_UserID)));
		this.UserTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.UserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 83, true);
		this.UserTextBox.Name = "UserTextBox";
		this.UserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
		this.UserTextBox.TabIndex = 2;
		// 
		// FiscalUserTextBox
		// 
		this.BindingSource.SetBindingMember(this.FiscalUserTextBox, "AutomaticSignaturePasswordCollection.GP_Name");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.AutomaticSignatureExternalPassword)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GlbStaffWrapper)(null)).AutomaticSignaturePasswordCollection)).SyncRoot)).GP_Name)));
		this.FiscalUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 112, true);
		this.FiscalUserTextBox.Name = "FiscalUserTextBox";
		this.FiscalUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
		this.FiscalUserTextBox.TabIndex = 3;
		// 
		// ClearAutomaticSignatureButton
		// 
		this.ClearAutomaticSignatureButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("40192fde-b47f-4b1b-9f01-f156c2611a53", "Clear Signature");
		this.ClearAutomaticSignatureButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 3, true);
		this.ClearAutomaticSignatureButton.Name = "ClearAutomaticSignatureButton";
		this.ClearAutomaticSignatureButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
		this.ClearAutomaticSignatureButton.TabIndex = 5;
		this.ClearAutomaticSignatureButton.ToolTipCaption = null;
		// 
		// AddAutomaticSignatureButton
		// 
		this.AddAutomaticSignatureButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("38ef0ef1-22f0-4d8c-a1f9-c69f3e8f3f51", "Add Signature");
		this.AddAutomaticSignatureButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 3, true);
		this.AddAutomaticSignatureButton.Name = "AddAutomaticSignatureButton";
		this.AddAutomaticSignatureButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
		this.AddAutomaticSignatureButton.TabIndex = 4;
		this.AddAutomaticSignatureButton.ToolTipCaption = null;
		// 
		// GlbStaffAutomaticSignatureUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.ClearAutomaticSignatureButton);
		this.Controls.Add(this.AddAutomaticSignatureButton);
		this.Controls.Add(this.IsConfigurationActiveCheckBox);
		this.Controls.Add(this.DelegateDropEdit);
		this.Controls.Add(this.UserTextBox);
		this.Controls.Add(this.FiscalUserTextBox);
		this.Name = "GlbStaffAutomaticSignatureUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 135, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.DelegateDropEdit.ResumeLayout(true);
		this.DelegateDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	Enterprise.ZArchitecture.GUI.ZCheckBox IsConfigurationActiveCheckBox;
	Enterprise.ZArchitecture.GUI.ZDropEdit DelegateDropEdit;
	Enterprise.ZArchitecture.ZTextBox UserTextBox;
	Enterprise.ZArchitecture.ZTextBox FiscalUserTextBox;
	ZArchitecture.GUI.ZButton ClearAutomaticSignatureButton;
	ZArchitecture.GUI.ZButton AddAutomaticSignatureButton;

	#endregion
}
