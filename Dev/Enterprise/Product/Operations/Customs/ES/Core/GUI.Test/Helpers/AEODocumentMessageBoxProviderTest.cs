using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class AEODocumentMessageBoxProviderTest : TestCaseWithFactory
	{
		public void TestAskIfShouldRemoveAEODocument()
		{
			var docTypeForMessage = "Document description";
			var popupMessage = "Do you want to remove '" + docTypeForMessage + "' documents from Inv.Headers?";

			ZFormModaliser.ShowDialogsInTest = false;

			CombineAssertions(() =>
			{
				var provider = new AEODocumentMessageBoxProvider();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var result = provider.AskIfShouldRemoveAEODocument(docTypeForMessage);
				AssertEquals("When dialog answer is NO method returns false", false, result);
				AssertEquals("When calling provider method we get a new message in user notification", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = provider.AskIfShouldRemoveAEODocument(docTypeForMessage);
				AssertEquals("When dialog answer is YES method returns true", true, result);
				AssertEquals("When calling provider method we get a new message in user notification", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(popupMessage));
			});
		}
	}
}
