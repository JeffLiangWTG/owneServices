using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocBuilderTemplateMerge;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.PreviewableDocument;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.Pdf;
using NUnit.Framework;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using static System.Text.Encoding;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class ExcelInterfaceTest : TransactionedTestCase
	{
		public void TestInterfaceExceptionThorwedWhenIllegalPath()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile(
				"Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.Customized Document Elements Illegal Hyperlink.xls",
				"Customized Document Elements Illegal Hyperlink.xls");

			using (var excelInterface = new ExcelInterfaceForTesting())
			{
				excelInterface.LoadExcelFile(tempFileName);

				using (TempFile tempFile = TempFile.New())
				{
					AssertExceptionThrown<ExcelInterfaceException>("ExcelInterfaceException should be raised when a illegal hyper link in the template.", "Illegal characters in path., possibly because there are some illegal characters in the hyper link(s).", () =>
					{
						excelInterface.SaveToFile(tempFile.Filename);
					});
				}

				using (Stream stream = new MemoryStream())
				{
					AssertExceptionThrown<ExcelInterfaceException>("ExcelInterfaceException should be raised when a illegal hyper link in the template.", "Illegal characters in path., possibly because there are some illegal characters in the hyper link(s).", () =>
					{
						excelInterface.SaveToStream(stream);
					});
				}
			}
		}

		public void TestSignaturePlaceholder()
		{
			using (var excelInterface = new ExcelInterfaceForTesting())
			using (var stream = new MemoryStream())
			{
				SetDigitalSignatureRegistry();

				excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
				excelInterface.ExportToPdfAndScale(stream, 100, null, TPdfType.Standard, true, PdfSigningOptionCodes.Placeholder);

				var signedConvertedPDF = stream.ToArray();

				try
				{
					var expectedSignature = new List<(string key, string value)>()
					{
						("Location", ""),
						("Reason", $@"{Core.Constants.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document."),
						("ContactInfo", "")
					};

					var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(signedConvertedPDF);

					AssertContainsExactElementsInAnyOrder(expectedSignature, actualSignature);
				}
				finally
				{ }
			}
		}

		public void TestExportHasAValidCertificate()
		{
			using (var excelInterface = new ExcelInterfaceForTesting())
			using (var stream = new MemoryStream())
			{
				SetDigitalSignatureRegistry();
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExportToPdfAndScale(stream, 100, null, TPdfType.Standard, true);
					Fail("Exception should be thrown before reaching this line");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.DigitalSignatureNotValid, exception.Type);
					AssertEquals("exception.Message", """
1. A certificate chain could not be built to a trusted root authority.
2. The revocation function was unable to check revocation for the certificate.
3. The revocation function was unable to check revocation because the revocation server was offline.
""", exception.Message);
				}
				catch (CryptographicException ex)
				{
					Fail("Expect no other types of exception: " + ex.ToString());
				}
			}
		}

		public void TestExportToPdfAndScale_WhenErrorGettingCertificateChain()
		{
			using (var excelInterface = new ExcelInterfaceForTesting())
			using (var stream = new MemoryStream())
			{
				SetDigitalSignatureRegistry(false);
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExportToPdfAndScale(stream, 100, null, TPdfType.Standard, true);
					Fail("Exception should be thrown before reaching this line");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.ErrorGettingCertificateChain, exception.Type);
					AssertEquals("exception.Message", @"There was a problem with getting certificate chain, please check your certificate installation.

Error: The specified network password is not correct.
", exception.Message);
				}
				catch (Exception ex)
				{
					Fail("Expect no other types of exception: " + ex.ToString());
				}
			}
		}

		public void TestSetPrintMargins()
		{
			using (var xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				xlInterface.SetPrintMargins(new TXlsMargins(1, 2, 3, 4, 5, 6));

				var margins = xlInterface.Xls.GetPrintMargins();
				AssertEquals("margins.Left", 1.0, margins.Left);
				AssertEquals("margins.Top", 2.0, margins.Top);
				AssertEquals("margins.Right", 3.0, margins.Right);
				AssertEquals("margins.Bottom", 4.0, margins.Bottom);
				AssertEquals("margins.Header", 5.0, margins.Header);
				AssertEquals("margins.Footer", 6.0, margins.Footer);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetPaperSize()
		{
			using (var excelInterface = New())
			{
				var docBuilderDocumentsDir = Path.Combine(BaseSourcePath, SystemDocumentElementsMerger.DocBuilderDocumentsDir);
				var systemDocumentElementsFile = Path.Combine(docBuilderDocumentsDir, SystemDocumentElementsMerger.SystemDocumentElementsFileName);
				excelInterface.LoadExcelFile(systemDocumentElementsFile);
				excelInterface.SetPaperSize(123, 456);

				var widthInTenthsOfMillimeter = (int)(excelInterface.Xls.PrintPaperDimensions.Width * 2.54); //PrintPaperDimensions.Width is in 100ths of inch
				var heightInTenthsOfMillimeter = (int)(excelInterface.Xls.PrintPaperDimensions.Height * 2.54);  //PrintPaperDimensions.Height is in 100ths of inch
				AssertEquals("Paper size should be Undefined", TPaperSize.Undefined, excelInterface.Xls.PrintPaperSize);
				AssertEquals("Paper width should be 123 tenths of milimeter", 123, widthInTenthsOfMillimeter);
				AssertEquals("Paper height should be 456 tenths of milimeter", 456, heightInTenthsOfMillimeter);
			}
		}

		public void TestBackgroundColourSetting()
		{
			using (TempFile tempFile = TempFile.New())
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];

					var cellFormat = workSheet.GetCellFormat(0, 0);
					cellFormat.FillPattern = FillPatternStyle.Automatic;
					cellFormat.BackgroundColor = System.Drawing.Color.Blue;
					workSheet.SetCellFormat(0, 0, cellFormat);

					cellFormat = workSheet.GetCellFormat(0, 1);
					cellFormat.FillPattern = FillPatternStyle.Automatic;
					cellFormat.BackgroundColor = System.Drawing.Color.Blue;
					workSheet.SetCellFormat(0, 1, cellFormat);

					cellFormat = workSheet.GetCellFormat(1, 0);
					cellFormat.FillPattern = FillPatternStyle.Automatic;
					cellFormat.BackgroundColor = System.Drawing.Color.Green;
					workSheet.SetCellFormat(1, 0, cellFormat);

					cellFormat = workSheet.GetCellFormat(1, 1);
					cellFormat.FillPattern = FillPatternStyle.Automatic;
					cellFormat.BackgroundColor = System.Drawing.Color.Green;
					workSheet.SetCellFormat(1, 1, cellFormat);

					CombineAssertions(delegate
					{
						AssertEquals("First Column First Row Color", System.Drawing.Color.Blue.ToArgb(), workSheet.GetCellFormat(0, 0).BackgroundColor.ToArgb());
						AssertEquals("Second Column First Row Color", System.Drawing.Color.Blue.ToArgb(), workSheet.GetCellFormat(0, 1).BackgroundColor.ToArgb());
						AssertEquals("First Column Second Row Color", System.Drawing.Color.Green.ToArgb(), workSheet.GetCellFormat(1, 0).BackgroundColor.ToArgb());
						AssertEquals("Second Column Second Row Color", System.Drawing.Color.Green.ToArgb(), workSheet.GetCellFormat(1, 1).BackgroundColor.ToArgb());
					});

					excelInterface.SaveToFile(tempFile.Filename);
				}

				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(tempFile.Filename);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];

					CombineAssertions(delegate
					{
						AssertEquals("First Column First Row Color", System.Drawing.Color.Blue.ToArgb(), workSheet.GetCellFormat(0, 0).BackgroundColor.ToArgb());
						AssertEquals("Second Column First Row Color", System.Drawing.Color.Blue.ToArgb(), workSheet.GetCellFormat(0, 1).BackgroundColor.ToArgb());
						AssertEquals("First Column Second Row Color", System.Drawing.Color.Green.ToArgb(), workSheet.GetCellFormat(1, 0).BackgroundColor.ToArgb());
						AssertEquals("Second Column Second Row Color", System.Drawing.Color.Green.ToArgb(), workSheet.GetCellFormat(1, 1).BackgroundColor.ToArgb());
					});
				}
			}
		}

		public void TestSaveToFileWithTooManyCellFormatsHasExceptionWrapped()
		{
			using (var file = TempFile.New())
			{
				AssertSaveMethodWrapsCellFormatException((ExcelInterface excelInterface) =>
				{
					excelInterface.SaveToFile(file.Filename);
				});
			}
		}

		public void TestSavingExcelFileWithTooManyCellFormatsThrowsExcelInterfaceExceptionTypeTooManyCellFormats()
		{
			AssertSaveMethodWrapsCellFormatException((ExcelInterface excelInterface) =>
			{
				using (Stream stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
				}
			});
		}

		public void TestExcelInterfaceWrapsFlexCoreFontStyleNotSupportedException()
		{
			using (var excelInterface = new ExcelInterfaceThatThrowFlexCoreFontStyleNotSupportedException())
			using (var stream = new MemoryStream())
			{
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExportToPdfAndScale(stream, 100, null);
					Fail("Exception should be thrown before reaching this line");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.ErrorFontNotSupported, exception.Type);
					AssertEquals("exception.Message", @"Bad font
Details: [The font ""blah"" is a very bad font.]", exception.Message);
				}
				catch (Exception)
				{
					Fail("Expect no other types of exception");
				}
			}
		}

		public void TestExcelInterfaceWrapsFlexCoreFontStyleNotFoundException()
		{
			using (var excelInterface = new ExcelInterfaceThatThrowFlexCoreFontStyleNotFoundException())
			using (var stream = new MemoryStream())
			{
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExportToPdfAndScale(stream, 100, null);
					Fail("Exception should be thrown before reaching this line");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.ErrorFontNotFound, exception.Type);
					AssertEquals("exception.Message", @"Font not found
Details: [The font ""blah"" was not found.]", exception.Message);
				}
				catch (Exception)
				{
					Fail("Expect no other types of exception");
				}
			}
		}

		public void TestExcelInterfaceWrapsFlexPdfFontStyleNotFoundException()
		{
			using (var excelInterface = new ExcelInterfaceThatThrowFlexPdfFontStyleNotFoundException())
			using (var stream = new MemoryStream())
			{
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExportToPdfAndScale(stream, 100, null);
					Fail("Exception should be thrown before reaching this line");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.ErrorFontNotFound, exception.Type);
					AssertEquals("exception.Message", @"Font not found
Details: [The font ""blah"" was not found.]", exception.Message);
				}
				catch (Exception)
				{
					Fail("Expect no other types of exception");
				}
			}
		}

		public void TestExcelInterfaceWrapsFlexCoreFontStyleNotSupportedArgumentException()
		{
			using (var excelInterface = new ExcelInterfaceThatThrowFlexCoreFontStyleNotSupportedArgumentException())
			using (var stream = new MemoryStream())
			{
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExportToPdfAndScale(stream, 100, null);
					Fail("Exception should be thrown before reaching this line");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.ErrorFontNotSupported, exception.Type);
					AssertEquals("exception.Message", @"Bad font
Details: [Font 'Arial Unicode MS' does not support style 'Regular']", exception.Message);
				}
				catch (Exception)
				{
					Fail("Expect no other types of exception");
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopySheetWithOLEObjectInsideNoExceptionThrown()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "SimpleTestWithOLEObject.xls");
				AssertNoExceptionThrown(() => xlInterface.CopySheet(xlInterface.WorkSheets[0], 1));
			}
		}

		[ExpectNoExceptions]
		public void TestLoadXLSXFileIsSupported()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsPath);
			}
		}

		public void TestLoadUnSupportedExcelFileThrowsException()
		{
			using (var excelInterface = new ExcelInterface())
			{
				try
				{
					var fileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TIFPage.tif");
					excelInterface.LoadExcelFile(fileName);
					Fail("Exception was not thrown.");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.FileFormatNotSupported, exception.Type);
					AssertEquals(ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, exception.Message);
				}
				catch (Exception)
				{
					Fail("ExcelInterfaceException was not thrown");
				}
			}
		}

		public void TestLoadUnSupportedExcelStreamThrowsException()
		{
			using (var excelInterface = new ExcelInterface())
			using (var fileStream = resourceRetriever.Value.GetStream("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.TIFPage.tif"))
			{
				try
				{
					excelInterface.LoadExcelFile(fileStream);
					Fail("Exception was not thrown.");
				}
				catch (ExcelInterfaceException exception)
				{
					AssertEquals("exception.Type", ExcelInterfaceExceptionType.FileFormatNotSupported, exception.Type);
					AssertEquals(ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, exception.Message);
				}
				catch (Exception)
				{
					Fail("ExcelInterfaceException was not thrown");
				}
			}
		}

		public void TestExcelInterfaceWrapsFlexCelPdfException()
		{
			var exceptionThrown = false;
			Exception otherExceptionThrown = null;
			using (var excelInterface = new ExcelInterfaceThatThrowsFlexCelPdfException())
			using (var stream = new MemoryStream())
			{
				try
				{
					excelInterface.LoadExcelFile(DocumentTestXlsFilePath);
					excelInterface.ExceptionMessage = "Can not load font NonExistingFont";
					excelInterface.ExportToPdfAndScale(stream, 100, null);
				}
				catch (DocumentEngineException e)
				{
					exceptionThrown = true;
					AssertEquals("Exception should contain sheet name", true, e.Message.Contains(excelInterface.Xls.ActiveSheetByName));
					AssertEquals("Exception should contain file name of workbook", true, e.Message.Contains(excelInterface.Xls.ActiveFileName));
				}
				catch (Exception e)
				{
					otherExceptionThrown = e;
				}
				finally
				{
					if (!exceptionThrown)
					{
						if (otherExceptionThrown == null)
						{
							Fail("No exceptions thrown");
						}
						else
						{
							Fail(otherExceptionThrown.GetType().ToString() + " thrown instead of " + typeof(DocumentEngineException).ToString() + @"
Other exception's Message: " + otherExceptionThrown.Message);
						}
					}
				}
			}
		}

		public void TestMaxRowCountSupportedByCurrentExcelFile()
		{
			using (var excelInterface = New())
			{
				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsPath);
				AssertEquals(Excel.MaxRowCountSupported97_2003, excelInterface.MaxRowCountSupportedByCurrentExcelFile);

				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsxPath);
				AssertEquals(Excel.MaxRowCountSupported2007, excelInterface.MaxRowCountSupportedByCurrentExcelFile);
			}
		}

		public void TestMaxColCountSupportedByCurrentExcelFile()
		{
			using (var excelInterface = New())
			{
				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsPath);
				AssertEquals(Excel.MaxColCountSupported97_2003, excelInterface.MaxColCountSupportedByCurrentExcelFile);

				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsxPath);
				AssertEquals(Excel.MaxColCountSupported2007, excelInterface.MaxColCountSupportedByCurrentExcelFile);
			}
		}

		public void TestMaxFormulaTextSizeSupportedByCurrentExcelFile()
		{
			using (ExcelInterface excelInterface = New())
			{
				AssertEquals(FlxConsts.Max_FormulaLen97_2003, excelInterface.GetMaxFormulaTextSizeSupportedByCurrentExcelFile(TFileFormats.Xls));
				AssertEquals(FlxConsts.Max_FormulaLen2007, excelInterface.GetMaxFormulaTextSizeSupportedByCurrentExcelFile(TFileFormats.Xlsx));
			}
		}

		public void TestValidCellReference()
		{
			using (ExcelInterface excelInterface = New())
			{
				AssertEquals("Col 0", "A1", excelInterface.GetCellReference(0, 0));
				AssertEquals("Col 1", "B1", excelInterface.GetCellReference(0, 1));
				AssertEquals("Col 24", "Y1", excelInterface.GetCellReference(0, 24));
				AssertEquals("Col 25", "Z1", excelInterface.GetCellReference(0, 25));
				AssertEquals("Col 26", "AA1", excelInterface.GetCellReference(0, 26));
				AssertEquals("Col 27", "AB1", excelInterface.GetCellReference(0, 27));
				AssertEquals("Col 51", "AZ1", excelInterface.GetCellReference(0, 51));
				AssertEquals("Col 52", "BA1", excelInterface.GetCellReference(0, 52));
				AssertEquals("Col 233", "HZ1", excelInterface.GetCellReference(0, 233));
				AssertEquals("Col 234", "IA1", excelInterface.GetCellReference(0, 234));
				AssertEquals("Col 254", "IU1", excelInterface.GetCellReference(0, 254));
				AssertEquals("Col 255", "IV1", excelInterface.GetCellReference(0, 255));
				AssertEquals("Col 255", "IV10", excelInterface.GetCellReference(9, 255));
				AssertEquals("Col 255", "IV65536", excelInterface.GetCellReference(65535, 255));
				AssertEquals("Col 256", "IW65537", excelInterface.GetCellReference(65536, 256));
				AssertEquals("Col 15123", "VIR1012346", excelInterface.GetCellReference(1012345, 15123));
			}
		}

		public void TestInvalidRowCellReference()
		{
			using (ExcelInterface excelInterface = New())
			{
				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsPath);
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(Excel.MaxRowCountSupported97_2003 - 1, 0));
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(Excel.MaxRowCountSupported97_2003, 0));

				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsxPath);
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(Excel.MaxRowCountSupported97_2003 - 1, 0));
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(Excel.MaxRowCountSupported97_2003, 0));
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(Excel.MaxRowCountSupported2007 - 1, 0));
				AssertExceptionThrown<ArgumentException>(() => excelInterface.GetCellReference(Excel.MaxRowCountSupported2007, 0));
			}
		}

		public void TestInvalidColCellReference()
		{
			using (ExcelInterface excelInterface = New())
			{
				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsPath);
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(0, Excel.MaxColCountSupported97_2003 - 1));
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(0, Excel.MaxColCountSupported97_2003));

				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsxPath);
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(0, Excel.MaxColCountSupported97_2003 - 1));
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(0, Excel.MaxColCountSupported97_2003));
				AssertNoExceptionThrown(() => excelInterface.GetCellReference(0, Excel.MaxColCountSupported2007 - 1));
				AssertExceptionThrown<ArgumentException>(() => excelInterface.GetCellReference(0, Excel.MaxColCountSupported2007));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetSheetName()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
				AssertEquals("Sheet name", "Boris", xlInterface.GetSheetName(0));
				AssertEquals("Sheet name", "Natasha", xlInterface.GetSheetName(1));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetSheetNameMultipleTimes()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
				AssertEquals("Sheet name (1)", "Boris", xlInterface.GetSheetName(0));
				AssertEquals("Sheet name (2)", "Boris", xlInterface.GetSheetName(0));
				xlInterface.ActiveWorksheet = 0;
				AssertEquals("Sheet name (active sheet)", "Boris", xlInterface.GetSheetName(0));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPrinterDriverInfo()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetPrinterDriverInfo.xls");
				AssertNotNull(xlInterface.GetPrinterDriverSettings());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetPrinterDriverInfo()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
				AssertNull(xlInterface.GetPrinterDriverSettings());
				using (ExcelInterface xlInterface2 = New())
				{
					xlInterface2.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetPrinterDriverInfo.xls");
					AssertNotNull(xlInterface2.GetPrinterDriverSettings());
					xlInterface.SetPrinterDriverSettings(xlInterface2.GetPrinterDriverSettings());
					AssertNotNull(xlInterface.GetPrinterDriverSettings());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestOpenningMultipleInterfaces()
		{
			using (ExcelInterface x1 = New())
			{
				x1.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
				using (ExcelInterface x2 = New())
				{
					x2.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
					using (ExcelInterface x3 = New())
					{
						x3.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
						using (ExcelInterface x4 = New())
						{
							x4.LoadExcelFile(UnitTestingConstants.TestFilesDir + "GetSheetName.xls");
						}
					}
				}
			}
		}

		public void TestIndexerWithByteArrayNonRTF()
		{
			var byteArray = UTF8.GetBytes("NonRTF");
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				xlInterface.WorkSheets[0][1, 1] = byteArray;
				AssertEquals("NonRTF", xlInterface.WorkSheets[0][1, 1]);
			}
		}

		public void TestIndexerWithByteArrayRTF()
		{
			var byteArray = UTF8.GetBytes("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 x\\par\r\n}\r\n\0");
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				xlInterface.WorkSheets[0][1, 1] = byteArray;
				AssertEquals("x", xlInterface.WorkSheets[0][1, 1]);
			}
		}

		public void TestIndexer1()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("#row", xlInterface.WorkSheets[0][1, 0]);
			}
		}

		public void TestIndexer2()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("DocumentName", xlInterface.WorkSheets[1][1, 0]);
			}
		}

		public void TestIndexer3()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				xlInterface.WorkSheets[0][1, 0] = "test";
				AssertEquals("test", xlInterface.WorkSheets[0][1, 0]);
			}
		}

		public void TestIndexer4()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				xlInterface.WorkSheets[1][1, 0] = "test";
				AssertEquals("test", xlInterface.WorkSheets[1][1, 0]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetIsA1CellSelected()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "A1CellSelected.xls");

				var nbVisibleSheets = 0;
				for (var i = 1; i <= xlInterface.Xls.SheetCount; i++)
				{
					xlInterface.Xls.ActiveSheet = i;
					if (xlInterface.Xls.SheetVisible == TXlsSheetVisible.Visible)
					{
						nbVisibleSheets++;
						AssertEquals(false, xlInterface.IsA1Selected());
					}
				}
				AssertEquals("Should contains 3 visible sheets", 3, nbVisibleSheets);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestA1CellSelectedBeforeSave()
		{
			using (var excelInterface = New())
			{
				excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "A1CellSelected.xls");
				AssertEquals(false, excelInterface.IsA1Selected());
				using (var memoryStream = new MemoryStream())
				{
					excelInterface.SaveToStream(memoryStream);
					using (ExcelInterface anotherExcelInterface = New())
					{
						memoryStream.Position = 0;
						anotherExcelInterface.LoadExcelFile(memoryStream);

						int nbVisibleSheets = 0;
						for (int i = 1; i <= anotherExcelInterface.Xls.SheetCount; i++)
						{
							anotherExcelInterface.Xls.ActiveSheet = i;
							if (anotherExcelInterface.Xls.SheetVisible == TXlsSheetVisible.Visible)
							{
								nbVisibleSheets++;
								AssertEquals(true, anotherExcelInterface.IsA1Selected());
							}
						}
						AssertEquals("Should contain 3 visible sheets", 3, nbVisibleSheets);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllPanesGetScrolledToFirstCellBeforeSave()
		{
			using (ExcelInterface excelInterface = New())
			{
				excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "A1CellSelectedWithMultiplePanes.xls");
				AssertEquals(1, excelInterface.Xls.SheetCount);
				AssertEquals(false, excelInterface.IsA1Selected());
				AssertEquals("B1", excelInterface.Xls.GetWindowScroll(TPanePosition.UpperLeft).CellRef);
				AssertEquals("X1", excelInterface.Xls.GetWindowScroll(TPanePosition.UpperRight).CellRef);
				AssertEquals("B18", excelInterface.Xls.GetWindowScroll(TPanePosition.LowerLeft).CellRef);
				AssertEquals("X18", excelInterface.Xls.GetWindowScroll(TPanePosition.LowerRight).CellRef);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					using (ExcelInterface anotherExcelInterface = New())
					{
						stream.Position = 0;
						anotherExcelInterface.LoadExcelFile(stream);

						AssertEquals(1, excelInterface.Xls.SheetCount);
						AssertEquals(true, anotherExcelInterface.IsA1Selected());
						AssertEquals("A1", excelInterface.Xls.GetWindowScroll(TPanePosition.UpperLeft).CellRef);
						AssertEquals("A1", excelInterface.Xls.GetWindowScroll(TPanePosition.UpperRight).CellRef);
						AssertEquals("A1", excelInterface.Xls.GetWindowScroll(TPanePosition.LowerLeft).CellRef);
						AssertEquals("A1", excelInterface.Xls.GetWindowScroll(TPanePosition.LowerRight).CellRef);
					}
				}
			}
		}

		public void TestActiveWorksheet()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("#row", xlInterface.WorkSheets[0][1, 0]);
				AssertEquals("DocumentName", xlInterface.WorkSheets[1][1, 0]);
			}
		}

		public void TestClearRange()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("#row", xlInterface.WorkSheets[0][1, 0]);
				xlInterface.WorkSheets[0].ClearRange(0, 0, 2, 255);
				AssertEquals("", xlInterface.WorkSheets[0][1, 0]);
			}
		}

		public void TestRemoveRows()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("#row", xlInterface.WorkSheets[0][1, 0]);
				xlInterface.WorkSheets[0].RemoveRows(0, 1);
				AssertEquals("#row", xlInterface.WorkSheets[0][0, 0]);
			}
		}

		public void TestSaveExcelFile()
		{
			AssertSaveExcelFile(ReportTestXlsFilePath, "testOut.xls");
		}

		public void TestSaveExcelFileAsXlsx()
		{
			AssertSaveExcelFile(ReportTestXlsxFilePath, "testOut.xlsx");
		}

		[ExpectNoExceptions]
		public void TestExistingLoadExcelFile()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
			}
		}

		[ExpectNoExceptions]
		public void TestExistingLoadExcelFileAsXlsx()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectException(typeof(ExcelInterfaceException))]
		public void TestNonExistingLoadExcelFile()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "NonExistingFileName.xls");
			}
		}

		public void TestShouldNotCacheIncorrectExcelInterfaceWhenEnableCacheLoadedExcelInterfaceByBinary()
		{
			AssertNoExceptionThrown(() =>
			{
				using (ExcelInterface.EnableCacheLoadedExcelInterfaceByBinary())
				{
					AssertExceptionThrown<ExcelInterfaceException>(() => ExcelInterface.GetLoadedExcelInterface(ZBlob.Empty));
				}
			});
		}

		public void TestGetLoadedExcelInterface()
		{
			ExcelInterface excelInterface1;
			using (ExcelInterface.EnableCacheLoadedExcelInterfaceByBinary())
			using (var stream1 = File.OpenRead(DocumentTestXlsFilePath))
			using (excelInterface1 = ExcelInterface.GetLoadedExcelInterface(stream1))
			using (var excelInterface2 = ExcelInterface.GetLoadedExcelInterface(stream1))
			{
				AssertNotNull("ExcelInterface from stream1 should not be null.", excelInterface1);
				AssertEquals("ExcelInterface from stream1 should be in the cache.", excelInterface1, excelInterface2);

				excelInterface1.Dispose();
				Assert("ExcelInterface in the cache will not be disposed.", excelInterface1.WorkSheets.Count > 0);
			}

			AssertEquals("ExcelInterface has been disposed out of cache Scope.", 0, excelInterface1.WorkSheets.Count);

			ExcelInterface excelInterface3;
			using (var stream2 = File.OpenRead(DocumentTestXlsFilePath))
			using (excelInterface3 = ExcelInterface.GetLoadedExcelInterface(stream2))
			using (var excelInterface4 = ExcelInterface.GetLoadedExcelInterface(stream2))
			{
				AssertNotNull("ExcelInterface from stream2 should not be null.", excelInterface3);
				AssertNotEquals("ExcelInterface from stream2 should not be in the cache.", excelInterface3, excelInterface4);

				excelInterface4.Dispose();
				AssertEquals("ExcelInterface not cached can be disposed.", 0, excelInterface4.WorkSheets.Count);
			}

			AssertEquals("ExcelInterface has been disposed out of using block.", 0, excelInterface3.WorkSheets.Count);

			AssertExceptionThrown<ExcelInterfaceException>(() =>
			{
				using (var excelInterface5 = ExcelInterface.GetLoadedExcelInterface(ZBlob.Empty))
				{
				}
			});

			AssertExceptionThrown<ExcelInterfaceException>(() =>
			{
				using (ExcelInterface.EnableCacheLoadedExcelInterfaceByBinary())
				using (var excelInterface5 = ExcelInterface.GetLoadedExcelInterface(ZBlob.Empty))
				{
				}
			});
		}

		public void TestGetTFileFormat()
		{
			AssertEquals(TFileFormats.Xls, ExcelInterface.GetTFileFormat("xls"));
			AssertEquals(TFileFormats.Xls, ExcelInterface.GetTFileFormat("XLS"));
			AssertEquals(TFileFormats.Xlsx, ExcelInterface.GetTFileFormat("xlsx"));
			AssertEquals(TFileFormats.Xlsx, ExcelInterface.GetTFileFormat("XlsX"));
			AssertEquals(TFileFormats.Automatic, ExcelInterface.GetTFileFormat("XXX"));
		}

		public void TestGetExtensionForExcel()
		{
			AssertEquals(AttachmentTypeList.Codes.Xls, ExcelInterface.GetExtensionForExcel(TFileFormats.Xls));
			AssertEquals(AttachmentTypeList.Codes.Xlsx, ExcelInterface.GetExtensionForExcel(TFileFormats.Xlsx));
			AssertEquals(string.Empty, ExcelInterface.GetExtensionForExcel(TFileFormats.Pxl));
		}

		public void TestGetExtensionForExcelFromFile()
		{
			using (ExcelInterface excelInterface = New())
			{
				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsPath);
				AssertEquals(AttachmentTypeList.Codes.Xls, excelInterface.GetExtensionForExcelFromFile());

				excelInterface.LoadExcelFile(EmptyAndValidTemplateXlsxPath);
				AssertEquals(AttachmentTypeList.Codes.Xlsx, excelInterface.GetExtensionForExcelFromFile());
			}
		}

		public void TestGetCellFormula()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("=NOW()", xlInterface.WorkSheets[0].GetCellFormula(18, 5));
				AssertEquals("=SUM(I26:I30)", xlInterface.WorkSheets[0].GetCellFormula(30, 8));
			}
		}

		public void TestSetCellFormula()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				xlInterface.WorkSheets[0][0, 5] = 1;
				xlInterface.WorkSheets[0][1, 5] = 3;
				xlInterface.WorkSheets[0][2, 5] = 7;
				xlInterface.WorkSheets[0][3, 5] = 9;
				xlInterface.WorkSheets[0].SetCellFormula(4, 5, "=SUM(F1:F4)", 20);
				AssertEquals("20", xlInterface.WorkSheets[0][4, 5].ToString());
				AssertEquals("=SUM(F1:F4)", xlInterface.WorkSheets[0].GetCellFormula(4, 5));
			}
		}

		public void TestSheetCount()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				AssertEquals(3, xlInterface.SheetCount);
			}
		}

		public void TestDuplicateRows()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("#row", xlInterface.WorkSheets[0][1, 0]);
				xlInterface.WorkSheets[0].DuplicateRows(1, 1, 2, 2);
				AssertEquals("#row", xlInterface.WorkSheets[0][2, 0]);
				AssertEquals("#row", xlInterface.WorkSheets[0][3, 0]);
			}
		}

		public void TestCopySheet()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("#row", xlInterface.WorkSheets[0][1, 0]);
				AssertEquals(3, xlInterface.WorkSheets.Count);
				xlInterface.CopySheet(xlInterface.WorkSheets[0], 2);
				AssertEquals(5, xlInterface.WorkSheets.Count);
				AssertEquals("#row", xlInterface.WorkSheets[1][1, 0]);
				AssertEquals("#row", xlInterface.WorkSheets[2][1, 0]);
				AssertEquals("DocumentName", xlInterface.WorkSheets[3][1, 0]);
				AssertEquals("Sections", xlInterface.WorkSheets[4][1, 0]);
				AssertEquals("OutputType", xlInterface.WorkSheets[3][0, 0]);
			}
		}

		public void TestGetRowHeight()
		{
			using (ExcelInterface excelInterface = New())
			{
				excelInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(255, excelInterface.WorkSheets[0].GetRowHeight(1));

				excelInterface.Xls.SetRowHidden(2, true); //FexCel rows start at 1.
				AssertEquals(0, excelInterface.WorkSheets[0].GetRowHeight(1));

				excelInterface.Xls.SetRowHidden(2, false);
				AssertEquals(255, excelInterface.WorkSheets[0].GetRowHeight(1));

				excelInterface.WorkSheets[0].SetRowHeight(1, 0);
				AssertEquals(0, excelInterface.WorkSheets[0].GetRowHeight(1));
			}
		}

		public void TestSetRowHeight()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(255, xlInterface.WorkSheets[0].GetRowHeight(1));
				xlInterface.WorkSheets[0].SetRowHeight(1, 510);
				AssertEquals(510, xlInterface.WorkSheets[0].GetRowHeight(1));
			}
		}

		public void TestTopMargin()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(0.984251968503937, xlInterface.GetTopMargin());
			}
		}

		public void TestBottomMargin()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(0.984251968503937, xlInterface.GetBottomMargin());
			}
		}

		public void TestGetCellWidth()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(2742, xlInterface.WorkSheets[0].GetCellWidth(1, 3));
				AssertEquals(8044, xlInterface.WorkSheets[0].GetCellWidth(2, 3));
			}
		}

		public void TestGetCellFontSize()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(200, xlInterface.WorkSheets[0].GetCellFontSize(1, 3));
				AssertEquals(320, xlInterface.WorkSheets[0].GetCellFontSize(2, 3));
			}
		}

		public void TestGetCharCountFittingInCell()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(9, xlInterface.WorkSheets[0].GetCharCountFittingInCell(1, 3));
				AssertEquals(17, xlInterface.WorkSheets[0].GetCharCountFittingInCell(2, 3));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCharCountFittingInCell2()
		{
			using (ExcelInterface xlInterface = New())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "AutoHeightTester.xls");
				AssertEquals(17, xlInterface.WorkSheets[0].GetCharCountFittingInCell(44, 3));
			}
		}

		public void TestNewXlsFile()
		{
			string tempFileName = Env.GetTempFileName();
			try
			{
				using (ExcelInterface xlInterface = New())
				{
					xlInterface.NewExcelFile(4);
					AssertEquals(4, xlInterface.WorkSheets.Count);
					xlInterface.WorkSheets[2][42, 69] = "testing";
					xlInterface.SaveToFile(tempFileName);
				}

				using (ExcelInterface xlInterface = New())
				{
					xlInterface.LoadExcelFile(tempFileName);
					AssertEquals(4, xlInterface.WorkSheets.Count);
					AssertEquals("Value written to new sheet should be the same when we read it back", "testing", xlInterface.WorkSheets[2][42, 69]);
				}
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		public void TestPreviewInXl()
		{
			string lastTempFileName = null;

			using (var xlInterface1 = New())
			{
				const int WorkSheetCount = 1;
				xlInterface1.NewExcelFile(WorkSheetCount);
				xlInterface1.WorkSheets[0][0, 0] = "Homer";
				using (ExcelInterface xlInterface2 = New())
				{
					xlInterface1.PreviewInXl();
					lastTempFileName = xlInterface1.LastFileName;
					xlInterface2.LoadExcelFile(xlInterface1.LastFileName);
					AssertEquals("PreviewInXl() should have written an Excel file containing valid data.", "Homer", xlInterface2.WorkSheets[0][0, 0]);
				}
			}

			System.Threading.Thread.Sleep(3000); // ensure adequate time for delete thread to finish
			AssertEquals("The temp file created by PreviewInXL() should have been deleted.", false, File.Exists(lastTempFileName));
		}

		public void TestPreviewInXlDoesNotDeleteFileBeforeXlProcessEnds()
		{
			string lastTempFileName = null;

			using (var xlInterface1 = new ExcelInterfaceWithDelayedProcess(5))
			{
				const int WorkSheetCount = 1;
				xlInterface1.NewExcelFile(WorkSheetCount);
				xlInterface1.WorkSheets[0][0, 0] = "Homer";
				using (ExcelInterface xlInterface2 = New())
				{
					xlInterface1.PreviewInXl();
					lastTempFileName = xlInterface1.LastFileName;
					xlInterface2.LoadExcelFile(xlInterface1.LastFileName);
					AssertEquals("PreviewInXl() should have written an Excel file containing valid data.", "Homer", xlInterface2.WorkSheets[0][0, 0]);
				}
			}

			System.Threading.Thread.Sleep(3000);
			AssertEquals("The temp file created by PreviewInXL() should not yet be deleted.", true, File.Exists(lastTempFileName));

			System.Threading.Thread.Sleep(10000);
			AssertEquals("The temp file created by PreviewInXL() should have been deleted.", false, File.Exists(lastTempFileName));
		}

		public void TestPreviewInXl_ShouldDisposeFileOnIOExceptions()
		{
			try
			{
				using (var sourceTempFile = TempFile.NewWithExtension("XLS"))
				{
					var tempFile = new TempFileWithDisposedStatus(sourceTempFile.Filename);

					using (var excelInterface = New())
					using (var stream = File.OpenRead(tempFile.Filename))
					{
						excelInterface.PreviewInXl(tempFile);

						AssertMatch(new Regex("The process cannot access the file '.+' because it is being used by another process."), UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Should try to dispose the file - has caused DisposableLeakListener to fail unit tests", true, tempFile.IsDisposed);
					}
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestPreviewInXlWithNullProcess()
		{
			string lastTempFileName = null;

			using (var xlInterface1 = new ExcelInterfaceWithNullProcess())
			{
				const int WorkSheetCount = 1;
				xlInterface1.NewExcelFile(WorkSheetCount);
				xlInterface1.WorkSheets[0][0, 0] = "Homer";
				using (ExcelInterface xlInterface2 = New())
				{
					xlInterface1.PreviewInXl();
					lastTempFileName = xlInterface1.LastFileName;
					xlInterface2.LoadExcelFile(xlInterface1.LastFileName);
					AssertEquals("PreviewInXl() should have written an Excel file containing valid data.", "Homer", xlInterface2.WorkSheets[0][0, 0]);
				}
			}

			System.Threading.Thread.Sleep(3000); // ensure adequate time for delete thread to finish
			AssertEquals("The temp file created by PreviewInXL() should have been deleted.", false, File.Exists(lastTempFileName));
		}

		public void TestPreviewInXlWithoutDeletingFile()
		{
			string tempFileName = Env.GetTempFileName();

			try
			{
				using (ExcelInterface xlInterface = New())
				{
					const int WorkSheetCount = 1;
					xlInterface.NewExcelFile(WorkSheetCount);
					xlInterface.WorkSheets[0][0, 0] = "Homer";
					xlInterface.PreviewInXlWithoutDeletingFile(tempFileName);
				}

				using (ExcelInterface xlInterface = New())
				{
					xlInterface.LoadExcelFile(tempFileName);
					AssertEquals("PreviewInXlWithoutDeletingFile() should have written an Excel file containing valid data.", "Homer", xlInterface.WorkSheets[0][0, 0]);
				}
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToMultiPageTiffAndScaleDoesNotPrintBlackForLandscape()
		{
			using (var tempFile = TempFile.NewWithExtension("TIF"))
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "OnePagePortraitOnePageLandscape.xls");
				xlInterface.ExportToMultiPageTiffAndScale(tempFile.Filename, false, 100, true, 196, PixelFormat.Format1bppIndexed, null);

				using (var result = (Bitmap)Bitmap.FromFile(tempFile.Filename))
				{
					AssertEquals("Page Count", 2, result.GetFrameCount(FrameDimension.Page));
					result.SelectActiveFrame(FrameDimension.Page, 1);
					var rightmostPixelColour = result.GetPixel(result.Width - 1, 0); // get the rightmost pixel from the 2nd page.

					// colour is in ARGB format, so not a named colour
					AssertEquals("Corner should be white", Color.FromArgb(255, Color.White), rightmostPixelColour);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveWorkSheetHavingFrozenPanelToXlsxFile()
		{
			using (var xlInterface = new ExcelInterface())
			using (var outputStream = new MemoryStream())
			using (var reader = new TZippyReader())
			{
				var fileName = Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, "ExcelFileHavingFrozenPanel.xls");
				xlInterface.LoadExcelFile(fileName);
				xlInterface.SaveToStream(outputStream, "XLSX");

				reader.Open(outputStream, true);
				var xml = XmlReader.Create(reader.GetFile("xl/worksheets/sheet1.xml"));
				var topLeftCellAttributeValue = string.Empty;

				while (xml.Read())
				{
					if (xml.Name == "pane")
					{
						topLeftCellAttributeValue = xml.GetAttribute("topLeftCell");
						break;
					}
				}
				AssertEquals("Value of topLeftCell attribute should be equal to the CellRef of frozenPane", xlInterface.Xls.GetFrozenPanes().CellRef, topLeftCellAttributeValue);
			}
		}

		public void TestCreateBitmapFailedAndThrowsException()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				//No Exception
				Bitmap bmp = xlInterface.CreateBitmap(300, new TPaperDimensions("aaa", 10, 10), PixelFormat.Format24bppRgb);
			}

			// Throw Argument Exception
			using (MockExcelInterfaceWhichThrowArgumentException xlInterface = new MockExcelInterfaceWhichThrowArgumentException())
			{
				try
				{
					Bitmap bmp = xlInterface.CreateBitmap(300, new TPaperDimensions("bbb", 1000, 1000), PixelFormat.Format24bppRgb);
					Fail("Should throw exception one line above");
				}
				catch (BitmapCreationException ex)
				{
					Assert("Exception should be BitmapCreationException", ex is BitmapCreationException);
				}
			}

			// Throw Generic Exception
			using (MockExcelInterfaceWhichThrowGenericException xlInterface = new MockExcelInterfaceWhichThrowGenericException())
			{
				try
				{
					Bitmap bmp = xlInterface.CreateBitmap(300, new TPaperDimensions("ccc", 1000, 1000), PixelFormat.Format24bppRgb);
					Fail("Should throw exception one line above");
				}
				catch (BitmapCreationException ex)
				{
					Assert("Exception should be BitmapCreationException", ex is BitmapCreationException);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPrintTitlesRange()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "PrintTitlesSingle.xlsx");

				var printTitlesRangeSheet1 = excelInterface.GetPrintTitlesRange(0);
				AssertEquals(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), printTitlesRangeSheet1.Name);
				AssertEquals("='Sheet 1'!$B:$D,'Sheet 1'!$5:$7", printTitlesRangeSheet1.RangeFormula);
			}

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "PrintTitlesMultiple.xlsx");

				var printTitlesRangeSheet1 = excelInterface.GetPrintTitlesRange(0);
				AssertEquals(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), printTitlesRangeSheet1.Name);
				AssertEquals("='Sheet 1'!$B:$D,'Sheet 1'!$5:$7", printTitlesRangeSheet1.RangeFormula);

				var printTitlesRangeSheet2 = excelInterface.GetPrintTitlesRange(1);
				AssertNull(printTitlesRangeSheet2);

				var printTitlesRangeSheet3 = excelInterface.GetPrintTitlesRange(2);
				AssertEquals(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), printTitlesRangeSheet3.Name);
				AssertEquals("='Sheet 3'!$B:$C,'Sheet 3'!$5:$6", printTitlesRangeSheet3.RangeFormula);
			}
		}

		public void TestSetPrintTitlesRange()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(3);

				var existingPrintTitlesSheet1 = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, 1);
				var existingPrintTitlesSheet2 = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, 2);
				var existingPrintTitlesSheet3 = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, 3);

				AssertNull(existingPrintTitlesSheet1);
				AssertNull(existingPrintTitlesSheet2);
				AssertNull(existingPrintTitlesSheet3);

				var newPrintTitlesRangeSheet1 = new TXlsNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 1, 0, "=$B:$D,$5:$7");
				excelInterface.SetPrintTitlesRange(newPrintTitlesRangeSheet1);

				var newPrintTitlesRangeSheet3 = new TXlsNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 3, 0, "=$A:$F,$4:$8");
				excelInterface.SetPrintTitlesRange(newPrintTitlesRangeSheet3);

				existingPrintTitlesSheet1 = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, 1);
				existingPrintTitlesSheet2 = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, 2);
				existingPrintTitlesSheet3 = excelInterface.Xls.GetNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), 0, 3);

				AssertEquals(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), existingPrintTitlesSheet1.Name);
				AssertEquals("=Sheet1!$B:$D,Sheet1!$5:$7", existingPrintTitlesSheet1.RangeFormula);

				AssertNull(existingPrintTitlesSheet2);

				AssertEquals(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), existingPrintTitlesSheet3.Name);
				AssertEquals("=Sheet3!$A:$F,Sheet3!$4:$8", existingPrintTitlesSheet3.RangeFormula);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShouldNotReportFlexCelXlsAdapterExceptionIfFileIsUnreadable()
		{
			using (var excelInterface = new ExcelInterface())
			{
				Globals.IsUserInteractive = false;
				ErrorReporter.Clear();

				AssertExceptionThrown<ExcelInterfaceException>(() => excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "BadExcelFormatTest.xls"));
				AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestXlsStreamReportedIfUnreadable_DoNotReportErrorWhenLoadExcelFile()
		{
			var xlsBadStream = File.ReadAllBytes(UnitTestingConstants.TestFilesDir + "BadExcelFormatTest.xls");

			using (var excelInterface = new ExcelInterface())
			using (var xlsStreamFile = new MemoryStream(xlsBadStream))
			{
				Globals.IsUserInteractive = false;
				ErrorReporter.Clear();

				AssertExceptionThrown<ExcelInterfaceException>(() => excelInterface.LoadExcelFile(xlsStreamFile));
				AssertNullOrEmpty("ErrorReporter.LastMessageReported", ErrorReporter.LastMessageReported);
			}
		}

		public void TestGetRangeFormulaWithNewSheetName()
		{
			AssertEquals(@"='Sheet 4'!$B:$C,'Sheet 4'!$5:$6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"='Sheet 3'!$B:$C,'Sheet 3'!$5:$6", "Sheet 3", "Sheet 4"));
			AssertEquals(@"='Sheet4'!$B:$C,'Sheet4'!$5:$6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"='Sheet 3'!$B:$C,'Sheet 3'!$5:$6", "Sheet 3", "Sheet4"));
			AssertEquals(@"='Sheet4'!$B:$C,'Sheet4'!$5:$6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"=Sheet3!$B:$C,Sheet3!$5:$6", "Sheet3", "Sheet4"));
			AssertEquals(@"$B:$C,$5:$6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"$B:$C,$5:$6", "Sheet3", "Sheet 4"));
			AssertEquals(@"=B:C,5:6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"=B:C,5:6", "Sheet3", "Sheet 4"));
			AssertEquals(@"=5:6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"=5:6", "Sheet3", "Sheet 4"));
			AssertEquals(@"='Test Sheet2'!$B:$C,'Test Sheet2'!$5:$6", ExcelInterface.GetRangeFormulaWithNewSheetName(@"='Test Sheet'!$B:$C,'Test Sheet'!$5:$6", "Test Sheet", "Test Sheet2"));
		}

		public void TestGetFirstAndLastRowsToRepeat()
		{
			var expectedValidRows = new Tuple<int, int>(5, 6);
			var expectedInvalidRows = new Tuple<int, int>(0, 0);

			AssertEquals(expectedValidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"'Sheet 3'!$B:$C,'Sheet 3'!$5:$6"));
			AssertEquals(expectedValidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=Sheet3!$B:$C,Sheet3!$5:$6"));
			AssertEquals(expectedValidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"$B:$C,$5:$6"));
			AssertEquals(expectedValidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=B:C,5:6"));
			AssertEquals(expectedValidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=5:6"));
			AssertEquals(new Tuple<int, int>(6, 6), ExcelInterface.GetFirstAndLastRowsToRepeat(@"=6:6"));

			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"'Sheet 3'!$B:$C,'Sheet 3'!$5:$4"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"'Sheet 3'!$B:$C"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=$B:$C"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=B:C"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=2:C"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=C:10"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=5:4"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=0:10"));
			AssertEquals(expectedInvalidRows, ExcelInterface.GetFirstAndLastRowsToRepeat(@"=0:0"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFormulaTooLongException()
		{
			using (var excelInterface = New())
			{
				excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "TestFormulaTooLong.xlsx");

				using (var memoryStream = new MemoryStream())
				{
					AssertExceptionThrown<DocumentEngineTooLongFormulaForThisFileFormatException>("Should throw formula too long exception", "Formula with too many characters for this file format in resulting report.", () => excelInterface.SaveToStream(memoryStream, "xls"));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetColumnWidthInPixels()
		{
			var xlsStream = File.ReadAllBytes(UnitTestingConstants.TestFilesDir + "Report1.xls");

			using (var excelInterface = new ExcelInterface())
			using (var xlsStreamFile = new MemoryStream(xlsStream))
			{
				Globals.IsUserInteractive = false;
				excelInterface.LoadExcelFile(xlsStreamFile);
				AssertEquals(191, excelInterface.GetColumnWidthInPixels(1, 2));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToHtmlAndScaleWithEDILink()
		{
			using (var xlInterface = New())
			{
				xlInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestFilesDir, @"Test With EDI Link.xlsx"));
				var result = xlInterface.ExportToHTMLAndScale(null, 100);
				var htmlString = UTF8.GetString(result.Data);
				AssertContains("<a href=\"edient:Command=ShowEditForm&amp;LicenceCode=WTLDCNJNC&amp;ControllerID=JobShipment&amp;BusinessEntityPK=208dd56f-16d8-43d6-a7c2-d2974bc3f923&amp;Hash=%2bnwydvFXRLOpdzyqY4EikfbeKxGiaOPDw\" title=\"Click Here\" style='text-decoration: none;'>", htmlString);
			}
		}

		public void TestNoExceptionForEmptyFileNameForHtmlOrHtmf()
		{
			using (var xlInterface = New())
			{
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
				AssertNoExceptionThrown(() => { xlInterface.ExportToHTMLAndScale("", 100); });
				AssertNoExceptionThrown(() => { xlInterface.ExportToHTMLAndScale(null, 100); });
				AssertNoExceptionThrown(() => { xlInterface.ExportFirstSheetToHTMLAndRestToPDF("", 100, null); });
				AssertNoExceptionThrown(() => { xlInterface.ExportFirstSheetToHTMLAndRestToPDF(null, 100, null); });
			}
		}

		public void TestExportFirstSheetToHTMLAndRestToPDF_TooLongFileName()
		{
			var tempDirectory = Temp.TempPathWithoutCreating;
			var fileName = $@"{tempDirectory}\BOOKING INFORMATION - S00001435 - PZT24,PZT25,PZT26,PZT27,PZT82,PZT58,PZT62,PZT63,PZT84,PZT86,PZT74,PZT75,PZT76,PZT77,PZT24,PZT25,PZT26,PZT27,PZT82,PZT58,PZT62,PZT63,PZT84,PZT86,PZT74,PZT75,PZT76,P.HTML";
			var safeFileName = PathValidation.GetFilePathWithValidLength(fileName);

			try
			{
				using (var stream = new FileStream(safeFileName, FileMode.Create))
				{
					stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
				}

				using (var xlInterface = New())
				{
					xlInterface.LoadExcelFile(DocumentTestXlsFilePath);
					AssertNoExceptionThrown(() =>
					{
						xlInterface.ExportFirstSheetToHTMLAndRestToPDF(safeFileName, 100, null, true);
					});
				}
			}
			finally
			{
				if (File.Exists(safeFileName))
				{
					File.Delete(safeFileName);
				}
			}
		}

		public void TestExportToHtmlAndScale()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(ReportTestXlsFilePath);

					var result = xlInterface.ExportToHTMLAndScale(tempFileName, 100);

					Assert(result.isHTML);
					Assert(result.Data.Length > 0);
					AssertEquals(tempFileName, result.Path);
					AssertEquals(8, result.AdditionalAttachments.Count);
					foreach (var attachment in result.AdditionalAttachments)
					{
						Assert(attachment.Data.Length > 0);
					}

					var htmlAsString = UTF8.GetString(result.Data);
					AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
					AssertContains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">", htmlAsString);
					AssertContains("{Consignee.oh_FullName}", htmlAsString);
					AssertContains("Excel Sheet: Seafreight Prealert", htmlAsString);
					AssertContains("Data should be normal text at all times", htmlAsString);
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(tempFileName);
					TempDirectory.DeleteDirectory(Path.Combine(Path.GetDirectoryName(tempFileName), DocumentConverter.GetSafeImagesDirectoryName(tempFileName)), false);
				}
			}
		}

		public void TestExportToHtmlAndScale_NoImages()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(DocumentTestXlsFilePath);

					var result = xlInterface.ExportToHTMLAndScale(tempFileName, 100);

					Assert(result.isHTML);
					Assert(result.Data.Length > 0);
					AssertEquals(tempFileName, result.Path);
					AssertEquals(0, result.AdditionalAttachments.Count);

					var htmlAsString = UTF8.GetString(result.Data);
					AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
					AssertContains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">", htmlAsString);
					AssertContains("Century", htmlAsString);
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(tempFileName);
				}
			}
		}

		public void TestExportToHtmfAndScale()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(ReportTestXlsFilePath);

					var result = xlInterface.ExportFirstSheetToHTMLAndRestToPDF(tempFileName, 100, null);

					Assert(result.isHTML);
					Assert(result.Data.Length > 0);
					AssertEquals(tempFileName, result.Path);
					AssertEquals(9, result.AdditionalAttachments.Count);
					foreach (var attachment in result.AdditionalAttachments)
					{
						Assert(attachment.Data.Length > 0);
					}
					Assert(result.AdditionalAttachments[8].Filename.EndsWith(".pdf"));

					var htmlAsString = UTF8.GetString(result.Data);
					AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
					AssertContains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">", htmlAsString);
					AssertContains("{Consignee.oh_FullName}", htmlAsString);
					AssertNotContains("DocumentVersion", htmlAsString);
					AssertNotContains("Data should be normal text at all times", htmlAsString);
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(tempFileName);
					TempDirectory.DeleteDirectory(Path.Combine(Path.GetDirectoryName(tempFileName), DocumentConverter.GetSafeImagesDirectoryName(tempFileName)), false);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToHtmlAndScale_WhenPictureNotInHtml()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, @"TestHtmlContentNotContainsAllPictures.xls"));

					var result = xlInterface.ExportFirstSheetToHTMLAndRestToPDF(tempFileName, 100, null);

					Assert(result.isHTML);
					Assert(result.Data.Length > 0);
					AssertEquals(tempFileName, result.Path);
					AssertEquals(1, result.AdditionalAttachments.Count);
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(tempFileName);
					TempDirectory.DeleteDirectory(Path.Combine(Path.GetDirectoryName(tempFileName), DocumentConverter.GetSafeImagesDirectoryName(tempFileName)), false);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToHtmfAndScale_WhenPictureNotInHtml()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, @"TestHtmlContentNotContainsAllPicturesWithHTMF.xls"));

					var result = xlInterface.ExportFirstSheetToHTMLAndRestToPDF(tempFileName, 100, null);

					Assert(result.isHTML);
					Assert(result.Data.Length > 0);
					AssertEquals(tempFileName, result.Path);
					AssertEquals(2, result.AdditionalAttachments.Count);
					Assert(result.AdditionalAttachments[1].Filename.EndsWith(".pdf"));
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(tempFileName);
					TempDirectory.DeleteDirectory(Path.Combine(Path.GetDirectoryName(tempFileName), DocumentConverter.GetSafeImagesDirectoryName(tempFileName)), false);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToHtmfAndScale_WhenOnlyOneDocument()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentScanningFilesPath, @"test.xls"));

					var result = xlInterface.ExportFirstSheetToHTMLAndRestToPDF(tempFileName, 100, null);

					Assert(result.isHTML);
					Assert(result.Data.Length > 0);
					AssertEquals(tempFileName, result.Path);
					AssertEquals(0, result.AdditionalAttachments.Count);

					var htmlAsString = UTF8.GetString(result.Data);
					AssertContains("<meta name=\"Generator\" content=\"FlexCel", htmlAsString);
					AssertContains("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">", htmlAsString);
					AssertContains("This is a test document", htmlAsString);
				}
				finally
				{
					TempFile.TryDeleteHandleAllExceptions(tempFileName);
				}
			}
		}

		public void TestExportToHtmlAndScale_DeletesFilesIfAskedTo()
		{
			using (var xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				xlInterface.LoadExcelFile(DocumentTestXlsFilePath);

				xlInterface.ExportToHTMLAndScale(null, 100);
				xlInterface.ExportToHTMLAndScale(tempFileName, 100, true);

				Assert(!File.Exists(tempFileName));
				Assert(!Directory.Exists(Path.Combine(Path.GetDirectoryName(tempFileName), DocumentConverter.GetSafeImagesDirectoryName(tempFileName))));
			}
		}

		[RequiresSoftware(RequiredSoftware.OfficeFonts)]
		public void TestExportToPdfAndScale()
		{
			using (ExcelInterface xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(DocumentTestXlsFilePath);

					DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					xlInterface.ExportToPdfAndScale(tempFileName, 100, null);
					var fileInfo = new FileInfo(tempFileName);
					var fileSizeWithFonts = fileInfo.Length;

					DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					xlInterface.ExportToPdfAndScale(tempFileName, 100, null);
					fileInfo = new FileInfo(tempFileName);
					var fileSizeWithoutFonts = fileInfo.Length;

					Assert("file size with fonts should be > file size without fonts", fileSizeWithFonts > fileSizeWithoutFonts);
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.OfficeFonts)]
		public void TestExportToPdfAndScale_ToStream()
		{
			using (ExcelInterface xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(DocumentTestXlsFilePath);

					DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					using (var stream = File.Create(tempFileName))
					{
						xlInterface.ExportToPdfAndScale(stream, 100, null);
					}
					var fileInfo = new FileInfo(tempFileName);
					var fileSizeWithFonts = fileInfo.Length;

					DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					using (var stream = File.Create(tempFileName))
					{
						xlInterface.ExportToPdfAndScale(stream, 100, null);
					}
					fileInfo = new FileInfo(tempFileName);
					var fileSizeWithoutFonts = fileInfo.Length;

					Assert("file size with fonts should be > file size without fonts", fileSizeWithFonts > fileSizeWithoutFonts);
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToPdfAndScale_SpecificSheets()
		{
			using (var xlInterface = New())
			{
				var fullPdfTempFileName = Env.GetTempFileName();
				var pdfFileWithTwoSheets = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestFilesDir, "EmptyAndValidTemplateWithThreeSheets.xls"));
					using (var stream = File.Create(pdfFileWithTwoSheets))
					{
						xlInterface.ExportToPdfAndScale(stream, 100, null, new int[] { 1, 3 });
					}
					using (var pdfFile = PreviewableDocumentHelper.GetPreviewableDocument("PDF", File.ReadAllBytes(pdfFileWithTwoSheets)))
					{
						AssertEquals("Should only export 2 sheets if sheet index to export is set", 2, pdfFile.NumberOfPages);
					}
					using (var stream = File.Create(fullPdfTempFileName))
					{
						xlInterface.ExportToPdfAndScale(stream, 100, null);
					}
					using (var pdfFile = PreviewableDocumentHelper.GetPreviewableDocument("PDF", File.ReadAllBytes(fullPdfTempFileName)))
					{
						AssertEquals("Should export all sheets", 3, pdfFile.NumberOfPages);
					}
				}
				finally
				{
					File.Delete(fullPdfTempFileName);
					File.Delete(pdfFileWithTwoSheets);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToPdfWithUnicode()
		{
			DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertExportToPdf("UnicodeTest.xls", TPdfType.Standard, true);
			AssertExportToPdf("UnicodeTest.xls", TPdfType.PDFA1, true);
			AssertExportToPdf("UnicodeTest.xls", TPdfType.PDFA2, true);
			AssertExportToPdf("UnicodeTest.xls", TPdfType.PDFA3, true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToPdfAndScale_WithoutWatermark()
		{
			AssertExportToPdf("test.xls", TPdfType.Standard);
			AssertExportToPdf("test.xls", TPdfType.PDFA1);
			AssertExportToPdf("test.xls", TPdfType.PDFA2);
			AssertExportToPdf("test.xls", TPdfType.PDFA3);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToPdf_PDFA3()
		{
			FlexCelPdfA3FileInfo[] files = {
				new FlexCelPdfA3FileInfo(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, "Test.xls"), StandardMimeType.Xls, "Test Desc", TPdfAttachmentKind.Source),
				new FlexCelPdfA3FileInfo(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, "BarcodesTest.xlsx"), StandardMimeType.Xlsx, "BarcodesTest Desc", TPdfAttachmentKind.Data)
			};

			using (var excelInterface = New())
			using (var stream = new MemoryStream())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, "UnicodeTest.xls"));
				excelInterface.ExportToPdfAndScale(stream, 100, null, TPdfType.PDFA3, files: files);

				using (var doc = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly))
				{
					var objects = doc.Internals.GetAllObjects();

					AssertEquals("Attached file name", "(BarcodesTest.xlsx)", ((PdfDictionary)objects[33]).Elements["/F"].ToString());
					AssertEquals("Attached file description", "(BarcodesTest Desc)", ((PdfDictionary)objects[33]).Elements["/Desc"].ToString());
					AssertEquals("Attached file attachmentKind", "/Data", ((PdfDictionary)objects[33]).Elements["/AFRelationship"].ToString());
					AssertEquals("Attached file name", "(Test.xls)", ((PdfDictionary)objects[34]).Elements["/F"].ToString());
					AssertEquals("Attached file description", "(Test Desc)", ((PdfDictionary)objects[34]).Elements["/Desc"].ToString());
					AssertEquals("Attached file attachmentKind", "/Source", ((PdfDictionary)objects[34]).Elements["/AFRelationship"].ToString());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToFaxFormat()
		{
			ExportToImage(true, 196, PixelFormat.Format1bppIndexed, "test.xls", 5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportTo256ColorsFormat()
		{
			ExportToImage(false, 96, PixelFormat.Format8bppIndexed, "test.xls", 5);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportTo256ColorsFormat2()
		{
			//This will try to test the octree for a very big image. On the original implementation, it would overflow.
			ExportToImage(false, 300, PixelFormat.Format8bppIndexed, "Test256Colors.xls", 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportToTrueColorFormat()
		{
			ExportToImage(false, 300, PixelFormat.Format24bppRgb, "test.xls", 5);
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

		string documentTestXlsFilePath;
		string DocumentTestXlsFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(documentTestXlsFilePath))
				{
					documentTestXlsFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.Test.xls");
				}
				return documentTestXlsFilePath;
			}
		}

		string reportTestXlsFilePath;
		string ReportTestXlsFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(reportTestXlsFilePath))
				{
					reportTestXlsFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.test.xls");
				}
				return reportTestXlsFilePath;
			}
		}

		string reportTestXlsxFilePath;
		string ReportTestXlsxFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(reportTestXlsxFilePath))
				{
					reportTestXlsxFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.test.xlsx");
				}
				return reportTestXlsxFilePath;
			}
		}

		string emptyAndValidTemplateXlsPath;
		string EmptyAndValidTemplateXlsPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyAndValidTemplateXlsPath))
				{
					emptyAndValidTemplateXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls");
				}
				return emptyAndValidTemplateXlsPath;
			}
		}

		string emptyAndValidTemplateXlsxPath;
		string EmptyAndValidTemplateXlsxPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyAndValidTemplateXlsxPath))
				{
					emptyAndValidTemplateXlsxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xlsx");
				}
				return emptyAndValidTemplateXlsxPath;
			}
		}

		ExcelInterfaceForTesting New() => new ExcelInterfaceForTesting();

		void SetDigitalSignatureRegistry(bool isCorrectDigitalSignatureRegistry = true)
		{
			var digitalSignatureRegistry = new DigitalSignatureRegistry();
			digitalSignatureRegistry.DigitalSignature = pfx_BuildChainExtraStoreUntrustedRoot;
			digitalSignatureRegistry.CertificatePassword = "test";
			digitalSignatureRegistry.SignatureDetailsEmail = "Sango@Sango.com";
			digitalSignatureRegistry.SignatureDetailsLocation = "Sango";
			digitalSignatureRegistry.SignatureDetailsName = "Sango";

			if (!isCorrectDigitalSignatureRegistry)
			{
				digitalSignatureRegistry.CertificatePassword = "ErrorPassword";
			}
			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty,
					Guid.Empty,
					Guid.Empty,
					digitalSignatureRegistry
			);
		}

		static byte[] StringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length)
							 .Where(x => x % 2 == 0)
							 .Select(x => System.Convert.ToByte(hex.Substring(x, 2), 16))
							 .ToArray();
		}

		void AssertSaveExcelFile(string filename, string filenameOut)
		{
			var testFile = Env.TempPath + filenameOut;
			if (File.Exists(testFile))
			{
				File.Delete(testFile);
			}
			try
			{
				using (ExcelInterface xlInterface = New())
				{
					xlInterface.LoadExcelFile(filename);
					xlInterface.SaveToFile(testFile);
				}

				using (ExcelInterface xlInterface1 = New())
				{
					xlInterface1.LoadExcelFile(testFile);
					AssertEquals("#row", xlInterface1.WorkSheets[0][1, 0]);
				}
			}
			finally
			{
				File.Delete(testFile);
			}
		}

		void AssertExportToPdf(string fileName, TPdfType pdfType, bool checkUnicode = false)
		{
			using (ExcelInterface excelInterface = New())
			using (var stream = new MemoryStream())
			{
				excelInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentsFilesDir, fileName));
				excelInterface.ExportToPdfAndScale(stream, 100, null, pdfType);

				var actual = stream.ToArray();
				if (pdfType == TPdfType.PDFA2 || pdfType == TPdfType.PDFA3)
				{
					Assert("Should return PDF/A data", ImageToPDFConverter.IsPDFA(actual));
				}
				else
				{
					Assert("Should return PDF data", ImageToPDFConverter.IsPDF(actual));
				}
				if (checkUnicode)
				{
					using (var doc = PdfReader.Open(stream, PdfDocumentOpenMode.ReadOnly))
					{
						var fonts = new List<string>();
						var objects = doc.Internals.GetAllObjects();
						foreach (var obj in objects)
						{
							if (obj is PdfDictionary temp)
							{
								var font = temp.Elements["/BaseFont"]?.ToString().Replace(" ", string.Empty);
								if (!string.IsNullOrEmpty(font) && !fonts.Contains(font))
								{
									fonts.Add(font);
								}
							}
						}
						var fallbackFonts = excelInterface.ExportFallbackFonts_Exposed.Replace(" ", string.Empty).Split(';');
						var shouldHaveOne = fallbackFonts.Any(x => fonts.Any(y => y.Contains(x, StringComparison.CurrentCultureIgnoreCase)));
						Assert("At least one font of the PDF export should be in the list of the FlexCel fallback fonts", shouldHaveOne);
					}
				}
			}
		}

		void ExportToImage(bool toFaxFormat, int resolution, PixelFormat pixFmt, string fileName, int numPages)
		{
			using (ExcelInterface xlInterface = New())
			{
				var tempFileName = Env.GetTempFileName();
				try
				{
					xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + fileName);
					xlInterface.ExportToMultiPageTiffAndScale(tempFileName, false, 100, toFaxFormat, resolution, pixFmt, null);
					using (var bmp = (Bitmap)Bitmap.FromFile(tempFileName))
					{
						AssertEquals(bmp.GetFrameCount(FrameDimension.Page), numPages);
						for (var i = 0; i < bmp.GetFrameCount(FrameDimension.Page); i++)
						{
							bmp.SelectActiveFrame(FrameDimension.Page, i);
							AssertEquals(bmp.HorizontalResolution, (float)resolution);
							AssertEquals(bmp.VerticalResolution, (float)resolution);
							AssertEquals(bmp.PixelFormat, pixFmt);
						}
					}
				}
				finally
				{
					File.Delete(tempFileName);
				}
			}
		}

		delegate void ExcelInterfaceSaveMethod(ExcelInterface excelInterface);

		static void AssertSaveMethodWrapsCellFormatException(ExcelInterfaceSaveMethod executeSave)
		{
			using (var excelInterface = GetExcelInterfaceWithTooManyCellFormatsSetupToSave())
			{
				try
				{
					executeSave(excelInterface);
					Fail("Exception should have been thrown.");
				}
				catch (ExcelInterfaceException exception)
				{
					if (exception.Type != ExcelInterfaceExceptionType.TooManyCellStyles)
					{
						throw;
					}
				}
			}
		}

		static ExcelInterface GetExcelInterfaceWithTooManyCellFormatsSetupToSave()
		{
			var excelInterface = new ExcelInterface();

			excelInterface.NewExcelFile(1);
			AssertEquals("Pre-condtion: excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
			excelInterface.Xls.ActiveSheet = 1;

			while (excelInterface.Xls.StyleCount < 4050)
			{
				var format = excelInterface.Xls.GetFormat(excelInterface.Xls.GetCellFormat(1, 1));
				excelInterface.Xls.SetStyle(string.Format("Style {0}", excelInterface.Xls.StyleCount), format);
			}

			AssertEquals("Pre-condition: excelInterface.Xls.StyleCount", 4050, excelInterface.Xls.StyleCount);
			return excelInterface;
		}

		class ExcelInterfaceForTesting : ExcelInterface
		{
			protected override object OpenFile(string fileName)
			{
				LastFileName = fileName;
				return Process.Start("ping", "/h");
			}

			public string LastFileName;
		}

		sealed class TempFileWithDisposedStatus : TempFile
		{
			internal TempFileWithDisposedStatus(string fileName)
				: base(fileName)
			{
			}
		}

		sealed class MockExcelInterfaceWhichThrowArgumentException : ExcelInterface
		{
			protected override Bitmap GetNewBitmap(PixelFormat pixFmt, int width, int height) => throw new ArgumentException();
		}

		sealed class MockExcelInterfaceWhichThrowGenericException : ExcelInterface
		{
			protected override Bitmap GetNewBitmap(PixelFormat pixFmt, int width, int height) => throw new Exception();
		}

		sealed class ExcelInterfaceWithDelayedProcess : ExcelInterfaceForTesting
		{
			public ExcelInterfaceWithDelayedProcess(int secondsDelay)
			{
				this.secondsDelay = secondsDelay;
			}

			readonly int secondsDelay;

			protected override object OpenFile(string fileName)
			{
				LastFileName = fileName;
				return Process.Start("ping", string.Format("-n {0} 127.0.0.1", secondsDelay));
			}
		}

		sealed class ExcelInterfaceWithNullProcess : ExcelInterfaceForTesting
		{
			protected override object OpenFile(string fileName)
			{
				LastFileName = fileName;
				return null;
			}
		}

		sealed class ExcelInterfaceThatThrowsFlexCelPdfException : ExcelInterfaceForTesting
		{
			public ZString ExceptionMessage = "";

			protected override void RunDuringPDFExportForTesting()
			{
				throw new FlexCel.Pdf.FlexCelPdfException(ExceptionMessage);
			}
		}

		sealed class ExcelInterfaceThatThrowFlexCoreFontStyleNotSupportedException : ExcelInterfaceForTesting
		{
			protected override void RunDuringPDFExportForTesting()
			{
				throw new FlexCelCoreException(@"The font ""blah"" is a very bad font.", FlxErr.ErrFontNotSupported);
			}
		}

		sealed class ExcelInterfaceThatThrowFlexCoreFontStyleNotFoundException : ExcelInterfaceForTesting
		{
			protected override void RunDuringPDFExportForTesting()
			{
				throw new FlexCelCoreException(@"The font ""blah"" was not found.", FlxErr.ErrFontNotFound);
			}
		}

		sealed class ExcelInterfaceThatThrowFlexPdfFontStyleNotFoundException : ExcelInterfaceForTesting
		{
			protected override void RunDuringPDFExportForTesting()
			{
				throw new FlexCelPdfException(@"The font ""blah"" was not found.", PdfErr.ErrFontNotFound);
			}
		}

		sealed class ExcelInterfaceThatThrowFlexCoreFontStyleNotSupportedArgumentException : ExcelInterfaceForTesting
		{
			protected override void RunDuringPDFExportForTesting()
			{
				throw new ArgumentException(@"Font 'Arial Unicode MS' does not support style 'Regular'");
			}
		}

		///https://github.com/dotnet/corefx/blob/master/src/System.Security.Cryptography.X509Certificates/tests/TestData.cs
		/// https://github.com/dotnet/corefx/blob/master/src/System.Security.Cryptography.X509Certificates/tests/ChainTests.cs
		///
		static readonly byte[] pfx_BuildChainExtraStoreUntrustedRoot = StringToByteArray(
			"308213790201033082133506092A864886F70D010701A0821326048213223082" +
			"131E3082036706092A864886F70D010701A08203580482035430820350308203" +
			"4C060B2A864886F70D010C0A0102A08202B6308202B2301C060A2A864886F70D" +
			"010C0103300E040811E8B9808BA6E96C020207D004820290D11DA8713602105C" +
			"95792D65BCDFC1B7E3708483BF6CD83008082F89DAE4D003F86081B153BD4D4A" +
			"C122E802752DEA29F07D0B7E8F0FB8A762B4CAA63360F9F72CA5846771980A6F" +
			"AE2643CD412E6E4A101625371BBD48CC6E2D25191D256B531B06DB7CDAC04DF3" +
			"E10C6DC556D5FE907ABF32F2966A561C988A544C19B46DF1BE531906F2CC2263" +
			"A301302A857075C7A9C48A395241925C6A369B60D176419D75E320008D5EFD91" +
			"5257B160F6CD643953E85F19EBE4E4F72B9B787CF93E95F819D1E43EF01CCFA7" +
			"48F0E7260734EA9BC6039BA7557BE6328C0149718A1D9ECF3355082DE697B6CD" +
			"630A9C224D831B7786C7E904F1EF2D9D004E0E825DD74AC4A576CDFCA7CECD14" +
			"D8E2E6CCAA3A302871AE0BA979BB25559215D771FAE647905878E797BBA9FC62" +
			"50F30F518A8008F5A12B35CE526E31032B56EFE5A4121E1E39DC7339A0CE8023" +
			"24CDDB7E9497BA37D8B9F8D826F901C52708935B4CA5B0D4D760A9FB33B0442D" +
			"008444D5AEB16E5C32187C7038F29160DD1A2D4DB1F9E9A6C035CF5BCED45287" +
			"C5DEBAB18743AAF90E77201FEA67485BA3BBCE90CEA4180C447EE588AC19C855" +
			"638B9552D47933D2760351174D9C3493DCCE9708B3EFE4BE398BA64051BF52B7" +
			"C1DCA44D2D0ED5A6CFB116DDA41995FA99373C254F3F3EBF0F0049F1159A8A76" +
			"4CFE9F9CC56C5489DD0F4E924158C9B1B626030CB492489F6AD0A9DCAF3E141D" +
			"B4D4821B2D8A384110B6B0B522F62A9DC0C1315A2A73A7F25F96C530E2F700F9" +
			"86829A839B944AE6758B8DD1A1E9257F91C160878A255E299C18424EB9983EDE" +
			"6DD1C5F4D5453DD5A56AC87DB1EFA0806E3DBFF10A9623FBAA0BAF352F50AB5D" +
			"B16AB1171145860D21E2AB20B45C8865B48390A66057DE3A1ABE45EA65376EF6" +
			"A96FE36285C2328C318182301306092A864886F70D0109153106040401000000" +
			"306B06092B0601040182371101315E1E5C004D006900630072006F0073006F00" +
			"66007400200045006E00680061006E0063006500640020004300720079007000" +
			"74006F0067007200610070006800690063002000500072006F00760069006400" +
			"650072002000760031002E003030820FAF06092A864886F70D010706A0820FA0" +
			"30820F9C02010030820F9506092A864886F70D010701301C060A2A864886F70D" +
			"010C0106300E0408FFCC41FD8C8414F6020207D080820F68092C6010873CF9EC" +
			"54D4676BCFB5FA5F523D03C981CB4A3DC096074E7D04365DDD1E80BF366B8F9E" +
			"C4BC056E8CE0CAB516B9C28D17B55E1EB744C43829D0E06217852FA99CCF5496" +
			"176DEF9A48967C1EEB4A384DB7783E643E35B5B9A50533B76B8D53581F02086B" +
			"782895097860D6CA512514E10D004165C85E561DF5F9AEFD2D89B64F178A7385" +
			"C7FA40ECCA899B4B09AE40EE60DAE65B31FF2D1EE204669EFF309A1C7C8D7B07" +
			"51AE57276D1D0FB3E8344A801AC5226EA4ED97FCD9399A4EB2E778918B81B17F" +
			"E4F65B502595195C79E6B0E37EB8BA36DB12435587E10037D31173285D45304F" +
			"6B0056512B3E147D7B5C397709A64E1D74F505D2BD72ED99055161BC57B6200F" +
			"2F48CF128229EFBEBFC2707678C0A8C51E3C373271CB4FD8EF34A1345696BF39" +
			"50E8CE9831F667D68184F67FE4D30332E24E5C429957694AF23620EA7742F08A" +
			"38C9A517A7491083A367B31C60748D697DFA29635548C605F898B64551A48311" +
			"CB2A05B1ACA8033128D48E4A5AA263D970FE59FBA49017F29049CF80FFDBD192" +
			"95B421FEFF6036B37D2F8DC8A6E36C4F5D707FB05274CC0D8D94AFCC8C6AF546" +
			"A0CF49FBD3A67FB6D20B9FE6FDA6321E8ABF5F7CC794CFCC46005DC57A7BAFA8" +
			"9954E43230402C8100789F11277D9F05C78DF0509ECFBF3A85114FD35F4F17E7" +
			"98D60C0008064E2557BA7BF0B6F8663A6C014E0220693AE29E2AB4BDE5418B61" +
			"0889EC02FF5480BD1B344C87D73E6E4DB98C73F881B22C7D298059FE9D7ADA21" +
			"92BB6C87F8D25F323A70D234E382F6C332FEF31BB11C37E41903B9A59ADEA5E0" +
			"CBAB06DFB835257ABC179A897DEAD9F19B7DF861BE94C655DC73F628E065F921" +
			"E5DE98FFCBDF2A54AC01E677E365DD8B932B5BDA761A0032CE2127AB2A2B9DCB" +
			"63F1EA8A51FC360AB5BC0AD435F21F9B6842980D795A6734FDB27A4FA8209F73" +
			"62DD632FC5FB1F6DE762473D6EA68BFC4BCF983865E66E6D93159EFACC40AB31" +
			"AA178806CF893A76CAAA3279C988824A33AF734FAF8E21020D988640FAB6DB10" +
			"DF21D93D01776EEA5DAECF695E0C690ED27AD386E6F2D9C9482EA38946008CCB" +
			"8F0BD08F9D5058CF8057CA3AD50BB537116A110F3B3ACD9360322DB4D242CC1A" +
			"6E15FA2A95192FC65886BE2672031D04A4FB0B1F43AE8476CF82638B61B416AA" +
			"97925A0110B736B4D83D7977456F35D947B3D6C9571D8E2DA0E9DEE1E665A844" +
			"259C17E01E044FAB898AA170F99157F7B525D524B01BD0710D23A7689A615703" +
			"8A0697BD48FFE0253ABD6F862093574B2FC9BA38E1A6EC60AF187F10D79FF71F" +
			"7C50E87A07CC0A51099899F7336FE742ADEF25E720B8E0F8781EC7957D414CF5" +
			"D44D6998E7E35D2433AFD86442CCA637A1513BE3020B5334614277B3101ED7AD" +
			"22AFE50DE99A2AD0E690596C93B881E2962D7E52EE0A770FAF6917106A8FF029" +
			"8DF38D6DE926C30834C5D96854FFD053BDB020F7827FB81AD04C8BC2C773B2A5" +
			"9FDD6DDF7298A052B3486E03FECA5AA909479DDC7FED972192792888F49C40F3" +
			"910140C5BE264D3D07BEBF3275117AF51A80C9F66C7028A2C3155414CF939997" +
			"268A1F0AA9059CC3AA7C8BBEF880187E3D1BA8978CBB046E43289A020CAE11B2" +
			"5140E2247C15A32CF70C7AA186CBB68B258CF2397D2971F1632F6EBC4846444D" +
			"E445673B942F1F110C7D586B6728ECA5B0A62D77696BF25E21ED9196226E5BDA" +
			"5A80ECCC785BEEDE917EBC6FFDC2F7124FE8F719B0A937E35E9A720BB9ED72D2" +
			"1213E68F058D80E9F8D7162625B35CEC4863BD47BC2D8D80E9B9048811BDD8CB" +
			"B70AB215962CD9C40D56AE50B7003630AE26341C6E243B3D12D5933F73F78F15" +
			"B014C5B1C36B6C9F410A77CA997931C8BD5CCB94C332F6723D53A4CCC630BFC9" +
			"DE96EFA7FDB66FA519F967D6A2DB1B4898BB188DEB98A41FFA7907AE7601DDE2" +
			"30E241779A0FDF551FB84D80AAEE3D979F0510CD026D4AE2ED2EFB7468418CCD" +
			"B3BD2A29CD7C7DC6419B4637412304D5DA2DC178C0B4669CA8330B9713A812E6" +
			"52E812135D807E361167F2A6814CEF2A8A9591EFE2C18216A517473B9C3BF2B7" +
			"51E47844893DA30F7DCD4222D1A55D570C1B6F6A99AD1F9213BA8F84C0B14A6D" +
			"ED6A26EAFF8F89DF733EEB44117DF0FD357186BA4A15BD5C669F60D6D4C34028" +
			"322D4DDF035302131AB6FD08683804CC90C1791182F1AE3281EE69DDBBCC12B8" +
			"1E60942FD082286B16BE27DC11E3BB0F18C281E02F3BA66E48C5FD8E8EA3B731" +
			"BDB12A4A3F2D9E1F833DD204372003532E1BB11298BDF5092F2959FC439E6BD2" +
			"DC6C37E3E775DCBE821B9CBB02E95D84C15E736CEA2FDDAD63F5CD47115B4AD5" +
			"5227C2A02886CD2700540EBFD5BF18DC5F94C5874972FD5424FE62B30500B1A8" +
			"7521EA3798D11970220B2BE7EFC915FCB7A6B8962F09ABA005861E839813EDA3" +
			"E59F70D1F9C277B73928DFFC84A1B7B0F78A8B001164EB0824F2510885CA269F" +
			"DCBB2C3AE91BDE91A8BBC648299A3EB626E6F4236CCE79E14C803498562BAD60" +
			"28F5B619125F80925A2D3B1A56790795D04F417003A8E9E53320B89D3A3109B1" +
			"9BB17B34CC9700DA138FABB5997EC34D0A44A26553153DBCFF8F6A1B5432B150" +
			"58F7AD87C6B37537796C95369DAD53BE5543D86D940892F93983153B4031D4FA" +
			"B25DAB02C1091ACC1DAE2118ABD26D19435CD4F1A02BDE1896236C174743BCA6" +
			"A33FB5429E627EB3FD9F513E81F7BD205B81AAE627C69CF227B043722FA05141" +
			"39347D202C9B7B4E55612FC27164F3B5F287F29C443793E22F6ED6D2F353ED82" +
			"A9F33EDBA8F5F1B2958F1D6A3943A9614E7411FDBCA597965CD08A8042307081" +
			"BAC5A070B467E52D5B91CA58F986C5A33502236B5BAE6DB613B1A408D16B29D3" +
			"560F1E94AD840CFA93E83412937A115ABF68322538DA8082F0192D19EAAA41C9" +
			"299729D487A9404ECDB6396DDA1534841EAE1E7884FA43574E213AE656116D9E" +
			"F7591AA7BDE2B44733DFE27AA59949E5DC0EE00FDF42130A748DDD0FB0053C1A" +
			"55986983C8B9CEAC023CAD7EDFFA1C20D3C437C0EF0FC9868D845484D8BE6538" +
			"EAADA6365D48BA776EE239ED045667B101E3798FE53E1D4B9A2ACBBE6AF1E5C8" +
			"8A3FB03AD616404013E249EC34458F3A7C9363E7772151119FE058BD0939BAB7" +
			"64A2E545B0B2FDAA650B7E849C8DD4033922B2CAE46D0461C04A2C87657CB4C0" +
			"FFBA23DED69D097109EC8BFDC25BB64417FEEB32842DE3EFEF2BF4A47F08B9FC" +
			"D1907BC899CA9DA604F5132FB420C8D142D132E7E7B5A4BD0EF4A56D9E9B0ACD" +
			"88F0E862D3F8F0440954879FFE3AA7AA90573C6BFDC6D6474C606ACA1CD94C1C" +
			"3404349DD83A639B786AFCDEA1779860C05400E0479708F4A9A0DD51429A3F35" +
			"FBD5FB9B68CECC1D585F3E35B7BBFC469F3EAEEB8020A6F0C8E4D1804A3EB32E" +
			"B3909E80B0A41571B23931E164E0E1D0D05379F9FD3BF51AF04D2BE78BDB84BD" +
			"787D419E85626297CB35FCFB6ED64042EAD2EBC17BB65677A1A33A5C48ADD280" +
			"237FB2451D0EFB3A3C32354222C7AB77A3C92F7A45B5FB10092698D88725864A" +
			"3685FBDD0DC741424FCCD8A00B928F3638150892CAAB535CC2813D13026615B9" +
			"9977F7B8240E914ACA0FF2DCB1A9274BA1F55DF0D24CCD2BAB7741C9EA8B1ECD" +
			"E97477C45F88F034FDF73023502944AEE1FF370260C576992826C4B2E5CE9924" +
			"84E3B85170FCCAC3413DC0FF6F093593219E637F699A98BD29E8EE4550C128CA" +
			"182680FDA3B10BC07625734EE8A8274B43B170FC3AEC9AA58CD92709D388E166" +
			"AB4ADFD5A4876DC47C17DE51FDD42A32AF672515B6A81E7ABECFE748912B321A" +
			"FD0CBF4880298DD79403900A4002B5B436230EB6E49192DF49FAE0F6B60EBA75" +
			"A54592587C141AD3B319129006367E9532861C2893E7A2D0D2832DF4377C3184" +
			"5CB02A1D020282C3D2B7F77221F71FEA7FF0A988FEF15C4B2F6637159EEC5752" +
			"D8A7F4AB971117666A977370E754A4EB0DC52D6E8901DC60FCD87B5B6EF9A91A" +
			"F8D9A4E11E2FFDAB55FC11AF6EEB5B36557FC8945A1E291B7FF8931BE4A57B8E" +
			"68F04B9D4A9A02FC61AE913F2E2DDBEE42C065F4D30F568834D5BB15FDAF691F" +
			"197EF6C25AE87D8E968C6D15351093AAC4813A8E7B191F77E6B19146F839A43E" +
			"2F40DE8BE28EB22C0272545BADF3BD396D383B8DA8388147100B347999DDC412" +
			"5AB0AA1159BC6776BD2BF51534C1B40522D41466F414BDE333226973BAD1E6D5" +
			"7639D30AD94BEA1F6A98C047F1CE1294F0067B771778D59E7C722C73C2FF100E" +
			"13603206A694BF0ED07303BE0655DC984CA29893FD0A088B122B67AABDC803E7" +
			"3E5729E868B1CA26F5D05C818D9832C70F5992E7D15E14F9775C6AD24907CF2F" +
			"211CF87167861F94DCF9E3D365CB600B336D93AD44B8B89CA24E59C1F7812C84" +
			"DBE3EE57A536ED0D4BF948F7662E5BCBBB388C72243CFCEB720852D5A4A52F01" +
			"8C2C087E4DB43410FE9ABA3A8EF737B6E8FFDB1AB9832EBF606ED5E4BD62A86B" +
			"BCAE115C67682EDEA93E7845D0D6962C146B411F7784545851D2F327BEC7E434" +
			"4D68F137CDA217A3F0FF3B752A34C3B5339C79CB8E1AC690C038E85D6FC13379" +
			"090198D3555394D7A2159A23BD5EEF06EB0BCC729BB29B5BE911D02DA78FDA56" +
			"F035E508C722139AD6F25A6C84BED0E98893370164B033A2B52BC40D9BF5163A" +
			"F9650AB55EABB23370492A7D3A87E17C11B4D07A7296273F33069C835FD208BA" +
			"8F989A3CF8659054E2CCCFB0C983531DC6590F27C4A1D2C3A780FE945F7E52BB" +
			"9FFD2E324640E3E348541A620CD62605BBDB284AF97C621A00D5D1D2C31D6BD6" +
			"1149137B8A0250BC426417A92445A52574E999FB9102C16671914A1542E92DDE" +
			"541B2A0457112AF936DA84707CADFEA43BFEDAE5F58859908640420948086E57" +
			"FFD1B867C241D40197CB0D4AD58BB69B3724772E0079406A1272858AAA620668" +
			"F696955102639F3E95CFFC637EAF8AB54F0B5B2131AB292438D06E15F3826352" +
			"DEDC653DA5A4AACE2BB97061A498F3B6789A2310471B32F91A6B7A9944DDBB70" +
			"31525B3AE387214DC85A1C7749E9168F41272680D0B3C331D61175F23B623EEC" +
			"40F984C35C831268036680DE0821E5DEE5BB250C6984775D49B7AF94057371DB" +
			"72F81D2B0295FC6A51BCD00A697649D4346FDD59AC0DFAF21BFCC942C23C6134" +
			"FFBA2ABABC141FF700B52C5B26496BF3F42665A5B71BAC7F0C19870BD9873890" +
			"239C578CDDD8E08A1B0A429312FB24F151A11E4D180359A7FA043E8155453F67" +
			"265CB2812B1C98C144E7675CFC86413B40E35445AE7710227D13DC0B5550C870" +
			"10B363C492DA316FB40D3928570BF71BF47638F1401549369B1255DB080E5DFA" +
			"18EA666B9ECBE5C9768C06B3FF125D0E94B98BB24B4FD44E770B78D7B336E021" +
			"4FD72E77C1D0BE9F313EDCD147957E3463C62E753C10BB98584C85871AAEA9D1" +
			"F397FE9F1A639ADE31D40EAB391B03B588B8B031BCAC6C837C61B06E4B745052" +
			"474D33531086519C39EDD6310F3079EB5AC83289A6EDCBA3DC97E36E837134F7" +
			"303B301F300706052B0E03021A0414725663844329F8BF6DECA5873DDD8C96AA" +
			"8CA5D40414DF1D90CD18B3FBC72226B3C66EC2CB1AB351D4D2020207D0"
		);
	}
}
