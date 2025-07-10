using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class ImageRegistryDataType : RegistryDataType<Image>
	{
		public ImageRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, null)
		{
		}

		public ImageRegistryDataType(int maxWidth, int maxHeight)
			: this()
		{
			MaxWidth = maxWidth;
			MaxHeight = maxHeight;
		}

		public ImageRegistryDataType(int maxWidth, int maxHeight, int minWidth, int minHeight)
			: this(maxWidth, maxHeight)
		{
			MinWidth = minWidth;
			MinHeight = minHeight;
		}

		protected override bool ValuesAreEqualCore(Image a, Image b)
		{
			return ImagesAreIdentical(a, b);
		}

		protected override bool AllowNullCore
		{
			get { return true; }
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}

		protected override void ValidateCore(IRegistryItem registryItem, Image proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (proposedValue == null)
			{
				return;
			}

			var exceptionMessage = new List<string>();

			if (proposedValue.Height > MaxHeight)
			{
				exceptionMessage.Add(Res.GetString("687bae22-8e8f-4e91-a2ec-bc313c407fd3", "The height of this image cannot be greater than {0}, it's currently {1}.", MaxHeight, proposedValue.Height));
			}
			else if (proposedValue.Height < MinHeight)
			{
				exceptionMessage.Add(Res.GetString("43b275b7-7f15-4ddc-acbb-b58658503f59", "The height of this image cannot be less than {0}, it's currently {1}.", MinHeight, proposedValue.Height));
			}

			if (proposedValue.Width > MaxWidth)
			{
				exceptionMessage.Add(Res.GetString("7f7a0898-03f2-434f-9dad-8feef85d42cb", "The width of this image cannot be greater than {0}, it's currently {1}.", MaxWidth, proposedValue.Width));
			}
			else if (proposedValue.Width < MinWidth)
			{
				exceptionMessage.Add(Res.GetString("b6c6d1d9-8716-4129-9739-a066981c63fd", "The width of this image cannot be less than {0}, it's currently {1}.", MinWidth, proposedValue.Width));
			}

			if (!exceptionMessage.IsNullOrEmpty())
			{
				throw new RegistryValidationException(string.Join(System.Environment.NewLine, exceptionMessage));
			}
		}

		protected override Image CloneValue(Image value)
		{
			return (Image)value.Clone();
		}

		public override bool IsDeserializedDataAlive(object value)
		{
			Image image = value as Image;
			return image != null && !image.IsDisposed();
		}

		#region Max Width/Height

		public int MaxWidth
		{
			get { return maxWidth; }
			set { maxWidth = value; }
		}

		public int MaxHeight
		{
			get { return maxHeight; }
			set { maxHeight = value; }
		}

		int maxWidth = int.MaxValue;
		int maxHeight = int.MaxValue;

		#endregion

		#region Min Width/Height

		public int MinWidth
		{
			get { return minWidth; }
			set { minWidth = value; }
		}

		public int MinHeight
		{
			get { return minHeight; }
			set { minHeight = value; }
		}

		int minWidth;
		int minHeight;

		#endregion

		#region Serialisation

		bool deserializationHasFailedBefore;

		protected override Image DeserialiseCore(byte[] value)
		{
			Image result = null;
			MemoryStream stream = null;
			if (value.Length > 0)
			{
				try
				{
					stream = new MemoryStream(value); // Can't dispose the stream because the image holds a reference to it.
					result = Image.FromStream(stream);
				}
				catch (OutOfMemoryException) //GDI+ generic error
				{
					if (stream != null)
					{
						stream.Dispose();
					}
					if (!deserializationHasFailedBefore)
					{
						deserializationHasFailedBefore = true;
						throw; //we are going to catch and report in RegistryItemImpl.Deserialise so that we have access to name, etc. and can make a better report
					}
				}
			}
			return result;
		}

		protected override byte[] SerialiseCore(Image value)
		{
			byte[] result = Array.Empty<byte>();

			if (value != null)
			{
				using (TempFile tempFile = TempFile.New())
				{
					// Save to file rather than using streams so that we can keep the image's format.
					value.Save(tempFile.Filename);
					result = File.ReadAllBytes(tempFile.Filename);
				}
			}
			else
			{
				result = MagicNullImage;
			}

			return result;
		}

		#endregion

		public override bool IsNullDataRepresentation(object value)
		{
			if (value == null)
			{
				return true;
			}

			if (value is byte[])
			{
				return IsNullDataRepresentation(value as byte[]);
			}

			if (value is Image)
			{
				return IsNullDataRepresentation(value as Image);
			}

			return false;
		}

		public static bool IsNullDataRepresentation(byte[] value)
		{
			return IsNullImage(value);
		}

		public static bool IsNullDataRepresentation(Image value)
		{
			return value == null || ImagesAreIdentical(value, MagicNullBitmap);
		}

		static bool ImagesAreIdentical(Image a, Image b)
		{
			lock (a)
			{
				if (ReferenceEquals(a, b))
				{
					return true;
				}
				else if (a == null || b == null || (a.PhysicalDimension != b.PhysicalDimension))
				{
					return false;
				}
				else
				{
					var format = ImageFormat.Png;
					return ImageAsBytes(a, format).SequenceEqual(ImageAsBytes(b, format));
				}
			}
		}

		static byte[] ImageAsBytes(Image image, ImageFormat format)
		{
			using (var stream = new MemoryStream(image.Width * image.Height))
			{
				image.Save(stream, format);
				return stream.ToArray();
			}
		}

		static bool IsNullImage(byte[] value)
		{
			return value.Length == MagicNullImage.Length && value.SequenceEqual(MagicNullImage);
		}

		internal static Bitmap MagicNullBitmap
		{
			get { return new Bitmap(new MemoryStream(ImageRegistryDataType.MagicNullImage)); }
		}

		//1x1 transparent image
		internal static readonly byte[] MagicNullImage = new byte[] { 71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 247, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 33, 249, 4, 1, 0, 0, 255, 0, 44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 8, 4, 0, 255, 5, 4, 0, 59 };
	}
}
