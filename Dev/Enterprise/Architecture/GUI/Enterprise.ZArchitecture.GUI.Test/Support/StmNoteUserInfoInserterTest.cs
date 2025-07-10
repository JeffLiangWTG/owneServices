using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Core.Forms
{
	sealed class StmNoteUserInfoInserterTest : TransactionedTestCase
	{
		public void TestF5HotkeyIntoZTextBox_DisplayGMTOffsetOnF5UserInfoIsFalse()
		{
			using (var testForm = new ZForm())
			using (RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testZTextBox = new ZTextBox { CharacterCasing = CharacterCasing.Normal };
				testForm.Controls.Add(testZTextBox);
				testForm.Show();

				StmNoteUserInfoInserter.RegisterHotkeys(testZTextBox);
				AssertEquals("Initial Text", "", testZTextBox.Text);
				AssertEquals(false, RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.Value);

				WaitForSafeTime();
				KeySender.PostKeyDown(testZTextBox, Keys.F5);
				Application.DoEvents();

				AssertEquals("Text After F5", EnvProxy.Instance.CurrentUser.InitialsAndDateTime, testZTextBox.Text);
			}
		}

#if !WINZOR
//Equivalent unit test in WINZOR: ZRichTextBoxSupportF5InsertTimestampWhenDisplayGMTOffsetOnF5UserInfoIsFalse
		public void TestF5HotkeyIntoRichTextBox_DisplayGMTOffsetOnF5UserInfoIsFalse()
		{
			using (var testForm = new ZForm())
			using (RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testORichTextBox = new ZRichTextBox();
				testForm.Controls.Add(testORichTextBox);
				testForm.Show();
				Application.DoEvents();

				AssertEquals("Initial Text", "", testORichTextBox.Text);
				AssertEquals(false, RawDataRegistry.Instance.DisplayGMTOffsetOnF5UserInfo.Value);

				WaitForSafeTime();
				KeySender.PostKeyDown(testORichTextBox.RichEdit, Keys.F5);
				Application.DoEvents();

				AssertEquals("Text After F5", EnvProxy.Instance.CurrentUser.InitialsAndDateTime, testORichTextBox.RichEdit.Text);
			}
		}
#endif

		public void TestStmNoteUserInfoInserter_F5HotkeyInsertsInitialsAndDateTimeZTextbox()
		{
			using (var testForm = new ZForm())
			{
				var testZTextBox = new ZTextBox { CharacterCasing = CharacterCasing.Normal };
				testForm.Controls.Add(testZTextBox);
				testForm.Show();

				StmNoteUserInfoInserter.RegisterHotkeys(testZTextBox);
				AssertEquals("Initial Text", "", testZTextBox.Text);

				WaitForSafeTime();
				KeySender.PostKeyDown(testZTextBox, Keys.F5);
				Application.DoEvents();

				AssertEquals("Text After F5", EnvProxy.Instance.CurrentUser.InitialsAndDateTimeGmt, testZTextBox.Text);
			}
		}

#if !WINZOR
//Equivalent unit test in WINZOR: ZRichTextBoxSupportF5InsertTimestamp
		public void TestStmNoteUserInfoInserter_F5HotkeyInsertsIntoRichTextBox()
		{
			using (var testForm = new ZForm())
			{
				var testORichTextBox = new ZRichTextBox();
				testForm.Controls.Add(testORichTextBox);
				testForm.Show();
				Application.DoEvents();

				KeySender.PostKeyDown(testORichTextBox.RichEdit, Keys.F5);
				Application.DoEvents();

				AssertEquals("Text After F5", EnvProxy.Instance.CurrentUser.InitialsAndDateTimeGmt, testORichTextBox.RichEdit.Text);
			}
		}
#endif

		public void TestGetHotKeyUserTextWithCurrentLocalDateTimeIncludingGmt()
		{
			AssertEquals(EnvProxy.Instance.CurrentUser.InitialsAndDateTimeGmt, StaticCurrentFetcher.Instance.CurrentUser.GS_Code + ' ' + EnvProxy.Instance.Time.CurrentLocalDateTimeIncludingGmt + ": ");
		}

		#region Implementation

		void WaitForSafeTime()
		{
			while (EnvProxy.Instance.Time.CurrentLocalDateTime.Second >= 58)
			{
			}
		}

		#endregion
	}
}
