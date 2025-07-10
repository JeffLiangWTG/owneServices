using System.Drawing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualiserComponentTest : TestCase
	{
		public void TestSize()
		{
			VisualiserComponentForTesting testVC = new VisualiserComponentForTesting(new Point(0, 0), new Size(20, 15));
			AssertEquals(new Size(20, 15), testVC.Size);
		}

		public void TestLocation()
		{
			VisualiserComponentForTesting testVC = new VisualiserComponentForTesting(new Point(120, 340), new Size(20, 15));
			AssertEquals(new Point(120, 340), testVC.Location);
		}
	}
}
