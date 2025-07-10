using System;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test.ShortCutKey
{
	sealed class ShortcutKeyConveterTest : TestCase
	{
		public void TestConvertNormally()
		{
			ExecuteTest("Ctrl+Q", "Ctrl Q");
			ExecuteTest("Ctrl++", "Ctrl +");
			ExecuteTest("Ctrl+Q", "Ctrl Q");
			ExecuteTest("Alt+Shift+A", "Alt Shift A");
			ExecuteTest("Ctrl+Shift+Alt+Q", "Ctrl Shift Alt Q");
		}

		public void TestDuplicateKeys()
		{
			AssertExceptionThrown<ArgumentException>(() => ExecuteTest("Ctrl+Ctrl", string.Empty));
			AssertExceptionThrown<ArgumentException>(() => ExecuteTest("Ctrl+Q+Q", string.Empty));
			AssertExceptionThrown<ArgumentException>(() => ExecuteTest("Ctrl+Q+Shift+Q", string.Empty));
			AssertExceptionThrown<ArgumentException>(() => ExecuteTest("Ctrl++++", string.Empty));
			AssertExceptionThrown<ArgumentException>(() => ExecuteTest("Q+Q", string.Empty));
		}

		public void TestEmptyKey()
		{
			AssertExceptionThrown<ArgumentNullException>(() => ExecuteTest(string.Empty, string.Empty));
			AssertExceptionThrown<ArgumentNullException>(() => ExecuteTest(null, string.Empty));
		}
		void ExecuteTest(string shortCutKey, string expected)
		{
			var converter = new ShortcutKeyConverter();
			var result = string.Join(" ", converter.Convert(shortCutKey));
			Assert($"ShortCutKey Convert failure Expected:{expected}, Actually:{result}", result == expected);
		}
	}
}
