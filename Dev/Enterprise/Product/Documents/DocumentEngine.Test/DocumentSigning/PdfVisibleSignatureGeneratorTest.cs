using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using FlexCel.Core;
using FlexCel.Pdf;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	public class PdfVisibleSignatureGeneratorTest : TestCaseWithFactory
	{
		TPdfSignerFactory PlaceholderSignerFactory => new PdfSignatureFactory.PdfSignerFactory(new PlaceholderPdfSigner());
		readonly Func<ZGuid?, PdfVisibleSignatureGenerator> generatorForTestCreate = (ZGuid? branch) => new PdfVisibleSignatureGenerator(new TPaperDimensions(TPaperSize.A4), "signerName", 1, branch);
		PdfVisibleSignatureGenerator GeneratorForTest => generatorForTestCreate(Env.CurrentBranchPK);
		TUISize A4PaperSizeInPoints => new TPaperDimensions(TPaperSize.A4).SizeInPoints;

		readonly Lazy<ImageHelper> imageHelper = new Lazy<ImageHelper>(() => new ImageHelper());

		protected override void SetUp()
		{
			base.SetUp();
			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, default);
			DocumentsDataRegistry.Instance.OverrideSignedByUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, default);
			DocumentsDataRegistry.Instance.SignatureBackgroundImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, default);
			DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, default);
			DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, default);
			DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, default);
		}

		protected override void TearDown()
		{
			if (imageHelper.IsValueCreated)
			{
				imageHelper.Value.Dispose();
			}
		}

		public void TestVisibleSignatureSize()
		{
			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "short");
			DocumentsDataRegistry.Instance.OverrideSignedByUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Jason D' Signer");
			var result = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);
			AssertEquals(128, (int)result.Rect.Width);
		}

		public void TestVisibleSignatureSize_LongText()
		{
			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				"this is a very long name consisting of 80 characters which is the maximum length");
			var result = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);
			AssertGreaterThan(result.Rect.Width, 250);
		}

		public void TestVisibleSignatureSize_WithBackgroundImage()
		{
			DocumentsDataRegistry.Instance.SignatureBackgroundImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(400, 100));
			var result = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);
			AssertEquals(144, (int)result.Rect.Width);
		}

		public void TestVisibleSignatureAnchor()
		{
			var systemAnchor = DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft;
			var companyAnchor = DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.TopRight;

			var factory = new BusinessObjectFactory();

			var testCompany = factory.NewWithValidTestData<GlbCompany>();
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_GB_HomeBranch = testBranch.PK;

			var wrongCompany = factory.NewWithValidTestData<GlbCompany>();
			var wrongBranch = factory.NewWithValidTestData<GlbBranch>();
			wrongBranch.GB_GC = wrongCompany.PK;
			var wrongStaff = factory.NewWithValidTestData<GlbStaff>();
			wrongStaff.GS_GB_HomeBranch = wrongBranch.PK;

			factory.Save();

			DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemAnchor);
			DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyAnchor);
			DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0);
			DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0);

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var resultSystem = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);

				AssertNotNull(resultSystem);
				AssertEquals(0.0, resultSystem.Rect.X);
				AssertEquals(0.0, resultSystem.Rect.Y);
			}

			using (Env.SetTemporaryUserContext(testCompany.PK.ToGuid(), testBranch.PK.ToGuid(), Guid.Empty))
			{
				var resultCompany = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);

				AssertNotNull(resultCompany);
				AssertEquals(A4PaperSizeInPoints.Width, resultCompany.Rect.X + resultCompany.Rect.Width);
				AssertEquals(A4PaperSizeInPoints.Height, resultCompany.Rect.Y + resultCompany.Rect.Height);
			}

			using (Env.SetTemporaryUserContext(wrongCompany.PK.ToGuid(), wrongBranch.PK.ToGuid(), Guid.Empty))
			{
				var resultCompanyFail = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);

				AssertNotNull(resultCompanyFail);
				AssertEquals(0.0, resultCompanyFail.Rect.X);
				AssertEquals(0.0, resultCompanyFail.Rect.Y);
			}
		}

		public void TestVisibleSignatureMargins()
		{
			var systemMarginHorizontal = 15;
			var systemMarginVertical = 25;
			var companyMarginHorizontal = 18;
			var companyMarginVertical = 28;

			var factory = new BusinessObjectFactory();

			var testCompany = factory.NewWithValidTestData<GlbCompany>();
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_GB_HomeBranch = testBranch.PK;

			var wrongCompany = factory.NewWithValidTestData<GlbCompany>();
			var wrongBranch = factory.NewWithValidTestData<GlbBranch>();
			wrongBranch.GB_GC = wrongCompany.PK;
			var wrongStaff = factory.NewWithValidTestData<GlbStaff>();
			wrongStaff.GS_GB_HomeBranch = wrongBranch.PK;

			factory.Save();

			DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft);
			DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft);
			DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemMarginHorizontal);
			DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemMarginVertical);
			DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyMarginHorizontal);
			DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyMarginVertical);

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var resultSystem = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);

				AssertNotNull(resultSystem);
				AssertEquals(PdfVisibleSignatureGenerator.MillimetersToPoints(systemMarginHorizontal), resultSystem.Rect.X);
				AssertEquals(PdfVisibleSignatureGenerator.MillimetersToPoints(systemMarginVertical), resultSystem.Rect.Y);
			}

			using (Env.SetTemporaryUserContext(testCompany.PK.ToGuid(), testBranch.PK.ToGuid(), Guid.Empty))
			{
				var resultCompany = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);

				AssertNotNull(resultCompany);
				AssertEquals(PdfVisibleSignatureGenerator.MillimetersToPoints(companyMarginHorizontal), resultCompany.Rect.X);
				AssertEquals(PdfVisibleSignatureGenerator.MillimetersToPoints(companyMarginVertical), resultCompany.Rect.Y);
			}

			using (Env.SetTemporaryUserContext(wrongCompany.PK.ToGuid(), wrongBranch.PK.ToGuid(), Guid.Empty))
			{
				var resultCompanyFail = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);

				AssertNotNull(resultCompanyFail);
				AssertEquals(PdfVisibleSignatureGenerator.MillimetersToPoints(systemMarginHorizontal), resultCompanyFail.Rect.X);
				AssertEquals(PdfVisibleSignatureGenerator.MillimetersToPoints(systemMarginVertical), resultCompanyFail.Rect.Y);
			}
		}

		public void TestVisibleSignatureLocation_StaysWithinDocumentBoundaries()
		{
			DocumentsDataRegistry.Instance.SignatureImagePositioningAnchor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft);
			DocumentsDataRegistry.Instance.SignatureImagePositioningHorizontalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 200);
			DocumentsDataRegistry.Instance.SignatureImagePositioningVerticalMargin.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				"this is a very long name consisting of 80 characters, which is a maximum length!");
			var result = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);
			AssertEquals("Wide signature image goes over the edge of the document",
				A4PaperSizeInPoints.Width, result.Rect.X + result.Rect.Width);
		}

		public void TestVisibleSignatureSigner()
		{
			const string systemSignerName = "System Signer";
			const string companySignerName = "Company Signer";

			var factory = new BusinessObjectFactory();

			var testCompany = factory.NewWithValidTestData<GlbCompany>();
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_GB_HomeBranch = testBranch.PK;
			var wrongCompany = factory.NewWithValidTestData<GlbCompany>();
			var wrongBranch = factory.NewWithValidTestData<GlbBranch>();
			wrongBranch.GB_GC = wrongCompany.PK;
			var wrongStaff = factory.NewWithValidTestData<GlbStaff>();
			wrongStaff.GS_GB_HomeBranch = wrongBranch.PK;

			factory.Save();

			DocumentsDataRegistry.Instance.OverrideSignedByUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemSignerName);
			DocumentsDataRegistry.Instance.OverrideSignedByUsername.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companySignerName);
			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Signed By Label");

			var generatorDefault = generatorForTestCreate(null);
			AssertEquals(systemSignerName, generatorDefault.SignerName);

			var generatorTest = generatorForTestCreate(testBranch.PK);
			AssertEquals(companySignerName, generatorTest.SignerName);

			var generatorWrong = generatorForTestCreate(wrongBranch.PK);
			AssertEquals(systemSignerName, generatorWrong.SignerName);
		}

		void CheckVisibleSignaturePixels(CoordColorType[] pixels, Color? fontColor = null, Color? backgroundColor = null)
		{
			Color?[] colorTypeColors = new Color?[]
			{
				fontColor?.Normalize(),
				backgroundColor?.Normalize(),
			};

			var result = GeneratorForTest.GenerateTPdfVisibleSignature(PlaceholderSignerFactory);
			var image = imageHelper.Value.GetImageFromByteArray(result.ImageData);

			foreach (var pixel in pixels)
			{
				var expectedColor = colorTypeColors[(int)pixel.ColorType] ?? pixel.Color;
				AssertEquals($"Wrong {pixel.ColorType} color for pixel ({pixel.X}, {pixel.Y})", expectedColor, image.GetPixel(pixel.X, pixel.Y));
			}
		}

		public void TestVisibleSignature_FontAndBackColor()
		{
			var systemFontColor = Color.AliceBlue;
			var systemBackColor = Color.Blue;
			var companyFontColor = Color.Red;
			var companyBackColor = Color.Yellow;

			CoordColorType[] whiteBlackPixels = new CoordColorType[]
			{
				(0, 0, 0xFF000000.ToColor(), ColorType.Background),
				(4, 5, 0xFF000000.ToColor(), ColorType.Background),
				(63, 10, 0xFF000000.ToColor(), ColorType.Background),
				(74, 13, 0xFF000000.ToColor(), ColorType.Background),
				(46, 11, 0xFF000000.ToColor(), ColorType.Background),
				(125, 95, 0xFF000000.ToColor(), ColorType.Background),
				(9, 5, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
				(32, 10, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
				(57, 18, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
			};

			var factory = new BusinessObjectFactory();

			var testCompany = factory.NewWithValidTestData<GlbCompany>();
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_GB_HomeBranch = testBranch.PK;

			var wrongCompany = factory.NewWithValidTestData<GlbCompany>();
			var wrongBranch = factory.NewWithValidTestData<GlbBranch>();
			wrongBranch.GB_GC = wrongCompany.PK;
			var wrongStaff = factory.NewWithValidTestData<GlbStaff>();
			wrongStaff.GS_GB_HomeBranch = wrongBranch.PK;

			factory.Save();

			DocumentsDataRegistry.Instance.SignatureBackgroundImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			DocumentsDataRegistry.Instance.SignatureBackgroundImage.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, null);
			DocumentsDataRegistry.Instance.VisibleSignatureFontAndBackColor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ColorPairSelector { PrimaryColor = systemFontColor, SecondaryColor = systemBackColor });
			DocumentsDataRegistry.Instance.VisibleSignatureFontAndBackColor.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ColorPairSelector { PrimaryColor = companyFontColor, SecondaryColor = companyBackColor });

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				CheckVisibleSignaturePixels(whiteBlackPixels, systemFontColor, systemBackColor);
			}

			using (Env.SetTemporaryUserContext(testCompany.PK.ToGuid(), testBranch.PK.ToGuid(), Guid.Empty))
			{
				CheckVisibleSignaturePixels(whiteBlackPixels, companyFontColor, companyBackColor);
			}

			using (Env.SetTemporaryUserContext(wrongCompany.PK.ToGuid(), wrongBranch.PK.ToGuid(), Guid.Empty))
			{
				CheckVisibleSignaturePixels(whiteBlackPixels, systemFontColor, systemBackColor);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVisibleSignature_BackgroundImage()
		{
			var systemFontColor = Color.AliceBlue;
			var systemBackColor = Color.Blue;
			var companyFontColor = Color.Red;
			var companyBackColor = Color.Yellow;

			const string randomSystemImageFile = @"Enterprise\Product\Documents\DocumentEngine.Test\DocumentSigning\TestData\random_system_transparent.png";
			const string randomCompanyImageFile = @"Enterprise\Product\Documents\DocumentEngine.Test\DocumentSigning\TestData\random_company_transparent.png";

			CoordColorType[] randomSystemImagePixels = new CoordColorType[]
			{
				(0, 0, 0xFFFFFFFF.ToColor(), ColorType.Background),
				(9, 6, 0xFFE67E22.ToColor(), ColorType.Background),
				(64, 17, 0xFF3498DB.ToColor(), ColorType.Background),
				(45, 35, 0x00000000.ToColor(), ColorType.Background),
				(5, 19, 0x00000000.ToColor(), ColorType.Background),
				(125, 95, 0x00000000.ToColor(), ColorType.Background),
				(6, 4, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
				(32, 6, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
				(59, 12, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
			};

			CoordColorType[] randomCompanyImagePixels = new CoordColorType[]
			{
				(0, 0, 0xFFF1C40F.ToColor(), ColorType.Background),
				(9, 6, 0xFF34495E.ToColor(), ColorType.Background),
				(64, 17, 0xFF2ECC71.ToColor(), ColorType.Background),
				(45, 35, 0xFF9B59B6.ToColor(), ColorType.Background),
				(5, 19, 0xFF000000.ToColor(), ColorType.Background),
				(125, 95, 0xFF3498DB.ToColor(), ColorType.Background),
				(6, 4, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
				(32, 6, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
				(59, 12, 0xFFFFFFFF.ToColor(), ColorType.Foreground),
			};

			var systemBackgroundImage = Image.FromFile(BaseSourcePath + randomSystemImageFile);
			var companyBackgroundImage = Image.FromFile(BaseSourcePath + randomCompanyImageFile);

			var factory = new BusinessObjectFactory();

			var testCompany = factory.NewWithValidTestData<GlbCompany>();
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_GB_HomeBranch = testBranch.PK;

			var wrongCompany = factory.NewWithValidTestData<GlbCompany>();
			var wrongBranch = factory.NewWithValidTestData<GlbBranch>();
			wrongBranch.GB_GC = wrongCompany.PK;
			var wrongStaff = factory.NewWithValidTestData<GlbStaff>();
			wrongStaff.GS_GB_HomeBranch = wrongBranch.PK;

			factory.Save();

			DocumentsDataRegistry.Instance.SignatureBackgroundImage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemBackgroundImage);
			DocumentsDataRegistry.Instance.VisibleSignatureFontAndBackColor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ColorPairSelector { PrimaryColor = systemFontColor, SecondaryColor = systemBackColor });
			DocumentsDataRegistry.Instance.SignatureBackgroundImage.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyBackgroundImage);
			DocumentsDataRegistry.Instance.VisibleSignatureFontAndBackColor.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ColorPairSelector { PrimaryColor = companyFontColor, SecondaryColor = companyBackColor });

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				CheckVisibleSignaturePixels(randomSystemImagePixels, systemFontColor);
			}

			using (Env.SetTemporaryUserContext(testCompany.PK.ToGuid(), testBranch.PK.ToGuid(), Guid.Empty))
			{
				CheckVisibleSignaturePixels(randomCompanyImagePixels, companyFontColor);
			}

			using (Env.SetTemporaryUserContext(wrongCompany.PK.ToGuid(), wrongBranch.PK.ToGuid(), Guid.Empty))
			{
				CheckVisibleSignaturePixels(randomSystemImagePixels, systemFontColor);
			}
		}
	}
}
