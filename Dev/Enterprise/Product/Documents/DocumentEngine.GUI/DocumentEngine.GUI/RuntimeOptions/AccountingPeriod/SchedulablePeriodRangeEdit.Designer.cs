namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SchedulablePeriodRangeEdit
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
			this.LowValuePeriodEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodEdit();
			this.HighValuePeriodEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodEdit();
			this.FromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LowValuePeriodEdit.SuspendLayout();
			this.HighValuePeriodEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// LowValuePeriodEdit
			// 
			this.LowValuePeriodEdit.AllowDrop = true;
			this.LowValuePeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 3, true);
			this.LowValuePeriodEdit.Name = "LowValuePeriodEdit";
			this.LowValuePeriodEdit.ReadOnly = false;
			this.LowValuePeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.LowValuePeriodEdit.TabIndex = 1;
			// 
			// HighValuePeriodEdit
			// 
			this.HighValuePeriodEdit.AllowDrop = true;
			this.HighValuePeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 29, true);
			this.HighValuePeriodEdit.Name = "HighValuePeriodEdit";
			this.HighValuePeriodEdit.ReadOnly = false;
			this.HighValuePeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.HighValuePeriodEdit.TabIndex = 3;
			// 
			// FromLabel
			// 
			this.FromLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SchedulablePeriodRangeEdit|0582067d-3832-45d7-9694-ab9bb3d1977a", "From");
			this.FromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.FromLabel.Name = "FromLabel";
			this.FromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 14, true);
			this.FromLabel.TabIndex = 0;
			this.FromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ToLabel
			// 
			this.ToLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SchedulablePeriodRangeEdit|b138fc69-c8d0-4375-9c58-3525a1392b56", "To");
			this.ToLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 35, true);
			this.ToLabel.Name = "ToLabel";
			this.ToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 14, true);
			this.ToLabel.TabIndex = 2;
			this.ToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SchedulablePeriodRangeEdit
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToLabel);
			this.Controls.Add(this.FromLabel);
			this.Controls.Add(this.HighValuePeriodEdit);
			this.Controls.Add(this.LowValuePeriodEdit);
			this.Name = "SchedulablePeriodRangeEdit";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 55, true);
			this.Resize += new System.EventHandler(this.SchedulablePeriodRangeEdit_Resize);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LowValuePeriodEdit.ResumeLayout(true);
			this.LowValuePeriodEdit.PerformLayout();
			this.HighValuePeriodEdit.ResumeLayout(true);
			this.HighValuePeriodEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal SchedulablePeriodEdit LowValuePeriodEdit;
		internal SchedulablePeriodEdit HighValuePeriodEdit;
		internal Enterprise.ZArchitecture.ZLabel FromLabel;
		internal Enterprise.ZArchitecture.ZLabel ToLabel;
	}
}
