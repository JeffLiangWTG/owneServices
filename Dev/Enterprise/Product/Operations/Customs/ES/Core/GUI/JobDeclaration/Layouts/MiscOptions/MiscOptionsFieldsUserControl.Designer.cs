namespace Enterprise.Customs.ES.GUI;

partial class MiscOptionsFieldsUserControl
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
		this.AuthPerDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.DeclEmailAddrTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.OtherEmailAddrTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.SupportingInformationUserControl = new Enterprise.Customs.ES.GUI.SupportingInformationControl();
		this.DontSendImporterIdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.CertificateDropEdit.SuspendLayout();
		this.SupportingInformationUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// AuthPerDeclarationCheckBox
		// 
		this.AuthPerDeclarationCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.AuthPerDeclarationCheckBox, "ZG_AuthPerDeclaration");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_AuthPerDeclaration)));
		this.AuthPerDeclarationCheckBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("DC8F869E-66A9-43E7-8E2C-71F52E734D19", "[14] Authorization by operation");
		this.AuthPerDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 300, true);
		this.AuthPerDeclarationCheckBox.Name = "AuthPerDeclarationCheckBox";
		this.AuthPerDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
		this.AuthPerDeclarationCheckBox.TabIndex = 16;
		this.AuthPerDeclarationCheckBox.UseVisualStyleBackColor = true;
		// 
		// CertificateDropEdit
		// 
		this.CertificateDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.CertificateDropEdit, "JE_CustomsProfile");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_CustomsProfile)));
		this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 69, true);
		this.CertificateDropEdit.Name = "CertificateDropEdit";
		this.CertificateDropEdit.PreBoundMaxLength = 42;
		this.CertificateDropEdit.ShowDescriptionBox = false;
		this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 20, true);
		this.CertificateDropEdit.TabIndex = 2;
		// 
		// DeclEmailAddrTextBox
		// 
		this.BindingSource.SetBindingMember(this.DeclEmailAddrTextBox, "DeclEmailAddr");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).DeclEmailAddr)));
		this.DeclEmailAddrTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("E38BC52C-8087-4A91-AB27-9B71554A744D", "Declaration Email");
		this.DeclEmailAddrTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 320, true);
		this.DeclEmailAddrTextBox.Name = "DeclEmailAddrTextBox";
		this.DeclEmailAddrTextBox.ReadOnly = true;
		this.DeclEmailAddrTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
		this.DeclEmailAddrTextBox.TabIndex = 17;
		// 
		// OtherEmailAddrTextBox
		// 
		this.BindingSource.SetBindingMember(this.OtherEmailAddrTextBox, "ZG_OtherEmailAddr");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_OtherEmailAddr)));
		this.OtherEmailAddrTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("826f8bdd-7f8a-4757-90f9-c5f1c0af8e67", "Other Comm. Email");
		this.OtherEmailAddrTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 345, true);
		this.OtherEmailAddrTextBox.Name = "OtherEmailAddrTextBox";
		this.OtherEmailAddrTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
		this.OtherEmailAddrTextBox.TabIndex = 18;
		// 
		// SupportingInformationUserControl
		// 
		this.SupportingInformationUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
		this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 0, true);
		this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
		this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 400, true);
		this.SupportingInformationUserControl.TabIndex = 7;
		// 
		// DontSendImporterIdCheckBox
		// 
		this.DontSendImporterIdCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.DontSendImporterIdCheckBox, "ZG_DontSendImporterId");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_DontSendImporterId)));
		this.DontSendImporterIdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 300, true);
		this.DontSendImporterIdCheckBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("48FEF3D0-AC46-4631-A55F-469B27447E96", englishCaption: "Do not send Importer ID Number", englishFullDescription: "If ticked the Importer Identification Number will not be sent.", englishMediumCaption: "Do not send Imp. ID Num", englishShortCaption: "No Send Imp. ID");
		this.DontSendImporterIdCheckBox.Name = "DontSendImporterIdCheckBox";
		this.DontSendImporterIdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
		this.DontSendImporterIdCheckBox.TabIndex = 16;
		this.DontSendImporterIdCheckBox.UseVisualStyleBackColor = true;
		// 
		// MiscOptionsFieldsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.AuthPerDeclarationCheckBox);
		this.Controls.Add(this.CertificateDropEdit);
		this.Controls.Add(this.DeclEmailAddrTextBox);
		this.Controls.Add(this.OtherEmailAddrTextBox);
		this.Controls.Add(this.SupportingInformationUserControl);
		this.Controls.Add(this.DontSendImporterIdCheckBox);
		this.Name = "MiscOptionsFieldsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 596, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.CertificateDropEdit.ResumeLayout(true);
		this.CertificateDropEdit.PerformLayout();
		this.SupportingInformationUserControl.ResumeLayout(true);
		this.SupportingInformationUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.ZTextBox OtherEmailAddrTextBox;
	internal Enterprise.ZArchitecture.ZTextBox DeclEmailAddrTextBox;
	internal Enterprise.ZArchitecture.GUI.ZCheckBox AuthPerDeclarationCheckBox;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
	internal SupportingInformationControl SupportingInformationUserControl;
	internal Enterprise.ZArchitecture.GUI.ZCheckBox DontSendImporterIdCheckBox;
}
