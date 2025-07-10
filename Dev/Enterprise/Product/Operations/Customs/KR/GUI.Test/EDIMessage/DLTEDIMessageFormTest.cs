using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DLTEDIMessageForm))]
	sealed class DLTEDIMessageFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = Factory.New<EDIMessage>();
			bizO.EM_ReceiveTransmit = EDIMessage.Status.Received;
			bizO.ClearHasChanges();
			return new DLTEDIMessageForm(bizO);
		}

		public void TestAddMessageContentsTab()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message.EM_MessageNum = "1";
			using (var form = new DLTEDIMessageForm(message))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals(4, tabControl.Controls.Count);
				var index = 0;
				AssertEquals(tabControl.Controls[index++].Name, "MainTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "MessageContentsTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "NotesTabPage");
				AssertEquals(tabControl.Controls[index++].Name, "LogsTabPage");
			}
		}
	}
}
