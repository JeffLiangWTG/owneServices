using System;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class InternalIconsTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetAllImages()
		{
			foreach (InternalIconTypes type in Enum.GetValues(typeof(InternalIconTypes)))
			{
				Assert("Could not find " + type + " as an image or an icon.", InternalIcons.IsIcon(type) || InternalIcons.IsImage(type));
			}
		}
	}
}
