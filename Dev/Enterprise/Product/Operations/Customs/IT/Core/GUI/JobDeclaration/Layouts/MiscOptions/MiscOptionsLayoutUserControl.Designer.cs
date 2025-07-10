namespace Enterprise.Customs.IT.GUI;

partial class MiscOptionsLayoutUserControl
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
		this.SupportingInformationUserControl = new Enterprise.Customs.IT.GUI.SupportingInformationControl();
		this.PreClearingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.BadgeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.SubscriberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.DefermentAccountNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.BadgeCodeDropEdit.SuspendLayout();
		this.SubscriberDropEdit.SuspendLayout();
		this.DefermentAccountNumberDropEdit.SuspendLayout();
		this.SupportingInformationUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
		// 
		// SupportingInformationUserControl
		// 
		this.SupportingInformationUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
		this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
		this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
		this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 484, true);
		this.SupportingInformationUserControl.TabIndex = 0;
		// 
		// PreClearingCheckBox
		// 
		this.BindingSource.SetBindingMember(this.PreClearingCheckBox, "ZG_PreClearing");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).ZG_PreClearing)));
		this.PreClearingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 324, true);
		this.PreClearingCheckBox.Name = "PreClearingCheckBox";
		this.PreClearingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 24, true);
		this.PreClearingCheckBox.TabIndex = 17;
		// 
		// BadgeCodeDropEdit
		// 
		this.BadgeCodeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.BadgeCodeDropEdit, "JE_CustomsProfile");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_CustomsProfile)));
		this.BadgeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 44, true);
		this.BadgeCodeDropEdit.Name = "BadgeCodeDropEdit";
		this.BadgeCodeDropEdit.PreBoundMaxLength = 3;
		this.BadgeCodeDropEdit.ShowDescriptionBox = false;
		this.BadgeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
		this.BadgeCodeDropEdit.TabIndex = 1;
		// 
		// SubscriberDropEdit
		// 
		this.SubscriberDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SubscriberDropEdit, "JE_GS_NKCusAgent");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_GS_NKCusAgent)));
		this.SubscriberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 69, true);
		this.SubscriberDropEdit.Name = "SubscriberDropEdit";
		this.SubscriberDropEdit.PreBoundMaxLength = 3;
		this.SubscriberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
		this.SubscriberDropEdit.TabIndex = 2;
		// 
		// DefermentAccountNumberDropEdit
		// 
		this.DefermentAccountNumberDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.DefermentAccountNumberDropEdit, "JE_DefermentAccountNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_DefermentAccountNumber)));
		this.DefermentAccountNumberDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("MiscOptionsUserControl|e0d93328-fdb4-42ff-9d22-cf127389caff", "Approval Defer No.");
		this.DefermentAccountNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 45, true);
		this.DefermentAccountNumberDropEdit.Name = "DefermentAccountNumberDropEdit";
		this.DefermentAccountNumberDropEdit.PreBoundMaxLength = 16;
		this.DefermentAccountNumberDropEdit.ShowDescriptionBox = false;
		this.DefermentAccountNumberDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
		this.DefermentAccountNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
		this.DefermentAccountNumberDropEdit.TabIndex = 1;
		// 
		// MiscOptionsUserControl
		// 
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.SupportingInformationUserControl);
		this.Controls.Add(this.PreClearingCheckBox);
		this.Controls.Add(this.BadgeCodeDropEdit);
		this.Controls.Add(this.SubscriberDropEdit);
		this.Controls.Add(this.DefermentAccountNumberDropEdit);
		this.Name = "MiscOptionsLayoutUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 94, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.PreClearingCheckBox.ResumeLayout(true);
		this.PreClearingCheckBox.PerformLayout();
		this.BadgeCodeDropEdit.ResumeLayout(true);
		this.BadgeCodeDropEdit.PerformLayout();
		this.SubscriberDropEdit.ResumeLayout(true);
		this.SubscriberDropEdit.PerformLayout();
		this.DefermentAccountNumberDropEdit.ResumeLayout(true);
		this.DefermentAccountNumberDropEdit.PerformLayout();
		this.SupportingInformationUserControl.ResumeLayout(true);
		this.SupportingInformationUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	internal SupportingInformationControl SupportingInformationUserControl;
	internal ZArchitecture.GUI.ZCheckBox PreClearingCheckBox;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit BadgeCodeDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit SubscriberDropEdit;
	internal ZArchitecture.GUI.ZDropEdit DefermentAccountNumberDropEdit;

	#endregion
}
