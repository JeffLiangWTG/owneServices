using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using SystemImage = System.Drawing.Image;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Image))]
	sealed class ImageTest : ValueProviderTest
	{
		public void TestMacroWorksWithNullEnvironment()
		{
			RunInNullEnvironment(() =>
			{
				PrepareRenderer();
				Report.Renderer.SaveOriginalColumnWidths();

				AssertEquals("Replaced Result when User is Null", null, ValueProviderToTest.GetReplacement("<Image(USERSIGNATURE, 2, 4)>", Report));
			});
		}

		public void TestIControlSizeProvider_GetCellRange()
		{
			IControlSizeProvider providerToTest = new Image();
			AssertEquals(4, providerToTest.GetCellRange("<image(COMPANYLOGO , 2  , 4)>").Width);
			AssertEquals(2, providerToTest.GetCellRange("<image(COMPANYLOGO , 2  , 4)>").Height);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< image(12,1,2)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<image(imageName,1,2,Y)>", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<image(12,1)>", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<image(2)>", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<image()>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<image (2,111111111,1)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<image (2,111111111,1) >", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< image ( 2 , 111111111  , 1 )  >", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< image ( 2 , 111111111  , 1 )  >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< image ( signature , 111111111  , 1  , Y)  >", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< image ( .jpg , 111111111  , 1  , y)  >", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< image ( image , 111111111  , 1  , X)  >", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("< image ( an image , 111111111  , 1 )  >", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find(\"{P9_Description}\"==\"Create Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find(\"{P9_Description}\" == \"Create Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find(\"{P9_Description}\"==\"Create , Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find(\"{P9_Description}\" == \"Create , Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find(\"{P9_Description}\"==\"Create \" , Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find('{P9_Description}'==\"Create , Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Image(WorkflowItems.Tasks.Find('{P9_Description}' == \"Create , Quotation\").AssignedStaffMember.SignatureImage, 3, 19, Y)>", Passes.SecondPass));
		}

		[ExpectException(typeof(DataProviderException))]
		public void TestReplacementNonExistingField()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			ValueProviderToTest.GetReplacement("< image ( FileName , 2  , 4 )  >", Report);
		}

		public void TestImageMacroSetsIsAspectRatioLocked()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			Env.Registry.Freight.AirWaybill.HAWBLogo = new Bitmap(250, 80);

			ExcelImage result = ValueProviderToTest.GetReplacement("<image(HAWBLogo, 1, 1)>", Report) as ExcelImage;

			AssertEquals(false, result.IsAspectRatioLocked);
			result = ValueProviderToTest.GetReplacement("<image(HAWBLogo,1,1,Y)>", Report) as ExcelImage;

			AssertEquals(true, result.IsAspectRatioLocked);

			result = ValueProviderToTest.GetReplacement("<IMAGE(HAWBLogo, 1, 1, N)>", Report) as ExcelImage;

			AssertEquals(false, result.IsAspectRatioLocked);

			result = ValueProviderToTest.GetReplacement("<image(HAWBLogo, 1, 1, y )>", Report) as ExcelImage;

			AssertEquals("Not case-sensitive", true, result.IsAspectRatioLocked);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageMacro_DisposedImage()
		{
			var testImage = new Image();
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, new BizoWithLogo(), "", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Renderer.SaveOriginalColumnWidths();
				report.Renderer.CurrentAreaToProcess = report.Analyser.DocumentHeader;
				var result = (ExcelImage)testImage.GetReplacement("< image ( COMPANYLOGO , 2  , 4 )>", report);
				result.Image.Dispose();

				result = (ExcelImage)testImage.GetReplacement("< image ( COMPANYLOGO , 2  , 4 )>", report);
				AssertEquals("Should not get a disposed image", false, result.Image.IsDisposed());
			}
		}

		[ExpectNoExceptions]
		public void TestReplacementWithInvalidParameters()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			AssertNull("Parameter 'heightinrows' is invalid. Macro should return null.", ValueProviderToTest.GetReplacement("< image ( COMPANYLOGO , A  , 2 )  >", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in Image Macro: Parameter 'heightinrows' must be a positive integer but its value is 'A']",
							Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));

			Report.ErrorManager.ClearErrors();

			AssertNull("Parameter 'widthincolumns' is invalid. Macro should return null.", ValueProviderToTest.GetReplacement("< image ( COMPANYLOGO , 1  , B )  >", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in Image Macro: Parameter 'widthincolumns' must be a positive integer but its value is 'B']",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));

			Report.ErrorManager.ClearErrors();

			AssertNull("Parameters 'heightinrows' and 'widthincolumns' are invalid. Macro should return null.", ValueProviderToTest.GetReplacement("< image ( COMPANYLOGO , C  , D )  >", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in Image Macro: Parameter 'heightinrows' must be a positive integer but its value is 'C']
Severity: [Warning (without error report)] Message: [Error in Image Macro: Parameter 'widthincolumns' must be a positive integer but its value is 'D']",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(5, 5));
			object result = ValueProviderToTest.GetReplacement("< Image ( COMPANYLOGO , 2  , 4 )  >", Report);
			AssertEquals(new System.Drawing.Size(5, 5), ((ExcelImage)result).Image.Size);

			SystemDataRegistry.Instance.CompanyCheckLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(8, 8));
			result = ValueProviderToTest.GetReplacement("< image ( CHEQUELOGOWITHBANKDETAIL , 2  , 4 )  >", Report);
			AssertEquals(new System.Drawing.Size(8, 8), ((ExcelImage)result).Image.Size);

			Env.Registry.HouseBillOfLadingLogo = new System.Drawing.Bitmap(6, 6);
			result = ValueProviderToTest.GetReplacement("< image ( HBLLOGO , 2  , 4 )  >", Report);
			AssertEquals(Env.Registry.HouseBillOfLadingLogo.Size, ((ExcelImage)result).Image.Size);

			Env.Registry.Freight.AirWaybill.HAWBLogo = new System.Drawing.Bitmap(7, 7);
			result = ValueProviderToTest.GetReplacement("< image ( HAWBLOGO , 2  , 4 )  >", Report);
			AssertEquals(Env.Registry.Freight.AirWaybill.HAWBLogo.Size, ((ExcelImage)result).Image.Size);

			result = ValueProviderToTest.GetReplacement("< image ( HAWBLOGO , 25  , 80, Y )  >", Report);
			AssertEquals(Env.Registry.Freight.AirWaybill.HAWBLogo.Size, ((ExcelImage)result).Image.Size);
		}

		public void TestWidthIsUntouchedByColumnChanges()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(5, 5));
			object result = ValueProviderToTest.GetReplacement("< image ( COMPANYLOGO , 2  , 4 )  >", Report);
			AssertEquals(9872, ((ExcelImage)result).Width);

			Report.WorkSheetCurrentlyBeingProcessed.HideColumn(2);
			result = ValueProviderToTest.GetReplacement("< image ( COMPANYLOGO , 2  , 4 )  >", Report);
			AssertEquals(9872, ((ExcelImage)result).Width);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompanyLogo()
		{
			AssertLogo("CompanyLogo", new System.Drawing.Size(1, 1), SystemDataRegistry.Instance.CompanyLogo.Value);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHBLLogo()
		{
			AssertLogo("HBLLogo", new System.Drawing.Size(2, 2), Env.Registry.HouseBillOfLadingLogo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHAWBLogo()
		{
			AssertLogo("HAWBLogo", new System.Drawing.Size(3, 3), Env.Registry.Freight.AirWaybill.HAWBLogo);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUserSignature()
		{
			using (var signatureImage = SystemImage.FromFile(TransparentPNGPath))
			{
				var stream = new MemoryStream();
				signatureImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				GlbStaff.CurrentUser.GS_UserSignature = stream.ToArray();

				AssertLogo("UserSignature", new System.Drawing.Size(4, 4), signatureImage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLogoInTurkish()
		{
			using (var signatureImage = SystemImage.FromFile(TransparentPNGPath))
			{
				var stream = new MemoryStream();
				signatureImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				GlbStaff.CurrentUser.GS_UserSignature = stream.ToArray();

				using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Turkish))
				using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.Constants.Languages.Turkish)))
				{
					AssertLogo("UserSignature", new System.Drawing.Size(4, 4), signatureImage);
					AssertLogo("usersignature", new System.Drawing.Size(4, 4), signatureImage);
					AssertLogo("USERSIGNATURE", new System.Drawing.Size(4, 4), signatureImage);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestConcurrentResetImageValueProvider()
		{
			const int numberOfThreads = 100;
			var threads = new Thread[numberOfThreads];

			for (var i = 0; i < numberOfThreads; ++i)
			{
				threads[i] = new Thread(() =>
				{
					for (var j = 0; j < numberOfThreads; ++j)
					{
						ValueProviderToTest.Reset();
					}
				});
			}

			for (var i = 0; i < numberOfThreads; ++i)
			{
				threads[i].Start();
			}

			for (var i = 0; i < numberOfThreads; ++i)
			{
				threads[i].Join();
			}
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.Image, GetNewValueProvider().ComponentType);
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new System.Drawing.Bitmap(5, 5));
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var actualResult = ValueProviderToTest.GetReplacement(example, Report);
			AssertEquals(expectedResult, ((ExcelImage)actualResult).Image.Size);
			Assert(((ExcelImage)actualResult).IsAspectRatioLocked);
		}

		protected override List<FieldInfo> FieldCollection => new List<FieldInfo>() { typeof(Image).GetField("ImageValueProviders", BindingFlags.Instance | BindingFlags.NonPublic) };

		protected override ValueProvider GetNewValueProvider() => new Image();

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

		void AssertLogo(string macroName, Size expectedImageSize, System.Drawing.Image defaultImage)
		{
			Image testImage = new Image();
			ExcelTemplateForUnitTesting excelTemplate1 = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(new DocumentPack(), excelTemplate1, new BizoWithLogo(), "", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Renderer.SaveOriginalColumnWidths();
				report.Renderer.CurrentAreaToProcess = report.Analyser.DocumentHeader;
				AssertEquals(expectedImageSize, ((ExcelImage)testImage.GetReplacement("< image ( " + macroName + " , 2  , 4 )>", report)).Image.Size);
			}

			using (Report report = new Report(new DocumentPack(), excelTemplate1, new BizoWithLogo(), "", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Renderer.SaveOriginalColumnWidths();
				report.Renderer.CurrentAreaToProcess = report.Analyser.DocumentHeader;
				AssertNotNull("Should be able to render image with optional parameter",
					((ExcelImage)testImage.GetReplacement("<IMAGE(" + macroName + ", 2, 4, Y)>", report)).Image);
			}

			ExcelTemplateForUnitTesting excelTemplate2 = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(new DocumentPack(), excelTemplate2, new BizoWithNoLogo(), "", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Renderer.SaveOriginalColumnWidths();
				report.Renderer.CurrentAreaToProcess = report.Analyser.DocumentHeader;
				var result = testImage.GetReplacement("< Image ( " + macroName + " , 2 , 4 ) >", report);
				if (defaultImage == null)
				{
					AssertNull("Default image", result);
				}
				else
				{
					AssertEquals("Default image", defaultImage.Size, ((ExcelImage)result).Image.Size);
				}
			}
		}

		sealed class BizoWithLogo : DocumentWrapper
		{
			System.Drawing.Image companyLogo;
			System.Drawing.Image hblLogo;
			System.Drawing.Image hawbLogo;
			System.Drawing.Image userSignature;

			public override string ToString() => "BizoWithLogo";

			public System.Drawing.Image CompanyLogo => companyLogo ?? (companyLogo = new Bitmap(1, 1));

			public System.Drawing.Image HBLLogo => hblLogo ?? (hblLogo = new Bitmap(2, 2));

			public System.Drawing.Image HAWBLogo => hawbLogo ?? (hawbLogo = new Bitmap(3, 3));

			public System.Drawing.Image UserSignature => userSignature ?? (userSignature = new Bitmap(4, 4));
		}

		sealed class BizoWithNoLogo : DocumentWrapper
		{
			public override string ToString() => "BizoWithNoLogo";
		}
	}
}
