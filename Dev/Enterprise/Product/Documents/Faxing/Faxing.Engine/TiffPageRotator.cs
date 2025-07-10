using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.Faxing.Integration;

namespace Enterprise.Faxing.Engine
{
	public class TiffPageRotator
	{
		public void RotateLandscapeToPortrait(string filename)
		{
			Argument.NotNullOrEmpty(filename, nameof(filename));
			if (System.Environment.Is64BitProcess)
			{
				throw new FaxRouterException32Bit();
			}
			else
			{
				if (HasLandscapePages(filename))
				{
					ReorientLandscapePages(filename);
				}
			}
		}

		public bool HasLandscapePages(string filename)
		{
			Argument.NotNullOrEmpty(filename, nameof(filename));
			if (System.Environment.Is64BitProcess)
			{
				throw new FaxRouterException32Bit();
			}
			else
			{
				IntPtr img = TiffApi.OpenTiffFile(filename, 0);

				try
				{
					int i = 0;
					while (TiffApi.GetTiffImage(img, i))
					{
						BITMAPINFOHEADER bih;
						TiffApi.GetTiffDimensions(img, i, out bih);
						TiffApi.DropTiffImage(img, i);

						if (bih.biWidth > bih.biHeight)
						{
							return true;
						}

						++i;
					}
				}
				finally
				{
					if (img != IntPtr.Zero)
					{
						TiffApi.CloseTiffFile(img);
					}
				}
			}

			return false;
		}

		[SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		void ReorientLandscapePages(string filename)
		{
			Argument.NotNullOrEmpty(filename, nameof(filename));

			Image newImg = null;
			string tempFilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tif");
			try
			{
				using (Image sourceImg = Image.FromFile(filename))
				{
					bool faxFormat = false;
					if (Array.Exists(sourceImg.PropertyIdList, id => id == 0x103))
					{
						PropertyItem propItem = sourceImg.GetPropertyItem(0x103);
						if (propItem.Value[0] == 3)
						{
							faxFormat = true;
						}
					}

					Encoder enc = Encoder.SaveFlag;
					var encoderParametersCount = faxFormat ? 2 : 1;
					EncoderParameters ep = new EncoderParameters(encoderParametersCount);

					ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);
					if (faxFormat)
					{
						ep.Param[1] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT3);
					}

					foreach (Image page in GetRotatedPages(sourceImg))
					{
						if (newImg == null)
						{
							newImg = page;
							newImg.Save(tempFilename, GetTiffCodec(), ep);

							ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);
						}
						else
						{
							newImg.SaveAdd(page, ep);
						}
					}
				}

