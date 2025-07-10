using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(HotKeyMonitor))]
	sealed class HotKeyMonitorTest : TestCase
	{
		static bool DummyProcessor(object sender, Keys key)
		{
			return true;
		}

		static bool DummyExceptionProcessor(object sender, Keys key)
		{
			throw new NotImplementedException("This is a test exception.");
		}

		string CombineLogs(SortedDictionary<string, string> spans)
		{
			var builder = new StringBuilder();
			foreach (var kvp in spans)
			{
				builder.AppendLine(kvp.Value);
			}
			return builder.ToString();
		}

		public void TestCreateObject()
		{
			var monitor1 = ObjectFactory.Get<IHotKeyMonitor>();
			var monitor2 = ObjectFactory.Get<IHotKeyMonitor>();
			AssertEquals("Object reference is the same", true, ReferenceEquals(monitor1, monitor2));
		}

		public void TestDisabledDefault()
		{
			var monitor = ObjectFactory.Get<IHotKeyMonitor>();
			AssertEquals(false, monitor.Enabled);
		}

		[TestDateIncremental(milliseconds: 5)]
		public void TestSerialKeyPressEvents()
		{
			var hotKeyRegister = new HotkeyRegister();
			hotKeyRegister.RegisterHotKey(Keys.A, DummyProcessor, "The A key");
			hotKeyRegister.RegisterHotKey(Keys.Control | Keys.A, DummyProcessor, "The Ctrl and A key");
			hotKeyRegister.RegisterHotKey(Keys.Control | Keys.Shift | Keys.A, DummyProcessor, "The Ctrl, Shift and A key");
			hotKeyRegister.RegisterHotKey(Keys.Control | Keys.F1, DummyExceptionProcessor, "Will throw exception");

			var hotKeyMonitor = HotKeyMonitorProvider.GetHotKeyMonitor();
			hotKeyMonitor.Enabled = true;

			var form1 = new Form();
			form1.Name = "Form 1";
			hotKeyRegister.ProcessCmdKey(form1, Keys.B);
			hotKeyRegister.ProcessCmdKey(form1, Keys.A);
			hotKeyRegister.ProcessCmdKey(form1, Keys.Control | Keys.A);
			hotKeyRegister.ProcessCmdKey(form1, Keys.F2);
			AssertExceptionThrown("Should throw exception.", typeof(NotImplementedException), "This is a test exception.", () => hotKeyRegister.ProcessCmdKey(form1, Keys.Control | Keys.F1), assertStartsWith: true);
			hotKeyRegister.ProcessCmdKey(form1, Keys.Control | Keys.Shift | Keys.A);

			var form2 = new Form();
			form2.Name = "Form 2";
			hotKeyRegister.ProcessCmdKey(form2, Keys.L);
			hotKeyRegister.ProcessCmdKey(form2, Keys.J);

			var allSpans = hotKeyMonitor.GetAllSpans();
			var filteredSpans = hotKeyMonitor.GetFilteredSpans();
			var logs = CombineLogs(allSpans);
			AssertSpansCount(allSpans, filteredSpans);
			AssertAvailableHotKeys(logs);
			AssertAllKeyPressEvents(logs);
			AssertCallStack(logs);

			var environmentInfo = hotKeyMonitor.GetEnvironmentInfo();
			AssertEnvironmentInfo(environmentInfo);

			hotKeyMonitor.ClearAll();
			var allNewSpans = hotKeyMonitor.GetAllSpans();
			AssertEquals(0, allNewSpans.Count);

			hotKeyMonitor.Enabled = false;
			ErrorReporter.Clear();
		}

		public void AssertSpansCount(SortedDictionary<string, string> allSpans, SortedDictionary<string, string> filteredSpans)
		{
			AssertEquals(2, allSpans.Count);
			AssertEquals(1, filteredSpans.Count);
		}

		public void AssertAvailableHotKeys(string logs)
		{
			AssertContains("- A", logs);
			AssertContains("- A, Control", logs);
			AssertContains("- A, Shift, Control", logs);
			AssertContains("- F1, Control", logs);
		}

		public void AssertAllKeyPressEvents(string logs)
		{
			AssertContains("[Form 1] [B] [processor: ''] [processed: False]", logs);
			AssertContains("[Form 1] [A] [processor: 'DummyProcessor'] [processed: True]", logs);
			AssertContains("[Form 1] [Ctrl + A] [processor: 'DummyProcessor'] [processed: True]", logs);
			AssertContains("[Form 1] [F2] [processor: ''] [processed: False]", logs);
			AssertContains("[Form 1 (Form)] [Ctrl + F1] [processor: ''] [processed: False]", logs);
			AssertContains("[Form 1] [Ctrl + Shift + A] [processor: 'DummyProcessor'] [processed: True]", logs);
			AssertContains("[Form 2] [L] [processor: ''] [processed: False]", logs);
			AssertContains("[Form 2] [J] [processor: ''] [processed: False]", logs);
		}

		public void AssertCallStack(string logs)
		{
			AssertContains("System.NotImplementedException: This is a test exception.", logs);
		}

		public void AssertEnvironmentInfo(string environmentInfo)
		{
			AssertContains("*** Environment Info ***", environmentInfo);
			AssertContains("LoginName", environmentInfo);
			AssertContains("GlobalHotKeys", environmentInfo);
		}
	}
}
