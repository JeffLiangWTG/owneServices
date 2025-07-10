namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class DateTimeOffsetRangeFieldUserControl
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
			this.dateControl = new Enterprise.DocumentEngine.GUI.RuntimeOptions.Date.DateTimeOffsetRangeEditControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dateControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.DateTimeOffsetRangeField);
			// 
			// FieldLabel
			// 
			this.FieldLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateTimeOffsetRangeUserControl|9ffe0929-1c7c-44ef-9062-6c1bdb80812d", "Field");
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 23, true);
			this.FieldLabel.TabIndex = 0;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// dateControl
			// 
			this.dateControl.AllowDrop = true;
			this.dateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 2, true);
			this.dateControl.Name = "dateControl";
			this.dateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 55, true);
			this.dateControl.TabIndex = 1;
			// 
			// DateRangeFieldUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.dateControl);
			this.Controls.Add(this.FieldLabel);
			this.Name = "DateRangeFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 59, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dateControl.ResumeLayout(true);
			this.dateControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal Enterprise.ZArchitecture.ZLabel FieldLabel;
		internal Enterprise.DocumentEngine.GUI.RuntimeOptions.Date.DateTimeOffsetRangeEditControl dateControl;
	}
}
