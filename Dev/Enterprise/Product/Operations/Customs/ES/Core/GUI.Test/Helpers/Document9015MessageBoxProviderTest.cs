using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class Document9015MessageBoxProviderTest : TestCaseWithFactory
	{
		public void TestAskIfShouldRemove9015Documents()
		{
			var entryNumber = "ES000001";
			var popupMessage = "For Entry " + entryNumber + " there is a document 9015 in Supporting Documents to request a 50% VAT guaranteed amount reduction, but this declaration doesn't match criteria for this request. Do you want to remove 9015 documents?";

			ZFormModaliser.ShowDialogsInTest = false;

			CombineAssertions(() =>
			{
				var provider = new Document9015MessageBoxProvider();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var result = provider.AskIfShouldRemove9015Documents(entryNumber);
				AssertEquals("When dialog answer is NO method returns false", false, result);
				AssertEquals("When calling provider method we get a new message in user notification", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = provider.AskIfShouldRemove9015Documents(entryNumber);
				AssertEquals("When dialog answer is YES method returns true", true, result);
				AssertEquals("When calling provider method we get a new message in user notification", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
			});
		}
	}
}
