namespace Enterprise.Customs.BE.GUI
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
			this.FlightAndNationalityUserControl = new FlightAndNationalityUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FlightAndNationalityUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			//
			// FlightAndNationalityUserControl
			// 
			this.FlightAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FlightAndNationalityUserControl, ".");
			this.FlightAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 523, true);
			this.FlightAndNationalityUserControl.Name = "FlightAndNationalityUserControl";
			this.FlightAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 43, true);
			this.FlightAndNationalityUserControl.TabIndex = 25;
			//
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FlightAndNationalityUserControl);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 906, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FlightAndNationalityUserControl.ResumeLayout(true);
			this.FlightAndNationalityUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal FlightAndNationalityUserControl FlightAndNationalityUserControl;
	}
}
