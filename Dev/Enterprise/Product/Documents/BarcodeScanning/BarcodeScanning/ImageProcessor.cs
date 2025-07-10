using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Perform graphic manipulations of graphic data to help barcode location and recognition
	/// processes.
	/// </summary>
	internal abstract class ImageProcessor
	{
		public static void RemoveIsolatedPixels(byte[,] imageData, int imageHeight, int imageWidth)
		{
			unsafe
			{
				fixed (byte* pImageData = imageData)
				{
					unchecked
					{
						byte* ptr = pImageData + imageWidth + 1; //ImageData[1,1]
						bool yValueChanged = false;
						for (int y = 1; y < imageHeight - 1; y++)
						{
							if (yValueChanged)
							{
								ptr += 2;       //now positioned from ImageData[y-1, ImageWidth-1] to ImageData[y, 1]
							}
							for (int x = 1; x < imageWidth - 1; x++)
							{
								yValueChanged = false;
								byte current = *ptr;
								if (current != *(ptr - 1) &&                //ImageData[y, x] != ImageData[y, x - 1]
									current != *(ptr + 1) &&                //ImageData[y, x] != ImageData[y, x + 1] 
									current != *(ptr - imageWidth) &&       //ImageData[y, x] != ImageData[y - 1, x] 
									current != *(ptr + imageWidth))         //ImageData[y, x] != ImageData[y + 1, x]
								{
									*ptr = *(ptr - 1);                      //ImageData[y, x] = ImageData[y, x - 1];
								}
								ptr++;
							}
							yValueChanged = true;
						}
					}
				}
			}

			return;
		}

		public static void HorizontalFlipImageData(byte[,] imageData, int imageHeight, int imageWidth)
		{
			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth / 2; x++)
				{
					byte swap = imageData[y, x];
					imageData[y, x] = imageData[y, imageWidth - x - 1];
					imageData[y, imageWidth - x - 1] = swap;
				}
			}
		}

		public static void VerticalFlipImageData(byte[,] imageData, int imageHeight, int imageWidth)
		{
			for (int y = 0; y < imageHeight / 2; y++)
			{
				for (int x = 0; x < imageWidth; x++)
				{
					byte swap = imageData[y, x];
					imageData[y, x] = imageData[imageHeight - y - 1, x];
					imageData[imageHeight - y - 1, x] = swap;
				}
			}
		}

		/// <summary>
		/// Open a bitmap and return black and white pixel data as a two-dimensional array.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using GDI+ to manipulate images, no WinForms here")]
		public static byte[,] GetImageDataFromBitmap(Bitmap inputImage, out int imageHeight, out int imageWidth)
		{
			BitmapData rawData = inputImage.LockBits(new Rectangle(0, 0, inputImage.Width, inputImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
			imageHeight = inputImage.Height;
			imageWidth = inputImage.Width;
			byte[,] imageData = new byte[imageHeight, imageWidth];

			if (rawData.Stride < 0)
			{
				throw new ArgumentException("Images with negative stride (ie, Backwards scanned images) are not supported.");
			}

			unsafe
			{
				byte* p = (byte*)rawData.Scan0;
				int byteWidth = imageWidth * 4;
				int offset = rawData.Stride - byteWidth;

				fixed (byte* pImageData = imageData)
				{
					unchecked
					{
						byte* ptr = pImageData;
						bool blackState = false;

						for (int y = 0; y < imageHeight; y++)
						{
							blackState = false;
							for (int x = 0; x < imageWidth; x++)
							{
								*ptr = *p;

								if ((*ptr) > WhiteThreshold)
								{
									blackState = false;
								}

								if ((*ptr) < BlackThreshold)
								{
									blackState = true;
								}

								// Set pixel to active colour
								if (blackState)
								{
									*ptr = BlackPixel;
								}
								else
								{
									*ptr = WhitePixel;
								}
								ptr++;
								p += 4;
							}
							p += offset;
						}
					}
				}
			}

			inputImage.UnlockBits(rawData);

			imageHeight = inputImage.Height;
			imageWidth = inputImage.Width;
			return imageData;
		}

		#region Implementation

		protected const int BlackPixel = 0;
		protected const int WhitePixel = 255;
		protected const byte BlackThreshold = 127;
		protected const byte WhiteThreshold = 128;

		#endregion

	}
}

