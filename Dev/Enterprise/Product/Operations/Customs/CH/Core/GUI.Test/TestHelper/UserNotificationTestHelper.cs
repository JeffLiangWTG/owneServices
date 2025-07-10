using Enterprise.ZArchitecture.Environment;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.GUI.Testing;

public static class UserNotificationTestHelper
{
	public static void AssertLastMessage(string message, string expectedCaption, string expectedText, bool expectedWasError = false)
	{
		var actualWasError = UnitTestUserNotification.Instance.LastMessage.WasError;
		var actualCaption = UnitTestUserNotification.Instance.LastMessage.Caption;
		var actualText = UnitTestUserNotification.Instance.LastMessage.Text;
		var assertionMessage = $" {message}\nActual={(actualWasError ? "ERROR|" : "")}{actualCaption}|{actualText}\nExpected={(expectedWasError ? "ERROR|" : "")}{expectedCaption}|{expectedText}";
		AssertEquals($"WasError:{assertionMessage}", expectedWasError, actualWasError);
		AssertEquals($"Caption:{assertionMessage}", expectedCaption, actualCaption);
		AssertEquals($"Text:{assertionMessage}", expectedText, actualText);
	}
}
