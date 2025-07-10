namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class LicenceSettingDetailControl
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
		void InitializeComponent()
		{
			this.detailGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting);
			// 
			// detailGroupBox
			// 
			this.detailGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailGroupBox.Name = "detailGroupBox";
			this.detailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 156, true);
			this.detailGroupBox.TabIndex = 0;
			this.detailGroupBox.TabStop = false;
			this.detailGroupBox.Text = "Details";
			// 
			// LicenceSettingDetailControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailGroupBox);
			this.Name = "LicenceSettingDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 156, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox detailGroupBox;


	}
}
