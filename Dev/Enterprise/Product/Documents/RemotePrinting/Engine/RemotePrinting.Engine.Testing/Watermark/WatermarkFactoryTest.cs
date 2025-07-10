using System;
using System.Drawing;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	class WatermarkFactoryTest : PrintEngineTestCase
	{
		public void TestGetWatermark_Default()
		{
			var waterMark = WatermarkFactory.GetWatermark(new SerialisableWatermark());
			AssertNotNull("Watermark should not be null.", waterMark);
			AssertEquals("Default values for watermark should be set.", "DRAFT", waterMark.AsText);
			AssertEquals("Default values for watermark should be set.", WatermarkHorizontalAlign.Centre, waterMark.HorizontalAlign);
			AssertEquals("Default values for watermark should be set.", WatermarkVerticalAlign.Middle, waterMark.VerticalAlign);
			AssertEquals("Default values for watermark should be set.", 45, waterMark.Rotation);
			AssertEquals("Default values for watermark should be set.", 0f, waterMark.HorizontalOffset);
			AssertEquals("Default values for watermark should be set.", 0f, waterMark.VerticalOffset);
		}

		public void TestGetWatermark_InvalidArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WatermarkFactory.GetWatermark(null));
			AssertExceptionThrown<ArgumentException>(() => WatermarkFactory.GetWatermark(new SerialisableWatermark { UseTextWatermark = true, TextWatermark = null }));
			AssertExceptionThrown<ArgumentException>(() => WatermarkFactory.GetWatermark(new SerialisableWatermark { UseTextWatermark = false, ImageWatermark = null }));
		}

		public void TestGetWatermark_TextWatermark()
		{
			var watermarkInfo = new SerialisableWatermark
			{
				UseTextWatermark = true,
				TextWatermark = "Hello",
				HorizontalAlignment = nameof(WatermarkHorizontalAlign.Left),
				VerticalAlignment = nameof(WatermarkVerticalAlign.Top),
				HorizontalOffset = 3,
				VerticalOffset = 4,
				Rotation = 2,
				Opacity = 50,
				FontSize = 3
			};

			var textWatermark = (TextWatermark)WatermarkFactory.GetWatermark(watermarkInfo);
			AssertEquals(nameof(textWatermark.Text), "Hello", textWatermark.Text);
			AssertEquals(nameof(textWatermark.HorizontalAlign), WatermarkHorizontalAlign.Left, textWatermark.HorizontalAlign);
			AssertEquals(nameof(textWatermark.VerticalAlign), WatermarkVerticalAlign.Top, textWatermark.VerticalAlign);
			AssertEquals(nameof(textWatermark.HorizontalOffset), 3f, textWatermark.HorizontalOffset);
			AssertEquals(nameof(textWatermark.VerticalOffset), 4f, textWatermark.VerticalOffset);
			AssertEquals(nameof(textWatermark.Rotation), 2, textWatermark.Rotation);
			AssertEquals(nameof(textWatermark.TextColor), Color.FromArgb(50, 0, 0, 0), textWatermark.TextColor);
			AssertEquals(nameof(textWatermark.FontName), "Arial", textWatermark.FontName);
			AssertEquals(nameof(textWatermark.FontSize), 3, textWatermark.FontSize);
		}

		public void TestGetWatermark_ImageWatermark()
		{
			var watermarkInfo = new SerialisableWatermark
			{
				UseTextWatermark = false,
				ImageWatermark = new byte[] { 0xA, 0xB, 0xC, 0xD },
				HorizontalAlignment = nameof(WatermarkHorizontalAlign.Left),
				VerticalAlignment = nameof(WatermarkVerticalAlign.Top),
				HorizontalOffset = 3,
				VerticalOffset = 4,
				Rotation = 2,
			};

			var imageWatermark = (ImageWatermark)WatermarkFactory.GetWatermark(watermarkInfo);
			AssertArrayEqualsByElements(new byte[] { 0xA, 0xB, 0xC, 0xD }, imageWatermark.AsImage);
			AssertEquals(nameof(imageWatermark.HorizontalAlign), WatermarkHorizontalAlign.Left, imageWatermark.HorizontalAlign);
			AssertEquals(nameof(imageWatermark.VerticalAlign), WatermarkVerticalAlign.Top, imageWatermark.VerticalAlign);
			AssertEquals(nameof(imageWatermark.HorizontalOffset), 3f, imageWatermark.HorizontalOffset);
			AssertEquals(nameof(imageWatermark.VerticalOffset), 4f, imageWatermark.VerticalOffset);
			AssertEquals(nameof(imageWatermark.Rotation), 2, imageWatermark.Rotation);
		}
	}
}
