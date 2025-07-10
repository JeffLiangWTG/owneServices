
namespace Enterprise.Customs.KR.GUI
{
	partial class DeclarationCustomsDetailsUserControl
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
            this.DepartmentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DepartureCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ContainerPackDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.BondedAreaCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.LocationIDInBondedAreaTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.UnderbondMovementArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.SouthNorthTradeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.GoldTradeTransactionYNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsBrokerCommentUserControl = new Enterprise.Customs.KR.GUI.CustomsBrokerCommentUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.DepartmentCodeFindBox.SuspendLayout();
            this.DepartureCountryCodeFindBox.SuspendLayout();
            this.ContainerPackDropEdit.SuspendLayout();
            this.BondedAreaCodeFindBox.SuspendLayout();
            this.UnderbondMovementArrivalDateEdit.SuspendLayout();
            this.SouthNorthTradeTypeDropEdit.SuspendLayout();
            this.GoldTradeTransactionYNDropEdit.SuspendLayout();
            this.CustomsBrokerCommentUserControl.SuspendLayout();
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
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 17, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 0;
            // 
            // DepartmentCodeFindBox
            // 
            this.DepartmentCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepartmentCodeFindBox, "JE_CustomsDivision");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsDivision)));
            this.DepartmentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 17, true);
            this.DepartmentCodeFindBox.Name = "DepartmentCodeFindBox";
            this.DepartmentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DepartmentCodeFindBox.ParentType = null;
            this.DepartmentCodeFindBox.PreBoundMaxLength = 2;
            this.DepartmentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
            this.DepartmentCodeFindBox.TabIndex = 1;
            // 
            // DepartureCountryCodeFindBox
            // 
            this.DepartureCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepartureCountryCodeFindBox, "JE_CustomsLoadPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CustomsLoadPort)));
            this.DepartureCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 38, true);
            this.DepartureCountryCodeFindBox.Name = "DepartureCountryCodeFindBox";
            this.DepartureCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DepartureCountryCodeFindBox.ParentType = null;
            this.DepartureCountryCodeFindBox.PreBoundMaxLength = 2;
            this.DepartureCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
            this.DepartureCountryCodeFindBox.TabIndex = 2;
            // 
            // ContainerPackDropEdit
            // 
            this.ContainerPackDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ContainerPackDropEdit, "JE_ContainerPackMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ContainerPackMode)));
            this.ContainerPackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 38, true);
            this.ContainerPackDropEdit.Name = "ContainerPackDropEdit";
            this.ContainerPackDropEdit.PreBoundMaxLength = 2;
            this.ContainerPackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
            this.ContainerPackDropEdit.TabIndex = 3;
            // 
            // BondedAreaCodeFindBox
            // 
            this.BondedAreaCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BondedAreaCodeFindBox, "JE_LocationOtherInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationOtherInformation)));
            this.BondedAreaCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 59, true);
            this.BondedAreaCodeFindBox.Name = "BondedAreaCodeFindBox";
            this.BondedAreaCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BondedAreaCodeFindBox.ParentType = null;
            this.BondedAreaCodeFindBox.PreBoundMaxLength = 8;
            this.BondedAreaCodeFindBox.ShowDescriptionBox = false;
            this.BondedAreaCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
            this.BondedAreaCodeFindBox.TabIndex = 4;
            // 
            // LocationIDInBondedAreaTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocationIDInBondedAreaTextBox, "JE_LocationIDInBondedArea");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_LocationIDInBondedArea)));
            this.LocationIDInBondedAreaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 59, true);
            this.LocationIDInBondedAreaTextBox.Name = "LocationIDInBondedAreaTextBox";
            this.LocationIDInBondedAreaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
            this.LocationIDInBondedAreaTextBox.TabIndex = 5;
            // 
            // UnderbondMovementArrivalDateEdit
            // 
            this.UnderbondMovementArrivalDateEdit.AllowDrop = true;
            this.UnderbondMovementArrivalDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.UnderbondMovementArrivalDateEdit, "UnderbondMovementArrivalDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).UnderbondMovementArrivalDate)));
            this.UnderbondMovementArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 106, true);
            this.UnderbondMovementArrivalDateEdit.Name = "UnderbondMovementArrivalDateEdit";
            this.UnderbondMovementArrivalDateEdit.TabIndex = 6;
            // 
            // SouthNorthTradeTypeDropEdit
            // 
            this.SouthNorthTradeTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SouthNorthTradeTypeDropEdit, "JE_TradeIndicatorWithKP");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TradeIndicatorWithKP)));
            this.SouthNorthTradeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 107, true);
            this.SouthNorthTradeTypeDropEdit.Name = "SouthNorthTradeTypeDropEdit";
            this.SouthNorthTradeTypeDropEdit.PreBoundMaxLength = 1;
            this.SouthNorthTradeTypeDropEdit.ShowDescriptionBox = false;
            this.SouthNorthTradeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.SouthNorthTradeTypeDropEdit.TabIndex = 7;
            // 
            // GoldTradeTransactionYNDropEdit
            // 
            this.GoldTradeTransactionYNDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoldTradeTransactionYNDropEdit, "JE_GoldTrade");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GoldTrade)));
            this.GoldTradeTransactionYNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 107, true);
            this.GoldTradeTransactionYNDropEdit.Name = "GoldTradeTransactionYNDropEdit";
            this.GoldTradeTransactionYNDropEdit.PreBoundMaxLength = 1;
            this.GoldTradeTransactionYNDropEdit.ShowDescriptionBox = false;
            this.GoldTradeTransactionYNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
            this.GoldTradeTransactionYNDropEdit.TabIndex = 8;
            // 
            // CustomsBrokerCommentUserControl
            // 
            this.CustomsBrokerCommentUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsBrokerCommentUserControl, ".");
            this.CustomsBrokerCommentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 80, true);
            this.CustomsBrokerCommentUserControl.Name = "CustomsBrokerCommentUserControl";
            this.CustomsBrokerCommentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 22, true);
            this.CustomsBrokerCommentUserControl.TabIndex = 9;
            // 
            // DeclarationCustomsDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.CustomsBrokerCommentUserControl);
            this.Controls.Add(this.GoldTradeTransactionYNDropEdit);
            this.Controls.Add(this.SouthNorthTradeTypeDropEdit);
            this.Controls.Add(this.UnderbondMovementArrivalDateEdit);
            this.Controls.Add(this.LocationIDInBondedAreaTextBox);
            this.Controls.Add(this.BondedAreaCodeFindBox);
            this.Controls.Add(this.ContainerPackDropEdit);
            this.Controls.Add(this.DepartureCountryCodeFindBox);
            this.Controls.Add(this.DepartmentCodeFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Name = "DeclarationCustomsDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 137, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.DepartmentCodeFindBox.ResumeLayout(true);
            this.DepartmentCodeFindBox.PerformLayout();
            this.DepartureCountryCodeFindBox.ResumeLayout(true);
            this.DepartureCountryCodeFindBox.PerformLayout();
            this.ContainerPackDropEdit.ResumeLayout(true);
            this.ContainerPackDropEdit.PerformLayout();
            this.BondedAreaCodeFindBox.ResumeLayout(true);
            this.BondedAreaCodeFindBox.PerformLayout();
            this.UnderbondMovementArrivalDateEdit.ResumeLayout(true);
            this.UnderbondMovementArrivalDateEdit.PerformLayout();
            this.SouthNorthTradeTypeDropEdit.ResumeLayout(true);
            this.SouthNorthTradeTypeDropEdit.PerformLayout();
            this.GoldTradeTransactionYNDropEdit.ResumeLayout(true);
            this.GoldTradeTransactionYNDropEdit.PerformLayout();
            this.CustomsBrokerCommentUserControl.ResumeLayout(true);
            this.CustomsBrokerCommentUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		public ZArchitecture.GUI.ZCodeFindBox DepartmentCodeFindBox;
		public ZArchitecture.GUI.ZCodeFindBox DepartureCountryCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit ContainerPackDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox BondedAreaCodeFindBox;
		public ZArchitecture.ZTextBox LocationIDInBondedAreaTextBox;
		public ZArchitecture.GUI.ZDateEdit UnderbondMovementArrivalDateEdit;
		public ZArchitecture.GUI.ZDropEdit SouthNorthTradeTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit GoldTradeTransactionYNDropEdit;
		public CustomsBrokerCommentUserControl CustomsBrokerCommentUserControl;
	}
}
