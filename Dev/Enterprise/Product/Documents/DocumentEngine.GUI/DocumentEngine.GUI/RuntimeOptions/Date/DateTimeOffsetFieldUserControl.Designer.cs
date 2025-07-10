namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class DateTimeOffsetFieldUserControl
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
			this.FieldDateEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulableDateTimeOffsetEdit();
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FieldDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.DateTimeOffsetField);
			// 
			// FieldDateEdit
			// 
			this.FieldDateEdit.AllowDrop = true;
			this.FieldDateEdit.BindTo = "Value";
			this.FieldDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.FieldDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 0, true);
			this.FieldDateEdit.Name = "FieldDateEdit";
			this.FieldDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.FieldDateEdit.TabIndex = 2;
			// 
			// FieldLabel
			// 
			this.FieldLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateFieldUserControl|97ec7c36-891f-4918-90f6-044460ca7ec6", "Label");
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.FieldLabel.TabIndex = 1;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// DateFieldUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldDateEdit);
			this.Controls.Add(this.FieldLabel);
			this.Name = "DateTimeOffsetFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FieldDateEdit.ResumeLayout(true);
			this.FieldDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZLabel FieldLabel;
		internal Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulableDateTimeOffsetEdit FieldDateEdit;
	}
}
