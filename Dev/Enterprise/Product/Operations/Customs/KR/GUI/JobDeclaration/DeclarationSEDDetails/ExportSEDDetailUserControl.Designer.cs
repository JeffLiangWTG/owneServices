namespace Enterprise.Customs.KR.GUI
{
	partial class ExportSEDDetailUserControl
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
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CustomsDivisionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.GoodsDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.InspectionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.SimpleDRWAppDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.GoodsConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SubLocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocationQualifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.BondedAreaCodeFindtBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ContainerPackModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.LocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SEDDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LocationIDInBondedAreaTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.CustomsDivisionCodeFindBox.SuspendLayout();
            this.GoodsDestinationCodeFindBox.SuspendLayout();
            this.InspectionDateEdit.SuspendLayout();
            this.SimpleDRWAppDropEdit.SuspendLayout();
            this.GoodsConditionDropEdit.SuspendLayout();
            this.BondedAreaCodeFindtBox.SuspendLayout();
            this.ContainerPackModeDropEdit.SuspendLayout();
            this.SEDDetailsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "JE_CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 0;
            // 
            // CustomsDivisionCodeFindBox
            // 
            this.CustomsDivisionCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsDivisionCodeFindBox, "JE_CustomsDivision");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsDivision)));
            this.CustomsDivisionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 16, true);
            this.CustomsDivisionCodeFindBox.Name = "CustomsDivisionCodeFindBox";
            this.CustomsDivisionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsDivisionCodeFindBox.ParentType = null;
            this.CustomsDivisionCodeFindBox.PreBoundMaxLength = 2;
            this.CustomsDivisionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.CustomsDivisionCodeFindBox.TabIndex = 1;
            // 
            // GoodsDestinationCodeFindBox
            // 
            this.GoodsDestinationCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsDestinationCodeFindBox, "JE_GoodsDestination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GoodsDestination)));
            this.GoodsDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 37, true);
            this.GoodsDestinationCodeFindBox.Name = "GoodsDestinationCodeFindBox";
            this.GoodsDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.GoodsDestinationCodeFindBox.ParentType = null;
            this.GoodsDestinationCodeFindBox.PreBoundMaxLength = 2;
            this.GoodsDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.GoodsDestinationCodeFindBox.TabIndex = 2;
            // 
            // InspectionDateEdit
            // 
            this.InspectionDateEdit.AllowDrop = true;
            this.InspectionDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.InspectionDateEdit, "InspectionDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).InspectionDate)));
            this.InspectionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 37, true);
            this.InspectionDateEdit.Name = "InspectionDateEdit";
            this.InspectionDateEdit.TabIndex = 3;
            // 
            // SimpleDRWAppDropEdit
            // 
            this.SimpleDRWAppDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SimpleDRWAppDropEdit, "JE_SimpleDRWApp");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_SimpleDRWApp)));
            this.SimpleDRWAppDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 58, true);
            this.SimpleDRWAppDropEdit.Name = "SimpleDRWAppDropEdit";
            this.SimpleDRWAppDropEdit.PreBoundMaxLength = 2;
            this.SimpleDRWAppDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.SimpleDRWAppDropEdit.TabIndex = 5;
            // 
            // GoodsConditionDropEdit
            // 
            this.GoodsConditionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsConditionDropEdit, "JE_GoodsCondition");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GoodsCondition)));
            this.GoodsConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 80, true);
            this.GoodsConditionDropEdit.Name = "GoodsConditionDropEdit";
            this.GoodsConditionDropEdit.PreBoundMaxLength = 1;
            this.GoodsConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.GoodsConditionDropEdit.TabIndex = 7;
            // 
            // SubLocationOfGoodsTextBox
            // 
            this.BindingSource.SetBindingMember(this.SubLocationOfGoodsTextBox, "JE_SubLocationOfGoods");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_SubLocationOfGoods)));
            this.SubLocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 126, true);
            this.SubLocationOfGoodsTextBox.Name = "SubLocationOfGoodsTextBox";
            this.SubLocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 20, true);
            this.SubLocationOfGoodsTextBox.TabIndex = 10;
            // 
            // LocationQualifierTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocationQualifierTextBox, "JE_LocationQualifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationQualifier)));
            this.LocationQualifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 168, true);
            this.LocationQualifierTextBox.Name = "LocationQualifierTextBox";
            this.LocationQualifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
            this.LocationQualifierTextBox.TabIndex = 12;
            // 
            // BondedAreaCodeFindtBox
            // 
            this.BondedAreaCodeFindtBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BondedAreaCodeFindtBox, "JE_LocationOtherInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationOtherInformation)));
            this.BondedAreaCodeFindtBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 104, true);
            this.BondedAreaCodeFindtBox.Name = "BondedAreaCodeFindtBox";
            this.BondedAreaCodeFindtBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BondedAreaCodeFindtBox.ParentType = null;
            this.BondedAreaCodeFindtBox.ShowDescriptionBox = false;
            this.BondedAreaCodeFindtBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.BondedAreaCodeFindtBox.TabIndex = 8;
            // 
            // ContainerPackModeDropEdit
            // 
            this.ContainerPackModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ContainerPackModeDropEdit, "JE_ContainerPackMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ContainerPackMode)));
            this.ContainerPackModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 80, true);
            this.ContainerPackModeDropEdit.Name = "ContainerPackModeDropEdit";
            this.ContainerPackModeDropEdit.PreBoundMaxLength = 3;
            this.ContainerPackModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.ContainerPackModeDropEdit.TabIndex = 6;
            // 
            // LocationOfGoodsTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocationOfGoodsTextBox, "JE_LocationOfGoods");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
            this.LocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 146, true);
            this.LocationOfGoodsTextBox.Name = "LocationOfGoodsTextBox";
            this.LocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 20, true);
            this.LocationOfGoodsTextBox.TabIndex = 11;
            // 
            // SEDDetailsGroupBox
            // 
            this.SEDDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("aa1a4129-2c75-4e5d-b283-0e5cd5cd0519", "SED Details");
            this.SEDDetailsGroupBox.Controls.Add(this.LocationIDInBondedAreaTextBox);
            this.SEDDetailsGroupBox.Controls.Add(this.CustomsDivisionCodeFindBox);
            this.SEDDetailsGroupBox.Controls.Add(this.LocationOfGoodsTextBox);
            this.SEDDetailsGroupBox.Controls.Add(this.InspectionDateEdit);
            this.SEDDetailsGroupBox.Controls.Add(this.ContainerPackModeDropEdit);
            this.SEDDetailsGroupBox.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.SEDDetailsGroupBox.Controls.Add(this.BondedAreaCodeFindtBox);
            this.SEDDetailsGroupBox.Controls.Add(this.LocationQualifierTextBox);
            this.SEDDetailsGroupBox.Controls.Add(this.GoodsDestinationCodeFindBox);
            this.SEDDetailsGroupBox.Controls.Add(this.SubLocationOfGoodsTextBox);
            this.SEDDetailsGroupBox.Controls.Add(this.SimpleDRWAppDropEdit);
            this.SEDDetailsGroupBox.Controls.Add(this.GoodsConditionDropEdit);
            this.SEDDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SEDDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SEDDetailsGroupBox.Name = "SEDDetailsGroupBox";
            this.SEDDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 198, true);
            this.SEDDetailsGroupBox.TabIndex = 1;
            this.SEDDetailsGroupBox.TabStop = false;
            // 
            // LocationIDInBondedAreaTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocationIDInBondedAreaTextBox, "JE_LocationIDInBondedArea");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationIDInBondedArea)));
            this.LocationIDInBondedAreaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 104, true);
            this.LocationIDInBondedAreaTextBox.Name = "LocationIDInBondedAreaTextBox";
            this.LocationIDInBondedAreaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
            this.LocationIDInBondedAreaTextBox.TabIndex = 9;
            // 
            // ExportSEDDetailUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SEDDetailsGroupBox);
            this.Name = "ExportSEDDetailUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 198, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.CustomsDivisionCodeFindBox.ResumeLayout(true);
            this.CustomsDivisionCodeFindBox.PerformLayout();
            this.GoodsDestinationCodeFindBox.ResumeLayout(true);
            this.GoodsDestinationCodeFindBox.PerformLayout();
            this.InspectionDateEdit.ResumeLayout(true);
            this.InspectionDateEdit.PerformLayout();
            this.SimpleDRWAppDropEdit.ResumeLayout(true);
            this.SimpleDRWAppDropEdit.PerformLayout();
            this.GoodsConditionDropEdit.ResumeLayout(true);
            this.GoodsConditionDropEdit.PerformLayout();
            this.BondedAreaCodeFindtBox.ResumeLayout(true);
            this.BondedAreaCodeFindtBox.PerformLayout();
            this.ContainerPackModeDropEdit.ResumeLayout(true);
            this.ContainerPackModeDropEdit.PerformLayout();
            this.SEDDetailsGroupBox.ResumeLayout(false);
            this.SEDDetailsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CustomsDivisionCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox GoodsDestinationCodeFindBox;
		public ZArchitecture.GUI.ZDateEdit InspectionDateEdit;
		private ZArchitecture.GUI.ZDropEdit SimpleDRWAppDropEdit;
		private ZArchitecture.GUI.ZDropEdit GoodsConditionDropEdit;
		private ZArchitecture.ZTextBox LocationQualifierTextBox;
		private ZArchitecture.GUI.ZCodeFindBox BondedAreaCodeFindtBox;
		private ZArchitecture.GUI.ZDropEdit ContainerPackModeDropEdit;
		private ZArchitecture.ZTextBox SubLocationOfGoodsTextBox;
		private ZArchitecture.ZTextBox LocationOfGoodsTextBox;
		private ZArchitecture.GUI.ZGroupBox SEDDetailsGroupBox;
		private ZArchitecture.ZTextBox LocationIDInBondedAreaTextBox;
	}
}
