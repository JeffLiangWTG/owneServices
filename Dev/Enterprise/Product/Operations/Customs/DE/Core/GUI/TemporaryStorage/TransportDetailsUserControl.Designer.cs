namespace Enterprise.Customs.DE.GUI
{
	partial class TransportDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TransportDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DepartureDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransportMeansDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportRegistrationNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.DepartureDateDateEdit.SuspendLayout();
			this.TransportMeansDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2a57cc3d-8e1b-4d30-a8ce-54ce379bd428", "Transport Details");
			this.TransportDetailsGroupBox.Controls.Add(this.DepartureDateDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.TransportMeansDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.TransportModeDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.VesselCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.TransportRegistrationNumTextBox);
			this.TransportDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDetailsGroupBox.Name = "TransportDetailsGroupBox";
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 122, true);
			this.TransportDetailsGroupBox.TabIndex = 100;
			this.TransportDetailsGroupBox.TabStop = false;
			// 
			// DepartureDateDateEdit
			// 
			this.DepartureDateDateEdit.AllowDrop = true;
			this.DepartureDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateDateEdit, "SJH_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_DepartureDate)));
			this.DepartureDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.DepartureDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 94, true);
			this.DepartureDateDateEdit.Name = "DepartureDateDateEdit";
			this.DepartureDateDateEdit.TabIndex = 4;
			// 
			// TransportMeansDropEdit
			// 
			this.TransportMeansDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportMeansDropEdit, "SJH_TransportMeansCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportMeansCode)));
			this.TransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 19, true);
			this.TransportMeansDropEdit.Name = "TransportMeansDropEdit";
			this.TransportMeansDropEdit.ShouldResizeByMaxLength = true;
			this.TransportMeansDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.TransportMeansDropEdit.TabIndex = 0;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "SJH_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 44, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.TransportModeDropEdit.TabIndex = 1;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "SJH_TransportRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).Lookups.RefVesselList)));
			this.VesselCodeFindBox.BindToList = "Lookups.RefVesselList";
			this.VesselCodeFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("96be447c-c02d-4c42-9adb-c91ad8476208", "Vessel");
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 69, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ShouldResize = true;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.VesselCodeFindBox.TabIndex = 2;
			this.VesselCodeFindBox.Visible = false;
			// 
			// TransportRegistrationNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportRegistrationNumTextBox, "SJH_TransportRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportRegNo)));
			this.TransportRegistrationNumTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("e448a7a9-924c-4c39-aafb-0b177d4a8c54", "Transport Reg. No.");
			this.TransportRegistrationNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 69, true);
			this.TransportRegistrationNumTextBox.Name = "TransportRegistrationNumTextBox";
			this.TransportRegistrationNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.TransportRegistrationNumTextBox.TabIndex = 3;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportDetailsGroupBox);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 122, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.DepartureDateDateEdit.ResumeLayout(true);
			this.DepartureDateDateEdit.PerformLayout();
			this.TransportMeansDropEdit.ResumeLayout(true);
			this.TransportMeansDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox TransportDetailsGroupBox;
		protected ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		protected ZArchitecture.ZTextBox TransportRegistrationNumTextBox;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth TransportMeansDropEdit;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth TransportModeDropEdit;
		protected ZArchitecture.GUI.ZDateEdit DepartureDateDateEdit;
	}
}
