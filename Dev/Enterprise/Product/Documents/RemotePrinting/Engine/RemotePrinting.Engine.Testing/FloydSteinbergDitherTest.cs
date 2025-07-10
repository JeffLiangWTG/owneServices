#if DEBUG
using System;
using System.Drawing;
using System.Drawing.Imaging;
using FlexCel.Draw;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public class FloydSteinbergDitherTest : TestCase
	{
		void CheckPattern(Brush sourceBrush, Color oddColor, Color evenColor)
		{
			using (Bitmap source = new Bitmap(10, 10, PixelFormat.Format32bppPArgb))
			{
				using (Graphics gr = Graphics.FromImage(source))
				{
					gr.FillRectangle(sourceBrush, 0, 0, 10, 10);
				}

				using (Bitmap result = FloydSteinbergDither.ConvertToBlackAndWhite(source))
				{
					AssertEquals(result.Width, source.Width);
					AssertEquals(result.Height, source.Height);
					for (int x = 0; x < result.Width; x++)
					{
						for (int y = 0; y < result.Height; y++)
						{
							Color cl = result.GetPixel(x, y);
							if ((x + y) % 2 == 0)
							{
								AssertEquals(cl.ToArgb(), evenColor.ToArgb());
							}
							else
							{
								Assert(cl.ToArgb() == oddColor.ToArgb());
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Floy Steinberg should do a checker pattern when gray is 50%.
		/// </summary>
		public void TestCheckerDither()
		{
			CheckPattern(Brushes.Gray, Color.Black, Color.White);
		}

		/// <summary>
		/// This image should be all black.
		/// </summary>
		public void TestBlackDither()
		{
			CheckPattern(Brushes.Black, Color.Black, Color.Black);
		}

		/// <summary>
		/// This image should be all white.
		/// </summary>
		public void TestWhiteDither()
		{
			CheckPattern(Brushes.White, Color.White, Color.White);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidSourceImage()
		{
			Bitmap source = new Bitmap(10, 10, PixelFormat.Format24bppRgb);
			FloydSteinbergDither.ConvertToBlackAndWhite(source);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestInvalidDestImage()
		{
			Bitmap source = new Bitmap(10, 10, PixelFormat.Format32bppPArgb);
			Bitmap dest = new Bitmap(10, 10, PixelFormat.Format32bppPArgb);
			FloydSteinbergDither.ConvertToBlackAndWhite(source, dest);
		}
	}
}
#endif
