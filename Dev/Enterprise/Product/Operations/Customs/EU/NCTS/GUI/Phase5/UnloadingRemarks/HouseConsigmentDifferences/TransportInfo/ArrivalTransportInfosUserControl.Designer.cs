namespace Enterprise.Customs.EU.NCTS.GUI
{
	internal partial class ArrivalTransportInfosUserControl
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
			this.TransportInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportInfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.IArrivalCusTransportMeansCollection<Enterprise.Customs.EU.NCTS.Business.ArrivalCusTransportMeans>);
			// 
			// TransportInfoGroupBox
			// 
			this.TransportInfoGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9ed88ca6-e315-4970-bbbb-1c0c1acb8c64", "Transport Info");
			this.TransportInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportInfoGroupBox.Name = "TransportInfoGroupBox";
			this.TransportInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 121, true);
			this.TransportInfoGroupBox.TabIndex = 0;
			this.TransportInfoGroupBox.TabStop = false;
			// 
			// ArrivalTransportInfosUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportInfoGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Name = "ArrivalTransportInfosUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 121, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportInfoGroupBox.ResumeLayout(false);
			this.TransportInfoGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox TransportInfoGroupBox;
	}
}
