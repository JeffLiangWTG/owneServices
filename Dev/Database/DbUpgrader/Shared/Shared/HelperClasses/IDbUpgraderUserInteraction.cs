using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Shared
{
	public interface IDbUpgraderUserInteraction
	{
		bool PromptConfirmDisconnectUsers(KeyValuePair<string, string>[] loggedInUsers);
		bool PromptUserConfirmation(string title, string message, string[] detailLines);
		bool PromptRetryAction(string title, string message);
	}
}
