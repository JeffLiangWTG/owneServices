namespace Enterprise.Customs.KR.GUI
{
	partial class TransportDetailsUserControl
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
            this.HouseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ArrivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.PortofLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ForeignCityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.FreightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.StartDateDateEdit.SuspendLayout();
            this.ArrivalDateDateEdit.SuspendLayout();
            this.PortofLoadingCodeFindBox.SuspendLayout();
            this.ForeignCityCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // HouseBillTextBox
            // 
            this.BindingSource.SetBindingMember(this.HouseBillTextBox, "JE_HouseBill");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_HouseBill)));
            this.HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 7, true);
            this.HouseBillTextBox.Name = "HouseBillTextBox";
            this.HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
            this.HouseBillTextBox.TabIndex = 0;
            // 
            // StartDateDateEdit
            // 
            this.StartDateDateEdit.AllowDrop = true;
            this.StartDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.StartDateDateEdit, "JE_ExportDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ExportDate)));
            this.StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 59, true);
            this.StartDateDateEdit.Name = "StartDateDateEdit";
            this.StartDateDateEdit.TabIndex = 2;
            // 
            // ArrivalDateDateEdit
            // 
            this.ArrivalDateDateEdit.AllowDrop = true;
            this.ArrivalDateDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ArrivalDateDateEdit, "JE_DateOfArrival");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_DateOfArrival)));
            this.ArrivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 59, true);
            this.ArrivalDateDateEdit.Name = "ArrivalDateDateEdit";
            this.ArrivalDateDateEdit.TabIndex = 3;
            // 
            // PortofLoadingCodeFindBox
            // 
            this.PortofLoadingCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PortofLoadingCodeFindBox, "JE_RL_NKPortOfLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RL_NKPortOfLoading)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).Lookups.PortOfLoadings)));
            this.PortofLoadingCodeFindBox.BindToList = "Lookups.PortOfLoadings";
            this.PortofLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 85, true);
            this.PortofLoadingCodeFindBox.Name = "PortofLoadingCodeFindBox";
            this.PortofLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PortofLoadingCodeFindBox.ParentType = null;
            this.PortofLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
            this.PortofLoadingCodeFindBox.TabIndex = 4;
            // 
            // ForeignCityCodeFindBox
            // 
            this.ForeignCityCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ForeignCityCodeFindBox, "JE_RL_NKOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RL_NKOrigin)));
            this.ForeignCityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 111, true);
            this.ForeignCityCodeFindBox.Name = "ForeignCityCodeFindBox";
            this.ForeignCityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ForeignCityCodeFindBox.ParentType = null;
            this.ForeignCityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
            this.ForeignCityCodeFindBox.TabIndex = 5;
            // 
            // FreightCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.FreightCalcEdit, "PIDFreightAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).PIDFreightAmount)));
            this.FreightCalcEdit.DecimalPlaces = 2;
            this.FreightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 33, true);
            this.FreightCalcEdit.Name = "FreightCalcEdit";
            this.FreightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
            this.FreightCalcEdit.TabIndex = 6;
            this.FreightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FreightCalcEdit.TrackDisposedAccess = true;
            // 
            // TransportDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FreightCalcEdit);
            this.Controls.Add(this.ForeignCityCodeFindBox);
            this.Controls.Add(this.PortofLoadingCodeFindBox);
            this.Controls.Add(this.ArrivalDateDateEdit);
            this.Controls.Add(this.StartDateDateEdit);
            this.Controls.Add(this.HouseBillTextBox);
            this.Name = "TransportDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 145, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.StartDateDateEdit.ResumeLayout(true);
            this.StartDateDateEdit.PerformLayout();
            this.ArrivalDateDateEdit.ResumeLayout(true);
            this.ArrivalDateDateEdit.PerformLayout();
            this.PortofLoadingCodeFindBox.ResumeLayout(true);
            this.PortofLoadingCodeFindBox.PerformLayout();
            this.ForeignCityCodeFindBox.ResumeLayout(true);
            this.ForeignCityCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox HouseBillTextBox;
		private ZArchitecture.GUI.ZDateEdit StartDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit ArrivalDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox PortofLoadingCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ForeignCityCodeFindBox;
		private ZArchitecture.ZCalcEdit FreightCalcEdit;
	}
}
