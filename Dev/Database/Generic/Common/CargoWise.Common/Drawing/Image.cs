using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

namespace CargoWise.Common
{
	public static class ImageExtensions
	{
		public static void AddPage(this Image image, Image pageToAdd)
		{
			Argument.NotNull(image, nameof(image));
			image.SaveAdd(pageToAdd, GetEncoderParameters(Encoder.SaveFlag, EncoderValue.FrameDimensionPage));
		}

		static EncoderParameters GetEncoderParameters(Encoder encoder, EncoderValue value)
		{
			var result = new EncoderParameters();
			result.Param[0] = new EncoderParameter(encoder, (long)value);
			return result;
		}

		#region IsDisposed

		public static bool IsDisposed(this Image image)
		{
			if (GetNativeImagePtr == null)
			{
				if (!fieldSearchPerformed)
				{
					FieldInfo field = typeof(Image).GetField("nativeImage", BindingFlags.Instance | BindingFlags.NonPublic);
					GetNativeImagePtr = field != null ? field.GetValue : null;
					fieldSearchPerformed = true;
				}

				if (GetNativeImagePtr == null)
				{
					try
					{
						return image.Width < 0; // Will fail here with exception if image was disposed
					}
					catch (ArgumentException)
					{
						return true;
					}
				}
			}
			return (IntPtr)GetNativeImagePtr(image) == IntPtr.Zero;
		}

		delegate object GetNativeImagePtrDelegate(object componet);

		[ThreadStatic]
		static GetNativeImagePtrDelegate GetNativeImagePtr;

		[ThreadStatic]
		static bool fieldSearchPerformed;

		#endregion
	}
}
