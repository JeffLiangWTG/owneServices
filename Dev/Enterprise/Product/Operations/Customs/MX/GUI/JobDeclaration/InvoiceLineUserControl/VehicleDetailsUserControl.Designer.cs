namespace Enterprise.Customs.MX.GUI
{
	partial class VehicleDetailsUserControl
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
			this.VehicleVINTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleMileageCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.VehicleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VehicleVINTextBox.SuspendLayout();
			this.VehicleMileageCalcDropEdit.SuspendLayout();
			this.VehicleDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.JobComInvoiceLine);
			// 
			// VehicleVINTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleVINTextBox, "VehicleVIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).VehicleVIN)));
			this.VehicleVINTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 19, true);
			this.VehicleVINTextBox.Name = "VehicleVINTextBox";
			this.VehicleVINTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.VehicleVINTextBox.TabIndex = 0;
			// 
			// VehicleMileageCalcDropEdit
			// 
			this.VehicleMileageCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleMileageCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).VehicleMileage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).VehicleMileageUQ)));
			this.VehicleMileageCalcDropEdit.BindToAmount = "VehicleMileage";
			this.VehicleMileageCalcDropEdit.BindToUnit = "VehicleMileageUQ";
			this.VehicleMileageCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 45, true);
			this.VehicleMileageCalcDropEdit.MaxValue = new decimal(new int[] {
            999999,
            0,
            0,
            0});
			this.VehicleMileageCalcDropEdit.Name = "VehicleMileageCalcDropEdit";
			this.VehicleMileageCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.VehicleMileageCalcDropEdit.TabIndex = 1;
			this.VehicleMileageCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// VehicleDetailsGroupBox
			// 
			this.VehicleDetailsGroupBox.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("A02D1511-C875-415C-8D79-5C41A84514E2", "Vehicle Details");
			this.VehicleDetailsGroupBox.Controls.Add(this.VehicleVINTextBox);
			this.VehicleDetailsGroupBox.Controls.Add(this.VehicleMileageCalcDropEdit);
			this.VehicleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 1, true);
			this.VehicleDetailsGroupBox.Name = "VehicleDetailsGroupBox";
			this.VehicleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 74, true);
			this.VehicleDetailsGroupBox.TabIndex = 12;
			this.VehicleDetailsGroupBox.TabStop = false;
			// 
			// VehicleDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VehicleDetailsGroupBox);
			this.Name = "VehicleDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 77, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VehicleVINTextBox.ResumeLayout(true);
			this.VehicleVINTextBox.PerformLayout();
			this.VehicleMileageCalcDropEdit.ResumeLayout(true);
			this.VehicleMileageCalcDropEdit.PerformLayout();
			this.VehicleDetailsGroupBox.ResumeLayout(false);
			this.VehicleDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox VehicleVINTextBox;
		internal ZArchitecture.GUI.ZCalcDropEdit VehicleMileageCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox VehicleDetailsGroupBox;
	}
}
