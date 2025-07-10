namespace Enterprise.Customs.KR.GUI
{
	partial class LEXSEDDetailsUserControl
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
			this.BondedAreaCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CrewCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BlanketDeclarationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GridUserControl = new Enterprise.Customs.KR.GUI.GridUserControl();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsDivisionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SubLocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BondedAreaCodeFindBox.SuspendLayout();
			this.BlanketDeclarationDropEdit.SuspendLayout();
			this.DeclarationDateEdit.SuspendLayout();
			this.GridUserControl.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.CustomsDivisionCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// BondedAreaCodeFindBox
			// 
			this.BondedAreaCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedAreaCodeFindBox, "JE_LocationOtherInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationOtherInformation)));
			this.BondedAreaCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 24, true);
			this.BondedAreaCodeFindBox.Name = "BondedAreaCodeFindBox";
			this.BondedAreaCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BondedAreaCodeFindBox.ParentType = null;
			this.BondedAreaCodeFindBox.ShowDescriptionBox = false;
			this.BondedAreaCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.BondedAreaCodeFindBox.TabIndex = 2;
			// 
			// CrewCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CrewCountCalcEdit, "JE_NoOfCrew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_NoOfCrew)));
			this.CrewCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 24, true);
			this.CrewCountCalcEdit.Name = "CrewCountCalcEdit";
			this.CrewCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.CrewCountCalcEdit.TabIndex = 3;
			this.CrewCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CrewCountCalcEdit.TrackDisposedAccess = true;
			// 
			// BlanketDeclarationDropEdit
			// 
			this.BlanketDeclarationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BlanketDeclarationDropEdit, "JE_IsBlanketDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_IsBlanketDeclaration)));
			this.BlanketDeclarationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 67, true);
			this.BlanketDeclarationDropEdit.Name = "BlanketDeclarationDropEdit";
			this.BlanketDeclarationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.BlanketDeclarationDropEdit.TabIndex = 5;
			// 
			// DeclarationDateEdit
			// 
			this.DeclarationDateEdit.AllowDrop = true;
			this.DeclarationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DeclarationDateEdit, "JE_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_EntryDate)));
			this.DeclarationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 67, true);
			this.DeclarationDateEdit.Name = "DeclarationDateEdit";
			this.DeclarationDateEdit.TabIndex = 6;
			// 
			// GridUserControl
			// 
			this.GridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GridUserControl, ".");
			this.GridUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 91, true);
			this.GridUserControl.Name = "GridUserControl";
			this.GridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 141, true);
			this.GridUserControl.TabIndex = 7;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 3, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 0;
			// 
			// CustomsDivisionCodeFindBox
			// 
			this.CustomsDivisionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsDivisionCodeFindBox, "JE_CustomsDivision");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsDivision)));
			this.CustomsDivisionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 3, true);
			this.CustomsDivisionCodeFindBox.Name = "CustomsDivisionCodeFindBox";
			this.CustomsDivisionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsDivisionCodeFindBox.ParentType = null;
			this.CustomsDivisionCodeFindBox.PreBoundMaxLength = 2;
			this.CustomsDivisionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.CustomsDivisionCodeFindBox.TabIndex = 1;
			// 
			// SubLocationOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubLocationOfGoodsTextBox, "JE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_SubLocationOfGoods)));
			this.SubLocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 45, true);
			this.SubLocationOfGoodsTextBox.Name = "SubLocationOfGoodsTextBox";
			this.SubLocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
			this.SubLocationOfGoodsTextBox.TabIndex = 4;
			// 
			// LEXSEDDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Controls.Add(this.CustomsDivisionCodeFindBox);
			this.Controls.Add(this.BondedAreaCodeFindBox);
			this.Controls.Add(this.CrewCountCalcEdit);
			this.Controls.Add(this.SubLocationOfGoodsTextBox);
			this.Controls.Add(this.BlanketDeclarationDropEdit);
			this.Controls.Add(this.DeclarationDateEdit);
			this.Controls.Add(this.GridUserControl);
			this.Name = "LEXSEDDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 231, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BondedAreaCodeFindBox.ResumeLayout(true);
			this.BondedAreaCodeFindBox.PerformLayout();
			this.BlanketDeclarationDropEdit.ResumeLayout(true);
			this.BlanketDeclarationDropEdit.PerformLayout();
			this.DeclarationDateEdit.ResumeLayout(true);
			this.DeclarationDateEdit.PerformLayout();
			this.GridUserControl.ResumeLayout(true);
			this.GridUserControl.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.CustomsDivisionCodeFindBox.ResumeLayout(true);
			this.CustomsDivisionCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox BondedAreaCodeFindBox;
		internal ZArchitecture.ZCalcEdit CrewCountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit BlanketDeclarationDropEdit;
		internal ZArchitecture.GUI.ZDateEdit DeclarationDateEdit;
		internal GridUserControl GridUserControl;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsDivisionCodeFindBox;
		internal ZArchitecture.ZTextBox SubLocationOfGoodsTextBox;
	}
}
