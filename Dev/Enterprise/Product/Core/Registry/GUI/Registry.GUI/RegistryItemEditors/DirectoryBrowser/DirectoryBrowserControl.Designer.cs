using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class DirectoryBrowserControl : ZUserControl
	{
		protected Enterprise.ZArchitecture.ZTextBox DirectoryTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton DirectoryBrowserButton;

		void InitializeComponent()
		{
			this.DirectoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirectoryBrowserButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DirectoryTextBox
			// 
			this.DirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DirectoryTextBox.Name = "DirectoryTextBox";
			this.DirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.DirectoryTextBox.TabIndex = 0;
			// 
			// DirectoryBrowserButton
			// 
			this.DirectoryBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DirectoryBrowserButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 0, true);
			this.DirectoryBrowserButton.Name = "DirectoryBrowserButton";
			this.DirectoryBrowserButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 22, true);
			this.DirectoryBrowserButton.TabIndex = 1;
			this.DirectoryBrowserButton.Text = "...";
			this.DirectoryBrowserButton.Click += new System.EventHandler(this.DirectoryBrowserButton_Click);
			// 
			// DirectoryBrowserControl
			// 
			this.Controls.Add(this.DirectoryBrowserButton);
			this.Controls.Add(this.DirectoryTextBox);
			this.Name = "DirectoryBrowserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
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
