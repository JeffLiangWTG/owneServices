using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.DbUpgrader
{
	public sealed class DbUpgraderUserInteraction : IDbUpgraderUserInteraction
	{
		public bool PromptConfirmDisconnectUsers(KeyValuePair<string, string>[] loggedInUsers)
		{
			EnsureNotInUnitTest(loggedInUsers);

			using (var form = new ConfirmationForm())
			{
				return form.ShowLoggedInUsersMessage("Confirm Close Connections", "The following connections must be closed. Proceed?", loggedInUsers, MessageBoxButtons.YesNo, true);
			}
		}

		public bool PromptRetryAction(string title, string message)
		{
			if (Globals.IsTest)
			{
				return false; // Default value for testing. Mock IDbUpgraderUserInteraction and update GlobalServiceProvider to alter the return values of these methods in tests.
			}

			var result = Globals.Message.Show(message, title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
			return result == DialogResult.Retry;
		}

		public bool PromptUserConfirmation(string title, string message, string[] detailLines)
		{
			if (Globals.IsTest)
			{
				return true; // Default value for testing. Mock IDbUpgraderUserInteraction and update GlobalServiceProvider to alter the return values of these methods in tests.
			}

			using (var form = new ConfirmationForm())
			{
				return form.ShowMessage(title, message, detailLines, MessageBoxButtons.YesNo, false);
			}
		}

		[Conditional("DEBUG")]
		static void EnsureNotInUnitTest(KeyValuePair<string, string>[] loggedInUsers)
		{
			if (Globals.IsTest)
			{
				throw new InvalidOperationException("You can't prompt for user input in a unit test. Mock IDbUpgraderUserInteraction and update GlobalServiceProvider to alter the return values of these methods in tests." + System.Environment.CommandLine +
													"Additional information for the Users of confirmation:" + System.Environment.CommandLine +
												   string.Join(System.Environment.CommandLine, loggedInUsers.Select(v => $"[{v.Key},{v.Value}]").ToArray()));
			}
		}
	}
}
