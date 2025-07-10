using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataPurge
{
	public partial class DataPurgeUserForm : ZChildForm
	{
		public DataPurgeUserForm() : this(new DataPurger())
		{
		}

		DataPurgeUserForm(DataPurger purger) : base(purger)
		{
			InitializeComponent();
			this.Purger = purger;
			this.SystemWideTabPage.TabInitialized += (o, e) => this.RunSystemWideWithoutTransactionButton.Visible = purger.IsNonTransactionalPurgeAllowed;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		bool CheckRequirementsBeforeRun()
		{
			bool result = false;
			if (ConfirmToBlockIfOtherUsersAreConnected())
			{
				if (GetUserConfirmationForPurging())
				{
					result = true;
				}
			}
			return result;
		}

		bool ConfirmToBlockIfOtherUsersAreConnected()
		{
			var result = true;

			IActiveUserQuery activeUserQuery = new ActiveUserQuery();
			var otherUsers = activeUserQuery.GetActiveUsers(false);
			var otherEnterpriseUsers = ActiveUserQuery.GetEnterpriseActiveUserSessions(false);

			var otherUserCount = otherUsers.Length;

			if (otherUserCount > 0)
			{
				result = true;

				var messageBuilder = new StringBuilder();
				messageBuilder.Append(Res.GetString("f765aa95-41b8-4807-89f9-bde579c77cbd", "The following") + " ");
				messageBuilder.Append((otherUserCount == 1) ? Res.GetString("d29d5564-bf44-474b-b25f-defcff3ce96a", "user is") : Res.GetString("c02cbad6-7d14-48b0-86b9-6cdc75a0ccde", "users are"));
				messageBuilder.Append(" " + Res.GetString("a0503b08-70ad-44a3-afde-824245c8c4d1", "also connected:") + "\r\n\r\n");

				foreach (var user in otherUsers)
				{
					messageBuilder.AppendLine(user);
				}

				if (otherEnterpriseUsers.Any())
				{
					var logOffUsersMessage = "\r\n" + Res.GetString("C537B0DF-F093-495F-9F2B-4E17F267BD3B", "The Purge Data action cannot proceed with users logged in.  Would you like to log the users out?") + "\r\n\r\n";
					result = Globals.Message.Show(messageBuilder.ToString() + logOffUsersMessage, FormHeading, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
				}
				else
				{
					messageBuilder.Append("\r\n" + Res.GetString("81826e74-3100-4fc2-84c9-fb745937bb7c", "All other users are required to log off for data purge to prevent potential problems, such as inconsistent data view and database deadlock. However, you can still proceed at your own risk. Do you wish to continue?"));
					result = Globals.Message.Show(messageBuilder.ToString(), FormHeading, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
				}
			}

			return result;
		}

		bool GetUserConfirmationForPurging()
		{
			var confirmationMessage = Res.GetString("523ad83a-93a4-4c1f-8377-da5fa387ffd3", "You have selected to purge data. The data will be permanently deleted and it cannot be reverted.");
			var confirmationPrompt = Res.GetString("183d9d59-5f46-49fc-b184-d98fa3bdeac0", "To continue with purging this data please confirm this action.");
			return (Globals.Message.ShowConfirmation(confirmationMessage, FormHeading, confirmationPrompt, Res.GetString("471a07a2-11e3-4700-a85c-d06a267675f1", "Confirm"), MessageBoxIcon.Warning) == DialogResult.OK);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		void RunScript(Action<DataPurger> purgeAction)
		{
			if (Purger.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				if (CheckRequirementsBeforeRun())
				{
					try
					{
						Cursor.Current = Cursors.WaitCursor;
						purgeAction(Purger);
					}
					finally
					{
						Cursor.Current = Cursors.Default;
					}
				}
			}
		}

		internal DataPurger Purger;

		void RunSystemWideButton_Click(object sender, EventArgs e)
		{
			RunScript(new Action<DataPurger>((p) => p.BackupAndPurgeSystemWideTransactional()));
		}

		void RunCompanySpecificButton_Click(object sender, EventArgs e)
		{
			RunScript(new Action<DataPurger>((p) => p.BackupAndPurgeCompanySpecific()));
		}

		void RunSystemWideWithoutTransactionButton_Click(object sender, EventArgs e)
		{
			RunScript(new Action<DataPurger>((p) => p.BackupAndPurgeSystemWideWithoutTransaction()));
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
