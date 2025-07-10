using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Testing
{
	class RestoreDbConsoleTests : TestCase
	{
		public void TestConsoleConfirmationPromptAutoResponse()
		{
			var restoreDbConsole = new RestoreDbConsole();

			var auditPromptYes = new ConfirmationPromptArgs
			{
				PromptType = PromptType.ConfirmAuditDBExcluded
			};

			var auditPromptNo = new ConfirmationPromptArgs
			{
				PromptType = PromptType.ConfirmAuditDBExcluded
			};

			var noBackupPromptYes = new ConfirmationPromptArgs
			{
				PromptType = PromptType.ConfirmNoBackupOfTestDb
			};

			Defaults.ResetDefaultInstance();
			restoreDbConsole.DbTools_OnConfirmationPrompt(auditPromptNo);
			Defaults.Instance.IsAuditDBExcludedFromRestore = true;
			restoreDbConsole.DbTools_OnConfirmationPrompt(auditPromptYes);
			Defaults.ResetDefaultInstance();
			restoreDbConsole.DbTools_OnConfirmationPrompt(noBackupPromptYes);

			CombineAssertions(() =>
			{
				Assert(auditPromptNo.Result == ConfirmationPromptResult.No);
				Assert(auditPromptYes.Result == ConfirmationPromptResult.Yes);
				Assert(noBackupPromptYes.Result == ConfirmationPromptResult.Default);
			});
		}
	}
}
