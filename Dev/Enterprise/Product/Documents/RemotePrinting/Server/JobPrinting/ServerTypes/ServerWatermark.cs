using System;
using System.IO;
using System.Xml;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	public class ServerWatermark
	{
		public ServerWatermark()
		{
		}

		public ServerWatermark(byte[] binaryXml)
		{
			InitialiseProperties(binaryXml);
		}

		#region Properties

		public bool UseTextWatermark
		{
			get { return fUseTextWatermark; }
			set { fUseTextWatermark = value; }
		}

		protected bool fUseTextWatermark = true;

		public string TextWatermark
		{
			get { return fTextWatermark; }
			set { fTextWatermark = value; }
		}

		protected string fTextWatermark = "DRAFT";

		public byte[] ImageWatermark
		{
			get { return fImageWatermark; }
			set { fImageWatermark = value; }
		}

		protected byte[] fImageWatermark = Array.Empty<byte>();

		public string HorizontalAlignment
		{
			get { return fHorizontalAlignment; }
			set { fHorizontalAlignment = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected string fHorizontalAlignment = "Centre";

		public string VerticalAlignment
		{
			get { return fVerticalAlignment; }
			set { fVerticalAlignment = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected string fVerticalAlignment = "Middle";

		public int Rotation
		{
			get { return fRotation; }
			set { fRotation = value; }
		}

		protected int fRotation = 45;

		public int FontSize
		{
			get { return fFontSize; }
			set { fFontSize = value; }
		}

		protected int fFontSize = 120;

		public int Opacity
		{
			get { return fOpacity; }
			set { fOpacity = value; }
		}

		protected int fOpacity = 40;

		public int HorizontalOffset
		{
			get { return fHorizontalOffset; }
			set { fHorizontalOffset = value; }
		}

		protected int fHorizontalOffset;

		public int VerticalOffset
		{
			get { return fVerticalOffset; }
			set { fVerticalOffset = value; }
		}

		protected int fVerticalOffset;

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void InitialiseProperties(byte[] binaryXml)
		{
			if (binaryXml != null && binaryXml.Length > 0)
			{
				using (MemoryStream stream = new MemoryStream(binaryXml))
				using (XmlTextReader reader = new XmlTextReader(stream))
				{
					reader.ReadStartElement("Watermark");

					var userTextWatermarkString = reader.ReadElementString("UseTextWatermark");
					fUseTextWatermark = (userTextWatermarkString.Trim().ToUpper() == "Y");
					fTextWatermark = reader.ReadElementString("TextWatermark");
					fHorizontalAlignment = reader.ReadElementString("HorizontalAlignment");
					fVerticalAlignment = reader.ReadElementString("VerticalAlignment");
					fRotation = Convert.ToInt32(reader.ReadElementString("Rotation"));
					fFontSize = Convert.ToInt32(reader.ReadElementString("FontSize"));
					fOpacity = Convert.ToInt32(reader.ReadElementString("Opacity"));
					fHorizontalOffset = Convert.ToInt32(reader.ReadElementString("HorizontalOffset"));
					fVerticalOffset = Convert.ToInt32(reader.ReadElementString("VerticalOffset"));
					var imageWatermark = reader.ReadElementString("ImageWatermark");
					fImageWatermark = Convert.FromBase64String(imageWatermark);
				}
			}
		}
	}
}
