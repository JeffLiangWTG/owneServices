using System;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KMenuItemTest : TestCase
	{
		public void TestOnPopup()
		{
			var menuItem = new KMenuItem();
			AssertNoExceptionThrown(() =>
			{
				menuItem.OnPopup(EventArgs.Empty);
			});
			menuItem.Dispose();
		}

		public void TestStripAcceleratorKeys()
		{
			AssertEquals("Single & should be Striped", "Short Cut", KMenuItem.StripAcceleratorKeys("Short &Cut"));
			AssertEquals("double && should be Striped to &", "You & Me", KMenuItem.StripAcceleratorKeys("You && Me"));
			AssertEquals("Striped with(&)", "You & Me(1)", KMenuItem.StripAcceleratorKeys("You && Me(&1)"));
		}

		public void TestStripAcceleratorKeysButKeepAmpersandInText()
		{
			AssertEquals("Single & should be Striped", "Short Cut", KMenuItem.StripAcceleratorKeysButKeepAmpersandInText("Short &Cut"));
			AssertEquals("double && should not be Striped", "You && Me", KMenuItem.StripAcceleratorKeysButKeepAmpersandInText("You && Me"));
			AssertEquals("3 Ampersands should Striped one Ampersand", "You && Me", KMenuItem.StripAcceleratorKeysButKeepAmpersandInText("You &&& Me"));
			AssertEquals("4 Ampersands should not be Striped", "You &&&& Me", KMenuItem.StripAcceleratorKeysButKeepAmpersandInText("You &&&& Me"));
			AssertEquals("Striped with(&)", "You && Me(1)", KMenuItem.StripAcceleratorKeysButKeepAmpersandInText("You && Me(&1)"));
		}
	}
}
