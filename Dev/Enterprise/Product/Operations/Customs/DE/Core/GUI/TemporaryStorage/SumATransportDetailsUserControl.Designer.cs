namespace Enterprise.Customs.DE.GUI
{
	partial class SumATransportDetailsUserControl
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
			this.LoadingPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ArrivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BorderTransportMeansDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.TransportMeansDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.DepartureDateDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoadingPlaceCodeFindBox.SuspendLayout();
			this.ArrivalDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.LoadingPlaceCodeFindBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ContainerCountCalcEdit);
			this.TransportDetailsGroupBox.Controls.Add(this.BorderTransportMeansDescriptionTextBox);
			this.TransportDetailsGroupBox.Controls.Add(this.ArrivalDateDateEdit);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 222, true);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DepartureDateDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportModeDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportMeansDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ArrivalDateDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.BorderTransportMeansDescriptionTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.ContainerCountCalcEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportRegistrationNumTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.LoadingPlaceCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselCodeFindBox, 0);
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 92, true);
			this.VesselCodeFindBox.TabIndex = 4;
			// 
			// TransportRegistrationNumTextBox
			// 
			this.TransportRegistrationNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 92, true);
			// 
			// TransportMeansDropEdit
			// 
			this.TransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 17, true);
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 42, true);
			// 
			// DepartureDateDateEdit
			// 
			this.DepartureDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 142, true);
			this.DepartureDateDateEdit.TabIndex = 7;
			// 
			// LoadingPlaceCodeFindBox
			// 
			this.LoadingPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoadingPlaceCodeFindBox, "SJH_RL_NKLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_RL_NKLoading)));
			this.LoadingPlaceCodeFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("a3e36ecb-2c69-48ee-8e5a-f7a3337128ba", "Loading Place");
			this.LoadingPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 117, true);
			this.LoadingPlaceCodeFindBox.Name = "LoadingPlaceCodeFindBox";
			this.LoadingPlaceCodeFindBox.ShouldResize = true;
			this.LoadingPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.LoadingPlaceCodeFindBox.TabIndex = 6;
			// 
			// ContainerCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ContainerCountCalcEdit, "SJH_ContainerCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_ContainerCount)));
			this.ContainerCountCalcEdit.CaptionResourceString = null;
			this.ContainerCountCalcEdit.DecimalPlaces = 0;
			this.ContainerCountCalcEdit.Decimals = 0;
			this.ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 192, true);
			this.ContainerCountCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.ContainerCountCalcEdit.Name = "ContainerCountCalcEdit";
			this.ContainerCountCalcEdit.ShowGroupSeparators = false;
			this.ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.ContainerCountCalcEdit.TabIndex = 9;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_ArrivalDate)));
			this.ArrivalDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			this.ArrivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 167, true);
			this.ArrivalDateDateEdit.Name = "ArrivalDateDateEdit";
			this.ArrivalDateDateEdit.TabIndex = 8;
			// 
			// BorderTransportMeansDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.BorderTransportMeansDescriptionTextBox, "SJH_TransportMeansDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_TransportMeansDescription)));
			this.BorderTransportMeansDescriptionTextBox.CaptionResourceString = null;
			this.BorderTransportMeansDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 67, true);
			this.BorderTransportMeansDescriptionTextBox.Name = "BorderTransportMeansDescriptionTextBox";
			this.BorderTransportMeansDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.BorderTransportMeansDescriptionTextBox.TabIndex = 2;
			// 
			// SumATransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "SumATransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 222, true);
			this.TransportDetailsGroupBox.ResumeLayout(false);
			this.TransportDetailsGroupBox.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.TransportMeansDropEdit.ResumeLayout(true);
			this.TransportMeansDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.DepartureDateDateEdit.ResumeLayout(true);
			this.DepartureDateDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoadingPlaceCodeFindBox.ResumeLayout(true);
			this.LoadingPlaceCodeFindBox.PerformLayout();
			this.ArrivalDateDateEdit.ResumeLayout(true);
			this.ArrivalDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZCodeFindBox LoadingPlaceCodeFindBox;
		private ZArchitecture.ZCalcEdit ContainerCountCalcEdit;
		private ZArchitecture.GUI.ZDateEdit ArrivalDateDateEdit;
		private ZArchitecture.ZTextBox BorderTransportMeansDescriptionTextBox;
	}
}
