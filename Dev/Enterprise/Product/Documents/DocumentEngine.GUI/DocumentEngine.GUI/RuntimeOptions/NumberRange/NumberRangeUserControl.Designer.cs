using Enterprise.DocumentEngine.GUI.RuntimeOptions.NumberRange;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class NumberRangeUserControl : RuntimeOptionUserControl
	{
		ZLabel FieldLabel;
		NumberRangeEditControl NumberRangeEditControl;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NumberRangeEditControl = new NumberRangeEditControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
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
			// NumberRangeEditControl
			//
			this.NumberRangeEditControl.AllowDrop = true;
			this.NumberRangeEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 1, true);
			this.NumberRangeEditControl.Name = "NumberControl";
			this.NumberRangeEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 27, true);
			this.NumberRangeEditControl.TabIndex = 1;
			// 
			// NumberRangeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldLabel);
			this.Controls.Add(this.NumberRangeEditControl);
			this.Name = "NumberRangeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 29, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
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
