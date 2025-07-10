using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Shared
{
	public partial class ConfirmationForm : Form // Cannot use ODesignableForm as it might trigger DB hits
	{
		readonly System.ComponentModel.Container components;

		public ConfirmationForm()
		{
			InitializeComponent();
		}

		public bool ShowMessage(string title, string message, string[] detailLines, MessageBoxButtons buttonsToShow, bool allowEmail = false)
		{
			SendEmailButton.Visible = allowEmail;
			Text = title;
			ConfirmationMessageLabel.Text = message;
			DetailLines = detailLines;

			PopulateDetailListBoxAfterSettingDetailLines();
			SetButtons(buttonsToShow);

			TopMost = true;
			bool result = (ShowDialog() == DialogResult.Yes);

			return result;
		}

		public bool ShowLoggedInUsersMessage(string title, string message, KeyValuePair<string, string>[] loggedInUsers, MessageBoxButtons buttonsToShow, bool allowEmail)
		{
			SendEmailButton.Visible = allowEmail;

			string[] detailLines = new string[loggedInUsers.Length];
			string[] loggedInUserCodes = new string[loggedInUsers.Length];

			for (int i = 0; i < loggedInUsers.Length; i++)
			{
				KeyValuePair<string, string> loggedInUser = loggedInUsers[i];

				if (!String.IsNullOrEmpty(loggedInUser.Value))
				{
					detailLines[i] = loggedInUser.Value;
					loggedInUserCodes[i] = (String.IsNullOrEmpty(loggedInUser.Key)) ? "" : loggedInUser.Key;
				}
			}

			this.LoggedInUserCodes = loggedInUserCodes;

			return ShowMessage(title, message, detailLines, buttonsToShow);
		}

		string[] DetailLines;
		string[] LoggedInUserCodes;

		void PopulateDetailListBoxAfterSettingDetailLines()
		{
			foreach (string desc in DetailLines)
			{
				if (!string.IsNullOrEmpty(desc))
				{
					DetailListBox.Items.Add(desc);
				}
			}
		}

		void SetButtons(MessageBoxButtons buttonsToShow)
		{
			if (buttonsToShow == MessageBoxButtons.RetryCancel)
			{
				TrueButton.Text = "Retry";
				FalseButton.Text = "Cancel";
			}
			else
			{
				TrueButton.Text = "Yes";
				FalseButton.Text = "No";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "No reference to ZArchitecture.GUI, no integration with Remote Desktop Services during upgrade")]
		void SendEmailButton_Click(object sender, EventArgs e)
		{
			StringBuilder recipients = new StringBuilder();

			foreach (string userCode in LoggedInUserCodes)
			{
				if (!string.IsNullOrWhiteSpace(userCode))
				{
					var objRecipient = Db.Connection.ExecuteScalar(
						$"SELECT {GlbStaffSchema.Constants.GS_EmailAddress} FROM {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName} WHERE {GlbStaffSchema.Constants.GS_Code} = @userCode",
						cmd => cmd.AddParameterBasedOnDbColumn("@userCode", userCode, GlbStaffSchema.GS_Code));

					string recipient = (objRecipient == null || objRecipient == DBNull.Value) ? null : objRecipient.ToString().Trim();

					if (!String.IsNullOrEmpty(recipient))
					{
						recipients.Append(recipient + ";");
					}
				}
			}

			if (recipients.Length > 0)
			{
				Process.Start("mailto:" + recipients.ToString() + "?Subject=Please%20log%20out"); // No reference to ZArchitecture.GUI, no integration with Remote Desktop Services during upgrade
			}
		}

		#region Auto

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

		#endregion
	}
}
