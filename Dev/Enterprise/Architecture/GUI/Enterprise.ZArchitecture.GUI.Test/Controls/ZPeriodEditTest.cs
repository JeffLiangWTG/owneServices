using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZPeriodEditTest : TestCaseWithDummy
	{
		[TestDate(2006, 4, 5)]
		public void TestPeriodEditBinding()
		{
			Dummy.Z0_Number = 200304;

			using (var testForm = new ZPeriodEditTestForm(Dummy))
			{
				testForm.Show();
				AssertEquals("MaxLength", 6, testForm.PeriodEdit.MaxLength);
				AssertEquals("Bound Value", "200304", testForm.PeriodEdit.Text);

				AssertSettingTextSetsDummy("0105", 200105, testForm);
				AssertSettingTextSetsDummy("TEST", 0, testForm); // non-digit values can only get into the Text in special circumstances, test these for extra robustness - should be handled.
				AssertSettingTextSetsDummy("200605", 200605, testForm);
				AssertSettingTextSetsDummy("TESTEX", 0, testForm);
				AssertSettingTextSetsDummy("9905", 199905, testForm);
				AssertSettingTextSetsDummy("2006XX", 0, testForm);
				AssertSettingTextSetsDummy("209905", 209905, testForm);
				AssertSettingTextSetsDummy("XX0005", 0, testForm);
				AssertSettingTextSetsDummy("199905", 199905, testForm);
				AssertSettingTextSetsDummy("20XX05", 0, testForm);
				AssertSettingTextSetsDummy("210605", 210605, testForm);
				AssertSettingTextSetsDummy("2006050", 0, testForm);
				AssertSettingTextSetsDummy("999912", 999912, testForm);
				AssertSettingTextSetsDummy("", 0, testForm);
				AssertSettingTextSetsDummy("000101", 101, testForm);
				AssertSettingTextSetsDummy("200613", 200613, testForm);
				AssertSettingTextSetsDummy("613", 0, testForm);
				AssertSettingTextSetsDummy("200699", 200699, testForm);
				AssertSettingTextSetsDummy("20613", 0, testForm);
				AssertSettingTextSetsDummy(" 20613", 0, testForm);
				AssertSettingTextSetsDummy("20613 ", 0, testForm);
				AssertSettingTextSetsDummy(" 613", 0, testForm);
				AssertSettingTextSetsDummy("613 ", 0, testForm);
				AssertSettingTextSetsDummy("206 13", 0, testForm);
				AssertSettingTextSetsDummy("206-13", 0, testForm);
				AssertSettingTextSetsDummy("-20613", 0, testForm);
				AssertSettingTextSetsDummy("-613", 0, testForm);
			}
		}

		void AssertSettingTextSetsDummy(string text, int expectedParsedValue, ZPeriodEditTestForm testForm)
		{
			testForm.PeriodEdit.Focus();
			testForm.PeriodEdit.Text = text;
			testForm.Button.Focus();
			AssertEquals("Z0_Number Value", expectedParsedValue, Dummy.Z0_Number);

			testForm.PeriodEdit.Focus();
			testForm.PeriodEdit.Text = "200601"; // revert to a known working state
			testForm.Button.Focus();
			AssertEquals("Z0_Number Value", 200601, Dummy.Z0_Number);
		}

#if !WINZOR

		[DeveloperOnlyTest]
		public void TestPaste()
		{
			using (var testPeriodEdit = new ZPeriodEdit())
			using (var control = new Control())
			{
				testPeriodEdit.Text = "";

				SafeClipboard.SetDataObject("ABC");
				System.Threading.Thread.Sleep(50); // we need to sleep to allow the clipboard to catch up.
				Assert("Can Paste should now be false", !((IPastableControl)testPeriodEdit).TryPaste());
				AssertEquals("Paste should do nothing when not a valid number", "", testPeriodEdit.Text);

				SafeClipboard.SetDataObject(control);
				System.Threading.Thread.Sleep(50);
				Assert("Can Paste should now be false", !((IPastableControl)testPeriodEdit).TryPaste());
				AssertEquals("Paste should do nothing when not a string", "", testPeriodEdit.Text);

				var action = new Action(() => SafeClipboard.SetDataObject("123"));
				action.Invoke();
				ClipboardTestHelper.RetryIfCopyOrCutFailed<IDataObject>(action);
				Assert("Can Paste should now be true", ((IPastableControl)testPeriodEdit).TryPaste());
				AssertEquals("Should paste valid number", "123", testPeriodEdit.Text);

				SafeClipboard.SetDataObject("1.2");
				System.Threading.Thread.Sleep(50);
				Assert("Can Paste should now return false", !((IPastableControl)testPeriodEdit).TryPaste());
				AssertEquals("Should revert to original value", "123", testPeriodEdit.Text);

				action = () => SafeClipboard.SetDataObject("0");
				action.Invoke();
				ClipboardTestHelper.RetryIfCopyOrCutFailed<IDataObject>(action);
				Assert("Can Paste should now be true", ((IPastableControl)testPeriodEdit).TryPaste());
				AssertEquals("Should paste valid number", "0", testPeriodEdit.Text);
			}
		}

#endif
	}
}