				if (newImg != null)
				{
					newImg.Dispose();
					newImg = null;
					if (File.Exists(filename))
					{
						File.Delete(filename);
					}

					{
						File.Move(tempFilename, filename);
					}
				}
			}
			catch (Exception ex)
			{
				throw new FaxComponentException("Exception whilst rotating tiff image.", ex);
			}
			finally
			{
				if (newImg != null)
				{
					newImg.Dispose();
				}

				if (File.Exists(tempFilename))
				{
					File.Delete(tempFilename);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Image mannipulation, no need for scaling")]
		IEnumerable<Image> GetRotatedPages(Image img)
		{
			int bitsPerPixel = Image.GetPixelFormatSize(img.PixelFormat);

			int pageCount = img.GetFrameCount(FrameDimension.Page);
			for (int i = 0; i < pageCount; i++)
			{
				img.SelectActiveFrame(FrameDimension.Page, i);

				Bitmap page;
				if (img.Width > img.Height)
				{
					page = new Bitmap(img.Height, img.Width, img.PixelFormat);
					page.SetResolution(img.VerticalResolution, img.HorizontalResolution);
				}
				else
				{
					page = new Bitmap(img.Width, img.Height, img.PixelFormat);
					page.SetResolution(img.HorizontalResolution, img.VerticalResolution);
				}

				BitmapData sourceData = null;
				BitmapData destData = null;
				try
				{
					sourceData = ((Bitmap)img).LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
					int sourceBytesLength = sourceData.Stride * sourceData.Height;
					byte[] sourceBytes = new byte[sourceBytesLength];
					Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytesLength);

					destData = page.LockBits(new Rectangle(0, 0, page.Width, page.Height), ImageLockMode.WriteOnly, page.PixelFormat);
					if (img.Width > img.Height)
					{
						int destBytesLength = destData.Stride * destData.Height;
						byte[] destBytes = new byte[destBytesLength];

						for (int v = 0; v < sourceData.Height; v++)
						{
							for (int h = 0; h < sourceData.Width; h++)
							{
								int pixel = GetPixel(sourceBytes, sourceData.Stride, bitsPerPixel, h, v);
								SetPixel(destBytes, destData.Stride, bitsPerPixel, sourceData.Height - v - 1, h, pixel);
							}
						}

						Marshal.Copy(destBytes, 0, destData.Scan0, destBytesLength);
					}
					else
					{
						Marshal.Copy(sourceBytes, 0, destData.Scan0, sourceBytesLength);
					}
				}
				finally
				{
					if (sourceData != null)
					{
						((Bitmap)img).UnlockBits(sourceData);
					}

					if (destData != null)
					{
						page.UnlockBits(destData);
					}
				}

				yield return page;
			}
		}

		int GetPixel(byte[] bytes, int stride, int bitsPerPixel, int h, int v)
		{
			Argument.NotNull(bytes, nameof(bytes));

			int pos = stride * v + ((bitsPerPixel * h) >> 3);
			int pixel = 0;
			if (bitsPerPixel >= 8)
			{
				int bytesPerPixel = bitsPerPixel >> 3;
				for (int i = 0; i < bytesPerPixel; i++)
				{
					pixel |= bytes[pos + i] << ((3 - i) << 3);
				}
			}
			else
			{
				int pixelPos = (bitsPerPixel * h) & 0x7;
				pixel = new BitVector32(bytes[pos])[GetSection(bitsPerPixel, pixelPos)];
			}

			return pixel;
		}

		void SetPixel(byte[] bytes, int stride, int bitsPerPixel, int h, int v, int pixel)
		{
			Argument.NotNull(bytes, nameof(bytes));

			int pos = stride * v + ((bitsPerPixel * h) >> 3);
			if (bitsPerPixel >= 8)
			{
				int bytesPerPixel = bitsPerPixel >> 3;
				for (int i = 0; i < bytesPerPixel; i++)
				{
					bytes[pos + i] = (byte)((pixel >> ((3 - i) << 3)) & 0xFF);
				}
			}
			else
			{
				int pixelPos = (bitsPerPixel * h) & 0x7;
				BitVector32 bv = new BitVector32(bytes[pos]);
				bv[GetSection(bitsPerPixel, pixelPos)] = pixel;
				bytes[pos] = (byte)bv.Data;
			}
		}

		BitVector32.Section GetSection(int bitsPerPixel, int pos)
		{
			if (sections == null)
			{
				sections = new Dictionary<int, BitVector32.Section[]>();
			}
			BitVector32.Section[] result;
			if (!sections.TryGetValue(bitsPerPixel, out result))
			{
				short maxValue = (short)(1 << (bitsPerPixel - 1));
				result = new BitVector32.Section[8 / bitsPerPixel];
				result[result.Length - 1] = BitVector32.CreateSection(maxValue);
				for (int i = result.Length - 2; i >= 0; i--)
				{
					result[i] = BitVector32.CreateSection(maxValue, result[i + 1]);
				}
				sections.Add(bitsPerPixel, result);
			}
			var j = pos >> (bitsPerPixel - 1);
			return result[j];
		}

		Dictionary<int, BitVector32.Section[]> sections;

		static ImageCodecInfo GetTiffCodec()
		{
			var encoders = ImageCodecInfo.GetImageEncoders();
			if (encoders != null)
			{
				return Array.Find(encoders, codec => codec.MimeType == "image/tiff");
			}
			else
			{
				throw new Exception("ImageCodecInfo has no Encoders");
			}
		}
	}
}
