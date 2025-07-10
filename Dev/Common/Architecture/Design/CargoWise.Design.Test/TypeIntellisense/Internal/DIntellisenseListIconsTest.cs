using System;
using NUnit.Framework;

namespace CargoWise.Design.TypeIntellisense.Testing
{
	public class DIntellisenseListIconsTest : TestCase
	{
		public void TestAllImagesExist()
		{
			foreach (IntellisenseItemType type in Enum.GetValues(typeof(IntellisenseItemType)))
			{
				AssertNotNull(IntellisenseListIcons.GetImage(type));
			}
		}
	}
}
