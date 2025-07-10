using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Test
{
	class EntryMessageUserControlTest : TestCaseWithFactory
	{
		public void TestNewMessageUserControl()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			using (var form = new ZForm(declaration))
			using (var control = new EntryMessageUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesTabControl = control.FindSingle<ZTemplateTabControl>("MessagesTabControl");
				var messagesTabPage = control.FindSingle<ZTabPage>("MessagesTabPage");
				messagesTabControl.SelectedTab = messagesTabPage;
				AssertNoExceptionThrown(() => messagesTabPage.FindSingle<EU.GUI.MessageUserControl>((x) => x is MessageUserControl));
			}

			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			using (var form = new ZForm(declaration))
			using (var control = new EntryMessageUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesTabControl = control.FindSingle<ZTemplateTabControl>("MessagesTabControl");
				var messagesTabPage = control.FindSingle<ZTabPage>("MessagesTabPage");
				messagesTabControl.SelectedTab = messagesTabPage;
				AssertNoExceptionThrown(() => messagesTabPage.FindSingle<EU.GUI.MessageUserControl>((x) => x is ExportMessageUserControl));
			}
		}
	}
}
