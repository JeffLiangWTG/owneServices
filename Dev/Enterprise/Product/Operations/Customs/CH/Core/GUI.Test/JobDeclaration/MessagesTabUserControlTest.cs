using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public class MessagesTabUserControlTest : TestCaseWithFactory
{
	public void TestInterpretedMessageTextIsOfTypeZWebBrowser()
	{
		var testDec = Factory.New<JobDeclaration>();

		using (var form = new ZForm(testDec))
		using (var messageUserControl = new MessageUserControl())
		{
			messageUserControl.Dock = DockStyle.Fill;
			form.Controls.Add(messageUserControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var messagesTabUserControl =
				(MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
			ZWebBrowser interpretedMessageTextControl = null;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("InterpretedMessageTextWebBrowser can be casted as a ZWebBrowser", () =>
				{
					interpretedMessageTextControl =
						messagesTabUserControl.Controls.Find("InterpretedMessageTextWebBrowser", true)
							.SingleOrDefault() as ZWebBrowser;
				});

				AssertNotNull("InterpretedMessageTextWebBrowser was found and it is not null",
					interpretedMessageTextControl);
			});
		}
	}
}
