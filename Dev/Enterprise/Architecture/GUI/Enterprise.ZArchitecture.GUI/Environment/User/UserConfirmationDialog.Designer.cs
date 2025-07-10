using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Environment
{
	public partial class UserConfirmationDialog
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
		void InitializeComponent()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
		{
			this.ConfirmationStringTextBox = new KTextBox();
			this.ConfirmationPromptLabel = new KLabel();
			this.ExpectedStringLabel = new UserConfirmationStringLabel();
			this.DontAskMeAgainInThisSessionCheckBox = new KCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// ConfirmationStringTextBox
			//
			this.ConfirmationStringTextBox.Font = new Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			this.ConfirmationStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 72, true);
			this.ConfirmationStringTextBox.Name = "ConfirmationStringTextBox";
			this.ConfirmationStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ConfirmationStringTextBox.TabIndex = 2;
			this.ConfirmationStringTextBox.TextChanged += new EventHandler(this.ConfirmationStringTextBox_TextChanged);
			this.ConfirmationStringTextBox.KeyPress += new KeyPressEventHandler(ConfirmationStringTextBox_KeyPress);
			//
			// ConfirmationPromptLabel
			//
			this.ConfirmationPromptLabel.Font = new Font(OFont.NormalFontName, 8F);
			this.ConfirmationPromptLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.ConfirmationPromptLabel.Name = "ConfirmationPromptLabel";
			this.ConfirmationPromptLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 32, true);
			this.ConfirmationPromptLabel.TabIndex = 4;
			this.ConfirmationPromptLabel.Text = "<Prompt:>";
			//
			// ExpectedStringLabel
			//
			this.ExpectedStringLabel.Font = new Font(OFont.NormalFontName, 8F);
			this.ExpectedStringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.ExpectedStringLabel.Name = "ExpectedStringLabel";
			this.ExpectedStringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 32, true);
			this.ExpectedStringLabel.TabIndex = 5;
			this.ExpectedStringLabel.Text = "<ExpectedString>";
			//
			// DontAskMeAgainInThisSessionCheckBox
			//
			this.DontAskMeAgainInThisSessionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 102, true);
			this.DontAskMeAgainInThisSessionCheckBox.Name = "DontAskMeAgainInThisSessionCheckBox";
			this.DontAskMeAgainInThisSessionCheckBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DontAskMeAgainInThisSessionCheckBox.TextAlign = ContentAlignment.MiddleLeft;
			this.DontAskMeAgainInThisSessionCheckBox.Text = Res.GetString("UserConfirmationDialog|DontAskMeAgainInThisSessionCheckBox", "Don't ask me again in this session");
			this.DontAskMeAgainInThisSessionCheckBox.TabIndex = 5;
			//
			// UserConfirmationDialog
			//

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 144, true);
			this.Controls.Add(this.ConfirmationStringTextBox);
			this.Controls.Add(this.ConfirmationPromptLabel);
			this.Controls.Add(this.ExpectedStringLabel);
			this.Controls.Add(this.DontAskMeAgainInThisSessionCheckBox);
			this.Name = "UserConfirmationDialog";
			this.Text = "UserConfirmationDialog";
			this.Controls.SetChildIndex(this.PictureBox, 0);
			this.Controls.SetChildIndex(this.ConfirmationPromptLabel, 0);
			this.Controls.SetChildIndex(this.ExpectedStringLabel, 0);
			this.Controls.SetChildIndex(this.Button1, 0);
			this.Controls.SetChildIndex(this.Button2, 0);
			this.Controls.SetChildIndex(this.Button3, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.ConfirmationStringTextBox, 0);
			this.Controls.SetChildIndex(this.DontAskMeAgainInThisSessionCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
