using System.Drawing;
using System.Windows.Forms;
using Enterprise.Registry.GUI.HotSpotPictureBox;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class PictureBoxWithHotSpotsTest : TestCase
	{
		public void TestHotSpotClick()
		{
			using (var mapBitmap = new Bitmap(100, 100))
			{
				using (var graphics = Graphics.FromImage(mapBitmap))
				{
					graphics.FillRectangle(Brushes.BurlyWood, 0, 0, 50, 100);
					graphics.FillRectangle(Brushes.Goldenrod, 50, 0, 50, 100);
				}

				using (var pictureBoxWithHotSpots = new PictureBoxWithHotSpots())
				{
					var hotSpot1 = new HotSpot("HotSpot1", Color.BurlyWood);
					var hotSpot2 = new HotSpot("HotSpot2", Color.Goldenrod);

					pictureBoxWithHotSpots.HotSpots.Add(hotSpot1);
					pictureBoxWithHotSpots.HotSpots.Add(hotSpot2);

					HotSpot lastHotSpotClicked = null;
					pictureBoxWithHotSpots.HotSpotClick += new HotSpotEventHandler(delegate(object sender, HotSpotEventArgs e)
					{
						lastHotSpotClicked = e.HotSpot;
					});

					CombineAssertions(delegate()
					{
						pictureBoxWithHotSpots.MapBitmap = null;

						pictureBoxWithHotSpots.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 50, 50, 0));
						AssertEquals("Pre-condition: lastHotSpotClicked", null, lastHotSpotClicked);

						pictureBoxWithHotSpots.MapBitmap = mapBitmap;

						pictureBoxWithHotSpots.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
						AssertEquals("lastHotSpotClicked", hotSpot1, lastHotSpotClicked);

						pictureBoxWithHotSpots.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 50, 0, 0));
						AssertEquals("lastHotSpotClicked", hotSpot2, lastHotSpotClicked);

						pictureBoxWithHotSpots.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0));
						AssertEquals("lastHotSpotClicked", null, lastHotSpotClicked);

						pictureBoxWithHotSpots.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 49, 99, 0));
						AssertEquals("lastHotSpotClicked", hotSpot1, lastHotSpotClicked);

						pictureBoxWithHotSpots.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 99, 99, 0));
						AssertEquals("lastHotSpotClicked", hotSpot2, lastHotSpotClicked);
					});
				}
			}
		}

		public void TestMapImage()
		{
			var pictureBox = new PictureBoxWithHotSpots();
			AssertNull("pictureBox.MapImage", pictureBox.MapBitmap);

			var mapImage = new Bitmap(1, 1);
			pictureBox.MapBitmap = mapImage;
			AssertEquals("pictureBox.MapImage", mapImage, pictureBox.MapBitmap);
		}
	}
}
