using System;
using System.Drawing;
using System.IO;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class DeliveryInfoTest : TransactionedTestCase
	{
		public void TestIsReportFormat()
		{
			Assert("Document format set by default", Info.DeliveryFormat == DeliveryInfo.DeliveryFormats.Document);
		}

		public void TestFileContents()
		{
			AssertEquals(0, Info.FileContents.Length);
			byte[] contents = new byte[3] { 1, 2, 3 };
			Info.FileContents.Write(contents, 0, contents.Length);
			AssertEquals(3, Info.FileContents.Length);
		}

		public void TestEmailSubjectLine()
		{
			AssertEquals("", Info.EmailSubjectLine);
			Info.EmailSubjectLine = "my subject line";
			AssertEquals("my subject line", Info.EmailSubjectLine);
		}

		public void TestAttachedFilename()
		{
			AssertEquals("", Info.AttachedFilename);
			Info.AttachedFilename = "blah.txt";
			AssertEquals("blah.txt", Info.AttachedFilename);
		}

		public void TestEmailFromAddress()
		{
			AssertEquals("", Info.EmailFromAddress);
			Info.EmailFromAddress = "abc@edi.com.au";
			AssertEquals("abc@edi.com.au", Info.EmailFromAddress);
		}

		public void TestEmailSignature()
		{
			AssertEquals("", Info.EmailSignature);
			Info.EmailSignature = "DEMO Company Brand Name";
			AssertEquals("DEMO Company Brand Name", Info.EmailSignature);
		}

		public void TestParentTableName()
		{
			AssertEquals("", Info.ParentTableName);
			Info.ParentTableName = "my table name";
			AssertEquals("my table name", Info.ParentTableName);
		}

		public void TestRelatedBusinessContext()
		{
			AssertEquals("", Info.RelatedBusinessContext);
			Info.RelatedBusinessContext = "XXX";
			AssertEquals("XXX", Info.RelatedBusinessContext);
		}

		public void TestDocumentType()
		{
			AssertEquals("", Info.DocumentType);
			Info.DocumentType = "ABC";
			AssertEquals("ABC", Info.DocumentType);
		}

		public void TestParentGuid()
		{
			Assert(Info.ParentGuid.IsEmpty);
			ZGuid testGuid = ZGuid.NewZGuid();
			Info.ParentGuid = testGuid;
			AssertEquals(testGuid, Info.ParentGuid);
		}

		public void TestTrailingSpace()
		{
			AssertEquals("Default", 0, Info.TrailingSpace);
			Info.TrailingSpace = 4;
			AssertEquals("Set value", 4, Info.TrailingSpace);
		}

		public void TestCopies()
		{
			AssertEquals("Default", (short)1, Info.Copies);
			Info.Copies = 6;
			AssertEquals("Set value", (short)6, Info.Copies);
		}

		public void TestIsCoverSheet()
		{
			AssertEquals("Default", false, Info.IsCoverSheet);
			Info.IsCoverSheet = true;
			AssertEquals("Set value", true, Info.IsCoverSheet);
		}

		public void TestIsSuitableForMerge()
		{
			Info.IsCoverSheet = true;
			AssertEquals("Not suitable for merge - cover sheet", false, Info.IsSuitableForMerge);

			Info.IsCoverSheet = false;
			Info.DeliveryFormat = DeliveryInfo.DeliveryFormats.Document;
			AssertEquals("suitable for merge - delivery format of document", true, Info.IsSuitableForMerge);

			Info.DeliveryFormat = DeliveryInfo.DeliveryFormats.Report;
			AssertEquals("Suitable for merge - delivery format of report", true, Info.IsSuitableForMerge);

			Info.DeliveryFormat = DeliveryInfo.DeliveryFormats.TIFF;
			AssertEquals("not suitable for merge - delivery format of tif", false, Info.IsSuitableForMerge);
		}

		public void TestJobSubmittedBy()
		{
			AssertEquals("Default is the current user", GlbStaff.CurrentUser.GS_Code, Info.JobSubmittedBy);

			Info.JobSubmittedBy = "ABC";
			AssertEquals("set value", "ABC", Info.JobSubmittedBy);
		}

		public void TestIsDraft()
		{
			DeliveryInfo info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			AssertEquals("Draft should be false by default", false, info.ShowDraftWatermark);
			info.ShowDraftWatermark = true;
			AssertEquals("Draft should now be true", true, info.ShowDraftWatermark);
			info.ShowDraftWatermark = false;
			AssertEquals("Draft should now be false", false, info.ShowDraftWatermark);
		}

		public void TestFilePath()
		{
			DeliveryInfo info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			AssertNull("file path should be empty by default", info.FilePath);

			var filePath = Path.Combine(Env.TempPath, "1.pdf");
			info.FilePath = filePath;
			AssertEquals("file path should not be empty", filePath, info.FilePath);
		}

		public void TestPDFEncryptionPassword()
		{
			DeliveryInfo info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			AssertNullOrEmpty("PDFEncryptionPassword should be empty by default", info.PDFEncryptionPassword);

			info.PDFEncryptionPassword = "AAAA";
			AssertEquals("PDFEncryptionPassword should not be empty", "AAAA", info.PDFEncryptionPassword);
		}

		public void TestWatermark()
		{
			var previousValue = DocumentsDataRegistry.Instance.Watermark.Value;
			try
			{
				var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				{
					using (var image = Image.FromFile(TransparentPNGPath))
					{
						var newRegistry = new DocumentEngineCore.Registry.Watermark();
						newRegistry.TextWatermark = "Hello";
						newRegistry.HorizontalAlignment = "Left";
						newRegistry.VerticalAlignment = "Top";
						newRegistry.HorizontalOffset = 10;
						newRegistry.VerticalOffset = 20;
						newRegistry.Rotation = 65;
						newRegistry.FontSize = 90;
						newRegistry.Opacity = 30;
						newRegistry.ImageWatermark = image;
						DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistry);

						var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
						var watermark = info.Watermark;
						AssertNull("Should be null by default if the info is not marked 'IsDraft'", watermark);

						info.ShowDraftWatermark = true;
						watermark = info.Watermark;
						var textWatermark = watermark as TextWatermark;
						AssertNotNull("Watermark should be text watermark", textWatermark);
						AssertEquals("Text should be the value from the registry", "Hello", textWatermark.AsText);
						AssertEquals(watermark.AsImage, null);
						AssertEquals("Should get value from registry", WatermarkHorizontalAlign.Left, textWatermark.HorizontalAlign);
						AssertEquals("Should get value from registry", WatermarkVerticalAlign.Top, textWatermark.VerticalAlign);
						AssertEquals("Should get value from registry", 10f, textWatermark.HorizontalOffset);
						AssertEquals("Should get value from registry", 20f, textWatermark.VerticalOffset);
						AssertEquals("Should get value from registry", 65, textWatermark.Rotation);
						AssertEquals("Should get value from registry", 90, textWatermark.FontSize);
						AssertEquals("Should get value from registry", Color.FromArgb(30, 0, 0, 0), textWatermark.TextColor);

						newRegistry.UseTextWatermark = false;
						DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistry);

						info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
						info.ShowDraftWatermark = true;
						watermark = info.Watermark;
						AssertNotNull("Watermark should be image", watermark.AsImage);
						AssertEquals("Watermark text should be empty", string.Empty, watermark.AsText);
						AssertEquals("Should get value from registry", WatermarkHorizontalAlign.Left, watermark.HorizontalAlign);
						AssertEquals("Should get value from registry", WatermarkVerticalAlign.Top, watermark.VerticalAlign);
						AssertEquals("Should get value from registry", 10f, watermark.HorizontalOffset);
						AssertEquals("Should get value from registry", 20f, watermark.VerticalOffset);
					}
				}

				registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
				{
					var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
					var watermark = info2.Watermark;
					var nonCommercialUseWatermark = watermark as TextWatermark;
					AssertNotNull("Watermark should be text watermark", nonCommercialUseWatermark);
					AssertEquals("Text should be default value for non commercial use watermark", WatermarkHelper.NonCommercialUseWatermarkText, nonCommercialUseWatermark.AsText);
					AssertEquals(watermark.AsImage, null);
					AssertEquals("Should get default value for non commercial use watermark", WatermarkHorizontalAlign.Centre, nonCommercialUseWatermark.HorizontalAlign);
					AssertEquals("Should get default value for non commercial use watermark", WatermarkVerticalAlign.Middle, nonCommercialUseWatermark.VerticalAlign);
					AssertEquals("Should get default value for non commercial use watermark", 0f, nonCommercialUseWatermark.HorizontalOffset);
					AssertEquals("Should get default value for non commercial use watermark", 0f, nonCommercialUseWatermark.VerticalOffset);
					AssertEquals("Should get default value for non commercial use watermark", 45, nonCommercialUseWatermark.Rotation);
					AssertEquals("Should get default value for non commercial use watermark", 52, nonCommercialUseWatermark.FontSize);
					AssertEquals("Should get default value for non commercial use watermark", Color.FromArgb(60, 0, 0, 0), nonCommercialUseWatermark.TextColor);
				}
			}
			finally
			{
				DocumentsDataRegistry.Instance.Watermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousValue);
			}
		}

		public void TestCustomWatermarkText()
		{
			using (var image = Image.FromFile(TransparentPNGPath))
			{
				var newRegistry = new DocumentEngineCore.Registry.Watermark();
				newRegistry.TextWatermark = "text1";
				newRegistry.HorizontalAlignment = "Left";
				newRegistry.VerticalAlignment = "Top";
				newRegistry.HorizontalOffset = 10;
				newRegistry.VerticalOffset = 20;
				newRegistry.Rotation = 65;
				newRegistry.FontSize = 90;
				newRegistry.Opacity = 30;
				newRegistry.ImageWatermark = image;

				using (DocumentsDataRegistry.Instance.Watermark.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistry))
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
					registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
					{
						var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
						AssertNullOrEmpty(info.CustomWatermarkText);
						AssertNull("Should be null if CustomWatermarkText is empty", info.Watermark);

						info.CustomWatermarkText = (NoResString)"text2";
						Assert(!info.ShowDraftWatermark);
						AssertEquals("text2", info.Watermark.AsText);

						info.ShowDraftWatermark = true;
						Assert(info.ShowDraftWatermark);
						AssertEquals("text2", info.Watermark.AsText);
					}

					registrationKey.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
					{
						var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
						info2.CustomWatermarkText = (NoResString)"text3";
						AssertEquals("Text should be default value for non commercial use watermark", WatermarkHelper.NonCommercialUseWatermarkText, info2.Watermark.AsText);
					}
				}
			}
		}

		public void TestHasDataForCurrentFileFormat()
		{
			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			Assert(info.HasDataForCurrentFileFormat);
			info.HasDataForCurrentFileFormat = false;
			Assert(!info.HasDataForCurrentFileFormat);
		}

		public void TestConstructor_WhenCurrentStaffIsNull_ShouldNotThrow()
		{
			using (Enterprise.Environment.Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(GlbStaff.CurrentUser);
				AssertNoExceptionThrown(() => new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document));
			}
		}

		DeliveryInfo Info;
		protected override void SetUp()
		{
			base.SetUp();
			Info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string transparentPNGPath;
		string TransparentPNGPath
		{
			get
			{
				if (string.IsNullOrEmpty(transparentPNGPath))
				{
					transparentPNGPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TransparentPNG.png");
				}
				return transparentPNGPath;
			}
		}
	}
}
