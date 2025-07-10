namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5TransportMeansGroupUserControl
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
			this.TransportMeansGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportMeansDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportMeansGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.EnRouteIncidentCollection);
			// 
			// TransportMeansGroupBox
			// 
			this.TransportMeansGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("bfff17b0-b77a-4749-8792-c66ea86ca31c", "Transport Means");
			this.TransportMeansGroupBox.Controls.Add(this.TransportMeansDynamicLayoutPanel);
			this.TransportMeansGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TransportMeansGroupBox, false);
			this.TransportMeansGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportMeansGroupBox.Name = "TransportMeansGroupBox";
			this.TransportMeansGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 150, true);
			this.TransportMeansGroupBox.TabIndex = 0;
			this.TransportMeansGroupBox.TabStop = false;
			this.TransportMeansGroupBox.Text = "Transport Means";
			// 
			// TransportMeansDynamicLayoutPanel
			// 
			this.TransportMeansDynamicLayoutPanel.AllowDrop = true;
			this.TransportMeansDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportMeansDynamicLayoutPanel.Name = "TransportMeansDynamicLayoutPanel";
			this.TransportMeansDynamicLayoutPanel.TabIndex = 1;
			// 
			// Phase5EventTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "Phase5TransportMeansGroupUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportMeansGroupBox.ResumeLayout(false);
			this.TransportMeansGroupBox.PerformLayout();
			this.Controls.Add(TransportMeansGroupBox);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportMeansDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZGroupBox TransportMeansGroupBox;
	}
}
