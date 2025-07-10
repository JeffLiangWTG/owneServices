using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Date
{
	partial class DateTimeOffsetRangeEditControl
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
			this.FromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromDateEdit = new SchedulableDateTimeOffsetEdit();
			this.ToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToDateEdit = new SchedulableDateTimeOffsetEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FromDateEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// FromLabel
			// 
			this.FromLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateTimeOffsetRangeEditControl|d26e67c6-ae6b-442f-b2fc-b9979f5e7573", "From");
			this.FromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.FromLabel.Name = "FromLabel";
			this.FromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 14, true);
			this.FromLabel.TabIndex = 1;
			this.FromLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FromLabel.UseCompatibleTextRendering = true;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FromDateEdit.BindTo = "ValueLow";
			this.FromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 0, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.FromDateEdit.TabIndex = 2;
			// 
			// ToLabel
			// 
			this.ToLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("DateTimeOffsetRangeEditControl|973ad819-8f77-4850-bad3-274824338b5f", "To");
			this.ToLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.ToLabel.Name = "ToLabel";
			this.ToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 14, true);
			this.ToLabel.TabIndex = 3;
			this.ToLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ToLabel.UseCompatibleTextRendering = true;
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ToDateEdit.BindTo = "ValueHigh";
			this.ToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 29, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.ToDateEdit.TabIndex = 4;
			// 
			// DateRangeEditControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToDateEdit);
			this.Controls.Add(this.ToLabel);
			this.Controls.Add(this.FromDateEdit);
			this.Controls.Add(this.FromLabel);
			this.Name = "DateTimeOffsetRangeEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 55, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.ZLabel FromLabel;
		public SchedulableDateTimeOffsetEdit FromDateEdit;
		public ZArchitecture.ZLabel ToLabel;
		public SchedulableDateTimeOffsetEdit ToDateEdit;
	}
}
