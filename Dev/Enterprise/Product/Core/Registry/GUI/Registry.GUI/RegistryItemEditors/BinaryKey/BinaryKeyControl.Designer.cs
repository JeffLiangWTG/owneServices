using System;

namespace Enterprise.Registry.GUI
{
	partial class BinaryKeyControl
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
			this.passphraseForKeyGenerationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.hexadecimalKeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.generateKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.viewKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// passphraseForKeyGenerationTextBox
			// 
			this.passphraseForKeyGenerationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.passphraseForKeyGenerationTextBox.Name = "passphraseForKeyGenerationTextBox";
			this.passphraseForKeyGenerationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 17, true);
			this.passphraseForKeyGenerationTextBox.TabIndex = 1;
			// 
			// hexadecimalKeyTextBox
			// 
			this.hexadecimalKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 92, true);
			this.hexadecimalKeyTextBox.Multiline = true;
			this.hexadecimalKeyTextBox.Name = "hexadecimalKeyTextBox";
			this.hexadecimalKeyTextBox.PasswordChar = '*';
			this.hexadecimalKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 55, true);
			this.hexadecimalKeyTextBox.TabIndex = 4;
			// 
			// generateKeyButton
			// 
			this.generateKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 37, true);
			this.generateKeyButton.Name = "generateKeyButton";
			this.generateKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 23, true);
			this.generateKeyButton.TabIndex = 2;
			this.generateKeyButton.Text = "Generate Key";
			this.generateKeyButton.UseVisualStyleBackColor = true;
			this.generateKeyButton.Click += new System.EventHandler(this.GenerateKeyButton_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 10, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 23, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Enter a passphrase to automatically generate a key:";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 66, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 23, true);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.Text = "Alternatively, enter a key directly in hexadecimal format:";
			// 
			// viewKeyButton
			// 
			this.viewKeyButton.IsCaptionOverridden = true;
			this.viewKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 149, true);
			this.viewKeyButton.Name = "viewKeyButton";
			this.viewKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 23, true);
			this.viewKeyButton.TabIndex = 5;
			this.viewKeyButton.Text = "View";
			this.viewKeyButton.ToolTipCaption = null;
			this.viewKeyButton.UseVisualStyleBackColor = true;
			this.viewKeyButton.Click += new System.EventHandler(this.viewKeyButton_Click);
			// 
			// BinaryKeyControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.viewKeyButton);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.generateKeyButton);
			this.Controls.Add(this.hexadecimalKeyTextBox);
			this.Controls.Add(this.passphraseForKeyGenerationTextBox);
			this.Name = "BinaryKeyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 174, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.ZTextBox passphraseForKeyGenerationTextBox;
		protected internal ZArchitecture.ZTextBox hexadecimalKeyTextBox;
		protected internal ZArchitecture.GUI.ZButton generateKeyButton;
		protected internal void GenerateKeyButton_Click_Internal(object sender, EventArgs e) => GenerateKeyButton_Click(sender, e);
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		protected internal ZArchitecture.GUI.ZButton viewKeyButton;
	}
}
