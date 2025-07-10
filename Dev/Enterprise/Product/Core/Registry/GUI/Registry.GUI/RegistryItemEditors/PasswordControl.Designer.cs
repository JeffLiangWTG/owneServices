using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class PasswordControl : ZUserControl
	{
		internal Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton ViewButton;

		void InitializeComponent()
		{
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.PasswordTextBox.TabIndex = 0;
			// 
			// ViewButton
			// 
			this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2461cc12-ce7f-4b30-a22b-6281b76171ec", "View");
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 0, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewButton.TabIndex = 1;
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// PasswordControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.ViewButton);
			this.Name = "PasswordControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
