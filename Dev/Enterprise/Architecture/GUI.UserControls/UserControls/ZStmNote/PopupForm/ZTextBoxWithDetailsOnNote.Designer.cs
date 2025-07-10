using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZTextBoxWithDetailsOnNote
	{
		void InitializeComponent()
		{
			this.textBox = new InnerTextBox(this);
			this.SuspendLayout();
			// 
			// textBox
			// 
			this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox.Name = "textBox";
			this.textBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.textBox.TabIndex = 0;
			this.textBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			//
			// popupButton
			//
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetTop(this.popupButton, CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(this.popupButton.Top) - 1, true);
			this.popupButton.BackColor = System.Drawing.Color.FromArgb(255, 196, 199, 200);
			//
			// ZTextBoxWithDetailsOnNote
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.textBox);
			this.Name = "ZTextBoxWithDetailsOnNote";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.Controls.SetChildIndex(this.textBox, 0);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private InnerTextBox textBox;
	}
}
