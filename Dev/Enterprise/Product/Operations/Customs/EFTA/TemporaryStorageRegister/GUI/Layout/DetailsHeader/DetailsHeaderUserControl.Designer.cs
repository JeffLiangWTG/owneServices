namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

partial class DetailsHeaderUserControl
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
		this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.PreviousReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.ArrvialDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
		this.PresentationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
		this.ATBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.PreviousReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.CustomerReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.StatusDropEdit.SuspendLayout();
		this.CustomsOfficeCodeFindBox.SuspendLayout();
		this.PreviousReferenceTypeDropEdit.SuspendLayout();
		this.ArrvialDateEdit.SuspendLayout();
		this.PresentationDateEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader);
		// 
		// StatusDropEdit
		// 
		this.StatusDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.StatusDropEdit, "SRH_Status");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_Status)));
		this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 27, true);
		this.StatusDropEdit.Name = "StatusDropEdit";
		this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
		this.StatusDropEdit.TabIndex = 0;
		// 
		// CustomsOfficeCodeFindBox
		// 
		this.CustomsOfficeCodeFindBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "SRH_CustomsOffice");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_CustomsOffice)));
		this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 148, true);
		this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
		this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.CustomsOfficeCodeFindBox.ParentType = null;
		this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
		this.CustomsOfficeCodeFindBox.TabIndex = 1;
		// 
		// PreviousReferenceTypeDropEdit
		// 
		this.PreviousReferenceTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.PreviousReferenceTypeDropEdit, "SRH_PreviousReferenceType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_PreviousReferenceType)));
		this.PreviousReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 50, true);
		this.PreviousReferenceTypeDropEdit.Name = "PreviousReferenceTypeDropEdit";
		this.PreviousReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
		this.PreviousReferenceTypeDropEdit.TabIndex = 2;
		// 
		// ArrvialDateEdit
		// 
		this.ArrvialDateEdit.AllowDrop = true;
		this.ArrvialDateEdit.AutoCompleteMonthThreshold = 1;
		this.BindingSource.SetBindingMember(this.ArrvialDateEdit, "SRH_ArrivalDate");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_ArrivalDate)));
		this.ArrvialDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 75, true);
		this.ArrvialDateEdit.Name = "ArrvialDateEdit";
		this.ArrvialDateEdit.TabIndex = 3;
		// 
		// PresentationDateEdit
		// 
		this.PresentationDateEdit.AllowDrop = true;
		this.PresentationDateEdit.AutoCompleteMonthThreshold = 1;
		this.BindingSource.SetBindingMember(this.PresentationDateEdit, "SRH_PresentationDate");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_PresentationDate)));
		this.PresentationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
		this.PresentationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 98, true);
		this.PresentationDateEdit.Name = "PresentationDateEdit";
		this.PresentationDateEdit.TabIndex = 4;
		// 
		// ATBNumberTextBox
		// 
		this.BindingSource.SetBindingMember(this.ATBNumberTextBox, "SRH_Reference");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_Reference)));
		this.ATBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 4, true);
		this.ATBNumberTextBox.Name = "ATBNumberTextBox";
		this.ATBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
		this.ATBNumberTextBox.TabIndex = 5;
		// 
		// PreviousReferenceNumberTextBox
		// 
		this.BindingSource.SetBindingMember(this.PreviousReferenceNumberTextBox, "SRH_PreviousReference");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_PreviousReference)));
		this.PreviousReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 122, true);
		this.PreviousReferenceNumberTextBox.Name = "PreviousReferenceNumberTextBox";
		this.PreviousReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 15, true);
		this.PreviousReferenceNumberTextBox.TabIndex = 6;
		// 
		// CustomerReferenceTextBox
		// 
		this.BindingSource.SetBindingMember(this.CustomerReferenceTextBox, "SRH_InternalReference");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).SRH_InternalReference)));
		this.CustomerReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 175, true);
		this.CustomerReferenceTextBox.Name = "CustomerReferenceTextBox";
		this.CustomerReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
		this.CustomerReferenceTextBox.TabIndex = 7;
		// 
		// DetailsHeaderUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.StatusDropEdit);
		this.Controls.Add(this.CustomsOfficeCodeFindBox);
		this.Controls.Add(this.PreviousReferenceTypeDropEdit);
		this.Controls.Add(this.ArrvialDateEdit);
		this.Controls.Add(this.PresentationDateEdit);
		this.Controls.Add(this.ATBNumberTextBox);
		this.Controls.Add(this.PreviousReferenceNumberTextBox);
		this.Controls.Add(this.CustomerReferenceTextBox);
		this.Name = "DetailsHeaderUserControl";
		this.ShouldSerializeTabPageMethods = true;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 208, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.StatusDropEdit.ResumeLayout(true);
		this.StatusDropEdit.PerformLayout();
		this.CustomsOfficeCodeFindBox.ResumeLayout(true);
		this.CustomsOfficeCodeFindBox.PerformLayout();
		this.PreviousReferenceTypeDropEdit.ResumeLayout(true);
		this.PreviousReferenceTypeDropEdit.PerformLayout();
		this.ArrvialDateEdit.ResumeLayout(true);
		this.ArrvialDateEdit.PerformLayout();
		this.PresentationDateEdit.ResumeLayout(true);
		this.PresentationDateEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDropEdit StatusDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
	internal ZArchitecture.GUI.ZDropEdit PreviousReferenceTypeDropEdit;
	internal ZArchitecture.GUI.ZDateEdit PresentationDateEdit;
	internal ZArchitecture.GUI.ZDateEdit ArrvialDateEdit;
	internal ZArchitecture.ZTextBox ATBNumberTextBox;
	internal ZArchitecture.ZTextBox PreviousReferenceNumberTextBox;
	internal ZArchitecture.ZTextBox CustomerReferenceTextBox;
}
