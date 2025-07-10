using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI.Testing
{
	public class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestInterpretedMessageTextWebBrowser()
		{
			const string messageInterpretation = @"<html></head><body><div>Some text here</div></body></html>";

			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var message = header.Messages.AddNew();

			using var form = new ZForm(declaration);
			using var userControl = new CustomsEntriesAndEntryLinesUserControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			userControl.SetDataBinding(declaration, "");
			form.Show();

			var interpretedMessageTextBox = userControl.FindSingle<ZTextBox>("InterpretedMessageTextBox");
			var interpretedMessageTextWebBrowser = userControl.FindSingle<ZWebBrowser>("InterpretedMessageTextWebBrowser");

			message.EM_MessageInterpretation = messageInterpretation;
			while (interpretedMessageTextWebBrowser.ReadyState != WebBrowserReadyState.Complete)
			{
				Application.DoEvents();
			}
			UserIdleWorker.Flush();
			AssertEquals(messageInterpretation, interpretedMessageTextWebBrowser.DocumentText.Trim());
		}

		public void TestEnableFindDialog()
		{
			using var control = new MessagesTabUserControl();
			var messageTextTextBox = control.FindSingle<ZTextBox>("MessageTextTextBox");
			AssertEquals(false, messageTextTextBox.HideSelection);
			AssertEquals(true, messageTextTextBox.EnableFindDialog);
		}
	}
}
