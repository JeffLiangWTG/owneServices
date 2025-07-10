using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Testing
{
	public class StatementMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestMessageInterpretationWebBrowserShouldRefresh_WhenMessageInterpretationTextBoxChanges()
		{
			var statement = Factory.New<CusStatementHeader>();
			using (var form = new ZForm(statement))
			using (var control = new StatementMessagesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var messageInterpretationTextBox = control.FindSingle<ZTextBox>("MessageInterpretationTextBox");
				AssertEquals(true, messageInterpretationTextBox.Visible);

				messageInterpretationTextBox.Text = "<html><body><h1>Where are you from?</h1></body></html>";

				var messageInterpretationWebBrowser = control.FindSingle<ZWebBrowser>("MessageInterpretationWebBrowser");
				AssertEquals("MessageInterpretationWebBrowser should refresh the document when MessageInterpretationTextBox changes.", "<html><body><h1>Where are you from?</h1></body></html>", messageInterpretationWebBrowser.DocumentText);

				messageInterpretationTextBox.Text = "<html><body><h1>King's Landing.</h1></body></html>";
				AssertEquals("MessageInterpretationWebBrowser should refresh the document when MessageInterpretationTextBox changes.", "<html><body><h1>King's Landing.</h1></body></html>", messageInterpretationWebBrowser.DocumentText);
			}
		}
	}
}
