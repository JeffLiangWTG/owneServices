using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test.ShortCutKey
{
	sealed class ShortcutKeyResourcesTest : TestCase
	{
		public void TestSupportedShortCutKeys()
		{
			for (var i = 0; i < 9; i++)
			{
				TestKey(i.ToString());
			}
			for (var i = 1; i < 13; i++)
			{
				TestKey($"F{i}");
			}
			for (var c = 'A'; c <= 'Z'; c++)
			{
				TestKey(c.ToString());
			}
			TestKey("Ctrl");
			TestKey("Shift");
			TestKey("Space");
			TestKey("Enter");
			TestKey("Alt");
			TestKey("-");
			TestKey("+");
			TestKey("=");
		}

		public void TestUnsupportedKeysThrowException()
		{
			AssertExceptionThrown<KeyNotFoundException>(() => TestKey("++"));
			AssertExceptionThrown<KeyNotFoundException>(() => TestKey("WrongKey"));
		}

		void TestKey(string key)
		{
			var result = ShortcutKeyResources.GetKeyString(key);
			Assert($"ShortCutKeyResources GetKey failure Expected:{key}, Actually:{result}", string.Equals(key, result, System.StringComparison.OrdinalIgnoreCase));
		}
	}
}
