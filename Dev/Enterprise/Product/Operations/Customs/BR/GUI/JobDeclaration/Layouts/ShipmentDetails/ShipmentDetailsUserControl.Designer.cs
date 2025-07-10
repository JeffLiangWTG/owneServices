namespace Enterprise.Customs.BR.GUI
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShipmentDetailsUserControl));
			this.UcrAndBillTypeUserControl = new Enterprise.Customs.BR.GUI.UcrAndBillTypeUserControl();
			this.CargoArrivalUserControl = new Enterprise.Customs.BR.GUI.CargoArrivalUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UcrAndBillTypeUserControl.SuspendLayout();
			this.CargoArrivalUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// UcrAndBillTypeUserControl
			// 
			this.UcrAndBillTypeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UcrAndBillTypeUserControl, ".");
			this.UcrAndBillTypeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 4, true);
			this.UcrAndBillTypeUserControl.Name = "UcrAndBillTypeUserControl";
			this.UcrAndBillTypeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.UcrAndBillTypeUserControl.TabIndex = 1;
			// 
			// CargoArrivalUserControl
			// 
			this.CargoArrivalUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoArrivalUserControl, ".");
			this.CargoArrivalUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 33, true);
			this.CargoArrivalUserControl.Name = "CargoArrivalUserControl";
			this.CargoArrivalUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.CargoArrivalUserControl.TabIndex = 2;
			// 
			// ShipmentDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CargoArrivalUserControl);
			this.Controls.Add(this.UcrAndBillTypeUserControl);
			this.Name = "ShipmentDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 149, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UcrAndBillTypeUserControl.ResumeLayout(true);
			this.UcrAndBillTypeUserControl.PerformLayout();
			this.CargoArrivalUserControl.ResumeLayout(true);
			this.CargoArrivalUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal UcrAndBillTypeUserControl UcrAndBillTypeUserControl;
		internal CargoArrivalUserControl CargoArrivalUserControl;

	}
}
