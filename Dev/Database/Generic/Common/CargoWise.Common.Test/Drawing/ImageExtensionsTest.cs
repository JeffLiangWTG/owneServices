using System.Drawing;
using NUnit.Framework;

namespace CargoWise.Common
{
	class ImageExtensionsTest : TestCase
	{
		public void TestIsDisposed()
		{
			using (var bitmap = new Bitmap(1, 1))
			{
				AssertEquals("IsDisposed should be false", false, bitmap.IsDisposed());
				bitmap.Dispose();
				AssertEquals("IsDisposed should be true", true, bitmap.IsDisposed());
			}
		}
	}
}