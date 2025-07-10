using Enterprise.DocumentEngine.GUI.RuntimeOptions.TextRange;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class MonthYearPeriodUserControl : RuntimeOptionUserControl
	{
		ZLabel FieldLabel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			this.monthYearPeriodEditControl = new Enterprise.DocumentEngine.GUI.RuntimeOptions.MonthYearPeriodEditControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.monthYearPeriodEditControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// FieldLabel
			// 
			this.FieldLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 26, true);
			this.FieldLabel.TabIndex = 0;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			this.FieldLabel.UseMnemonic = false;
			// 
			// monthYearPeriodEditControl
			// 
			this.monthYearPeriodEditControl.AllowDrop = true;
			this.monthYearPeriodEditControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.monthYearPeriodEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 0, true);
			this.monthYearPeriodEditControl.Name = "monthYearPeriodEditControl";
			this.monthYearPeriodEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 26, true);
			this.monthYearPeriodEditControl.TabIndex = 1;
			// 
			// MonthYearPeriodUserControl
			// 
			this.Controls.Add(this.monthYearPeriodEditControl);
			this.Controls.Add(this.FieldLabel);
			this.Name = "MonthYearPeriodUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.monthYearPeriodEditControl.ResumeLayout(true);
			this.monthYearPeriodEditControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private Enterprise.DocumentEngine.GUI.RuntimeOptions.MonthYearPeriodEditControl monthYearPeriodEditControl;
	}
}
