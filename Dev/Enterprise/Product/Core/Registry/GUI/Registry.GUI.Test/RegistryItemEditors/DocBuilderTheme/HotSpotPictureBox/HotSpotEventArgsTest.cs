using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.GUI.HotSpotPictureBox;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class HotSpotEventArgsTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			var spot = new HotSpot("Funny Toes", Color.Firebrick);
			HotSpotEventArgs args = new HotSpotEventArgs(spot);
			AssertEquals("args.HotSpot", spot, args.HotSpot);
		}
	}
}
