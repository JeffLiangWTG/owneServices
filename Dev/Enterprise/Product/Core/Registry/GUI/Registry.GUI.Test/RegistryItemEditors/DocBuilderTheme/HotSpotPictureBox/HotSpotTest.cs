using System.Drawing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.GUI.HotSpotPictureBox;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class HotSpotTest : TestCaseWithFactory
	{
		public void TestHotSpot()
		{
			HotSpot hotSpot = new HotSpot("Robertia", Color.DarkViolet);
			AssertEquals("hotSpot.Code", "Robertia", hotSpot.Code);
			AssertEquals("hotSpot.Color", Color.DarkViolet, hotSpot.Color);
			AssertEquals("hotSpot.ToString()", "HotSpot Robertia - Color: 148, 0, 211", hotSpot.ToString());
		}
	}
}
