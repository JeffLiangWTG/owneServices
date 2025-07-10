using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Shared
{
	public partial class ConfirmationForm
	{
		Label ConfirmationMessageLabel;
		Button TrueButton;
		Button FalseButton;
		Button SendEmailButton;
		ListBox DetailListBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ConfirmationMessageLabel = new Label();
			this.DetailListBox = new ListBox();
			this.TrueButton = new Button();
			this.FalseButton = new Button();
			this.SendEmailButton = new Button();
			this.SuspendLayout();
			// 
			// ConfirmationMessageLabel
			// 
			this.ConfirmationMessageLabel.Anchor = ((AnchorStyles.Top | AnchorStyles.Left)
						| AnchorStyles.Right);
			this.ConfirmationMessageLabel.Location = ControlDpiScalingHelper.NewScaledPoint(8, 4);
			this.ConfirmationMessageLabel.Name = "ConfirmationMessageLabel";
			this.ConfirmationMessageLabel.Size = ControlDpiScalingHelper.NewScaledSize(640, 48);
			this.ConfirmationMessageLabel.TabIndex = 0;
			this.ConfirmationMessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// DetailListBox
			// 
			this.DetailListBox.Anchor = (((AnchorStyles.Top | AnchorStyles.Bottom)
						| AnchorStyles.Left)
						| AnchorStyles.Right);
			this.DetailListBox.Location = ControlDpiScalingHelper.NewScaledPoint(4, 56);
			this.DetailListBox.Name = "DetailListBox";
			this.DetailListBox.Size = ControlDpiScalingHelper.NewScaledSize(644, 147);
			this.DetailListBox.TabIndex = 1;
			// 
			// TrueButton
			// 
			this.TrueButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.TrueButton.DialogResult = DialogResult.Yes;
			this.TrueButton.FlatStyle = FlatStyle.System;
			this.TrueButton.Location = ControlDpiScalingHelper.NewScaledPoint(484, 216);
			this.TrueButton.Name = "TrueButton";
			this.TrueButton.Size = ControlDpiScalingHelper.NewScaledSize(75, 23);
			this.TrueButton.TabIndex = 2;
			this.TrueButton.Text = "Yes";
			// 
			// FalseButton
			// 
			this.FalseButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.FalseButton.DialogResult = DialogResult.No;
			this.FalseButton.FlatStyle = FlatStyle.System;
			this.FalseButton.Location = ControlDpiScalingHelper.NewScaledPoint(564, 216);
			this.FalseButton.Name = "FalseButton";
			this.FalseButton.Size = ControlDpiScalingHelper.NewScaledSize(75, 23);
			this.FalseButton.TabIndex = 3;
			this.FalseButton.Text = "No";
			// 
			// SendEmailButton
			// 
			this.SendEmailButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.SendEmailButton.FlatStyle = FlatStyle.System;
			this.SendEmailButton.Location = ControlDpiScalingHelper.NewScaledPoint(356, 216);
			this.SendEmailButton.Name = "SendEmailButton";
			this.SendEmailButton.Size = ControlDpiScalingHelper.NewScaledSize(120, 23);
			this.SendEmailButton.TabIndex = 4;
			this.SendEmailButton.Text = "Email Logged in Users";
			this.SendEmailButton.Click += new System.EventHandler(this.SendEmailButton_Click);
			// 
			// ConfirmationForm
			// 

			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(656, 242);
			this.Controls.Add(this.DetailListBox);
			this.Controls.Add(this.SendEmailButton);
			this.Controls.Add(this.FalseButton);
			this.Controls.Add(this.TrueButton);
			this.Controls.Add(this.ConfirmationMessageLabel);
			this.MinimizeBox = false;
			this.Name = "ConfirmationForm";
			this.StartPosition = FormStartPosition.CenterParent;
			this.Text = "ConfirmationForm";
			this.ResumeLayout(false);
		}
	}
}
