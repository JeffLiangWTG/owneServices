using Enterprise.DocumentEngine.GUI.RuntimeOptions.TextRange;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class TextRangeFieldUserControl : RuntimeOptionUserControl
	{
		ZLabel FieldLabel;
		TextRangeEditControl textRangeEditControl;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			this.textRangeEditControl = new Enterprise.DocumentEngine.GUI.RuntimeOptions.TextRange.TextRangeEditControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.textRangeEditControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// FieldLabel
			// 
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 26, true);
			this.FieldLabel.TabIndex = 0;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// textRangeEditControl
			// 
			this.textRangeEditControl.AllowDrop = true;
			this.textRangeEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 1, true);
			this.textRangeEditControl.Name = "textRangeEditControl";
			this.textRangeEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 27, true);
			this.textRangeEditControl.TabIndex = 1;
			// 
			// TextRangeFieldUserControl
			// 
			this.Controls.Add(this.FieldLabel);
			this.Controls.Add(this.textRangeEditControl);
			this.Name = "TextRangeFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 29, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.textRangeEditControl.ResumeLayout(true);
			this.textRangeEditControl.PerformLayout();
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
	}
}
