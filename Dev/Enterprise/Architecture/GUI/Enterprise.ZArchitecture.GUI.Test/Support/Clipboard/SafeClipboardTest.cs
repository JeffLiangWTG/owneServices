using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1088:Do Not Use System.Windows.Forms.Clipboard", Justification = "Testing SafeClipboard")]
	sealed class SafeClipboardTest : TestCase
	{
		readonly Control control = new Form();

		[DeveloperOnlyTest]
		public void TestGetDataObject()
		{
			IDataObject dataObject = new DataObject("abcde");
			var action = new Action(() => SafeClipboard.SetDataObject(dataObject));
			action.Invoke();

			var copiedData = ClipboardTestHelper.RetryIfCopyOrCutFailed<IDataObject>(action);
			AssertEquals("abcde", copiedData.GetData(typeof(string)));
		}

		[DeveloperOnlyTest]
		[GuiTest]
		public void TestGetDataObjectFromDifferentThread()
		{
			IDataObject dataObject = new DataObject("abcde");
			var action = new Action(() => Clipboard.SetDataObject(dataObject));
			ClipboardTestHelper.RetryActionOnlyForNativeClipboard(action);

			IDataObject result = null;

			control.Show();

			var secondThread = new Thread(() => { result = SafeClipboard.GetDataObject(); });

			secondThread.Start();
			while (secondThread.ThreadState != System.Threading.ThreadState.Stopped)
			{
				Application.DoEvents(); //have to pump messages b/c Invoke uses the message pump to do its dirty work
			}

			control.Hide();

			AssertEquals("abcde", result.GetData(typeof(string)));
		}

		[DeveloperOnlyTest]
		public void TestSetDataObject()
		{
			IDataObject dataObject = new DataObject("abcde");
			Assert("SetDataObject should return true on success.", SafeClipboard.SetDataObject(dataObject));
			AssertEquals("abcde", Clipboard.GetDataObject().GetData(typeof(string)));
		}

		[DeveloperOnlyTest]
		public void TestGetText()
		{
			SafeClipboard.SetText("qwerty");
			AssertEquals("qwerty", SafeClipboard.GetText());
		}

		[DeveloperOnlyTest]
		public void TestSetText()
		{
			Assert(SafeClipboard.SetText("qwerty"));
			AssertEquals("qwerty", Clipboard.GetText());
		}

		[DeveloperOnlyTest]
		public void TestGetUnicodeText()
		{
			const string input = "😐";

			SafeClipboard.SetText(input);
			AssertEquals(input, SafeClipboard.GetText(TextDataFormat.UnicodeText));
		}

		[DeveloperOnlyTest]
		public void TestContainsText()
		{
			SafeClipboard.SetText("hello world");
			AssertEquals(true, SafeClipboard.ContainsText());

			SafeClipboard.Clear();

			AssertEquals(false, SafeClipboard.ContainsText());
		}

		[DeveloperOnlyTest]
		public void TestGetData()
		{
			object data = 123;
			Clipboard.SetData("XYZ", data);
			AssertEquals(123, SafeClipboard.GetData("XYZ"));
		}

		[DeveloperOnlyTest]
		public void TestSetData()
		{
			object data = 789;
			Assert(SafeClipboard.SetData("XYZ", data));
			AssertEquals(789, Clipboard.GetData("XYZ"));
		}

		public void TestClear()
		{
			SafeClipboard.SetText("abcde");
			SafeClipboard.Clear();

			var text = ClipboardTestHelper.RetryGetForNativeClipboard<string>();
			AssertEquals(string.Empty, text);
		}

		public void TestCopyNullData()
		{
			AssertEquals(false, SafeClipboard.SetDataObject(null));
		}

		#region TestRaceCondition

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestRaceConditionSafe()
		{
			using (new ClipboardViewer())
			{
				var secondThread = new Thread(() => Clipboard.SetText("abcd"));
				secondThread.SetApartmentState(ApartmentState.STA);
				secondThread.Start();
				var ok = SafeClipboard.SetText("qwer");
				secondThread.Join();
				var text = SafeClipboard.GetText();
				if (ok)
				{
					AssertEquals("qwer", text);
				}
				else
				{
					AssertEquals("abcd", text);
				}
			}
		}

		class ClipboardViewer : Form
		{
			public ClipboardViewer()
			{
				hWndreviousPrev = SetClipboardViewer(Handle);
			}

			protected override void Dispose(bool disposing)
			{
				ChangeClipboardChain(Handle, hWndreviousPrev);
				base.Dispose(disposing);
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == WM_DRAWCLIPBOARD)
				{
					Thread.Sleep(MillisecondsToSleep);
					if (hWndreviousPrev != IntPtr.Zero)
					{
						SendMessage(hWndreviousPrev, m.Msg, m.WParam, m.LParam);
					}
					return;
				}
				base.WndProc(ref m);
			}

			const int MillisecondsToSleep = 1000;
			readonly IntPtr hWndreviousPrev;
		}

		const int WM_DRAWCLIPBOARD = 0x308;

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		#endregion
	}
}
