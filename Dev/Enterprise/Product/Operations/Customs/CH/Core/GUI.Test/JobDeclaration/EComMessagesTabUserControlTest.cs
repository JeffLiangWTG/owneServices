using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public class EComMessagesTabUserControlTest : TestCaseWithFactory
{
	public void TestColumns()
	{
		using (var form = new ZForm())
		using (var control = new EComMessagesTabUserControl())
		{
			form.Controls.Add(control);
			form.Show();
			var messagesGrid = (MessageZGrid)control.Controls.Find("MessagesGrid", true).First();
			var visibleColumns = messagesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible).ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(visibleColumns, "EM_MessageSubTypeDescription", 0, caption: "Sub Type Description");
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(visibleColumns, "EM_ReceiveTransmit", 1, "Direction");
				UserControlTestHelper.AssertColumnStyles<ZDateEditColumnStyleInfo>(visibleColumns, "EM_MessageDateTime", 2, "Message Time");
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(visibleColumns, "EM_User", 3, "Sender");
				UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(visibleColumns, "EM_Status", 4, "Status");
				Assert("ReadOnly", messagesGrid.ReadOnly);
				AssertEquals("Count", 5, visibleColumns.Length);
			});
		}
	}

	public void TestInterpretedMessageTextIsOfTypeZWebBrowser()
	{
		var testDec = Factory.New<JobDeclaration>();
		testDec.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		using (var form = new ZForm(testDec))
		using (var messageUserControl = new MessageUserControl())
		{
			messageUserControl.Dock = DockStyle.Fill;
			form.Controls.Add(messageUserControl);
			form.SetDataBinding(testDec, ".");
			form.Show();

			var ecomMessagesTabUserControl = messageUserControl.Controls.Find("EComMessageUserControl", true).SingleOrDefault() as EComMessagesTabUserControl;
			ZWebBrowser interpretedMessageTextControl = null;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("InterpretedMessageTextWebBrowser can be casted as a ZWebBrowser", () =>
				{
					interpretedMessageTextControl = ecomMessagesTabUserControl.Controls.Find("EComInterpretedMessageTextWebBrowser", true).SingleOrDefault() as ZWebBrowser;
				});

				AssertNotNull("InterpretedMessageTextWebBrowser was found and it is not null", interpretedMessageTextControl);
			});
		}
	}
}
