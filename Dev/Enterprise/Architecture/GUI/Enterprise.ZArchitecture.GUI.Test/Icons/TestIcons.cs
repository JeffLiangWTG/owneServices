using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestIcons : TestCase
	{
		public void TestImageList()
		{
			AssertEquals("ColorDepth", ColorDepth.Depth32Bit, Icons.ImageList.ColorDepth);
		}

		public void TestGetImageIndexForIcon()
		{
			int firstAccessIndex = Icons.GetImageIndex(IconTypes.Phone20x16);
			int secondAccessIndex = Icons.GetImageIndex(IconTypes.Phone20x16);
			Assert("Image found", -1 != firstAccessIndex);
			AssertEquals("Image cached", firstAccessIndex, secondAccessIndex);
		}

		public void TestGetImageIndexForImage()
		{
			int firstAccessIndex = Icons.GetImageIndex(IconTypes.eDocsToReadLarge);
			int secondAccessIndex = Icons.GetImageIndex(IconTypes.eDocsToReadLarge);
			Assert("Image found", -1 != firstAccessIndex);
			AssertEquals("Image cached", firstAccessIndex, secondAccessIndex);
		}

		public void TestGetAllImages()
		{
			foreach (IconTypes type in Enum.GetValues(typeof(IconTypes)))
			{
				if (type != IconTypes.None)
				{
					TestGetIconOrImage(type);
				}
			}
		}

		public void TestGetResourcesImage()
		{
			AssertNotNull(Properties.Resources.Clear);
			AssertNotNull(Properties.Resources.Cross);
			AssertNotNull(Properties.Resources.Pause);
			AssertNotNull(Properties.Resources.Search);
			AssertNotNull(Properties.Resources.Start);
			AssertNotNull(Properties.Resources.Stop);
			AssertNotNull(Properties.Resources.Warning);
		}

		public void TestGetIconOrImage(IconTypes type)
		{
			if (Icons.IsIcon(type))
			{
				AssertNotNull(Icons.GetIcon(type));
			}
			else if (Icons.IsImage(type))
			{
				AssertNotNull(Icons.GetImage(type));
			}
			else
			{
				Fail("Could not find " + type + " as an image or an icon.");
			}
		}
	}
}
