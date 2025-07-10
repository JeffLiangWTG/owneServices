namespace Enterprise.Customs.IN.GUI;

partial class DeclarationOtherDetailsUserControl
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
			this.OriginStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExporterClassTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IECCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EPZCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BranchSerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizedDealerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeOfExporterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SealByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RotationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RotationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StuffingAtDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SampleAccompaniedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsRegistrationSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.TranshipperGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OriginStateDropEdit.SuspendLayout();
			this.EPZCodeDropEdit.SuspendLayout();
			this.TypeOfExporterDropEdit.SuspendLayout();
			this.SealByDropEdit.SuspendLayout();
			this.RotationDateDateEdit.SuspendLayout();
			this.StuffingAtDropEdit.SuspendLayout();
			this.SampleAccompaniedDropEdit.SuspendLayout();
			this.GoodsRegistrationSeparatorUserControl.SuspendLayout();
			this.TranshipperGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
			// 
			// OriginStateDropEdit
			// 
			this.OriginStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginStateDropEdit, "JE_RW_NKOriginState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_RW_NKOriginState)));
			this.OriginStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 31, true);
			this.OriginStateDropEdit.Name = "OriginStateDropEdit";
			this.OriginStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.OriginStateDropEdit.TabIndex = 1;
			// 
			// ExporterClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterClassTextBox, "ExporterClassDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).ExporterClassDescription)));
			this.ExporterClassTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExporterClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 57, true);
			this.ExporterClassTextBox.Name = "ExporterClassTextBox";
			this.ExporterClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.ExporterClassTextBox.TabIndex = 2;
			// 
			// IECCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.IECCodeTextBox, "IECCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).IECCode)));
			this.IECCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 5, true);
			this.IECCodeTextBox.Name = "IECCodeTextBox";
			this.IECCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.IECCodeTextBox.TabIndex = 0;
			// 
			// EPZCodeDropEdit
			// 
			this.EPZCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EPZCodeDropEdit, "JE_EPZCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_EPZCode)));
			this.EPZCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 83, true);
			this.EPZCodeDropEdit.Name = "EPZCodeDropEdit";
			this.EPZCodeDropEdit.PreBoundMaxLength = 2;
			this.EPZCodeDropEdit.ShouldResizeByMaxLength = false;
			this.EPZCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.EPZCodeDropEdit.TabIndex = 3;
			// 
			// BranchSerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BranchSerialNumberTextBox, "BranchSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).BranchSerialNumber)));
			this.BranchSerialNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BranchSerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 109, true);
			this.BranchSerialNumberTextBox.Name = "BranchSerialNumberTextBox";
			this.BranchSerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.BranchSerialNumberTextBox.TabIndex = 4;
			// 
			// AuthorizedDealerCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorizedDealerCodeTextBox, "AuthorizedDealerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).AuthorizedDealerCode)));
			this.AuthorizedDealerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 135, true);
			this.AuthorizedDealerCodeTextBox.Name = "AuthorizedDealerCodeTextBox";
			this.AuthorizedDealerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.AuthorizedDealerCodeTextBox.TabIndex = 5;
			// 
			// TypeOfExporterDropEdit
			// 
			this.TypeOfExporterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeOfExporterDropEdit, "JE_ExporterType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_ExporterType)));
			this.TypeOfExporterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 161, true);
			this.TypeOfExporterDropEdit.Name = "TypeOfExporterDropEdit";
			this.TypeOfExporterDropEdit.PreBoundMaxLength = 2;
			this.TypeOfExporterDropEdit.ShouldResizeByMaxLength = false;
			this.TypeOfExporterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.TypeOfExporterDropEdit.TabIndex = 6;
			// 
			// SealByDropEdit
			// 
			this.SealByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SealByDropEdit, "JE_SealBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_SealBy)));
			this.SealByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 190, true);
			this.SealByDropEdit.Name = "SealByDropEdit";
			this.SealByDropEdit.PreBoundMaxLength = 2;
			this.SealByDropEdit.ShouldResizeByMaxLength = false;
			this.SealByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.SealByDropEdit.TabIndex = 6;
			// 
			// RotationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RotationNumberTextBox, "JE_RotationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_RotationNumber)));
			this.RotationNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RotationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 221, true);
			this.RotationNumberTextBox.Name = "RotationNumberTextBox";
			this.RotationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.RotationNumberTextBox.TabIndex = 7;
			this.RotationNumberTextBox.TrackDisposedAccess = true;
			// 
			// RotationDateDateEdit
			// 
			this.RotationDateDateEdit.AllowDrop = true;
			this.RotationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RotationDateDateEdit, "JE_RotationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_RotationDate)));
			this.RotationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 249, true);
			this.RotationDateDateEdit.Name = "RotationDateDateEdit";
			this.RotationDateDateEdit.TabIndex = 8;
			// 
			// StuffingAtDropEdit
			// 
			this.StuffingAtDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StuffingAtDropEdit, "JE_StuffingAt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_StuffingAt)));
			this.StuffingAtDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 277, true);
			this.StuffingAtDropEdit.Name = "StuffingAtDropEdit";
			this.StuffingAtDropEdit.PreBoundMaxLength = 2;
			this.StuffingAtDropEdit.ShouldResizeByMaxLength = false;
			this.StuffingAtDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.StuffingAtDropEdit.TabIndex = 7;
			// 
			// SampleAccompaniedDropEdit
			// 
			this.SampleAccompaniedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SampleAccompaniedDropEdit, "JE_SampleAccompanied");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).JE_SampleAccompanied)));
			this.SampleAccompaniedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 305, true);
			this.SampleAccompaniedDropEdit.Name = "SampleAccompaniedDropEdit";
			this.SampleAccompaniedDropEdit.PreBoundMaxLength = 2;
			this.SampleAccompaniedDropEdit.ShouldResizeByMaxLength = false;
			this.SampleAccompaniedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.SampleAccompaniedDropEdit.TabIndex = 8;
			//
			// GoodsRegistrationSeparatorUserControl
			//
			this.GoodsRegistrationSeparatorUserControl.AllowDrop = true;
			this.GoodsRegistrationSeparatorUserControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("182AB6DA-0C91-4EC0-A2AE-E5FCA960CB49", "Goods Registration");
			this.GoodsRegistrationSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 333, true);
			this.GoodsRegistrationSeparatorUserControl.Name = "GoodsRegistrationSeparatorUserControl";
			this.GoodsRegistrationSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 15, true);
			this.GoodsRegistrationSeparatorUserControl.TabIndex = 9;
			//
			// TranshipperGuidFindBox
			//
			this.TranshipperGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TranshipperGuidFindBox, "TranshipperDocAddress+OrganisationPK");
			this.TranshipperGuidFindBox.BindToList = "Lookups+TranshipperCollection";
			this.TranshipperGuidFindBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("0ABF6581-79F2-4B2E-B2AF-C229BD4416E1", "Transhipper");
			this.TranshipperGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 355, true);
			this.TranshipperGuidFindBox.Name = "TranshipperGuidFindBox";
			this.TranshipperGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 20, true);
			this.TranshipperGuidFindBox.TabIndex = 10;
			// 
			// DeclarationOtherDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RotationDateDateEdit);
			this.Controls.Add(this.RotationNumberTextBox);
			this.Controls.Add(this.SampleAccompaniedDropEdit);
			this.Controls.Add(this.OriginStateDropEdit);
			this.Controls.Add(this.ExporterClassTextBox);
			this.Controls.Add(this.IECCodeTextBox);
			this.Controls.Add(this.EPZCodeDropEdit);
			this.Controls.Add(this.BranchSerialNumberTextBox);
			this.Controls.Add(this.AuthorizedDealerCodeTextBox);
			this.Controls.Add(this.TypeOfExporterDropEdit);
			this.Controls.Add(this.SealByDropEdit);
			this.Controls.Add(this.StuffingAtDropEdit);
			this.Controls.Add(this.GoodsRegistrationSeparatorUserControl);
			this.Controls.Add(this.TranshipperGuidFindBox);
			this.Name = "DeclarationOtherDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 388, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OriginStateDropEdit.ResumeLayout(true);
			this.OriginStateDropEdit.PerformLayout();
			this.EPZCodeDropEdit.ResumeLayout(true);
			this.EPZCodeDropEdit.PerformLayout();
			this.TypeOfExporterDropEdit.ResumeLayout(true);
			this.TypeOfExporterDropEdit.PerformLayout();
			this.SealByDropEdit.ResumeLayout(true);
			this.SealByDropEdit.PerformLayout();
			this.RotationDateDateEdit.ResumeLayout(true);
			this.RotationDateDateEdit.PerformLayout();
			this.StuffingAtDropEdit.ResumeLayout(true);
			this.StuffingAtDropEdit.PerformLayout();
			this.SampleAccompaniedDropEdit.ResumeLayout(true);
			this.SampleAccompaniedDropEdit.PerformLayout();
			this.GoodsRegistrationSeparatorUserControl.ResumeLayout(true);
			this.GoodsRegistrationSeparatorUserControl.PerformLayout();
			this.TranshipperGuidFindBox.ResumeLayout(true);
			this.TranshipperGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZDropEdit OriginStateDropEdit;
	internal Enterprise.ZArchitecture.ZTextBox ExporterClassTextBox;
	internal Enterprise.ZArchitecture.ZTextBox IECCodeTextBox;
	internal ZArchitecture.GUI.ZDropEdit EPZCodeDropEdit;
	internal ZArchitecture.ZTextBox BranchSerialNumberTextBox;
	internal Enterprise.ZArchitecture.ZTextBox AuthorizedDealerCodeTextBox;
	internal ZArchitecture.GUI.ZDropEdit TypeOfExporterDropEdit;
	internal ZArchitecture.GUI.ZDropEdit SealByDropEdit;
	internal Enterprise.ZArchitecture.ZTextBox RotationNumberTextBox;
	internal Enterprise.ZArchitecture.GUI.ZDateEdit RotationDateDateEdit;
	internal ZArchitecture.GUI.ZDropEdit StuffingAtDropEdit;
	internal ZArchitecture.GUI.ZDropEdit SampleAccompaniedDropEdit;
	internal ZArchitecture.GUI.SeparatorUserControl GoodsRegistrationSeparatorUserControl;
	internal Enterprise.ZArchitecture.GUI.ZGuidFindBox TranshipperGuidFindBox;
}
