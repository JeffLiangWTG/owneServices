using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(XmlJournalImportController))]
	public class XmlJournalImportControllerTest : ZSingletonControllerBasherTest
	{
		public void TestInterfaceConnectorIsNotRequiredForImport()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var controller = new XmlJournalImportController();
			using (var form = controller.ShowNewForm())
			{
				AssertEquals("should not have shown an error message about interfaceconnector", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.XmlJournalImport;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
		}
	}
}
