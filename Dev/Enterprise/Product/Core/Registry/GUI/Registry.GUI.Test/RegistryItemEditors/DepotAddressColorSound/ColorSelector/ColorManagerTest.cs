using System.Drawing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ColorManagerTest : TestCase
	{
		public void TestColorFromString()
		{
			var validColor = "123,123,123";
			var invalidColor = "ABC,ABC,ABC";

			AssertEquals("Valid color should be returned", Color.FromArgb(123, 123, 123), ColorManager.ColorFromString(validColor));
			AssertEquals("Empty color should be returned", Color.Empty, ColorManager.ColorFromString(invalidColor));
		}
	}
}
