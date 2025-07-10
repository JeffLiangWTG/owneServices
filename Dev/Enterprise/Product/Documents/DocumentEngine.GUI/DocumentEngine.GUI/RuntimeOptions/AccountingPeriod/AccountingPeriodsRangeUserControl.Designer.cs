namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class AccountingPeriodsRangeUserControl
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
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PeriodRangeEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodRangeEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PeriodRangeEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.AccountingPeriodsRangeField);
			// 
			// FieldLabel
			// 
			this.FieldLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingPeriodsRangeUserControl|62ea163d-ed88-4cc1-97c9-a7691e86efcc", "Field");
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 1, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.FieldLabel.TabIndex = 0;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// PeriodRangeEdit
			// 
			this.PeriodRangeEdit.AllowDrop = true;
			this.PeriodRangeEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PeriodRangeEdit.BindToHigh = "PeriodTo";
			this.PeriodRangeEdit.BindToLow = "PeriodFrom";
			this.PeriodRangeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 2, true);
			this.PeriodRangeEdit.Name = "PeriodRangeEdit";
			this.PeriodRangeEdit.ReadOnly = false;
			this.PeriodRangeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 52, true);
			this.PeriodRangeEdit.TabIndex = 1;
			// 
			// AccountingPeriodsRangeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodRangeEdit);
			this.Controls.Add(this.FieldLabel);
			this.Name = "AccountingPeriodsRangeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 57, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PeriodRangeEdit.ResumeLayout(true);
			this.PeriodRangeEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZLabel FieldLabel;
		internal SchedulablePeriodRangeEdit PeriodRangeEdit;
	}
}
