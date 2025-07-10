namespace Enterprise.PAVE.MENT.GUI
{
	partial class SeriesConfigurationControl
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
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.dataSeriesColumnControl = new Enterprise.PAVE.MENT.GUI.DataSeriesColumnControl();
			this.seriesFilterControl = new Enterprise.PAVE.MENT.GUI.SeriesFilterControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.dataSeriesColumnControl);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.seriesFilterControl);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 433, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.splitContainer.TabIndex = 0;
			// 
			// dataSeriesColumnControl
			// 
			this.dataSeriesColumnControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dataSeriesColumnControl, ".");
			this.dataSeriesColumnControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataSeriesColumnControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dataSeriesColumnControl.Name = "dataSeriesColumnControl";
			this.dataSeriesColumnControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 160, true);
			this.dataSeriesColumnControl.TabIndex = 0;
			// 
			// seriesFilterControl
			// 
			this.seriesFilterControl.AllowDrop = true;
			this.seriesFilterControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.seriesFilterControl, ".");
			this.seriesFilterControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seriesFilterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.seriesFilterControl.Name = "seriesFilterControl";
			this.seriesFilterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 296, true);
			this.seriesFilterControl.TabIndex = 0;
			// 
			// SeriesConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "SeriesConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 433, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer;
		private DataSeriesColumnControl dataSeriesColumnControl;
		private SeriesFilterControl seriesFilterControl;
	}
}
