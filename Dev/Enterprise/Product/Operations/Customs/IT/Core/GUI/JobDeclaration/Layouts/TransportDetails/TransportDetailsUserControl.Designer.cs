namespace Enterprise.Customs.IT.GUI
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
			this.TransportInlandAirUserControl = new Enterprise.Customs.IT.GUI.TransportInlandAirUserControl();
			this.TransportInlandRoadUserControl = new Enterprise.Customs.IT.GUI.TransportInlandRoadUserControl();
			this.InlandTransportModeAndMeansUserControl = new Enterprise.Customs.IT.GUI.InlandTransportModeAndMeansUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportInlandAirUserControl.SuspendLayout();
			this.TransportInlandRoadUserControl.SuspendLayout();
			this.InlandTransportModeAndMeansUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// TransportInlandAirUserControl
			// 
			this.TransportInlandAirUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandAirUserControl, ".");
			this.TransportInlandAirUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 34, true);
			this.TransportInlandAirUserControl.Name = "TransportInlandAirUserControl";
			this.TransportInlandAirUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 45, true);
			this.TransportInlandAirUserControl.TabIndex = 1;
			// 
			// TransportInlandRoadUserControl
			// 
			this.TransportInlandRoadUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportInlandRoadUserControl, ".");
			this.TransportInlandRoadUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 87, true);
			this.TransportInlandRoadUserControl.Name = "TransportInlandRoadUserControl";
			this.TransportInlandRoadUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 65, true);
			this.TransportInlandRoadUserControl.TabIndex = 2;
			// 
			// InlandTransportModeAndMeansUserControl
			// 
			this.InlandTransportModeAndMeansUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandTransportModeAndMeansUserControl, ".");
			this.InlandTransportModeAndMeansUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 158, true);
			this.InlandTransportModeAndMeansUserControl.Name = "InlandTransportModeAndMeansUserControl";
			this.InlandTransportModeAndMeansUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			this.InlandTransportModeAndMeansUserControl.TabIndex = 3;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InlandTransportModeAndMeansUserControl);
			this.Controls.Add(this.TransportInlandRoadUserControl);
			this.Controls.Add(this.TransportInlandAirUserControl);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 211, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportInlandAirUserControl.ResumeLayout(true);
			this.TransportInlandAirUserControl.PerformLayout();
			this.TransportInlandRoadUserControl.ResumeLayout(true);
			this.TransportInlandRoadUserControl.PerformLayout();
			this.InlandTransportModeAndMeansUserControl.ResumeLayout(true);
			this.InlandTransportModeAndMeansUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal TransportInlandAirUserControl TransportInlandAirUserControl;
		internal TransportInlandRoadUserControl TransportInlandRoadUserControl;
		internal InlandTransportModeAndMeansUserControl InlandTransportModeAndMeansUserControl;
	}
}
