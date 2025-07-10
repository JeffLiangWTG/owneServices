namespace Enterprise.Customs.KR.GUI
{
	partial class ShipmentDetailsUserControl
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
            this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.OriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.EstimatedDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.FinalDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.EstimatedArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ShipmentDetailsScreeningUserControl = new Enterprise.Customs.GUI.ShipmentDetailsScreeningUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.WeightCalcDropEdit.SuspendLayout();
            this.VolumeCalcDropEdit.SuspendLayout();
            this.OriginCodeFindBox.SuspendLayout();
            this.EstimatedDepartureDateEdit.SuspendLayout();
            this.FinalDestinationCodeFindBox.SuspendLayout();
            this.EstimatedArrivalDateEdit.SuspendLayout();
            this.ShipmentDetailsScreeningUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // WeightCalcDropEdit
            // 
            this.WeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalWeightUnit)));
            this.WeightCalcDropEdit.BindToAmount = "JE_TotalWeight";
            this.WeightCalcDropEdit.BindToUnit = "JE_TotalWeightUnit";
            this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 100, true);
            this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
            this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.WeightCalcDropEdit.TabIndex = 9;
            this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // VolumeCalcDropEdit
            // 
            this.VolumeCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalVolume)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalVolumeUnit)));
            this.VolumeCalcDropEdit.BindToAmount = "JE_TotalVolume";
            this.VolumeCalcDropEdit.BindToUnit = "JE_TotalVolumeUnit";
            this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 100, true);
            this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
            this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.VolumeCalcDropEdit.TabIndex = 10;
            this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
            // 
            // OriginCodeFindBox
            // 
            this.OriginCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "JE_RL_NKOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RL_NKOrigin)));
            this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 28, true);
            this.OriginCodeFindBox.Name = "OriginCodeFindBox";
            this.OriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OriginCodeFindBox.ParentType = null;
            this.OriginCodeFindBox.PreBoundMaxLength = 5;
            this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
            this.OriginCodeFindBox.TabIndex = 11;
            // 
            // EstimatedDepartureDateEdit
            // 
            this.EstimatedDepartureDateEdit.AllowDrop = true;
            this.EstimatedDepartureDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EstimatedDepartureDateEdit, "JE_DateAtOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_DateAtOrigin)));
            this.EstimatedDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 28, true);
            this.EstimatedDepartureDateEdit.Name = "EstimatedDepartureDateEdit";
            this.EstimatedDepartureDateEdit.TabIndex = 12;
            // 
            // FinalDestinationCodeFindBox
            // 
            this.FinalDestinationCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FinalDestinationCodeFindBox, "JE_RL_NKFinalDestination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RL_NKFinalDestination)));
            this.FinalDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 63, true);
            this.FinalDestinationCodeFindBox.Name = "FinalDestinationCodeFindBox";
            this.FinalDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.FinalDestinationCodeFindBox.ParentType = null;
            this.FinalDestinationCodeFindBox.PreBoundMaxLength = 5;
            this.FinalDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
            this.FinalDestinationCodeFindBox.TabIndex = 13;
            // 
            // EstimatedArrivalDateEdit
            // 
            this.EstimatedArrivalDateEdit.AllowDrop = true;
            this.EstimatedArrivalDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EstimatedArrivalDateEdit, "JE_DateAtFinalDestination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_DateAtFinalDestination)));
            this.EstimatedArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 63, true);
            this.EstimatedArrivalDateEdit.Name = "EstimatedArrivalDateEdit";
            this.EstimatedArrivalDateEdit.TabIndex = 14;
            // 
            // ShipmentDetailsScreeningUserControl
            // 
            this.ShipmentDetailsScreeningUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ShipmentDetailsScreeningUserControl, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobDeclaration)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)))));
            this.ShipmentDetailsScreeningUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 135, true);
            this.ShipmentDetailsScreeningUserControl.Name = "ShipmentDetailsScreeningUserControl";
            this.ShipmentDetailsScreeningUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 21, true);
            this.ShipmentDetailsScreeningUserControl.TabIndex = 15;
			this.ShipmentDetailsScreeningUserControl.TabStop = false;
            // 
            // ShipmentDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ShipmentDetailsScreeningUserControl);
            this.Controls.Add(this.EstimatedArrivalDateEdit);
            this.Controls.Add(this.FinalDestinationCodeFindBox);
            this.Controls.Add(this.EstimatedDepartureDateEdit);
            this.Controls.Add(this.OriginCodeFindBox);
            this.Controls.Add(this.VolumeCalcDropEdit);
            this.Controls.Add(this.WeightCalcDropEdit);
            this.Name = "ShipmentDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 183, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.WeightCalcDropEdit.ResumeLayout(true);
            this.WeightCalcDropEdit.PerformLayout();
            this.VolumeCalcDropEdit.ResumeLayout(true);
            this.VolumeCalcDropEdit.PerformLayout();
            this.OriginCodeFindBox.ResumeLayout(true);
            this.OriginCodeFindBox.PerformLayout();
            this.EstimatedDepartureDateEdit.ResumeLayout(true);
            this.EstimatedDepartureDateEdit.PerformLayout();
            this.FinalDestinationCodeFindBox.ResumeLayout(true);
            this.FinalDestinationCodeFindBox.PerformLayout();
            this.EstimatedArrivalDateEdit.ResumeLayout(true);
            this.EstimatedArrivalDateEdit.PerformLayout();
            this.ShipmentDetailsScreeningUserControl.ResumeLayout(true);
            this.ShipmentDetailsScreeningUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		internal ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox OriginCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit EstimatedDepartureDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox FinalDestinationCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit EstimatedArrivalDateEdit;
		internal Customs.GUI.ShipmentDetailsScreeningUserControl ShipmentDetailsScreeningUserControl;
	}
}
