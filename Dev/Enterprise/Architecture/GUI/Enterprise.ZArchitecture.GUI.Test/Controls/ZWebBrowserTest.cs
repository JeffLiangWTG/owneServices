using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZWebBrowserTest : ZControlBaseTestCase<ZWebBrowser>
	{
#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
		public void TestDefaultContextMenuStripDoesntRestrictUs()
		{
			var defaultMenuStrip = WebBrowser.ContextMenuStrip;
			AssertEquals(2, WebBrowser.ContextMenuStrip.Items.Count);
			defaultMenuStrip.Items.Add(new ToolStripMenuItem());
			AssertEquals(3, WebBrowser.ContextMenuStrip.Items.Count);

			var customMenuStrip = new ContextMenuStrip();
			WebBrowser.ContextMenuStrip = customMenuStrip;
			AssertEquals(0, WebBrowser.ContextMenuStrip.Items.Count);

			customMenuStrip.Items.Add(new ToolStripMenuItem());
			AssertEquals(1, WebBrowser.ContextMenuStrip.Items.Count);

			WebBrowser.ResetContextMenuStripToDefault();
			AssertEquals(3, WebBrowser.ContextMenuStrip.Items.Count);
		}

		public void TestSelectAllShortcut()
		{
			Form.Show();

			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.G));
			AssertEquals("Select All should not have been called", false, WebBrowser.SelectAllCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.R));
			AssertEquals("Select All should not have been called", false, WebBrowser.SelectAllCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.T));
			AssertEquals("Select All should not have been called", false, WebBrowser.SelectAllCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.A));
			AssertEquals("Select All should have been called", true, WebBrowser.SelectAllCalled);
		}

		public void TestUpDownKeysShouldBeInputKey()
		{
			Form.Show();

			using (ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isRemoteAppSession: true)))
			{
				var previewKeyDownEventArgs = new PreviewKeyDownEventArgs(Keys.Up);
				WebBrowser.OnPreviewKeyDown(previewKeyDownEventArgs);
				Assert(previewKeyDownEventArgs.IsInputKey);

				previewKeyDownEventArgs = new PreviewKeyDownEventArgs(Keys.Down);
				WebBrowser.OnPreviewKeyDown(previewKeyDownEventArgs);
				Assert(previewKeyDownEventArgs.IsInputKey);
			}

			using (ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isRemoteAppSession: false)))
			{
				var previewKeyDownEventArgs = new PreviewKeyDownEventArgs(Keys.Up);
				WebBrowser.OnPreviewKeyDown(previewKeyDownEventArgs);
				Assert(!previewKeyDownEventArgs.IsInputKey);

				previewKeyDownEventArgs = new PreviewKeyDownEventArgs(Keys.Down);
				WebBrowser.OnPreviewKeyDown(previewKeyDownEventArgs);
				Assert(!previewKeyDownEventArgs.IsInputKey);
			}
		}

		public void TestCopyShortcut()
		{
			Form.Show();

			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.G));
			AssertEquals("Copy should not have been called", false, WebBrowser.CopyToClipboardCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.R));
			AssertEquals("Copy should not have been called", false, WebBrowser.CopyToClipboardCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.T));
			AssertEquals("Copy should not have been called", false, WebBrowser.CopyToClipboardCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.C));
			AssertEquals("Copy should have been called", true, WebBrowser.CopyToClipboardCalled);

			WebBrowser.CopyToClipboardCalled = false;
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Shift | Keys.Insert));
			AssertEquals("Copy should not have been called", false, WebBrowser.CopyToClipboardCalled);
			WebBrowser.OnPreviewKeyDown(new PreviewKeyDownEventArgs(Keys.Control | Keys.Insert));
			AssertEquals("Copy should have been called", true, WebBrowser.CopyToClipboardCalled);
		}

		public void TestPreProcessMessage_NoParentForm()
		{
			using (var panel = new KPanelWithHotkeyTesting())
			{
				panel.Hotkeys.RegisterHotKey(Keys.F1, PressedF1);
				var webBrowser = new TestWebBrowser();
				panel.Controls.Add(webBrowser);

				KeySender.PostKeyDown(webBrowser, webBrowser.Handle, Keys.F1);
				Application.DoEvents();

				AssertEquals("F1 was pressed.", Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestStandardShortcuts()
		{
			Form.GlobalHotkeys_Exposed.RegisterHotKey(Keys.F1, PressedF1);
			Form.Show();

			WebBrowser.Focus();

			KeySender.PostKeyDown(WebBrowser, WebBrowser.Handle, Keys.F1);
			Application.DoEvents();

			AssertEquals("F1 was pressed.", Environment.UnitTestUserNotification.Instance.LastMessage.Text);
		}

		bool PressedF1(object sender, Keys keyPressed)
		{
			Environment.Globals.Message.Show("F1 was pressed.");
			return true;
		}

		public void TestDispose()
		{
			TestWebBrowser browser;
			using (browser = new TestWebBrowser())
			{
				browser.Url = new Uri("http://www.google.com");
			}
			AssertEquals("Url property must be set to null on dispose to prevent an AccessViolationException", null, browser.LastUrl);
		}

		public void TestIWebNavigate()
		{
			AssertEquals(true, typeof(IWebNavigate).IsAssignableFrom(typeof(ZWebBrowser)));
		}

		public void TestNewWindowShouldNotPop()
		{
			using (var from = new ZForm())
			using (var browser = new TestWebBrowser())
			{
				form.Controls.Add(browser);

				browser.DocumentText =
@"<html>
    <body>
        <a id='link' href='#tag' target='_blank'>Link</a>
        <a id='tag'>tag</a>
	</body>
</html>
";
				Application.DoEvents();

				browser.NewWindowCalled = false;
				browser.NewWindowCancelEventArgs = false;
				browser.Navigate("#tag", newWindow: true);

				Assert("new window Evnet should be called", browser.NewWindowCalled);
				Assert("new window EventArgs should be cancel", browser.NewWindowCancelEventArgs);

				browser.NewWindowCalled = false;
				browser.NewWindowCancelEventArgs = false;

				Application.DoEvents();
				var link = browser.Document.All["link"];
				link.InvokeMember("click");

				Assert("new window Evnet should be called", browser.NewWindowCalled);
				Assert("new window EventArgs should be cancel", browser.NewWindowCancelEventArgs);
			}
		}

		#region Test Classes

		class TestWebBrowser : ZWebBrowser
		{
			public new void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
			{
				base.OnPreviewKeyDown(e);
			}

			protected override void CopyToClipboardCore()
			{
				CopyToClipboardCalled = true;
			}

			public bool CopyToClipboardCalled { get; set; }

			protected override void SelectAllCore()
			{
				SelectAllCalled = true;
			}

			public bool SelectAllCalled { get; private set; }

			protected override void OnNewWindow(CancelEventArgs e)
			{
				base.OnNewWindow(e);
				NewWindowCalled = true;
				NewWindowCancelEventArgs = e.Cancel;
			}

			public bool NewWindowCalled { get; set; }
			public bool NewWindowCancelEventArgs { get; set; }

			public override Uri Url
			{
				get { return base.Url; }
				set
				{
					base.Url = value;
					LastUrl = value;
				}
			}

			public Uri LastUrl { get; private set; }
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			WebBrowser.WebBrowserShortcutsEnabled = false;
			WebBrowser.Dock = DockStyle.Fill;
			Form.Controls.Add(WebBrowser);

			WebBrowser.DocumentText = exampleMultilineText;
		}

		#region Example Text Strings
		const string exampleMultilineText = @"<html><body>Blah blah blah<br />
this is some<br />
multiline <strong>text</strong><br />
that we are <i>expecting</i><br />
to be selected</body></html>";
		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		class TestFormWithHotkeyTesting : KForm
		{
			internal HotkeyRegister GlobalHotkeys_Exposed => base.GlobalHotkeys;
		}

		class KPanelWithHotkeyTesting : KPanel
		{
			internal HotkeyRegister Hotkeys { get; } = new HotkeyRegister();

			protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
			{
				return base.ProcessCmdKey(ref msg, keyData) || Hotkeys.ProcessCmdKey(this, keyData);
			}
		}

		TestFormWithHotkeyTesting Form
		{ get { return form ?? (form = new TestFormWithHotkeyTesting()); } }
		TestFormWithHotkeyTesting form;

		TestWebBrowser WebBrowser
		{ get { return webBrowser ?? (webBrowser = new TestWebBrowser()); } }
		TestWebBrowser webBrowser;

		#endregion
#endif
	}
}
