using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class EntryMessageUserControlTest : TestCaseWithFactory
	{
		public void TestNewMessageUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryMessageUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				var messagesTabControl = control.FindSingle<ZTemplateTabControl>("MessagesTabControl");
				var messagesTabPage = control.FindSingle<ZTabPage>("MessagesTabPage");
				messagesTabControl.SelectedTab = messagesTabPage;
				AssertNoExceptionThrown(() => messagesTabPage.FindSingle<EU.GUI.MessageUserControl>((x) => x is ImportMessageUserControl));
			}

			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryMessageUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				var messagesTabControl = control.FindSingle<ZTemplateTabControl>("MessagesTabControl");
				var messagesTabPage = control.FindSingle<ZTabPage>("MessagesTabPage");
				messagesTabControl.SelectedTab = messagesTabPage;
				AssertNoExceptionThrown(() => messagesTabPage.FindSingle<EU.GUI.MessageUserControl>((x) => x is ExportMessageUserControl));
			}
		}
	}
}
