namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class MonthYearPeriodEditControl
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
			this.PeriodMonthEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.MonthYearEdit();
			this.PeriodYearEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.MonthYearEdit();
			this.PeriodLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PeriodYearEdit
			// 
			this.PeriodYearEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MonthYearPeriodEditControl|964cc43d-8735-431c-a39c-4cb581f2998b", "Year");
			this.PeriodYearEdit.DecimalPlaces = 0;
			this.PeriodYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 2, true);
			this.PeriodYearEdit.MaxLength = 4;
			this.PeriodYearEdit.MaxValue = 2079;
			this.PeriodYearEdit.Name = "PeriodYearEdit";
			this.PeriodYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 18, true);
			this.PeriodYearEdit.TabIndex = 2;
			this.PeriodYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PeriodYearEdit.TrackDisposedAccess = true;
			// 
			// PeriodLabel
			// 
			this.PeriodLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PeriodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 2, true);
			this.PeriodLabel.Name = "PeriodLabel";
			this.PeriodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(11, 18, true);
			this.PeriodLabel.TabIndex = 1;
			this.PeriodLabel.Text = "/";
			this.PeriodLabel.UseMnemonic = false;
			// 
			// PeriodMonthEdit
			// 
			this.PeriodMonthEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("MonthYearPeriodEditControl|87da453d-674a-4fc7-b564-53ddad88d538", "Month");
			this.PeriodMonthEdit.DecimalPlaces = 0;
			this.PeriodMonthEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.PeriodMonthEdit.MaxLength = 2;
			this.PeriodMonthEdit.MaxValue = 12;
			this.PeriodMonthEdit.Name = "PeriodMonthEdit";
			this.PeriodMonthEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 18, true);
			this.PeriodMonthEdit.TabIndex = 0;
			this.PeriodMonthEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PeriodMonthEdit.TrackDisposedAccess = true;
			// 
			// MonthYearPeriodEditControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodMonthEdit);
			this.Controls.Add(this.PeriodLabel);
			this.Controls.Add(this.PeriodYearEdit);
			this.Name = "MonthYearPeriodEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZLabel PeriodLabel;
		internal Enterprise.DocumentEngine.GUI.RuntimeOptions.MonthYearEdit PeriodYearEdit;
		internal Enterprise.DocumentEngine.GUI.RuntimeOptions.MonthYearEdit PeriodMonthEdit;
	}
}
