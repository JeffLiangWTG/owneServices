using CargoWise.Common;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.WebLauncher;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	public class WebUrlValidationUserPrompterTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestNoUserConfirmationFormIfGuiIsNotAvailable()
		{
			// Arrange
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var prompter = new WebUrlValidationUserPrompter();

				// Act
				var result = prompter.GetUserConfirmation("file://1");

				// Assert
				AssertEquals(false, result);
			}
		}

		[UseSnapshotProtection]
		public void TestGetUserConfirmationIfOk()
		{
			// Arrange
			using (new DisposableAction(() => UnitTestUserNotification.Instance.ClearMessagesAndAnswers()))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				var prompter = new WebUrlValidationUserPrompter();

				// Act
				var result = prompter.GetUserConfirmation("file://1");

				// Assert
				AssertEquals(true, result);
			}
		}

		[UseSnapshotProtection]
		public void TestGetUserConfirmationIfCancel()
		{
			// Arrange
			using (new DisposableAction(() => UnitTestUserNotification.Instance.ClearMessagesAndAnswers()))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
				var prompter = new WebUrlValidationUserPrompter();

				// Act
				var result = prompter.GetUserConfirmation("file://1");

				// Assert
				AssertEquals(false, result);
			}
		}
	}
}
