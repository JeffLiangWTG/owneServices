namespace Enterprise.Customs.DE.GUI
{
	partial class ReExportTransportDetailsUserControl
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
			this.UnloadingPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportDetailsGroupBox.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.TransportMeansDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.DepartureDateDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadingPlaceCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TransportDetailsGroupBox
			// 
			this.TransportDetailsGroupBox.Controls.Add(this.UnloadingPlaceCodeFindBox);
			this.TransportDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 145, true);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.DepartureDateDateEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportRegistrationNumTextBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.VesselCodeFindBox, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportModeDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.TransportMeansDropEdit, 0);
			this.TransportDetailsGroupBox.Controls.SetChildIndex(this.UnloadingPlaceCodeFindBox, 0);
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 67, true);
			// 
			// TransportRegistrationNumTextBox
			// 
			this.TransportRegistrationNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 67, true);
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
			this.DepartureDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 117, true);
			this.DepartureDateDateEdit.TabIndex = 5;
			// 
			// UnloadingPlaceCodeFindBox
			// 
			this.UnloadingPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingPlaceCodeFindBox, "SJH_RL_NKLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_RL_NKLoading)));
			this.UnloadingPlaceCodeFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("562769df-3f4f-435e-ba91-74b9739a5249", "Unloading Place");
			this.UnloadingPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 92, true);
			this.UnloadingPlaceCodeFindBox.Name = "UnloadingPlaceCodeFindBox";
			this.UnloadingPlaceCodeFindBox.ShouldResize = true;
			this.UnloadingPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.UnloadingPlaceCodeFindBox.TabIndex = 4;
			// 
			// REXTransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "REXTransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 145, true);
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
			this.UnloadingPlaceCodeFindBox.ResumeLayout(true);
			this.UnloadingPlaceCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox UnloadingPlaceCodeFindBox;
	}
}
