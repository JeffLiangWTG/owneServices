using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class HotkeyRegisterTest : TestCase
	{
		static bool DummyProcessor(object sender, Keys key)
		{
			return true;
		}

		public void TestIsRegistered()
		{
			var hotkeys = new HotkeyRegister();
			hotkeys.RegisterHotKey(Keys.Control | Keys.A, () => { });

			Assert("We have registred Ctrl + A, IsRegistered should be true", hotkeys.IsRegistered(Keys.Control | Keys.A));
			Assert("We havent registred Ctrl + B, IsRegestered should be false", !hotkeys.IsRegistered(Keys.Control | Keys.B));
		}

		public void TestIsRegistered_Range()
		{
			var hotkeys = new HotkeyRegister();
			hotkeys.RegisterHotKeyRange(Keys.Control | Keys.A, Keys.Control | Keys.D, (o, k) => { return true; });

			Assert("We have registred Ctrl + B, IsRegistered should be true", hotkeys.IsRegistered(Keys.Control | Keys.B));
			Assert("We havent registred Ctrl + Z, IsRegestered should be false", !hotkeys.IsRegistered(Keys.Control | Keys.Z));
		}

		public void TestClearRegisteredKeys()
		{
			var hotkeys = new HotkeyRegister();
			hotkeys.RegisterHotKey(Keys.A, DummyProcessor, "The A key");
			hotkeys.RegisterHotKey(Keys.Control | Keys.A, DummyProcessor, "The Ctrl and A key");
			hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.A, DummyProcessor, "The Ctrl, Shift and A key");
			hotkeys.Clear();
			AssertEquals(0, hotkeys.Descriptions.Count());
			Assert(!hotkeys.IsRegistered(Keys.A));
			Assert(!hotkeys.IsRegistered(Keys.Control | Keys.A));
			Assert(!hotkeys.IsRegistered(Keys.Control | Keys.Shift | Keys.A));
		}

		public void TestIsRegistered_DoesntFireDelegate()
		{
			var hotkeys = new HotkeyRegister();

			bool wasCalled = false;
			hotkeys.RegisterHotKey(Keys.Control | Keys.A, () => { wasCalled = true; });

			Assert("We have registred Ctrl + A, IsRegisteed should be true", hotkeys.IsRegistered(Keys.Control | Keys.A));
			Assert("IsRegistered should not have called the delegate", !wasCalled);
		}

		public void TestHotkeyDescriptionsAreOrdered()
		{
			var hotkeys = new HotkeyRegister();
			hotkeys.RegisterHotKey(Keys.A, DummyProcessor, "The A key");
			hotkeys.RegisterHotKey(Keys.Control | Keys.A, DummyProcessor, "The Ctrl and A key");
			hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.A, DummyProcessor, "The Ctrl, Shift and A key");
			hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.Alt | Keys.A, DummyProcessor, "The Ctrl, Shift, Alt and A key");

			var descriptions = string.Join("\r\n", hotkeys.Descriptions.Select(pair => pair.Key + " | " + pair.Value));
			AssertEquals("Should show key values, and be in the same order they were entered.",
@"A | The A key
Ctrl + A | The Ctrl and A key
Ctrl + Shift + A | The Ctrl, Shift and A key
Ctrl + Shift + Alt + A | The Ctrl, Shift, Alt and A key", descriptions);
		}

		public void TestHotkeyDescriptionsForRanges()
		{
			var hotkeys = new HotkeyRegister();
			hotkeys.RegisterHotKeyRange(Keys.A, Keys.Z, DummyProcessor, "The A to Z key");
			hotkeys.RegisterHotKeyRange(Keys.Control | Keys.A, Keys.Control | Keys.Z, DummyProcessor, "The ctrl and A to Z key");

			var descriptions = string.Join("\r\n", hotkeys.Descriptions.Select(pair => pair.Key + " | " + pair.Value));
			AssertEquals("Should group the ranges.",
@"A - Z | The A to Z key
Ctrl + A - Z | The ctrl and A to Z key", descriptions);
		}

		public void TestDigitsRemoveTheD()
		{
			var hotkeys = new HotkeyRegister();
			hotkeys.RegisterHotKeyRange(Keys.D0, Keys.D9, DummyProcessor, "Some numbers");

			AssertEquals("We dont want to show the 'D' for digit keys.", "0 - 9", hotkeys.Descriptions.Single().Key);
		}

		public void TestCanOnlyAddRangesThatMakeSense()
		{
			AssertCantRegister("'to' is less than 'from'", Keys.Z, Keys.A);
			AssertCantRegister("different control keys", Keys.Control | Keys.A, Keys.Control | Keys.Shift | Keys.A);
			AssertCantRegister("different control keys", Keys.Control | Keys.A, Keys.A);
			AssertCantRegister("nonsense range", Keys.Decimal, Keys.Down);
			AssertCantRegister("partial nonsense range", Keys.A, Keys.VolumeUp);
			AssertCantRegister("Jump from digit to char", Keys.D0, Keys.Z);

			AssertCanRegister("Letters", Keys.A, Keys.Z);
			AssertCanRegister("Digits", Keys.D0, Keys.D9);
			AssertCanRegister("F Keys", Keys.F1, Keys.F12);
		}

		void AssertCantRegister(string message, Keys from, Keys to)
		{
			var hotkey = new HotkeyRegister();
			AssertExceptionThrown<ArgumentException>("Cant register - " + message, () => hotkey.RegisterHotKeyRange(from, to, DummyProcessor));
		}

		void AssertCanRegister(string message, Keys from, Keys to)
		{
			var hotkey = new HotkeyRegister();
			AssertNoExceptionThrown("Can Register - " + message, () => hotkey.RegisterHotKeyRange(from, to, DummyProcessor));
		}

		public void TestAddingHotkeyRange()
		{
			// Lets register CTRL+A to CTRL+Z
			var hotkeys = new HotkeyRegister();
			var lastCalledKey = Keys.None;
			hotkeys.RegisterHotKeyRange(Keys.Control | Keys.A, Keys.Control | Keys.Z, (sender, key) => { lastCalledKey = key; return true; });

			Assert("First should be inclusive", hotkeys.ProcessCmdKey(null, Keys.Control | Keys.A));
			AssertEquals(Keys.Control | Keys.A, lastCalledKey);

			Assert("Last should be inclusive", hotkeys.ProcessCmdKey(null, Keys.Control | Keys.Z));
			AssertEquals(Keys.Control | Keys.Z, lastCalledKey);

			Assert("Should include inner values", hotkeys.ProcessCmdKey(null, Keys.Control | Keys.P));
			AssertEquals(Keys.Control | Keys.P, lastCalledKey);
		}

		public void TestRunningHotkeys()
		{
			var register = new HotkeyRegister();
			bool ctrlTWasCalled = false;
			register.RegisterHotKey(Keys.Control | Keys.T, (form, keypressed) => ctrlTWasCalled = true);

			bool ctrlShiftPWasCalled = false;
			register.RegisterHotKey(Keys.Control | Keys.Shift | Keys.P, (form, keypressed) => ctrlShiftPWasCalled = true);

			Assert("No key registered, should return false", !register.ProcessCmdKey(DummyForm, Keys.Control | Keys.D));
			Assert("The delegate should not have been run - Wrong key combo", !ctrlTWasCalled);

			Assert("A registered key was called, should return true", register.ProcessCmdKey(DummyForm, Keys.Control | Keys.T));
			Assert("The delegate should be called - Correct key combo", ctrlTWasCalled);

			Assert("We never pressed ctrlShiftP, should never have run the delegate", !ctrlShiftPWasCalled);
		}

		public void TestRegisterHotKey()
		{
			var register = new HotkeyRegister();
			var numCalled = 0;
			register.RegisterHotKey(Keys.Control | Keys.A, (form, key) => { numCalled = 1; return true; });
			register.RegisterHotKey(Keys.Control | Keys.A, (form, key) => { numCalled = 2; return false; }, "Collision");

			AssertEquals("Attemped to register the same hotkey twice. Key: Ctrl + A, Original Description: '<empty>', New Description: 'Collision'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Assert("Should still call the key", register.ProcessCmdKey(DummyForm, Keys.Control | Keys.A));
			AssertEquals("The first hotkey should be called", 1, numCalled);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public void TestReturnsTheRightValue()
		{
			var calledTimes = 0;

			var register = new HotkeyRegister();
			register.RegisterHotKey(Keys.Control | Keys.A, (form, keys) => ++calledTimes > 1);

			Assert("Should return false because delegate did", !register.ProcessCmdKey(DummyForm, Keys.Control | Keys.A));
			Assert($"Should now return true because delegate did (Called: {calledTimes})", register.ProcessCmdKey(DummyForm, Keys.Control | Keys.A));
		}

		public void TestProcessorError()
		{
			var register = new HotkeyRegister();
			register.RegisterHotKey(Keys.Control | Keys.E, (form, keys) => throw new Exception());

			ErrorReporter.Clear();

			AssertExceptionThrown<Exception>(() => register.ProcessCmdKey(DummyForm, Keys.Control | Keys.E));
			AssertEquals("HotkeyProcessCmdKeyError: Control: (KForm), keyData:(Ctrl + E)", ErrorReporter.LastMessageReported);
			AssertEquals("There has been an error processing the (Ctrl + E) hot key.", UnitTestUserNotification.Instance.LastMessage.Text);

			ErrorReporter.Clear();
			UnitTestUserNotification.Instance.ClearMessages();
		}

		KForm DummyForm => dummyForm ?? (dummyForm = new KForm());
		KForm dummyForm;

		protected override void TearDown()
		{
			dummyForm?.Dispose();
			base.TearDown();
		}
	}
}
