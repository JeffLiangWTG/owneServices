namespace Enterprise.Customs.BR.GUI
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
			this.VesselAndCountryUserControl = new Enterprise.Customs.BR.GUI.VesselAndCountryUserControl();
			this.CargoArrivalDocUtilizationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PlateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VesselAndCountryUserControl.SuspendLayout();
			this.CargoArrivalDocUtilizationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// CargoArrivalDocUtilizationDropEdit
			// 
			this.CargoArrivalDocUtilizationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoArrivalDocUtilizationDropEdit, "JE_CargoArrivalDocumentUtilization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_CargoArrivalDocumentUtilization)));
			this.CargoArrivalDocUtilizationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 29, true);
			this.CargoArrivalDocUtilizationDropEdit.Name = "CargoArrivalDocUtilizationDropEdit";
			this.CargoArrivalDocUtilizationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 18, true);
			this.CargoArrivalDocUtilizationDropEdit.TabIndex = 0;
			// 
			// VesselAndCountryUserControl
			// 
			this.VesselAndCountryUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselAndCountryUserControl, ".");
			this.VesselAndCountryUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 54, true);
			this.VesselAndCountryUserControl.Name = "VesselAndCountryUserControl";
			this.VesselAndCountryUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.VesselAndCountryUserControl.TabIndex = 5;
			// 
			// PlateTextBox
			// 
			this.BindingSource.SetBindingMember(this.PlateTextBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_VesselName)));
			this.PlateTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("BD73CCFC-F93A-47C6-834B-4C04C99D38C6", "Plate", "The Vehicle Plate.");
			this.PlateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 80, true);
			this.PlateTextBox.Name = "PlateTextBox";
			this.PlateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.PlateTextBox.TabIndex = 6;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CargoArrivalDocUtilizationDropEdit);
			this.Controls.Add(this.VesselAndCountryUserControl);
			this.Controls.Add(this.PlateTextBox);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 126, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VesselAndCountryUserControl.ResumeLayout(true);
			this.VesselAndCountryUserControl.PerformLayout();
			this.CargoArrivalDocUtilizationDropEdit.ResumeLayout(true);
			this.CargoArrivalDocUtilizationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CargoArrivalDocUtilizationDropEdit;
		internal VesselAndCountryUserControl VesselAndCountryUserControl;
		internal ZArchitecture.ZTextBox PlateTextBox;
	}
}
