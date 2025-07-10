using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(GLAccountSelectionForm))]
	public class GLAccountSelectionFormTest : ZFormBasherTest
	{
		readonly List<AccGLHeader> GLHeaderList = new List<AccGLHeader>();
		AccGLHeaderCollection Collection;

		protected override Form GetFormToBashCore()
		{
			Collection = new AccGLHeaderCollection(Factory);
			Collection.Load();
			return new GLAccountSelectionForm(Collection, GLHeaderList);
		}

		public void TestControls()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var messageLabel = form.Controls.Find("MessageLabel", false);
				var buttonPanel = form.Controls.Find("ButtonPanel", false);
				var filterControlPanel = form.Controls.Find("FilterControlPanel", false);

				AssertEquals(1, messageLabel.Length);
				AssertEquals(1, buttonPanel.Length);
				AssertEquals(1, filterControlPanel.Length);

				var panelOkCancelButtons = buttonPanel[0].Controls.Find("panelOkCancelButtons", false);
				AssertEquals(1, panelOkCancelButtons.Length);
				AssertEquals(1, panelOkCancelButtons[0].Controls.Find("OK_Button", false).Length);
				AssertEquals(1, panelOkCancelButtons[0].Controls.Find("Cancel_Button", false).Length);
				AssertEquals(1, filterControlPanel[0].Controls.Find("Grid", false).Length);
			}
		}

		public void TestOKButton()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var okButton = (ZButton)form.Controls.Find("OK_Button", true)[0];
				okButton.PerformClick();
				AssertEquals("Please select a Parent Account from the list", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!GLHeaderList.Any());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var grid = (ZGrid)form.Controls.Find("Grid", true)[0];
				grid.Select(0);
				okButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(DialogResult.OK, form.DialogResult);
				Assert(GLHeaderList.Count == 1);
				Assert(GLHeaderList[0].PK == Collection.First().PK);
			}
		}

		public void TestCancelButton()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var cancelButton = (ZButton)form.Controls.Find("Cancel_Button", true)[0];
				cancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}
	}
}
