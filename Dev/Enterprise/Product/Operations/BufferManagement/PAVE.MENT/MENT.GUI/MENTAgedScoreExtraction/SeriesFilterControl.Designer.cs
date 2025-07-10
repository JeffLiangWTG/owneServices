namespace Enterprise.PAVE.MENT.GUI
{
	partial class SeriesFilterControl
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
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.filterStripWrapperControl = new Enterprise.ZArchitecture.GUI.FilterStripWrapperControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("d88ce459-d84b-4fc7-bd20-16fb66c871fb", "Series Filter");
			this.zGroupBox1.Controls.Add(this.filterStripWrapperControl);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 274, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// filterStripWrapperControl
			// 
			this.filterStripWrapperControl.AllowDrop = true;
			this.filterStripWrapperControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.filterStripWrapperControl, "Extractions.SeriesFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).SeriesFilter)));
			this.filterStripWrapperControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterStripWrapperControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.filterStripWrapperControl.Name = "filterStripWrapperControl";
			this.filterStripWrapperControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 255, true);
			this.filterStripWrapperControl.TabIndex = 0;
			// 
			// SeriesFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "SeriesFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.FilterStripWrapperControl filterStripWrapperControl;
	}
}
