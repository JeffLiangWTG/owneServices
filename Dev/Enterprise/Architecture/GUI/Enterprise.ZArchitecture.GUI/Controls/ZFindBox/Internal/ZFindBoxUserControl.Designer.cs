using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public abstract partial class ZFindBoxUserControl
	{
		void InitializeComponent()
		{
			this.PopupButton = new ZButton.Bare();
			this.SuspendLayout();
			// 
			// CodeBox
			// 
			this.CodeBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.CodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeBox.Name = "CodeBox";
			this.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CodeBox.TabIndex = 0;
			// 
			// PopupButton
			// 
			this.PopupButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.PopupButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 0, true);
			this.PopupButton.Name = "PopupButton";
			this.PopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.PopupButton.TabIndex = 1;
			this.PopupButton.TabStop = false;
			this.PopupButton.Text = "...";
			this.PopupButton.Font = OFont.GetFontBold();
			this.PopupButton.Click += new EventHandler(this.PopupButton_Click);
			this.PopupButton.MouseUp += new MouseEventHandler(PopupButton_MouseUp);
			//
			// ZFindBoxUserControl
			//
			this.Controls.Add(this.PopupButton);
			this.Controls.Add(this.CodeBox);
			this.ResumeLayout(false);
		}

	}
}
