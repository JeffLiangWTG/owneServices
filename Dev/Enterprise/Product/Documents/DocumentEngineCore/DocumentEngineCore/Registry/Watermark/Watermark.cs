using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class Watermark : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string UseTextWatermark = "UseTextWatermark";
			public const string TextWatermark = "TextWatermark";
			public const string ImageWatermark = "ImageWatermark";
			public const string HorizontalAlignment = "HorizontalAlignment";
			public const string VerticalAlignment = "VerticalAlignment";
			public const string Rotation = "Rotation";
			public const string FontSize = "FontSize";
			public const string Opacity = "Opacity";
			public const string HorizontalOffset = "HorizontalOffset";
			public const string VerticalOffset = "VerticalOffset";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			UseTextWatermark = true;
			TextWatermark = "DRAFT";
			HorizontalAlignment = HorizontalAlignmentCodes.Centre;
			VerticalAlignment = VerticalAlignmentCodes.Middle;
			Rotation = 45;
			FontSize = 120;
			Opacity = 40;
			HorizontalOffset = 0;
			VerticalOffset = 0;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			Watermark result = new Watermark();
			result.ImageWatermark = ImageWatermark;
			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateTextWatermark();
			ValidateImageWatermark();
			ValidateHorizontalAlignment();
			ValidateVerticalAlignment();
			ValidateRotation();
			ValidateFontSize();
			ValidateOpacity();
			ValidateHorizontalOffset();
			ValidateVerticalOffset();
		}

		#region Bound Properties

		#region Use Text Watermark

		public ZBool UseTextWatermark
		{
			get { return useTextWatermark; }
			set { SetNonPersistentPropertyValue(UseTextWatermarkInfo, ref useTextWatermark, value); }
		}

		public ZPropertyInfo UseTextWatermarkInfo
		{
			get { return GetZPropertyInfo(Schema.UseTextWatermark); }
		}

		ZBool useTextWatermark;

		#endregion

		#region Text Watermark

		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString TextWatermark
		{
			get { return textWatermark; }
			set
			{
				CheckMaximumLength(TextWatermarkInfo, value);
				SetNonPersistentPropertyValue(TextWatermarkInfo, ref textWatermark, value);
				if (!IsValidationSuspended)
				{
					ValidateTextWatermark();
				}
			}
		}

		public ZPropertyInfo TextWatermarkInfo
		{
			get { return GetZPropertyInfo(Schema.TextWatermark); }
		}

		public void ValidateTextWatermark()
		{
			TextWatermarkInfo.ClearAllNotifications();

			if (UseTextWatermark)
			{
				MandatoryValidation.CheckEntered(TextWatermarkInfo, Res.GetString("4937b3db-557e-4643-a016-4e042d62c5f0", "Text Watermark"));
			}
		}

		ZString textWatermark;

		#endregion

		#region Image Watermark

		public Image ImageWatermark
		{
			get { return fImageWatermark; }
			set
			{
				fImageWatermark = value;

				if (!IsValidationSuspended)
				{
					ValidateImageWatermark();
				}
			}
		}

		// We are actually adding notifications to UseTextWaterMarkInfo.
		public void ValidateImageWatermark()
		{
			UseTextWatermarkInfo.ClearAllNotifications();

			if (!UseTextWatermark && ImageWatermark == null)
			{
				UseTextWatermarkInfo.AddError(Res.GetString("2b4ca2f1-50ca-4be5-91e2-27a34bb8122f", "Please select an Image Watermark."));
			}
			else if (ImageWatermark != null && !ImageWatermark.RawFormat.Equals(ImageFormat.Png))
			{
				UseTextWatermarkInfo.AddError(Res.GetString("8f8de5ad-c9ef-4c96-aa38-250a3d53424b", "The Image Watermark you have selected does not appear to be a valid PNG file. Please select a valid PNG file."));
			}
		}

		Image fImageWatermark;

		#endregion

		#region ImageWatermarkAsBytes

		public byte[] ImageWatermarkAsBytes
		{
			get { return fImageWatermarkAsBytes; }
		}

		byte[] fImageWatermarkAsBytes = Array.Empty<byte>();

		#endregion

		#region Horizontal Alignment

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString HorizontalAlignment
		{
			get { return horizontalAlignment; }
			set
			{
				CheckMaximumLength(HorizontalAlignmentInfo, value);
				SetNonPersistentPropertyValue(HorizontalAlignmentInfo, ref horizontalAlignment, value);
				if (!IsValidationSuspended)
				{
					ValidateHorizontalAlignment();
				}
			}
		}

		public ZPropertyInfo HorizontalAlignmentInfo
		{
			get { return GetZPropertyInfo(Schema.HorizontalAlignment); }
		}

		public void ValidateHorizontalAlignment()
		{
			HorizontalAlignmentInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(HorizontalAlignmentInfo, Res.GetString("4aa90a7a-46cb-47a1-baa5-8601aee8bc6a", "Horizontal Alignment"));
			ListValidation.ErrorIfInvalidCode(HorizontalAlignmentInfo, HorizontalAlignmentList);
		}

		ZString horizontalAlignment;

		#endregion

		#region Vertical Alignment

		[CargoWise.ComponentModel.MaxLength(6)]
		public ZString VerticalAlignment
		{
			get { return verticalAlignment; }
			set
			{
				CheckMaximumLength(VerticalAlignmentInfo, value);
				SetNonPersistentPropertyValue(VerticalAlignmentInfo, ref verticalAlignment, value);
				if (!IsValidationSuspended)
				{
					ValidateVerticalAlignment();
				}
			}
		}

		public ZPropertyInfo VerticalAlignmentInfo
		{
			get { return GetZPropertyInfo(Schema.VerticalAlignment); }
		}

		public void ValidateVerticalAlignment()
		{
			VerticalAlignmentInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(VerticalAlignmentInfo, Res.GetString("f8d02b8e-502b-48e3-9d17-9140bfcbd15c", "Vertical Alignment"));
			ListValidation.ErrorIfInvalidCode(VerticalAlignmentInfo, VerticalAlignmentList);
		}

		ZString verticalAlignment;

		#endregion

		#region Rotation

		public ZInt Rotation
		{
			get { return rotation; }
			set
			{
				SetNonPersistentPropertyValue(RotationInfo, ref rotation, value);
				if (!IsValidationSuspended)
				{
					ValidateRotation();
				}
			}
		}

		public ZPropertyInfo RotationInfo
		{
			get { return GetZPropertyInfo(Schema.Rotation); }
		}

		public void ValidateRotation()
		{
			RotationInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(RotationInfo, 0, 360);
		}

		ZInt rotation;

		#endregion

		#region Font Size

		public ZInt FontSize
		{
			get { return fontSize; }
			set
			{
				SetNonPersistentPropertyValue(FontSizeInfo, ref fontSize, value);
				if (!IsValidationSuspended)
				{
					ValidateFontSize();
				}
			}
		}

		public ZPropertyInfo FontSizeInfo
		{
			get { return GetZPropertyInfo(Schema.FontSize); }
		}

		public void ValidateFontSize()
		{
			FontSizeInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(FontSizeInfo, 10, 150);
		}

		ZInt fontSize;

		#endregion

		#region Opacity

		public ZInt Opacity
		{
			get { return opacity; }
			set
			{
				SetNonPersistentPropertyValue(OpacityInfo, ref opacity, value);
				if (!IsValidationSuspended)
				{
					ValidateOpacity();
				}
			}
		}

		public ZPropertyInfo OpacityInfo
		{
			get { return GetZPropertyInfo(Schema.Opacity); }
		}

		public void ValidateOpacity()
		{
			OpacityInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(OpacityInfo, 10, 70);
		}

		ZInt opacity;

		#endregion

		#region HorizontalOffset

		public ZInt HorizontalOffset
		{
			get { return horizontalOffset; }
			set
			{
				SetNonPersistentPropertyValue(HorizontalOffsetInfo, ref horizontalOffset, value);
				if (!IsValidationSuspended)
				{
					ValidateHorizontalOffset();
				}
			}
		}

		public ZPropertyInfo HorizontalOffsetInfo
		{
			get { return GetZPropertyInfo(Schema.HorizontalOffset); }
		}

		public void ValidateHorizontalOffset()
		{
			HorizontalOffsetInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(HorizontalOffsetInfo, 0, 100);
		}

		ZInt horizontalOffset;

		#endregion

		#region VerticalOffset

		public ZInt VerticalOffset
		{
			get { return verticalOffset; }
			set
			{
				SetNonPersistentPropertyValue(VerticalOffsetInfo, ref verticalOffset, value);
				if (!IsValidationSuspended)
				{
					ValidateVerticalOffset();
				}
			}
		}

		public ZPropertyInfo VerticalOffsetInfo
		{
			get { return GetZPropertyInfo(Schema.VerticalOffset); }
		}

		public void ValidateVerticalOffset()
		{
			VerticalOffsetInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(VerticalOffsetInfo, 0, 100);
		}

		ZInt verticalOffset;

		#endregion

		#endregion

		#region Lookups

		#region Horizontal Alignment List

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public abstract class HorizontalAlignmentCodes
		{
			public const string Left = "Left";
			public const string Centre = "Centre";
			public const string Right = "Right";
		}

		public static class HorizontalAlignmentDescriptions
		{
			public static readonly MultilingualString Left = ResString.GetMultilingualString("8725f117-60c9-4b9d-9c17-88de321ed8d2", "Left");
			public static readonly MultilingualString Centre = ResString.GetMultilingualString("6b17f28e-9576-4323-b991-e7be99f95261", "Center");
			public static readonly MultilingualString Right = ResString.GetMultilingualString("12be860d-6ac9-4848-956e-135347e8096e", "Right");
		}

		public CodeDescriptionPairList HorizontalAlignmentList
		{
			get
			{
				if (fHorizontalAlignmentList == null)
				{
					fHorizontalAlignmentList = new CodeDescriptionPairList();
					fHorizontalAlignmentList.AddPair(HorizontalAlignmentCodes.Left, HorizontalAlignmentDescriptions.Left);
					fHorizontalAlignmentList.AddPair(HorizontalAlignmentCodes.Centre, HorizontalAlignmentDescriptions.Centre);
					fHorizontalAlignmentList.AddPair(HorizontalAlignmentCodes.Right, HorizontalAlignmentDescriptions.Right);
				}

				return fHorizontalAlignmentList;
			}
		}

		CodeDescriptionPairList fHorizontalAlignmentList;

		#endregion

		#region Vertical Alignment List
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public abstract class VerticalAlignmentCodes
		{
			public const string Top = "Top";
			public const string Middle = "Middle";
			public const string Bottom = "Bottom";
		}

		public static class VerticalAlignmentDescriptions
		{
			public static readonly MultilingualString Top = ResString.GetMultilingualString("a3393c82-8620-4064-8e6f-22bb5c5bc759", "Top");
			public static readonly MultilingualString Middle = ResString.GetMultilingualString("736201d5-78fd-4053-a71b-8cc2c6649dc4", "Middle");
			public static readonly MultilingualString Bottom = ResString.GetMultilingualString("de4ac43d-e946-429f-b0e6-50291b6361e5", "Bottom");
		}

		public CodeDescriptionPairList VerticalAlignmentList
		{
			get
			{
				if (fVerticalAlignmentList == null)
				{
					fVerticalAlignmentList = new CodeDescriptionPairList();
					fVerticalAlignmentList.AddPair(VerticalAlignmentCodes.Top, VerticalAlignmentDescriptions.Top);
					fVerticalAlignmentList.AddPair(VerticalAlignmentCodes.Middle, VerticalAlignmentDescriptions.Middle);
					fVerticalAlignmentList.AddPair(VerticalAlignmentCodes.Bottom, VerticalAlignmentDescriptions.Bottom);
				}

				return fVerticalAlignmentList;
			}
		}

		CodeDescriptionPairList fVerticalAlignmentList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.UseTextWatermark, UseTextWatermark.ToString());
			writer.WriteElementString(Schema.TextWatermark, TextWatermark);
			writer.WriteElementString(Schema.HorizontalAlignment, HorizontalAlignment);
			writer.WriteElementString(Schema.VerticalAlignment, VerticalAlignment);
			writer.WriteElementString(Schema.Rotation, Rotation.ToString());
			writer.WriteElementString(Schema.FontSize, FontSize.ToString());
			writer.WriteElementString(Schema.Opacity, Opacity.ToString());
			writer.WriteElementString(Schema.HorizontalOffset, HorizontalOffset.ToString());
			writer.WriteElementString(Schema.VerticalOffset, VerticalOffset.ToString());

			using (MemoryStream imageStream = new MemoryStream())
			{
				if (ImageWatermark != null)
				{
					ImageWatermark.Save(imageStream, ImageFormat.Png);
				}

				writer.WriteElementString(Schema.ImageWatermark, Convert.ToBase64String(imageStream.ToArray()));
			}
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UseTextWatermark = new ZBool(reader.ReadElementString(Schema.UseTextWatermark));
			TextWatermark = reader.ReadElementString(Schema.TextWatermark);
			HorizontalAlignment = reader.ReadElementString(Schema.HorizontalAlignment);
			VerticalAlignment = reader.ReadElementString(Schema.VerticalAlignment);
			Rotation = ZInt.Parse(reader.ReadElementString(Schema.Rotation));
			FontSize = ZInt.Parse(reader.ReadElementString(Schema.FontSize));
			Opacity = ZInt.Parse(reader.ReadElementString(Schema.Opacity));
			HorizontalOffset = ZInt.Parse(reader.ReadElementString(Schema.HorizontalOffset));
			VerticalOffset = ZInt.Parse(reader.ReadElementString(Schema.VerticalOffset));

			byte[] imageBytes = Convert.FromBase64String(reader.ReadElementString(Schema.ImageWatermark));
			ImageWatermark = (imageBytes.Length == 0) ? null : Image.FromStream(new MemoryStream(imageBytes));
			fImageWatermarkAsBytes = (imageBytes.Length == 0) ? null : imageBytes;
		}

		#endregion
	}
}
