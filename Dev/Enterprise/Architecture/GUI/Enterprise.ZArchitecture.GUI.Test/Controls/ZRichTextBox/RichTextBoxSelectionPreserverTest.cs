using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	sealed class RichTextBoxSelectionPreserverTest : TestCaseWithFactory
	{
		public void TestPreserveSelection()
		{
			using (var form = new ZForm())
			using (var richEdit = new RichTextBox())
			{
				form.Controls.Add(richEdit);
				form.Show();
				Application.DoEvents();
#if !WINZOR
				richEdit.Rtf = ORtfTextUtil.TextToRtf("123456789");
#else
				richEdit.Html = ORtfTextUtil.RtfToHtml("123456789");
#endif
				richEdit.SelectionStart = 1;
				richEdit.SelectionLength = 2;

				using (new RichTextBoxSelectionPreserver(richEdit))
				{
					richEdit.SelectionStart = 3;
					richEdit.SelectionLength = 4;
				}
				AssertEquals("SelectionStart should be restored", 1, richEdit.SelectionStart);
				AssertEquals("SelectionLength should be restored", 2, richEdit.SelectionLength);
			}
		}
	}
}
