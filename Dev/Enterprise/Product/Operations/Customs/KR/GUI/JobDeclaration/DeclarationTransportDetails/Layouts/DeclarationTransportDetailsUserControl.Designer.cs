namespace Enterprise.Customs.KR.GUI
{
	partial class DeclarationTransportDetailsUserControl
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
            this.VesselCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CarrierKRCCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.VoyageDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.RadioCallSignTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MRNNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MRNTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TransshipmentPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TransshipmentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.VoyageFlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.FolioNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.IATALoadPortDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ExportDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.VesselCountryCodeFindBox.SuspendLayout();
            this.CarrierKRCCodeFindBox.SuspendLayout();
            this.MRNTypeDropEdit.SuspendLayout();
            this.TransshipmentPortCodeFindBox.SuspendLayout();
            this.TransshipmentDateEdit.SuspendLayout();
            this.IATALoadPortDropEdit.SuspendLayout();
            this.PortOfLoadingCodeFindBox.SuspendLayout();
            this.ExportDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // VesselCountryCodeFindBox
            // 
            this.VesselCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VesselCountryCodeFindBox, "JE_RN_NKTransportNationality");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RN_NKTransportNationality)));
            this.VesselCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 26, true);
            this.VesselCountryCodeFindBox.Name = "VesselCountryCodeFindBox";
            this.VesselCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.VesselCountryCodeFindBox.ParentType = null;
            this.VesselCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
            this.VesselCountryCodeFindBox.TabIndex = 21;
            // 
            // CarrierKRCCodeFindBox
            // 
            this.CarrierKRCCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CarrierKRCCodeFindBox, "JE_CarrierCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_CarrierCode)));
            this.CarrierKRCCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 226, true);
            this.CarrierKRCCodeFindBox.Name = "CarrierKRCCodeFindBox";
            this.CarrierKRCCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CarrierKRCCodeFindBox.ParentType = null;
            this.CarrierKRCCodeFindBox.PreBoundMaxLength = 3;
            this.CarrierKRCCodeFindBox.ShowDescriptionBox = false;
            this.CarrierKRCCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
            this.CarrierKRCCodeFindBox.TabIndex = 23;
            // 
            // VoyageDurationCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.VoyageDurationCalcEdit, "JE_VoyageDuration");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_VoyageDuration)));
            this.VoyageDurationCalcEdit.DecimalPlaces = 2;
            this.VoyageDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 111, true);
            this.VoyageDurationCalcEdit.Name = "VoyageDurationCalcEdit";
            this.VoyageDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
            this.VoyageDurationCalcEdit.TabIndex = 32;
            this.VoyageDurationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.VoyageDurationCalcEdit.TrackDisposedAccess = true;
            // 
            // RadioCallSignTextBox
            // 
            this.BindingSource.SetBindingMember(this.RadioCallSignTextBox, "VesselRadioCallSign");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).VesselRadioCallSign)));
            this.RadioCallSignTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 111, true);
            this.RadioCallSignTextBox.Name = "RadioCallSignTextBox";
            this.RadioCallSignTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.RadioCallSignTextBox.TabIndex = 31;
            // 
            // MRNNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.MRNNumberTextBox, "MRNJ3_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).MRNJ3_ReferenceNumber)));
            this.MRNNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 137, true);
            this.MRNNumberTextBox.Name = "MRNNumberTextBox";
            this.MRNNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
            this.MRNNumberTextBox.TabIndex = 34;
            // 
            // MRNTypeDropEdit
            // 
            this.MRNTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MRNTypeDropEdit, "JE_MRNType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_MRNType)));
            this.MRNTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 137, true);
            this.MRNTypeDropEdit.Name = "MRNTypeDropEdit";
            this.MRNTypeDropEdit.PreBoundMaxLength = 1;
            this.MRNTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
            this.MRNTypeDropEdit.TabIndex = 33;
            // 
            // TransshipmentPortCodeFindBox
            // 
            this.TransshipmentPortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransshipmentPortCodeFindBox, "JE_TransshipmentPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TransshipmentPort)));
            this.TransshipmentPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 78, true);
            this.TransshipmentPortCodeFindBox.Name = "TransshipmentPortCodeFindBox";
            this.TransshipmentPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransshipmentPortCodeFindBox.ParentType = null;
            this.TransshipmentPortCodeFindBox.PreBoundMaxLength = 5;
            this.TransshipmentPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
            this.TransshipmentPortCodeFindBox.TabIndex = 37;
            // 
            // TransshipmentDateEdit
            // 
            this.TransshipmentDateEdit.AllowDrop = true;
            this.TransshipmentDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.TransshipmentDateEdit, "JE_TransshipmentDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TransshipmentDate)));
            this.TransshipmentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 78, true);
            this.TransshipmentDateEdit.Name = "TransshipmentDateEdit";
            this.TransshipmentDateEdit.TabIndex = 38;
            // 
            // VoyageFlightNumberTextBox
            // 
            this.VoyageFlightNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.VoyageFlightNumberTextBox, "JE_VoyageFlightNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_VoyageFlightNo)));
            this.VoyageFlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 52, true);
            this.VoyageFlightNumberTextBox.Name = "VoyageFlightNumberTextBox";
            this.VoyageFlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.VoyageFlightNumberTextBox.TabIndex = 42;
            // 
            // FolioNumberTextBox
            // 
            this.FolioNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.FolioNumberTextBox, "JE_Folio");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_Folio)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FolioNumberTextBox, false);
            this.FolioNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 52, true);
            this.FolioNumberTextBox.Name = "FolioNumberTextBox";
            this.FolioNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.FolioNumberTextBox.TabIndex = 43;
            // 
            // IATALoadPortDropEdit
            // 
            this.IATALoadPortDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.IATALoadPortDropEdit, "JE_IATALoadPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_IATALoadPort)));
            this.IATALoadPortDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 178, true);
            this.IATALoadPortDropEdit.Name = "IATALoadPortDropEdit";
            this.IATALoadPortDropEdit.PreBoundMaxLength = 3;
            this.IATALoadPortDropEdit.ShowDescriptionBox = false;
            this.IATALoadPortDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
            this.IATALoadPortDropEdit.TabIndex = 46;
            // 
            // PortOfLoadingCodeFindBox
            // 
            this.PortOfLoadingCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "JE_RL_NKPortOfLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_RL_NKPortOfLoading)));
            this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 178, true);
            this.PortOfLoadingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
            this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
            this.PortOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PortOfLoadingCodeFindBox.ParentType = null;
            this.PortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
            this.PortOfLoadingCodeFindBox.ShowDescriptionBox = false;
            this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
            this.PortOfLoadingCodeFindBox.TabIndex = 44;
            // 
            // ExportDateEdit
            // 
            this.ExportDateEdit.AllowDrop = true;
            this.ExportDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ExportDateEdit, "JE_ExportDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ExportDate)));
            this.ExportDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 178, true);
            this.ExportDateEdit.Name = "ExportDateEdit";
            this.ExportDateEdit.TabIndex = 45;
            // 
            // DeclarationTransportDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.IATALoadPortDropEdit);
            this.Controls.Add(this.PortOfLoadingCodeFindBox);
            this.Controls.Add(this.ExportDateEdit);
            this.Controls.Add(this.VoyageFlightNumberTextBox);
            this.Controls.Add(this.FolioNumberTextBox);
            this.Controls.Add(this.TransshipmentPortCodeFindBox);
            this.Controls.Add(this.TransshipmentDateEdit);
            this.Controls.Add(this.MRNNumberTextBox);
            this.Controls.Add(this.MRNTypeDropEdit);
            this.Controls.Add(this.VoyageDurationCalcEdit);
            this.Controls.Add(this.RadioCallSignTextBox);
            this.Controls.Add(this.CarrierKRCCodeFindBox);
            this.Controls.Add(this.VesselCountryCodeFindBox);
            this.Name = "DeclarationTransportDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 277, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.VesselCountryCodeFindBox.ResumeLayout(true);
            this.VesselCountryCodeFindBox.PerformLayout();
            this.CarrierKRCCodeFindBox.ResumeLayout(true);
            this.CarrierKRCCodeFindBox.PerformLayout();
            this.MRNTypeDropEdit.ResumeLayout(true);
            this.MRNTypeDropEdit.PerformLayout();
            this.TransshipmentPortCodeFindBox.ResumeLayout(true);
            this.TransshipmentPortCodeFindBox.PerformLayout();
            this.TransshipmentDateEdit.ResumeLayout(true);
            this.TransshipmentDateEdit.PerformLayout();
            this.IATALoadPortDropEdit.ResumeLayout(true);
            this.IATALoadPortDropEdit.PerformLayout();
            this.PortOfLoadingCodeFindBox.ResumeLayout(true);
            this.PortOfLoadingCodeFindBox.PerformLayout();
            this.ExportDateEdit.ResumeLayout(true);
            this.ExportDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCodeFindBox CarrierKRCCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox VesselCountryCodeFindBox;
		internal ZArchitecture.ZCalcEdit VoyageDurationCalcEdit;
		internal ZArchitecture.ZTextBox RadioCallSignTextBox;
		internal ZArchitecture.ZTextBox MRNNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit MRNTypeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox TransshipmentPortCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit TransshipmentDateEdit;
		internal ZArchitecture.ZTextBox VoyageFlightNumberTextBox;
		internal ZArchitecture.ZTextBox FolioNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit IATALoadPortDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit ExportDateEdit;
	}
}
