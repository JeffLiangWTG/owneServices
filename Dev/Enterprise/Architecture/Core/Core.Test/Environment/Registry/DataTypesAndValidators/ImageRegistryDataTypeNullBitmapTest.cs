using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ImageRegistryDataType))]
	public class ImageRegistryDataTypeNullBitmapTest : RegistryDataTypeTestCase<ImageRegistryDataType>
	{
		protected override ImageRegistryDataType GetNewDataType()
		{
			return new ImageRegistryDataType(100, 200);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Bitmap testBitmap1 = SystemIcons.Information.ToBitmap();
			MemoryStream testBitmapStream1 = new MemoryStream();
			testBitmap1.Save(testBitmapStream1, ImageFormat.Bmp);

			Bitmap testBitmap2 = new Bitmap(100, 200);
			MemoryStream testBitmapStream2 = new MemoryStream();
			testBitmap2.Save(testBitmapStream2, ImageFormat.Bmp);

			ItemsToDispose.Add(testBitmap1);
			ItemsToDispose.Add(testBitmapStream1);
			ItemsToDispose.Add(testBitmap2);
			ItemsToDispose.Add(testBitmapStream2);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(testBitmap1, testBitmapStream1.ToArray()),
				new ValidSampleAndBinaryValueInDB(testBitmap2, testBitmapStream2.ToArray())
			};
		}

		protected override object GetNullRepresentation()
		{
			return ImageRegistryDataType.MagicNullBitmap.Clone();
		}

		protected override object[] GetInvalidSamples()
		{
			Image image1 = new Bitmap(101, 199);
			Image image2 = new Bitmap(99, 201);

			ItemsToDispose.Add(image1);
			ItemsToDispose.Add(image2);

			return new object[] { image1, image2 };
		}

		protected override void AssertValuesEqual(string message, object lhsObj, object rhsObj)
		{
			Bitmap lhs = lhsObj as Bitmap;
			Bitmap rhs = rhsObj as Bitmap;
			if (lhs != null && rhs != null)
			{
				AssertEquals("Width wrong; " + message, lhs.Width, rhs.Width);
				AssertEquals("Height wrong; " + message, lhs.Height, rhs.Height);
			}
			else
			{
				if (lhsObj == null && rhsObj != null)
				{
					AssertEquals("Width wrong; " + message, 1, rhs.Width);
					AssertEquals("Height wrong; " + message, 1, rhs.Height);
				}
				else
				{
					AssertEquals(message, lhs, rhs);
				}
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (itemsToDispose != null)
			{
				foreach (IDisposable item in itemsToDispose)
				{
					item.Dispose();
				}
			}
		}

		protected List<IDisposable> ItemsToDispose
		{
			get
			{
				if (itemsToDispose == null)
				{
					itemsToDispose = new List<IDisposable>();
				}
				return itemsToDispose;
			}
		}

		List<IDisposable> itemsToDispose;
	}
}
