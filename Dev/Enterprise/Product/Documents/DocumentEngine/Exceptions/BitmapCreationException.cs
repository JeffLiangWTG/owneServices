using System;
using System.Drawing.Imaging;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class BitmapCreationException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal BitmapCreationException(Exception innerException, int width, int height, float resolution, PixelFormat pixFmt)
			: base(null, innerException)
		{
			this.Resolution = Convert.ToDecimal(resolution);
			ControlDpiScalingHelper.SetWidth(this, width, false);
			ControlDpiScalingHelper.SetHeight(this, height, false);
			this.PixFmt = pixFmt;
		}

#if DEBUG
		public static BitmapCreationException NewForTesting(Exception innerException, int width, int height, float resolution, PixelFormat pixFmt)
		{
			return new BitmapCreationException(innerException, width, height, resolution, pixFmt);
		}
#endif

#if NETFRAMEWORK
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Binary Serialiization Key")]
		protected BitmapCreationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
			this.Resolution = info.GetDecimal((NoResString)"Resolution");
			ControlDpiScalingHelper.SetWidth(this, info.GetInt32((NoResString)"Width"), false);
			ControlDpiScalingHelper.SetHeight(this, info.GetInt32("Height"), false);
			string pixelFormat = info.GetString("PixelFormat");
			if (!string.IsNullOrEmpty(pixelFormat) && Enum.IsDefined(typeof(PixelFormat), pixelFormat))
			{
				this.PixFmt = (PixelFormat)Enum.Parse(typeof(PixelFormat), pixelFormat);
			}
		}
#endif

		#region Constructor For IJsonSerializable

		internal BitmapCreationException(BitmapCreationExceptionJsonData data)
			: base(null, new Exception(data.InnerExceptionMessage))
		{
			Resolution = data.Resolution;
			Width = data.Width;
			Height = data.Height;
			PixFmt = data.PixFmt;
		}

		#endregion

		public Decimal Resolution { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }
		public PixelFormat PixFmt { get; private set; }

		public bool IsDimensionOutsideRange
		{
			get { return (Width > 9999 || Height > 9999 || Width < 1 || Height < 1); }
		}

		public override String Message
		{
			get
			{
				return Res.GetString("2c692849-a10b-4b42-a3af-6bda4d84a19e", @"Bitmap creation failed. 
Your system might be low on memory or system resources, please close all other tasks, exit {0}, come back in and try again.
Parameters are: 
  Width = {1}
  Height = {2}
  Resolution = {3}
  Pixel Format = {4}",
				Core.Constants.ProductName,
				Width,
				Height,
				Resolution,
				PixFmt);
			}
		}

#if NET
		[Obsolete]
#endif
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Binary Serialiization Key")]
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Resolution", Resolution);
			info.AddValue("Width", Width);
			info.AddValue("Height", Height);
			info.AddValue("PixelFormat", PixFmt.ToString());
		}

		#region IJsonSerializable Members

		public object GetJsonData() =>
			new BitmapCreationExceptionJsonData
			{
				Resolution = Resolution,
				Width = Width,
				Height = Height,
				PixFmt = PixFmt,
				InnerExceptionMessage = InnerException?.Message,
			};

		#endregion
	}
}
