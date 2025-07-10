using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms
{
	public partial class DeveloperLoginForm
	{
		ZLabel oLabel1;
		ZLabel oLabel2;
		internal ZTextBox PasswordTextBox;
		ZArchitecture.GUI.ZButton OKButton;
		ZArchitecture.GUI.ZButton CloseButton;
		ZLabel InvalidLoginLabel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.oLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.oLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InvalidLoginLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// oLabel1
			//
			this.oLabel1.IsFontBold = true;
			this.oLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.oLabel1.Name = "oLabel1";
			this.oLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 23, true);
			this.oLabel1.TabIndex = 0;
			this.oLabel1.Text = "FOR DEVELOPER USE ONLY";
			//
			// oLabel2
			//
			this.oLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.oLabel2.Name = "oLabel2";
			this.oLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.oLabel2.TabIndex = 1;
			this.oLabel2.Text = "Password:";
			//
			// PasswordTextBox
			//
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.PasswordTextBox.TabIndex = 2;
			//
			// OKButton
			//
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.Text = "OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			//
			// CloseButton
			//
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 64, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			//
			// InvalidLoginLabel
			//
			this.InvalidLoginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 96, true);
			this.InvalidLoginLabel.Name = "InvalidLoginLabel";
			this.InvalidLoginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 16, true);
			this.InvalidLoginLabel.TabIndex = 5;
			//
			// DeveloperLoginForm
			//
			this.AcceptButton = this.OKButton;

			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 117, true);
			this.Controls.Add(this.InvalidLoginLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.oLabel2);
			this.Controls.Add(this.oLabel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "DeveloperLoginForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Developer Diagnostic Login";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
