using System;
using System.Drawing;
using System.IO;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(Watermark))]
	sealed class WatermarkTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("UseTextWatermark", true, BizObj.UseTextWatermark);
			AssertEquals("TextWatermark", "DRAFT", BizObj.TextWatermark);
			AssertEquals("ImageWatermark", null, BizObj.ImageWatermark);
			AssertEquals("HorizontalAlignment", "Centre", BizObj.HorizontalAlignment);
			AssertEquals("VerticalAlignment", "Middle", BizObj.VerticalAlignment);
			AssertEquals("Rotation", 45, BizObj.Rotation);
			AssertEquals("FontSize", 120, BizObj.FontSize);
			AssertEquals("Opacity", 40, BizObj.Opacity);
		}

		public void TestValidateTextWatermark()
		{
			AssertNoErrors("Precondition: TextWatermark should not have errors.", BizObj.TextWatermarkInfo);

			BizObj.UseTextWatermark = false;

			BizObj.TextWatermark = "";
			AssertNoErrors(BizObj.TextWatermarkInfo);

			BizObj.TextWatermark = "ABC";
			AssertNoErrors(BizObj.TextWatermarkInfo);

			BizObj.UseTextWatermark = true;

			BizObj.TextWatermark = "";
			AssertHasError(BizObj.TextWatermarkInfo, "Please enter a Text Watermark.");

			BizObj.TextWatermark = "ABC";
			AssertNoErrors(BizObj.TextWatermarkInfo);
		}

		public void TestValidateImageWatermark()
		{
			AssertNoErrors("Precondition: UseTextWatermark should not have errors.", BizObj.UseTextWatermarkInfo);

			BizObj.UseTextWatermark = true;

			BizObj.ImageWatermark = new Bitmap(1, 1);
			AssertHasError(BizObj.UseTextWatermarkInfo, "The Image Watermark you have selected does not appear to be a valid PNG file. Please select a valid PNG file.");

			BizObj.ImageWatermark = TestPngFile;
			AssertNoErrors(BizObj.UseTextWatermarkInfo);

			BizObj.ImageWatermark = null;
			AssertNoErrors(BizObj.UseTextWatermarkInfo);

			BizObj.UseTextWatermark = false;

			BizObj.ImageWatermark = new Bitmap(1, 1);
			AssertHasError(BizObj.UseTextWatermarkInfo, "The Image Watermark you have selected does not appear to be a valid PNG file. Please select a valid PNG file.");

			BizObj.ImageWatermark = TestPngFile;
			AssertNoErrors(BizObj.UseTextWatermarkInfo);

			BizObj.ImageWatermark = null;
			AssertHasError(BizObj.UseTextWatermarkInfo, "Please select an Image Watermark.");
		}

		public void TestValidateHorizontalAlignment()
		{
			AssertNoErrors("Precondition: HorizontalAlignment should not have errors.", BizObj.HorizontalAlignmentInfo);

			BizObj.HorizontalAlignment = "!@#";
			AssertHasError(BizObj.HorizontalAlignmentInfo, "Enter a valid selection.");

			BizObj.HorizontalAlignment = "";
			AssertHasError(BizObj.HorizontalAlignmentInfo, "Please enter a Horizontal Alignment.");

			BizObj.HorizontalAlignment = "Left";
			AssertNoErrors(BizObj.HorizontalAlignmentInfo);
		}

		public void TestValidateVerticalAlignment()
		{
			AssertNoErrors("Precondition: VerticalAlignment should not have errors.", BizObj.VerticalAlignmentInfo);

			BizObj.VerticalAlignment = "!@#";
			AssertHasError(BizObj.VerticalAlignmentInfo, "Enter a valid selection.");

			BizObj.VerticalAlignment = "";
			AssertHasError(BizObj.VerticalAlignmentInfo, "Please enter a Vertical Alignment.");

			BizObj.VerticalAlignment = "Top";
			AssertNoErrors(BizObj.VerticalAlignmentInfo);
		}

		public void TestValidateRotation()
		{
			AssertNoErrors("Precondition: Rotation should not have errors.", BizObj.RotationInfo);

			BizObj.Rotation = -1;
			AssertHasError(BizObj.RotationInfo, "Please enter a 'Rotation' within the range 0 to 360.");

			BizObj.Rotation = 361;
			AssertHasError(BizObj.RotationInfo, "Please enter a 'Rotation' within the range 0 to 360.");

			for (int i = 0; i < 361; i++)
			{
				BizObj.Rotation = i;
				AssertNoErrors(BizObj.RotationInfo);
			}
		}

		public void TestValidateFontSize()
		{
			AssertNoErrors("Precondition: FontSize should not have errors.", BizObj.FontSizeInfo);

			BizObj.FontSize = 9;
			AssertHasError(BizObj.FontSizeInfo, "Please enter a 'Font Size' within the range 10 to 150.");

			BizObj.FontSize = 151;
			AssertHasError(BizObj.FontSizeInfo, "Please enter a 'Font Size' within the range 10 to 150.");

			for (int i = 10; i < 151; i++)
			{
				BizObj.FontSize = i;
				AssertNoErrors(BizObj.FontSizeInfo);
			}
		}

		public void TestValidateOpacity()
		{
			AssertNoErrors("Precondition: Opacity should not have errors.", BizObj.OpacityInfo);

			BizObj.Opacity = 9;
			AssertHasError(BizObj.OpacityInfo, "Please enter an 'Opacity' within the range 10 to 70.");

			BizObj.Opacity = 71;
			AssertHasError(BizObj.OpacityInfo, "Please enter an 'Opacity' within the range 10 to 70.");

			for (int i = 10; i < 71; i++)
			{
				BizObj.Opacity = i;
				AssertNoErrors(BizObj.OpacityInfo);
			}
		}

		public void TestValidateHorizontalOffset()
		{
			AssertNoErrors("Precondition: HorizontalOffset should not have errors.", BizObj.HorizontalOffsetInfo);

			BizObj.HorizontalOffset = -1;
			AssertHasError(BizObj.HorizontalOffsetInfo, "Please enter a 'Horizontal Offset' within the range 0 to 100.");

			BizObj.HorizontalOffset = 101;
			AssertHasError(BizObj.HorizontalOffsetInfo, "Please enter a 'Horizontal Offset' within the range 0 to 100.");

			for (int i = 0; i < 101; i++)
			{
				BizObj.HorizontalOffset = i;
				AssertNoErrors(BizObj.HorizontalOffsetInfo);
			}
		}

		public void TestValidateVerticalOffset()
		{
			AssertNoErrors("Precondition: VerticalOffset should not have errors.", BizObj.VerticalOffsetInfo);

			BizObj.VerticalOffset = -1;
			AssertHasError(BizObj.VerticalOffsetInfo, "Please enter a 'Vertical Offset' within the range 0 to 100.");

			BizObj.VerticalOffset = 101;
			AssertHasError(BizObj.VerticalOffsetInfo, "Please enter a 'Vertical Offset' within the range 0 to 100.");

			for (int i = 0; i < 101; i++)
			{
				BizObj.VerticalOffset = i;
				AssertNoErrors(BizObj.VerticalOffsetInfo);
			}
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.UseTextWatermark = true;
			BizObj.TextWatermark = "";
			BizObj.ImageWatermark = new Bitmap(1, 1);
			BizObj.HorizontalAlignment = "!@#";
			BizObj.VerticalAlignment = "!@#";
			BizObj.Rotation = 999;
			BizObj.FontSize = 999;
			BizObj.Opacity = 999;
			BizObj.HorizontalOffset = 999;
			BizObj.VerticalOffset = 999;

			BizObj.ClearAllNotifications();

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.TextWatermarkInfo);
			AssertHasErrors(BizObj.UseTextWatermarkInfo);
			AssertHasErrors(BizObj.HorizontalAlignmentInfo);
			AssertHasErrors(BizObj.VerticalAlignmentInfo);
			AssertHasErrors(BizObj.RotationInfo);
			AssertHasErrors(BizObj.FontSizeInfo);
			AssertHasErrors(BizObj.OpacityInfo);
			AssertHasErrors(BizObj.HorizontalOffsetInfo);
			AssertHasErrors(BizObj.VerticalOffsetInfo);
		}

		public void TestHorizontalAlignmentList()
		{
			AssertEquals("HorizontalAlignmentCodes.Left", "Left", Watermark.HorizontalAlignmentCodes.Left);
			AssertEquals("HorizontalAlignmentCodes.Right", "Right", Watermark.HorizontalAlignmentCodes.Right);
			AssertEquals("HorizontalAlignmentCodes.Centre", "Centre", Watermark.HorizontalAlignmentCodes.Centre);

			AssertEquals("Count", 3, BizObj.HorizontalAlignmentList.Count);
			AssertEquals("ContainsCode(\"Left\")", true, BizObj.HorizontalAlignmentList.ContainsCode("Left"));
			AssertEquals("ContainsCode(\"Right\")", true, BizObj.HorizontalAlignmentList.ContainsCode("Right"));
			AssertEquals("ContainsCode(\"Centre\")", true, BizObj.HorizontalAlignmentList.ContainsCode("Centre"));
		}

		public void TestHorizontalAlignmentListDescription()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				mockChs.Put("8725f117-60c9-4b9d-9c17-88de321ed8d2", new ResourceStringData("8725f117-60c9-4b9d-9c17-88de321ed8d2", "左边"));
				mockChs.Put("6b17f28e-9576-4323-b991-e7be99f95261", new ResourceStringData("6b17f28e-9576-4323-b991-e7be99f95261", "中间"));
				mockChs.Put("12be860d-6ac9-4848-956e-135347e8096e", new ResourceStringData("12be860d-6ac9-4848-956e-135347e8096e", "右边"));

				AssertEquals("Left description", "左边", BizObj.HorizontalAlignmentList["Left"].Description);
				AssertEquals("Centre description", "中间", BizObj.HorizontalAlignmentList["Centre"].Description);
				AssertEquals("Right description", "右边", BizObj.HorizontalAlignmentList["Right"].Description);
			}
		}

		public void TestVerticalAlignmentList()
		{
			AssertEquals("VerticalAlignmentCodes.Top", "Top", Watermark.VerticalAlignmentCodes.Top);
			AssertEquals("VerticalAlignmentCodes.Middle", "Middle", Watermark.VerticalAlignmentCodes.Middle);
			AssertEquals("VerticalAlignmentCodes.Bottom", "Bottom", Watermark.VerticalAlignmentCodes.Bottom);

			AssertEquals("Count", 3, BizObj.VerticalAlignmentList.Count);
			AssertEquals("ContainsCode(\"Top\")", true, BizObj.VerticalAlignmentList.ContainsCode("Top"));
			AssertEquals("ContainsCode(\"Middle\")", true, BizObj.VerticalAlignmentList.ContainsCode("Middle"));
			AssertEquals("ContainsCode(\"Bottom\")", true, BizObj.VerticalAlignmentList.ContainsCode("Bottom"));
		}

		public void TestVerticalAlignmentListDescription()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				mockChs.Put("a3393c82-8620-4064-8e6f-22bb5c5bc759", new ResourceStringData("a3393c82-8620-4064-8e6f-22bb5c5bc759", "顶部"));
				mockChs.Put("736201d5-78fd-4053-a71b-8cc2c6649dc4", new ResourceStringData("736201d5-78fd-4053-a71b-8cc2c6649dc4", "中间"));
				mockChs.Put("de4ac43d-e946-429f-b0e6-50291b6361e5", new ResourceStringData("de4ac43d-e946-429f-b0e6-50291b6361e5", "底部"));

				AssertEquals("Top description", "顶部", BizObj.VerticalAlignmentList["Top"].Description);
				AssertEquals("Middle description", "中间", BizObj.VerticalAlignmentList["Middle"].Description);
				AssertEquals("Bottom description", "底部", BizObj.VerticalAlignmentList["Bottom"].Description);
			}
		}

		public void TestImageWatermarkAsBytes()
		{
			BizObj.ImageWatermark = TestPngFile;
			DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BizObj);
			RegistryItemDictionary.Instance.PurgeAll();
			Watermark readValue = DocumentsDataRegistry.Instance.Watermark.Value;
			using (TempFile tempFile = TempFile.New())
			{
				BizObj.ImageWatermark.Save(tempFile.Filename);
				byte[] contents = readValue.ImageWatermarkAsBytes;

				using (MemoryStream stream = new MemoryStream(contents))
				using (Bitmap img = (Bitmap)Bitmap.FromStream(stream))
				{
					AssertEquals("Watermark image and retrieved image are the same height", img.Height,
						BizObj.ImageWatermark.Height);
					AssertEquals("Watermark image and retrieved image are the same width", img.Width,
						BizObj.ImageWatermark.Width);

					for (int x = 0; x < img.Width; x++)
					{
						for (int y = 0; y < img.Height; y++)
						{
							AssertEquals("pixel colour should be the same", img.GetPixel(x, y),
								((Bitmap)BizObj.ImageWatermark).GetPixel(x, y));
						}
					}
				}
			}

			BizObj.ImageWatermark = null;
			DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BizObj);
			RegistryItemDictionary.Instance.PurgeAll();
			readValue = DocumentsDataRegistry.Instance.Watermark.Value;
			AssertNull("byte array should be null", readValue.ImageWatermarkAsBytes);
		}

		#region Implementation

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertImageEquals("ImageWatermark", ((Watermark)originalBusinessObject).ImageWatermark, ((Watermark)newBusinessObject).ImageWatermark);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			Watermark result = new Watermark();

			result.UseTextWatermark = true;
			result.TextWatermark = "Text";
			result.ImageWatermark = TestPngFile;
			result.HorizontalAlignment = "Left";
			result.VerticalAlignment = "Bottom";
			result.Rotation = 10;
			result.FontSize = 100;
			result.Opacity = 35;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new Watermark BizObj
		{
			get { return (Watermark)base.BizObj; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			TestPngFile = Image.FromFile(resourceRetriever.Value.SaveResourceToFile("TransparentPNG.png"));
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestPngFile.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
		Image TestPngFile;

		#endregion
	}
}
