using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFormPasterTest : TestCaseWithDummy
	{
#if !WINZOR

		public void TestPasteToActiveControl()
		{
			using (var testForm = new KForm())
			{
				using (var ctrl = new UserControl())
				{
					var pastableCtrl = new TestPastableControl();
					ctrl.Controls.Add(pastableCtrl);
					testForm.Controls.Add(ctrl);

					testForm.ActiveControl = pastableCtrl;
					AssertEquals("Not pasted yet", false, pastableCtrl.WasPasted);
					ZFormPaster.PasteToActiveControl(testForm);
					AssertEquals("Paste should occur", true, pastableCtrl.WasPasted);

					pastableCtrl.WasPasted = false;
					testForm.ActiveControl = pastableCtrl.Controls[0];
					ZFormPaster.PasteToActiveControl(testForm);
					AssertEquals("Paste should occur even though the IPastable control is a parent", true, pastableCtrl.WasPasted);
				}
			}
		}
#endif
		#region Copying to Clipboard

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestCopyLinkToClipboard()
		{
			Form.Show();
			Form.ControllerID = DummyControllerIDs.Dummy;

			var menuItem = Form.ActionsMenuItem.MenuItems.FindByText("Copy Hyperlink to Clipboard");
			var action = new Action(() => menuItem.PerformClick());
			action.Invoke();

			var clipboardData = ClipboardTestHelper.RetryIfCopyOrCutFailed<object>(action, DataFormats.Text);
			AssertEquals("Text", Dummy.HumanReadableName, clipboardData);
			AssertContains("<html><body><!--StartFragment--><a href=\"" + ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK) + "\">" + Dummy.HumanReadableName + "</a><!--EndFragment--></body></html>", SafeClipboard.GetData(DataFormats.Html).ToString());

			var rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			AssertEquals("Rtf link", true, rtf.Contains(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK)));
			AssertEquals("Rtf caption", true, rtf.Contains(Dummy.HumanReadableName));
		}

#if !WINZOR
// Equivalent version of this test for Winzor is TestCreateDesktopShortcut in Dev\Winzor\Enterprise.Winzor.Architecture.Test\ZFormMenuStrategyTest.cs 
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public void TestCreateShortcutsOnDesktop()
		{
			Form.Show();
			Form.ControllerID = DummyControllerIDs.Dummy;

			var shortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), Dummy.HumanReadableName + " (2)" + ".url");
			if (File.Exists(shortcutFile))
			{
				File.Delete(shortcutFile);
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Form.ActionsMenuItem.MenuItems.FindByText("Create Desktop Shortcut").PerformClick();
			AssertEquals("A shortcut shouldn't be created unless the user clicks OK", false, File.Exists(shortcutFile));

			var existingShortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), Dummy.HumanReadableName + ".url");
			File.WriteAllText(existingShortcutFile, "");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			try
			{
				Form.ActionsMenuItem.MenuItems.FindByText("Create Desktop Shortcut").PerformClick();
				AssertEquals("A shortcut file should be created", true, File.Exists(shortcutFile));
				var codeBase = Assembly.GetEntryAssembly().Location;
				AssertEquals("Shortcut file content",
@"[InternetShortcut]
URL=" + ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK) + @"
IconIndex=0
IconFile=" + new Uri(codeBase).LocalPath + @"
", File.ReadAllText(shortcutFile));
			}
			finally
			{
				File.Delete(existingShortcutFile);
				File.Delete(shortcutFile);
			}
		}
#endif

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestCopyFormToClipboard()
		{
			Form.Show();
			Form.ActionsMenuItem.MenuItems.FindByText("Copy Form to Clipboard").PerformClick();
			AssertEquals("Bitmap copied to clipboard", true, SafeClipboard.GetData(DataFormats.Bitmap) != null);
		}

#endregion

		#region Implementation

		#region Test Classes

		class TestZForm : ZForm
		{
			public TestZForm(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}
		}

		#endregion

		TestZForm Form
		{
			get { return form ?? (form = new TestZForm(Dummy)); }
		}
		TestZForm form;

		protected override void TearDown()
		{
			base.TearDown();
			UserIdleWorker.Flush();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
