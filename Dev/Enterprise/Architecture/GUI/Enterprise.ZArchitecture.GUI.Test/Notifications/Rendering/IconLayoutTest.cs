using System.Drawing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class IconLayoutTest : TestCase
	{
		public void TestLayoutAlign()
		{
			var rectangle = new Rectangle(0, 0, 10, 10);
			var clip = new Size(2, 2);

			var centered = new Rectangle(new Point((rectangle.Width / 2) - (clip.Width / 2), (rectangle.Height / 2) - (clip.Height / 2)), clip);
			Assert(centered == Layout(rectangle, clip, IconAlignment.Center));

			var leftAligned = new Rectangle(new Point(IconLayout.XOffset, IconLayout.YOffset), clip);
			Assert(leftAligned == Layout(rectangle, clip, IconAlignment.Left));

			var rightAligned = new Rectangle(new Point(rectangle.Width - clip.Width - IconLayout.XOffset, IconLayout.YOffset), clip);
			Assert(rightAligned == Layout(rectangle, clip, IconAlignment.Right));
		}

		public void TestLayoutTakesInAccountShiftForControlsWhichReportsTheyClientRectangleInproperly_SuchAsZDropButton()
		{
			var xShift = 4;
			var yShift = 5;

			var rectangle = new Rectangle(xShift, yShift, 10, 10);
			var clip = new Size(2, 2);

			var centered = new Rectangle(new Point((rectangle.Width / 2) - (clip.Width / 2) + xShift, (rectangle.Height / 2) - (clip.Height / 2) + yShift), clip);
			Assert(centered == Layout(rectangle, clip, IconAlignment.Center));

			var leftAligned = new Rectangle(new Point(IconLayout.XOffset + xShift, IconLayout.YOffset + yShift), clip);
			Assert(leftAligned == Layout(rectangle, clip, IconAlignment.Left));

			var rightAligned = new Rectangle(new Point(rectangle.Width - clip.Width - IconLayout.XOffset + xShift, IconLayout.YOffset + yShift), clip);
			Assert(rightAligned == Layout(rectangle, clip, IconAlignment.Right));
		}

		static Rectangle Layout(Rectangle area, Size clip, IconAlignment alignment)
		{
			return new IconLayout(null, alignment).Compute(area, clip);
		}
	}
}
