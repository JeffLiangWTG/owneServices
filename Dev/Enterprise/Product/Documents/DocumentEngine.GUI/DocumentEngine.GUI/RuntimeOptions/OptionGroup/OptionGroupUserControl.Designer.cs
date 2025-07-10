using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class OptionGroupUserControl : RuntimeOptionUserControl
	{
		protected ZCheckedListBox CheckedListBox;
		protected Enterprise.ZArchitecture.ZLabel FieldLabel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.CheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.FieldLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CheckedListBox
			// 
			this.CheckedListBox.BindingItems = null;
			this.CheckedListBox.CheckOnClick = true;
			this.CheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 0, true);
			this.CheckedListBox.Name = "CheckedListBox";
			this.CheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 19, true);
			this.CheckedListBox.TabIndex = 1;
			this.CheckedListBox.ThreeDCheckBoxes = true;
			// 
			// FieldLabel
			// 
			this.FieldLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FieldLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.FieldLabel.Name = "FieldLabel";
			this.FieldLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 4, 0, true);
			this.FieldLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.FieldLabel.TabIndex = 0;
			this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FieldLabel.UseCompatibleTextRendering = true;
			// 
			// OptionGroupUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldLabel);
			this.Controls.Add(this.CheckedListBox);
			this.Name = "OptionGroupUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 22, true);
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
