using System.Drawing;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public class TextWatermarkTest : TestCase
	{
		public void TestConstructorWithAllParameters()
		{
			var watermark = new TextWatermark("boo", WatermarkHorizontalAlign.Right, WatermarkVerticalAlign.Bottom,
				40, 30, 20, Color.Firebrick, "Verdana", 50, FontStyle.Italic);

			AssertEquals("boo", watermark.Text);
			AssertEquals(null, watermark.AsImage);
			AssertEquals("boo", watermark.AsText);
			AssertEquals(WatermarkHorizontalAlign.Right, watermark.HorizontalAlign);
			AssertEquals(WatermarkVerticalAlign.Bottom, watermark.VerticalAlign);
			AssertEquals(40f, watermark.HorizontalOffset);
			AssertEquals(30f, watermark.VerticalOffset);
			AssertEquals(20, watermark.Rotation);
			AssertEquals(50, watermark.FontSize);
			AssertEquals(Color.Firebrick, watermark.TextColor);
		}

		public void TestAsText()
		{
			string s1 = "MyTest";
			string s2 = "Other test";
			AssertEquals(s1, new TextWatermark(s1, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45, Color.Gray, "Arial", 120, FontStyle.Bold).AsText);
			AssertEquals(s2, new TextWatermark(s2, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45, Color.Gray, "Arial", 120, FontStyle.Bold).AsText);
		}

		public void TestAsImage()
		{
			string s = "MyTest";
			AssertEquals(null, new TextWatermark(s, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45, Color.Gray, "Arial", 120, FontStyle.Bold).AsImage);
		}
	}
}
