namespace Enterprise.Customs.ES.TemporaryStorage.GUI
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
			this.ContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ArrivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LoadingPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BorderTransportMeansDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportRegistrationNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.ArrivalDateDateEdit.SuspendLayout();
			this.LoadingPlaceCodeFindBox.SuspendLayout();
			this.DepartureDateDateEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.ContainerCountCalcEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.ArrivalDateDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.LoadingPlaceCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.BorderTransportMeansDescriptionTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.DepartureDateDateEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.TransportModeDropEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.VesselCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.TransportRegistrationNumTextBox);
			this.TransportDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportDetailsGroupBox.Name = "TransportDetailsGroupBox";
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 198, true);
			this.TransportDetailsGroupBox.TabIndex = 100;
			this.TransportDetailsGroupBox.TabStop = false;
			this.TransportDetailsGroupBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("147D1D17-7F1B-4094-9497-F3D740C31532", "Transport Details");
			// 
			// ContainerCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainerCountCalcEdit, "SJH_ContainerCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_ContainerCount)));
			this.ContainerCountCalcEdit.CaptionResourceString = null;
			this.ContainerCountCalcEdit.DecimalPlaces = 0;
			this.ContainerCountCalcEdit.Decimals = 0;
			this.ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 169, true);
			this.ContainerCountCalcEdit.MaxValue = new decimal(new int[] {
			9999,
			0,
			0,
			0});
			this.ContainerCountCalcEdit.Name = "ContainerCountCalcEdit";
			this.ContainerCountCalcEdit.ShowGroupSeparators = false;
			this.ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.ContainerCountCalcEdit.TabIndex = 7;
			this.ContainerCountCalcEdit.Text = "0";
			this.ContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ArrivalDateDateEdit
			// 
			this.ArrivalDateDateEdit.AllowDrop = true;
			this.ArrivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalDateDateEdit, "SJH_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_ArrivalDate)));
			this.ArrivalDateDateEdit.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("97AF09C7-8FF8-4034-AC98-36CFC4B359A5", "Arrival Date");
			this.ArrivalDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.ArrivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 144, true);
			this.ArrivalDateDateEdit.Name = "ArrivalDateDateEdit";
			this.ArrivalDateDateEdit.TabIndex = 6;
			// 
			// LoadingPlaceCodeFindBox
			// 
			this.LoadingPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoadingPlaceCodeFindBox, "SJH_RL_NKLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_RL_NKLoading)));
			this.LoadingPlaceCodeFindBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("C1561133-0E4A-4899-9BC3-076FE47EFA6B", "Loading Place");
			this.LoadingPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 94, true);
			this.LoadingPlaceCodeFindBox.Name = "LoadingPlaceCodeFindBox";
			this.LoadingPlaceCodeFindBox.ShouldResize = true;
			this.LoadingPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.LoadingPlaceCodeFindBox.TabIndex = 4;
			// 
			// BorderTransportMeansDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.BorderTransportMeansDescriptionTextBox, "SJH_TransportMeansDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportMeansDescription)));
			this.BorderTransportMeansDescriptionTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("A0FF997F-2D5F-4C60-903B-3DA0FB06CDE2", "Border Transport Info.");
			this.BorderTransportMeansDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 44, true);
			this.BorderTransportMeansDescriptionTextBox.Name = "BorderTransportMeansDescriptionTextBox";
			this.BorderTransportMeansDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.BorderTransportMeansDescriptionTextBox.TabIndex = 1;
			// 
			// DepartureDateDateEdit
			// 
			this.DepartureDateDateEdit.AllowDrop = true;
			this.DepartureDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateDateEdit, "SJH_DepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_DepartureDate)));
			this.DepartureDateDateEdit.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("AA90D1D1-BE58-4790-AD53-481E47ED727A", "Departure Date");
			this.DepartureDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.DepartureDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 119, true);
			this.DepartureDateDateEdit.Name = "DepartureDateDateEdit";
			this.DepartureDateDateEdit.TabIndex = 5;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "SJH_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 19, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.TransportModeDropEdit.TabIndex = 0;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "SJH_TransportRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).Lookups.RefVesselList)));
			this.VesselCodeFindBox.BindToList = "Lookups.RefVesselList";
			this.VesselCodeFindBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("3BE5A13F-0B85-4399-969F-9A57F011D5D4", "Vessel");
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 69, true);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportRegNo)));
			this.TransportRegistrationNumTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("57724083-EC85-4A74-94F7-E222FBFC50B1", "Transport Reg. No.");
			this.TransportRegistrationNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 69, true);
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
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 198, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.ArrivalDateDateEdit.ResumeLayout(true);
			this.ArrivalDateDateEdit.PerformLayout();
			this.LoadingPlaceCodeFindBox.ResumeLayout(true);
			this.LoadingPlaceCodeFindBox.PerformLayout();
			this.DepartureDateDateEdit.ResumeLayout(true);
			this.DepartureDateDateEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		protected ZArchitecture.ZTextBox TransportRegistrationNumTextBox;
		protected ZArchitecture.GUI.ZDateEdit DepartureDateDateEdit;
		private ZArchitecture.ZCalcEdit ContainerCountCalcEdit;
		private ZArchitecture.GUI.ZDateEdit ArrivalDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox LoadingPlaceCodeFindBox;
		private ZArchitecture.ZTextBox BorderTransportMeansDescriptionTextBox;
		private ZArchitecture.GUI.ZGroupBox TransportDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth TransportModeDropEdit;
	}
}
