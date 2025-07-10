using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestSetupMessageColumns()
		{
			var declaration = Factory.New<JobDeclaration>();

			using var form = new ZForm(declaration);
			using var userControl = new MessageUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var messagesGrid = (ZGrid)userControl.Controls.Find("MessagesGrid", true).First();

			CombineAssertions(() =>
			{
				AssertNotNull("User control should have EM_IsTestMessage column", messagesGrid.Columns[EDIMessage.Schema.EM_IsTestMessage]);
				AssertNotNull("Context menu to save message to disk", messagesGrid.ContextMenu.MenuItems.FindByText("Save Message to Disk"));
			});
		}

		public void TestMessageTextTextBox()
		{
			var declaration = Factory.New<JobDeclaration>();

			using var form = new ZForm(declaration);
			using var userControl = new MessageUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var textBox = userControl.FindSingle<ZTextBox>("MessageTextTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("BindTo", nameof(EDIMessage.EM_MessageTextIndentedXml), textBox.BindTo);
				AssertEquals("HideSelection", false, textBox.HideSelection);
				AssertEquals("EnableFindDialog", true, textBox.EnableFindDialog);
			});
		}

		public void TestInterpretedMessageTextWebBrowser()
		{
			var declaration = Factory.New<JobDeclaration>();

			using var form = new ZForm(declaration);
			using var userControl = new MessageUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(declaration, ".");
			form.Show();

			var interpretedMessageTextWebBrowser = userControl.FindSingle<ZWebBrowser>("InterpretedMessageTextWebBrowser");

			AssertEquals("IsWebBrowserContextMenuEnabled should be false", false, interpretedMessageTextWebBrowser.WebBrowserShortcutsEnabled);
		}

		[RequiresSTA]
		public void TestMessageDetailsTabPageCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			using var form = new ZForm(declaration);
			using var userControl = new MessageUserControl();
			form.Controls.Add(userControl);
			form.Show();

			AssertEquals("Interpretation", userControl.FindSingle<ZTabPage>("MessageDetailsTabPage").CaptionResourceString.Caption);
		}
	}
}
