using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.FormStrategies
{
	public class ZFormStatusBarStrategyTest : TestCaseWithDummy
	{
		public void TestIResourceStringParentControl()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Show();
				IDialogKeyDown parentControl = testForm;
				var updateStatusBar = (IUpdateStatusBar)parentControl;

				updateStatusBar.UpdateStatusBar("desc", null);
				AssertEquals("desc", testForm.MessageStatusBarPanel.Text);
				AssertEquals(OFont.GetFont(), testForm.MainStatusBar.Font);
				AssertEquals(ZFormStatusBarStrategy.DescriptionBrush, ZFormStatusBarStrategy.GetStatusBarTextBrush(testForm));

				updateStatusBar.UpdateStatusBar("err", CargoWise.ComponentModel.NotificationType.Error);
				AssertEquals("err", testForm.MessageStatusBarPanel.Text);
				AssertEquals(OFont.GetFontBold(), testForm.MainStatusBar.Font);
				AssertEquals(ZFormStatusBarStrategy.ErrorBrush, ZFormStatusBarStrategy.GetStatusBarTextBrush(testForm));

				updateStatusBar.UpdateStatusBar("warn", CargoWise.ComponentModel.NotificationType.Warning);
				AssertEquals("warn", testForm.MessageStatusBarPanel.Text);
				AssertEquals(OFont.GetFontBold(), testForm.MainStatusBar.Font);
				AssertEquals(ZFormStatusBarStrategy.WarningBrush, ZFormStatusBarStrategy.GetStatusBarTextBrush(testForm));

				updateStatusBar.UpdateStatusBar("me", CargoWise.EntityFramework.NotificationType.MessageError);
				AssertEquals("me", testForm.MessageStatusBarPanel.Text);
				AssertEquals(OFont.GetFontBold(), testForm.MainStatusBar.Font);
				AssertEquals(ZFormStatusBarStrategy.MessageErrorBrush, ZFormStatusBarStrategy.GetStatusBarTextBrush(testForm));
			}
		}

		#region Implementation

		#endregion
	}
}
