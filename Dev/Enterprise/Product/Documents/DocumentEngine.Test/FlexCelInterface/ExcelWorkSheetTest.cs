using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.BrandManager;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1140:ColumnNameCaseAnalyzer", Justification = "Column names are from Excel sheets")]
	sealed class ExcelWorkSheetTest : TestCaseWithFactory
	{
		public void TestSetColWidth_TrimReallyLargeWidth()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets.First();
				workSheet.SetColWidth(1, Int32.MaxValue);

				AssertEquals("Width should be trimmed", UInt16.MaxValue, workSheet.GetColWidth(1));
			}
		}

		[ExpectNoExceptions]
		public void TestSetCellToNull()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets.First();
				workSheet[0, 0] = null;
				workSheet[0, 0] = ZString.Empty;
				workSheet[0, 1] = ZGuid.Invalid;
				workSheet[1, 0] = ZDateTime.Invalid;
			}
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "You can not insert a disposed image into a worksheet. [A1]-[My Contents]-[]")]
		public void TestSetDisposedExcelImageOnCell()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var workSheet = excelInterface.WorkSheets.First();

				using (var bitmap = new Bitmap(1, 1))
				{
					var excelImage = new ExcelImage(bitmap, 1, 1, "My Bitmap");
					excelImage.Image.Dispose();

					workSheet[0, 0] = "My Contents";
					workSheet[0, 0] = excelImage;
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPrintTitlesRangeFormula()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_IsSystemDefined = ZBool.True;

			var template = CreateTemplate("RowsToRepeatAtTop.xls", TestFilesSubFolder.DocumentTestFiles);
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var dummy = Factory.New<DummyBODocSupportable>();
			for (var index = 0; index < 100; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = string.Format("Dummy {0}", index);
			}

			Factory.Save();

			var result = DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy, null);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(result.Output);

				AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
				AssertEquals("PrintTitlesRangeFormula", "=Document!$1:$2", DocumentEngineTestHelper.GetPrintTitlesRangeFormula(excelInterface, 0));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetRowHeightToOver8KWillCopyAndInsertExtraRows()
		{
			AssertEquals("Precondition: ExcelWorkSheet.MaxRowHeight - If this value changes, this test will need to be updated.", 8190, ExcelWorkSheet.MaxRowHeight);

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("LastPageFooterWithBorder.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report testReport = new Report(new DocumentPack(), excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					testReport.PrepareForRender();
					testReport.XlInterface.ActiveWorksheet = 0;
					var workSheet = testReport.XlInterface.WorkSheets[0];
					Assert(workSheet.ToString().EndsWith(@"{A}-[#PageFooter]
{C}-[PageFooter ]
{A}-[#LastPageFooter]
{C}-[LastPageFooter]
{A}-[#EndOfReport]"));
					AssertEquals(225, testReport.XlInterface.Xls.GetRowHeight(46));
					AssertEquals(225, testReport.XlInterface.Xls.GetRowHeight(47));
					int addedLines = workSheet.SetRowHeightInsertingExtraRowsIfRequiredToGetTheRightTotalHeightReturningNumberOfAdditionalRows(45, 19000);
					AssertEquals("AddedLines", 2, addedLines);
					Assert(workSheet.ToString().EndsWith(@"{A}-[#PageFooter]
{C}-[PageFooter ]
{A}-[#LastPageFooter]
{A}-[#LastPageFooter]
{A}-[#LastPageFooter]
{C}-[LastPageFooter]
{A}-[#EndOfReport]"));
					AssertEquals(8190, testReport.XlInterface.Xls.GetRowHeight(46));
					AssertEquals(8190, testReport.XlInterface.Xls.GetRowHeight(47));
					AssertEquals(2620, testReport.XlInterface.Xls.GetRowHeight(48));
					AssertEquals(225, testReport.XlInterface.Xls.GetRowHeight(49));
					AssertEquals(19000, testReport.XlInterface.Xls.GetRowHeight(46) + testReport.XlInterface.Xls.GetRowHeight(47) + testReport.XlInterface.Xls.GetRowHeight(48));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyAndInsertRows()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\CustomizableDocumentTemplates\Basic.xls"));
				excelInterface.ActiveWorksheet = 0;
				var workSheet = excelInterface.WorkSheets[0];

				using (var sourceExcelInterface = new ExcelInterface())
				{
					sourceExcelInterface.LoadExcelFile(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\CustomizableDocumentTemplates\Red Section Body 1.xls"));
					sourceExcelInterface.ActiveWorksheet = 0;
					var sourceWorkSheet = sourceExcelInterface.WorkSheets[0];

					workSheet.CopyAndInsertRows(sourceWorkSheet, 5, 3, 11);
				}

				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[#Config]
{A}-[Name=Basic Customizable Document Template]
{A}-[HideColumnIf]   {AY}-[1==1]   {AZ}-[1==1]   {BA}-[1==1]   {BB}-[1==1]   {BC}-[1==1]   {BD}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{C}-[This is Section Body 1.]
{A}-[#ConfigurableSection:BOD, Section Body 2]
{A}-[#SectionBody]
{C}-[This is Section Body 2.]
{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{C}-[This is Section Body 1. [RED]]
{A}-[#ConfigurableSection:GEN, Test Page Info]
{C}-[Page <CurrentPage> of <TotalPages>]
{A}-[#ConfigurableSection:BEX, Test Notes]
{A}-[#SectionBody:DATA=Notes]
{C}-[<Notes.Description>]
{A}-[#EndOfReport]", workSheet.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyAndInsertRowsContainsOLEObject()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\CustomizableDocumentTemplates\Basic.xls"));
				excelInterface.ActiveWorksheet = 0;
				var workSheet = excelInterface.WorkSheets[0];

				using (var sourceExcelInterface = new ExcelInterface())
				{
					sourceExcelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + @"SimpleTestWithOLEObject2.xls");
					sourceExcelInterface.ActiveWorksheet = 0;
					var sourceWorkSheet = sourceExcelInterface.WorkSheets[0];

					AssertNoExceptionThrown(() => workSheet.CopyAndInsertRows(sourceWorkSheet, 4, 10, 11));
				}
			}
		}

		public void TestRemoveRow()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, Name,
@"{A}-[Row 0]
{A}-[Row 1]
{A}-[Row 2]
{A}-[Row 3]
{A}-[Row 4]
{A}-[Row 5]");

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
				{
					excelInterface.LoadExcelFile(stream);
				}

				AssertEquals("Pre-condition: excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

				var workSheet = excelInterface.WorkSheets[0];
				workSheet.RemoveRow(3);
				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[Row 0]
{A}-[Row 1]
{A}-[Row 2]
{A}-[Row 4]
{A}-[Row 5]", workSheet.ToString());

				workSheet.RemoveRow(0);
				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[Row 1]
{A}-[Row 2]
{A}-[Row 4]
{A}-[Row 5]", workSheet.ToString());

				workSheet.RemoveRow(3);
				AssertMultilineASCIIEquals("workSheet.ToString()",
@"{A}-[Row 1]
{A}-[Row 2]
{A}-[Row 4]", workSheet.ToString());
			}
		}

		public void TestExceptionGetsWrappedWhenAddingImageFails()
		{
			try
			{
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					ExcelWorkSheet.ExceptionToThrowWhenInsertingPictureForTesting = new ExternalException("A generic error occurred in GDI+.");

					try
					{
						workSheet[0, 0] = new ExcelImage(new Bitmap(12, 14), 1, 540, "Hello Kitty");
						Fail("Expected an ExcelInterfaceException to be thrown.");
					}
					catch (Exception exception)
					{
						AssertEquals("Exception Thrown", typeof(ExcelInterfaceException), exception.GetType());
						AssertEquals("Exception Message", ExcelInterfaceExceptionBase.ErrorInsertingImageMessage, exception.Message);
					}
				}
			}
			finally
			{
				ExcelWorkSheet.ExceptionToThrowWhenInsertingPictureForTesting = null;
			}
		}

		public void TestClear()
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				workSheet[0, 0] = "Row 1";
				workSheet[1, 0] = "Row 2";
				AssertMultilineASCIIEquals("Precondition: Worksheet Content", "{A}-[Row 1]\r\n{A}-[Row 2]", workSheet.ToString());
				AssertEquals("Precondition: workSheet.IsHidden", false, workSheet.IsHidden);

				workSheet.Clear();
				AssertMultilineASCIIEquals("Worksheet Content", "", workSheet.ToString());
				AssertEquals("workSheet.IsHidden", false, workSheet.IsHidden);
			}
		}

		public void TestHide()
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				AssertEquals("workSheet.IsHidden", false, workSheet.IsHidden);
				workSheet.Hide();
				AssertEquals("workSheet.IsHidden", true, workSheet.IsHidden);
			}
		}

		public void TestGetAndSetCellFormatTakeNoteOfWrapText()
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				workSheet[0, 0] = "Hello World!";
				CellFormat format = workSheet.GetCellFormat(0, 0);
				AssertEquals("Pre-condition: format.WrapText", false, format.WrapText);
				format.WrapText = true;
				workSheet.SetCellFormat(0, 0, format);
				AssertEquals(true, workSheet.GetCellFormat(0, 0).WrapText);
			}
		}

		#region public void TestToStringWithICellFormatter

		public void TestToStringWithICellFormatter()
		{
			using (var xlInterface = new ExcelInterface())
			{
				var fileName = ReportTestXlsFilePath;
				xlInterface.LoadExcelFile(fileName);
				var workSheet = xlInterface.WorkSheets[0];
				AssertMultilineASCIIEquals("ToString(ICellFormatter) on " + fileName, ExpectedToStringWithICellFormatter.Trim(), workSheet.ToString(new ToUpperCellFormatter()));
			}
		}

		class ToUpperCellFormatter : CellFormatter
		{
			public override string Format(ExcelWorkSheet cellSource, int row, int column)
			{
				return base.Format(cellSource, row, column).ToUpperInvariant();
			}
		}
		const string ExpectedToStringWithICellFormatter = @"
{A}-[SECTION:DATASOURCE:SHIPMENT]
{A}-[#ROW]   {D}-[WEQ]
{A}-[#IMAGE:IMAGEGROUPLOGO]   {D}-[WQEWQE]   {H}-[PAGE: <PAGENUMBER>]
{H}-[REPORT RUN BY: <LOGINCODE>]

{A}-[{SHIPMENT.JS_ISCOLOAD}]   {F}-[{CONSIGNEE.OH_FULLNAME}]
{A}-[{OH_ADDRESS1}]   {F}-[    {OH_ADDRESS1}]
{A}-[{NONEXISTINGTABLE.OH_ADDRESS2}]   {F}-[    {OH_ADDRESS2}]
{A}-[{SHIPMENT.NONEXISTINGFIELD}]   {F}-[    {OH_CITY}, {OH_STATE}, {OH_POSTCODE}]
{A}-[OH_PHONE}]   {F}-[    {OH_PHONE}]
{A}-[{TESTFIELD]

{I}-[{}]
{H}-[}}]
{H}-[{{}}]
{A}-[VESSEL / VOYAGE]
{A}-[{JK_VESSEL}, V{JK_VOYAGE]   {F}-[38456.6634553241]

{B}-[LAST ROW OF MAIN SECTION]
{A}-[#END]

{A}-[SECTION:DATASOURCE:TABLE01_3]   {I}-[1]
{A}-[#HEADER]   {I}-[2]
{A}-[CONTAINER]   {B}-[SEAL]   {C}-[TAREWEIGHT]   {D}-[WEIGHT]   {E}-[VOLUME]   {I}-[3]
{A}-[#ROW]   {I}-[4]
{A}-[{JC_CONTAINERNUM}]   {B}-[{JC_SEALNUM}]   {C}-[{JC_TAREWEIGHT}]   {D}-[{JC_CARGOWEIGHT}]   {E}-[{JC_CUBIC}]   {I}-[5]
{I}-[15]
{A}-[#TOTALHEADER]
{B}-[TOTALS]   {C}-[PACKAGES]   {D}-[WEIGHT]   {E}-[VOLUME]
{A}-[#TOTALROW]
{C}-[SUM({JC_TAREWEIGHT})]   {D}-[MAX({JC_CARGOWEIGHT})]   {E}-[SUM({JC_CUBIC})]

{A}-[#END]
{A}-[SECTION:END]

{F}-[TEST]";
		#endregion

		public void TestSheetNameTranslated()
		{
			using (var resourceStrings = Res.UseMockData())
			using (var spreadSheet = new ExcelInterface())
			{
				resourceStrings.Put("ReportName|The Title", new ResourceStringData("", "Der Titel"));

				spreadSheet.NewExcelFile(1);
				var workSheet = spreadSheet.WorkSheets[0];
				workSheet.SheetNameOverride = "The Title";
				workSheet.UpdateSheetName();
				AssertEquals("Der Titel", spreadSheet.Xls.ActiveSheetByName);

				resourceStrings.Put("ReportName|The Title", new ResourceStringData("", "Der TiTel"));
				workSheet.UpdateSheetName();
				AssertEquals("Der TiTel", spreadSheet.Xls.ActiveSheetByName);
			}
		}

		public void TestSheetNameOverride()
		{
			using (var spreadSheet = new ExcelInterface())
			{
				spreadSheet.NewExcelFile(1);
				var workSheet = spreadSheet.WorkSheets[0];
				workSheet.SheetNameOverride = "";
				AssertNotEquals("Hello World", spreadSheet.Xls.ActiveSheetByName);
				AssertNotEquals("", spreadSheet.Xls.ActiveSheetByName);

				workSheet.SheetNameOverride = "Hello World";
				AssertNotEquals("Hello World", spreadSheet.Xls.ActiveSheetByName);
				AssertNotEquals("", spreadSheet.Xls.ActiveSheetByName);

				workSheet.UpdateSheetName();
				AssertEquals("Hello World", spreadSheet.Xls.ActiveSheetByName);

				workSheet.SheetNameOverride = "Hello WoRld";
				workSheet.UpdateSheetName();
				AssertEquals("Hello WoRld", spreadSheet.Xls.ActiveSheetByName);
			}
		}

		public void TestGetCellName()
		{
			AssertEquals("A1", ExcelWorkSheet.GetCellName(0, 0));
			AssertEquals("C2", ExcelWorkSheet.GetCellName(1, 2));
			AssertEquals("DT123", ExcelWorkSheet.GetCellName(122, 123));
		}

		#region TestInsertRows

		public void TestInsertRows()
		{
			using (ExcelInterface spreadSheet = new ExcelInterface())
			{
				spreadSheet.NewExcelFile(1);
				ExcelWorkSheet workSheet = spreadSheet.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=SourceWorkSheet";
				workSheet[2, 0] = "DataContext=.DummyBusinessObject";
				workSheet[3, 0] = "#EndOfReport";
				workSheet.InsertRows(3, 4);
				workSheet[3, 0] = "#ConfigurableSection:BOD, JohnBoy";
				workSheet[4, 0] = "#SectionBody:Data=JohnBoyCollection";
				workSheet[5, 1] = "JohnBoy";
				AssertMultilineASCIIEquals("InsertRows() result after populating data", ExpectedInsertRows.Trim(), workSheet.ToString());
			}
		}
		const string ExpectedInsertRows = @"
{A}-[#config]
{A}-[Name=SourceWorkSheet]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#ConfigurableSection:BOD, JohnBoy]
{A}-[#SectionBody:Data=JohnBoyCollection]
{B}-[JohnBoy]

{A}-[#EndOfReport]
";
		#endregion

		#region TestMoveRows

		void AssertMoveRowsTooManyRows(string fileName, string type)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(fileName);
				var workSheet = excelInterface.WorkSheets[0];
				int destinationRow = FlxConsts.Max_Rows2007;
				if (!string.Equals(type, "xlsx", StringComparison.OrdinalIgnoreCase))
				{
					destinationRow = FlxConsts.Max_Rows97_2003;
				}

				AssertNoExceptionThrown(() => workSheet.MoveRows(17, 18, destinationRow - 2));
				using (var stream = new MemoryStream())
				{
					AssertNoExceptionThrown(() => excelInterface.SaveToStream(stream, type));
				}

				if (string.Equals(type, "xlsx", StringComparison.OrdinalIgnoreCase))
				{
					AssertExceptionThrown(typeof(DocumentEngineTooManyRowsException), () => workSheet.MoveRows(17, 18, destinationRow));
					AssertExceptionThrown(typeof(DocumentEngineTooManyRowsException), () => workSheet.MoveRows(18, 18, destinationRow));
				}
				else
				{
					// the internal calculation is done assuming XLSX so it will always move the rows for XLS, it will fail when writing
					AssertNoExceptionThrown(() => workSheet.MoveRows(17, 18, destinationRow + 2));
					using (var stream = new MemoryStream())
					{
						AssertExceptionThrown(typeof(DocumentEngineTooManyRowsForThisFileFormatException), () => excelInterface.SaveToStream(stream, type));
					}
				}
			}
		}

		public void TestMoveRowsTooManyRows_XLS()
		{
			AssertMoveRowsTooManyRows(ReportTestXlsFilePath, "xls");
		}

		public void TestMoveRowsTooManyRows_XLSX()
		{
			AssertMoveRowsTooManyRows(ReportTestXlsxFilePath, "xlsx");
		}

		public void TestMoveRows()
		{
			using (var excelInterface = new ExcelInterface())
			{
				var fileName = ReportTestXlsFilePath;
				excelInterface.LoadExcelFile(fileName);
				var workSheet = excelInterface.WorkSheets[0];
				workSheet.MoveRows(17, 18, 16);
				AssertMultilineASCIIEquals("GetVisibleContentsAsString() on " + fileName, expectedMoveRows.Trim(), workSheet.ToString());
			}
		}
		const string expectedMoveRows = @"
{A}-[section:DataSource:Shipment]
{A}-[#row]   {D}-[weq]
{A}-[#image:ImageGroupLogo]   {D}-[wqewqe]   {H}-[Page: <PageNumber>]
{H}-[Report Run by: <LoginCode>]

{A}-[{Shipment.JS_IsCoload}]   {F}-[{Consignee.oh_FullName}]
{A}-[{oh_Address1}]   {F}-[    {oh_Address1}]
{A}-[{NonExistingTable.oh_Address2}]   {F}-[    {oh_Address2}]
{A}-[{Shipment.NonExistingField}]   {F}-[    {oh_city}, {oh_state}, {oh_postcode}]
{A}-[oh_phone}]   {F}-[    {oh_phone}]
{A}-[{TestField]

{I}-[{}]
{H}-[}}]
{A}-[Vessel / Voyage]
{A}-[{jk_vessel}, V{jk_voyage]   {F}-[38456.6634553241]
{H}-[{{}}]

{B}-[last row of main section]
{A}-[#end]

{A}-[section:DataSource:Table01_3]   {I}-[1]
{A}-[#header]   {I}-[2]
{A}-[Container]   {B}-[Seal]   {C}-[TareWeight]   {D}-[Weight]   {E}-[Volume]   {I}-[3]
{A}-[#row]   {I}-[4]
{A}-[{JC_ContainerNum}]   {B}-[{JC_SealNum}]   {C}-[{JC_TareWeight}]   {D}-[{JC_CargoWeight}]   {E}-[{JC_Cubic}]   {I}-[5]
{I}-[15]
{A}-[#totalheader]
{B}-[Totals]   {C}-[Packages]   {D}-[Weight]   {E}-[Volume]
{A}-[#totalrow]
{C}-[SUM({JC_TareWeight})]   {D}-[MAX({JC_CargoWeight})]   {E}-[SUM({JC_Cubic})]

{A}-[#end]
{A}-[section:end]

{F}-[test]";

		public void TestMoveRows_XLSX()
		{
			using (var excelInterface = new ExcelInterface())
			{
				var fileName = ReportTestXlsxFilePath;
				excelInterface.LoadExcelFile(fileName);
				var workSheet = excelInterface.WorkSheets[0];
				workSheet.MoveRows(32, 34, 16);
				AssertMultilineASCIIEquals("GetVisibleContentsAsString() on " + fileName, expectedMoveRowsXLSX.Trim(), workSheet.ToString());
			}
		}

		const string expectedMoveRowsXLSX = @"
{A}-[section:DataSource:Shipment]
{A}-[#row]   {D}-[weq]
{A}-[#image:ImageGroupLogo]   {D}-[wqewqe]   {H}-[Page: <PageNumber>]
{H}-[Report Run by: <LoginCode>]

{A}-[{Shipment.JS_IsCoload}]   {F}-[{Consignee.oh_FullName}]
{A}-[{oh_Address1}]   {F}-[    {oh_Address1}]
{A}-[{NonExistingTable.oh_Address2}]   {F}-[    {oh_Address2}]
{A}-[{Shipment.NonExistingField}]   {F}-[    {oh_city}, {oh_state}, {oh_postcode}]
{A}-[oh_phone}]   {F}-[    {oh_phone}]
{A}-[{TestField]

{I}-[{}]
{H}-[}}]
{B}-[Totals]   {C}-[Packages]   {D}-[Weight]   {E}-[Volume]   {JA}-[I'm so far away!]
{A}-[#totalrow]
{C}-[SUM({JC_TareWeight})]   {D}-[MAX({JC_CargoWeight})]   {E}-[SUM({JC_Cubic})]
{H}-[{{}}]
{A}-[Vessel / Voyage]
{A}-[{jk_vessel}, V{jk_voyage]   {F}-[41890.5306552083]

{B}-[last row of main section]
{A}-[#end]

{A}-[section:DataSource:Table01_3]   {I}-[1]
{A}-[#header]   {I}-[2]
{A}-[Container]   {B}-[Seal]   {C}-[TareWeight]   {D}-[Weight]   {E}-[Volume]   {I}-[3]
{A}-[#row]   {I}-[4]
{A}-[{JC_ContainerNum}]   {B}-[{JC_SealNum}]   {C}-[{JC_TareWeight}]   {D}-[{JC_CargoWeight}]   {E}-[{JC_Cubic}]   {I}-[5]
{I}-[15]
{A}-[#totalheader]

{A}-[#end]
{A}-[section:end]

{F}-[test]";

		#endregion

		#region TestDeleteColumn

		public void TestDeleteColumn()
		{
			using (ExcelInterface spreadSheet = new ExcelInterface())
			{
				spreadSheet.NewExcelFile(1);
				ExcelWorkSheet workSheet = spreadSheet.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=SourceWorkSheet";
				workSheet[2, 0] = "DataContext=.DummyBusinessObject";
				workSheet[3, 0] = "#ConfigurableSection:BOD, JohnBoy";
				workSheet[4, 0] = "#SectionBody:Data=JohnBoyCollection";
				workSheet[5, 1] = "JohnBoy";
				workSheet[7, 0] = "#EndOfReport";
				workSheet.DeleteColumn(2);
				AssertMultilineASCIIEquals("InsertRows() result after deleting column B", ExpectedInsertRows.Trim(), workSheet.ToString());
			}
		}

		#endregion

		#region TestToString

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestToString()
		{
			using (var xlInterface = new ExcelInterface())
			{
				var fileName = ReportTestXlsFilePath;
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "test.xls");
				var workSheet = xlInterface.WorkSheets[0];
				AssertMultilineASCIIEquals("ToString() on " + fileName, ExpectedToString.Trim(), workSheet.ToString());
			}
		}
		const string ExpectedToString = @"
{A}-[section:DataSource:Shipment]
{A}-[#row]   {D}-[weq]
{A}-[#image:ImageGroupLogo]   {D}-[wqewqe]   {H}-[Page: <PageNumber>]
{H}-[Report Run by: <LoginCode>]

{A}-[{Shipment.JS_IsCoload}]   {F}-[{Consignee.oh_FullName}]
{A}-[{oh_Address1}]   {F}-[    {oh_Address1}]
{A}-[{NonExistingTable.oh_Address2}]   {F}-[    {oh_Address2}]
{A}-[{Shipment.NonExistingField}]   {F}-[    {oh_city}, {oh_state}, {oh_postcode}]
{A}-[oh_phone}]   {F}-[    {oh_phone}]
{A}-[{TestField]

{I}-[{}]
{H}-[}}]
{H}-[{{}}]
{A}-[Vessel / Voyage]
{A}-[{jk_vessel}, V{jk_voyage]   {F}-[38456.6634553241]

{B}-[last row of main section]
{A}-[#end]

{A}-[section:DataSource:Table01_3]   {I}-[1]
{A}-[#header]   {I}-[2]
{A}-[Container]   {B}-[Seal]   {C}-[TareWeight]   {D}-[Weight]   {E}-[Volume]   {I}-[3]
{A}-[#row]   {I}-[4]
{A}-[{JC_ContainerNum}]   {B}-[{JC_SealNum}]   {C}-[{JC_TareWeight}]   {D}-[{JC_CargoWeight}]   {E}-[{JC_Cubic}]   {I}-[5]
{I}-[15]
{A}-[#totalheader]
{B}-[Totals]   {C}-[Packages]   {D}-[Weight]   {E}-[Volume]
{A}-[#totalrow]
{C}-[SUM({JC_TareWeight})]   {D}-[MAX({JC_CargoWeight})]   {E}-[SUM({JC_Cubic})]

{A}-[#end]
{A}-[section:end]

{F}-[test]";
		#endregion

		public void TestIsCellMerged()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(false, xlInterface.WorkSheets[0].DoesRowContainAMergedCellToNextRow(0));
				AssertEquals(true, xlInterface.WorkSheets[0].DoesRowContainAMergedCellToNextRow(41));
				AssertEquals(false, xlInterface.WorkSheets[0].DoesRowContainAMergedCellToNextRow(39));
				AssertEquals(false, xlInterface.WorkSheets[0].DoesRowContainAMergedCellToNextRow(46));
			}
		}

		public void TestGetCellFormatBordersInIndexedColors()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.ActiveWorksheet = 0;

				var xls = excelInterface.Xls;
				var format = xls.GetDefaultFormat;
				format.Borders.Top.Style = TFlxBorderStyle.Thin;
				format.Borders.Top.Color = TExcelColor.FromIndex(48);
				int formatNumber = xls.AddFormat(format);
				xls.SetCellFormat(1, 1, formatNumber);

				var excelWorkSheet = excelInterface.WorkSheets[0];
				var cellFormat = excelWorkSheet.GetCellFormat(0, 0);
				AssertEquals(Color.FromArgb(150, 150, 150).ToArgb(), cellFormat.Borders.Top.BorderColor.ToArgb());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCellFormatBorders()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "testCellBorder.xls");
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Top.BorderColor.A);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Top.BorderColor.R);
				AssertEquals((byte)0, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Top.BorderColor.G);
				AssertEquals((byte)0, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Top.BorderColor.B);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Bottom.BorderColor.A);
				AssertEquals((byte)0, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Bottom.BorderColor.R);
				AssertEquals((byte)32, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Bottom.BorderColor.G);
				AssertEquals((byte)96, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Bottom.BorderColor.B);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Left.BorderColor.A);
				AssertEquals((byte)0, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Left.BorderColor.R);
				AssertEquals((byte)176, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Left.BorderColor.G);
				AssertEquals((byte)80, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Left.BorderColor.B);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Right.BorderColor.A);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Right.BorderColor.R);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Right.BorderColor.G);
				AssertEquals((byte)0, xlInterface.WorkSheets[0].GetCellFormat(39, 7).Borders.Right.BorderColor.B);
			}
		}

		public void TestGetCellFormat()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);

				AssertEquals(true, xlInterface.WorkSheets[0].GetCellFormat(57, 5).FontBold);
				AssertEquals(true, xlInterface.WorkSheets[0].GetCellFormat(57, 5).FontItalic);
				AssertEquals(3437, xlInterface.WorkSheets[0].GetCellWidth(57, 5));
				AssertEquals(405, xlInterface.WorkSheets[0].GetCellHeight(57, 5));
				AssertEquals("Times New Roman", xlInterface.WorkSheets[0].GetCellFormat(57, 5).FontName);
				AssertEquals((byte)0, xlInterface.WorkSheets[0].GetCellFormat(57, 5).BackgroundColor.B);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(57, 5).BackgroundColor.G);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(57, 5).BackgroundColor.A);
				AssertEquals((byte)255, xlInterface.WorkSheets[0].GetCellFormat(57, 5).BackgroundColor.R);
				AssertEquals(false, xlInterface.WorkSheets[0].GetCellFormat(57, 5).IsBackgroundColorAutomatic);
				AssertEquals(CellBorderStyle.Thin, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Thin, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Thick, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Thick, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Right.BorderStyle);
				AssertEquals(Color.Black.ToArgb(), xlInterface.WorkSheets[0].GetCellFormat(57, 5).TextColor.ToArgb());
				AssertEquals("", xlInterface.WorkSheets[0].GetCellFormat(57, 5).FormatPattern);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetCellFormat_Alignment()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "AlignmentTest.xls");
				AssertEquals("Reading row with HAlignment not set, should default the alignment to General", HorizontalTextAlignment.General, xlInterface.WorkSheets[0].GetCellFormat(0, 4).HTextAlign);
				AssertEquals("Reading row with HAlignment not set, should default the alignment to Right, with 'Right' default alignment passed to function", HorizontalTextAlignment.Right, xlInterface.WorkSheets[0].GetCellFormat(0, 4, HorizontalTextAlignment.Right).HTextAlign);
				AssertEquals(HorizontalTextAlignment.Left, xlInterface.WorkSheets[0].GetCellFormat(0, 0).HTextAlign);
				AssertEquals(HorizontalTextAlignment.Centre, xlInterface.WorkSheets[0].GetCellFormat(0, 1).HTextAlign);
				AssertEquals(HorizontalTextAlignment.Right, xlInterface.WorkSheets[0].GetCellFormat(0, 2).HTextAlign);
				AssertEquals(HorizontalTextAlignment.Justify, xlInterface.WorkSheets[0].GetCellFormat(0, 3).HTextAlign);

				AssertEquals(VerticalTextAlignment.Top, xlInterface.WorkSheets[0].GetCellFormat(0, 0).VTextAlign);
				AssertEquals(VerticalTextAlignment.Middle, xlInterface.WorkSheets[0].GetCellFormat(0, 1).VTextAlign);
				AssertEquals(VerticalTextAlignment.Bottom, xlInterface.WorkSheets[0].GetCellFormat(0, 2).VTextAlign);
			}
		}

		public void TestIndexerSetForFormulas()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				xlInterface.WorkSheets[0][57, 5] = new TFormula("", null);
				AssertEquals("=0", ((TFormula)xlInterface.WorkSheets[0][57, 5]).Text);
				xlInterface.WorkSheets[0][57, 5] = new TFormula("=", null);
				AssertEquals("", xlInterface.WorkSheets[0][57, 5]);
			}
		}

		public void TestIndexerSetForFormattedCell()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				FormattedCellValue value = new FormattedCellValue(5.000, "0.000");
				xlInterface.WorkSheets[0][57, 5] = value;
				AssertEquals(5.000, xlInterface.WorkSheets[0][57, 5]);
			}
		}

		public void TestIndexerSetForHyperlink()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				AssertEquals("Pre-condition", 0, xlInterface.Xls.HyperLinkCount);

				ExcelWorkSheet worksheet = xlInterface.WorkSheets[0];
				ExcelHyperlink hyperlink = new ExcelHyperlink(THyperLinkType.URL, "ediSite", "http://www.edi.com.au", "", "", "Click here for edi site");
				worksheet[10, 10] = hyperlink;
				AssertEquals(1, xlInterface.Xls.HyperLinkCount);
				THyperLink generatedLink = xlInterface.Xls.GetHyperLink(1);
				AssertEquals("http://www.edi.com.au", generatedLink.Text);
				AssertEquals("Click here for edi site", generatedLink.Hint);
				AssertEquals("ediSite", generatedLink.Description);
				AssertEquals(THyperLinkType.URL, generatedLink.LinkType);
				AssertEquals("ediSite", worksheet[10, 10]);

				TFlxFormat linkCellFormat = xlInterface.Xls.GetCellVisibleFormatDef(11, 11);
				AssertEquals(TFlxUnderline.Single, linkCellFormat.Font.Underline);
				AssertEquals(TUIColor.FromArgb(255, 0, 0, 255), linkCellFormat.Font.Color.ToColor(xlInterface.Xls));
			}
		}

		public void TestIndexerSetStringWithEDIProtocol()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				AssertEquals("Pre-condition", 0, xlInterface.Xls.HyperLinkCount);

				ExcelWorkSheet worksheet = xlInterface.WorkSheets[0];
				ZString protocolStr = "edient:Command=ShowEditForm&TextToShow=Bob";
				worksheet[2, 2] = protocolStr;
				AssertEquals(1, xlInterface.Xls.HyperLinkCount);
				THyperLink generatedLink = xlInterface.Xls.GetHyperLink(1);
				AssertEquals("edient:Command=ShowEditForm&TextToShow=Bob", generatedLink.Text);
				AssertEquals($"Click here to view in {BrandingFactory.Instance.ProductName}", generatedLink.Hint);
				AssertEquals("Bob", generatedLink.Description);
				AssertEquals(THyperLinkType.URL, generatedLink.LinkType);
				AssertEquals("Bob", worksheet[2, 2]);

				TFlxFormat linkCellFormat = xlInterface.Xls.GetCellVisibleFormatDef(3, 3);
				AssertEquals(TFlxUnderline.Single, linkCellFormat.Font.Underline);
				AssertEquals(TUIColor.FromArgb(255, 0, 0, 255), linkCellFormat.Font.Color.ToColor(xlInterface.Xls));
			}
		}

		public void TestIndexerSetForHyperlink_EmptyLinkLocation()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				AssertEquals("Pre-condition", 0, xlInterface.Xls.HyperLinkCount);

				ExcelWorkSheet worksheet = xlInterface.WorkSheets[0];
				ExcelHyperlink hyperlink = new ExcelHyperlink(THyperLinkType.URL, "no link", "", "", "", "Click here for edi site");
				worksheet[20, 10] = hyperlink;
				AssertEquals(0, xlInterface.Xls.HyperLinkCount);
				AssertEquals("no link", worksheet[20, 10]);
			}
		}

		public void TestIndexerConvertsZboolsAndBoolsToTrueFalse()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				var zboolTrue = new ZBool(true);
				var zboolFalse = new ZBool(false);
				var boolTrue = true;
				var boolFalse = false;
				excelInterface.WorkSheets[0][57, 5] = zboolTrue;
				excelInterface.WorkSheets[0][58, 5] = zboolFalse;
				excelInterface.WorkSheets[0][59, 5] = boolTrue;
				excelInterface.WorkSheets[0][60, 5] = boolFalse;
				AssertEquals("Y", excelInterface.WorkSheets[0][57, 5]);
				AssertEquals("N", excelInterface.WorkSheets[0][58, 5]);
				AssertEquals("Y", excelInterface.WorkSheets[0][59, 5]);
				AssertEquals("N", excelInterface.WorkSheets[0][60, 5]);
			}
		}

		public void TestSetCellFormat()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);

				CellFormat fmt = new CellFormat();
				fmt.BackgroundColor = Color.Yellow;
				fmt.TextColor = Color.White;
				fmt.FillPattern = FillPatternStyle.Solid;
				fmt.FontName = "Arial";
				fmt.FontSize = 24;
				fmt.FontStyle = FontStyle.Bold;
				fmt.Borders.Top.BorderStyle = CellBorderStyle.Double;
				fmt.Borders.Top.BorderColor = Color.Red;
				fmt.Borders.Bottom.BorderColor = Color.Green;
				fmt.Borders.Left.BorderColor = Color.Blue;
				fmt.Borders.Right.BorderColor = Color.Yellow;
				fmt.FormatPattern = "#,##0.000";
				fmt.HTextAlign = HorizontalTextAlignment.Right;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				xlInterface.WorkSheets[0][57, 5] = "this is a test";

				AssertEquals(true, xlInterface.WorkSheets[0].GetCellFormat(57, 5).FontBold);
				AssertEquals(false, xlInterface.WorkSheets[0].GetCellFormat(57, 5).FontItalic);
				AssertEquals("Arial", xlInterface.WorkSheets[0].GetCellFormat(57, 5).FontName);
				AssertEquals(CellBorderStyle.Double, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.None, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.None, xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Right.BorderStyle);
				AssertEquals(Color.Red.ToArgb(), xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Top.BorderColor.ToArgb());
				AssertEquals(Color.Green.ToArgb(), xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Bottom.BorderColor.ToArgb());
				AssertEquals(Color.Blue.ToArgb(), xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Left.BorderColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), xlInterface.WorkSheets[0].GetCellFormat(57, 5).Borders.Right.BorderColor.ToArgb());
				AssertEquals(Color.White.ToArgb(), xlInterface.WorkSheets[0].GetCellFormat(57, 5).TextColor.ToArgb());
				AssertEquals("#,##0.000", xlInterface.WorkSheets[0].GetCellFormat(57, 5).FormatPattern);
				AssertEquals(HorizontalTextAlignment.Right, xlInterface.WorkSheets[0].GetCellFormat(57, 5).HTextAlign);
			}
		}

		public void TestSetCellFormat_HAlignment()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				CellFormat fmt = new CellFormat();
				fmt.HTextAlign = HorizontalTextAlignment.Left;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(HorizontalTextAlignment.Left, xlInterface.WorkSheets[0].GetCellFormat(57, 5).HTextAlign);

				fmt.HTextAlign = HorizontalTextAlignment.Right;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(HorizontalTextAlignment.Right, xlInterface.WorkSheets[0].GetCellFormat(57, 5).HTextAlign);

				fmt.HTextAlign = HorizontalTextAlignment.Centre;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(HorizontalTextAlignment.Centre, xlInterface.WorkSheets[0].GetCellFormat(57, 5).HTextAlign);

				fmt.HTextAlign = HorizontalTextAlignment.Justify;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(HorizontalTextAlignment.Justify, xlInterface.WorkSheets[0].GetCellFormat(57, 5).HTextAlign);
			}
		}

		public void TestSetCellFormat_VAlignment()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				CellFormat fmt = new CellFormat();
				fmt.VTextAlign = VerticalTextAlignment.Bottom;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(VerticalTextAlignment.Bottom, xlInterface.WorkSheets[0].GetCellFormat(57, 5).VTextAlign);

				fmt.VTextAlign = VerticalTextAlignment.Top;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(VerticalTextAlignment.Top, xlInterface.WorkSheets[0].GetCellFormat(57, 5).VTextAlign);

				fmt.VTextAlign = VerticalTextAlignment.Middle;
				xlInterface.WorkSheets[0].SetCellFormat(57, 5, fmt);
				AssertEquals(VerticalTextAlignment.Middle, xlInterface.WorkSheets[0].GetCellFormat(57, 5).VTextAlign);
			}
		}

		public void TestCopyBorders()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var format = new CellFormat();
				format.BackgroundColor = Color.Yellow;
				format.FillPattern = FillPatternStyle.Solid;
				format.FontName = "Arial";
				format.FontSize = 24;
				format.FontStyle = FontStyle.Bold;
				format.Borders.Top.BorderStyle = CellBorderStyle.Double;
				format.Borders.Left.BorderStyle = CellBorderStyle.Dash_dot;
				format.Borders.Right.BorderStyle = CellBorderStyle.Hair;
				format.Borders.Bottom.BorderStyle = CellBorderStyle.Slanted_dash_dot;
				excelInterface.WorkSheets[0].SetCellFormat(2, 500, format);
				excelInterface.WorkSheets[0][2, 500] = "this is a test";

				format.Borders.Top.BorderStyle = CellBorderStyle.None;
				format.Borders.Left.BorderStyle = CellBorderStyle.None;

				format.BackgroundColor = Color.Red;
				excelInterface.WorkSheets[0].SetCellFormat(2, 700, format);

				format.Borders.Bottom.BorderStyle = CellBorderStyle.None;
				format.Borders.Right.BorderStyle = CellBorderStyle.Thick;

				format.BackgroundColor = Color.Green;
				excelInterface.WorkSheets[0].SetCellFormat(7, 700, format);

				int originalBackgroundColor = excelInterface.WorkSheets[0].GetCellFormat(7, 500).BackgroundColor.ToArgb();

				excelInterface.WorkSheets[0].CopyBorderSettings(2, 7);

				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(2, 500).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(2, 700).FontBold);
				AssertEquals(false, excelInterface.WorkSheets[0].GetCellFormat(7, 500).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(7, 700).FontBold);

				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(2, 500).BackgroundColor.ToArgb());
				AssertEquals(Color.Red.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(2, 700).BackgroundColor.ToArgb());
				AssertEquals(originalBackgroundColor, excelInterface.WorkSheets[0].GetCellFormat(7, 500).BackgroundColor.ToArgb());
				AssertEquals(Color.Green.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(7, 700).BackgroundColor.ToArgb());

				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(2, 500).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(2, 500).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(2, 500).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(2, 500).Borders.Right.BorderStyle);

				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(2, 700).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(2, 700).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(2, 700).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(2, 700).Borders.Right.BorderStyle);

				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(7, 500).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(7, 500).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(7, 500).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(7, 500).Borders.Right.BorderStyle);

				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(7, 700).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(7, 700).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(7, 700).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(7, 700).Borders.Right.BorderStyle);

				AssertEquals(excelInterface.WorkSheets[0][2, 500], "this is a test");
				AssertEquals(excelInterface.WorkSheets[0][7, 500], "");
			}
		}

		public void TestClearTopLine()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var lineToClear = 2;

				TFlxFormat rowFormat = excelInterface.Xls.GetFormat(excelInterface.Xls.GetRowFormat(lineToClear + 1)); // This is actually lineToClear but FlexCel is 1-based
				rowFormat.Borders.Top.Style = TFlxBorderStyle.Medium;
				excelInterface.Xls.SetRowFormat(lineToClear + 1, excelInterface.Xls.AddFormat(rowFormat)); // This is actually lineToClear but FlexCel is 1-based

				var format = new CellFormat();
				format.BackgroundColor = Color.Yellow;
				format.FillPattern = FillPatternStyle.Solid;
				format.FontName = "Arial";
				format.FontSize = 24;
				format.FontStyle = FontStyle.Bold;
				format.Borders.Top.BorderStyle = CellBorderStyle.Double;
				format.Borders.Left.BorderStyle = CellBorderStyle.Dash_dot;
				format.Borders.Right.BorderStyle = CellBorderStyle.Hair;
				format.Borders.Bottom.BorderStyle = CellBorderStyle.Slanted_dash_dot;
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear, 5, format);
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear, 500, format);
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear + 1, 5, format);
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear + 1, 500, format);
				excelInterface.WorkSheets[0][lineToClear, 5] = "this is a test";
				excelInterface.WorkSheets[0][lineToClear, 500] = "this is another test";
				excelInterface.WorkSheets[0][lineToClear + 1, 5] = "this is yet another test";
				excelInterface.WorkSheets[0][lineToClear + 1, 500] = "again????";
				excelInterface.WorkSheets[0][lineToClear + 2, 5] = "...";
				excelInterface.WorkSheets[0][lineToClear + 2, 500] = "!!!";

				excelInterface.WorkSheets[0].ClearTopLine(lineToClear);

				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).FontBold);
				AssertEquals(false, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 5).FontBold);
				AssertEquals(false, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 500).FontBold);

				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).BackgroundColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).BackgroundColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).BackgroundColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).BackgroundColor.ToArgb());
				AssertEquals(0, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 5).BackgroundColor.ToArgb());
				AssertEquals(0, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 500).BackgroundColor.ToArgb());

				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 500).Borders.Top.BorderStyle);

				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 500).Borders.Bottom.BorderStyle);

				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 500).Borders.Left.BorderStyle);

				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 5).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 2, 500).Borders.Right.BorderStyle);
			}
		}

		public void TestClearBottomLine()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);

				var lineToClear = 3;

				TFlxFormat rowFormat = excelInterface.Xls.GetFormat(excelInterface.Xls.GetRowFormat(lineToClear + 1)); // This is actually lineToClear but FlexCel is 1-based
				rowFormat.Borders.Bottom.Style = TFlxBorderStyle.Medium;
				excelInterface.Xls.SetRowFormat(lineToClear + 1, excelInterface.Xls.AddFormat(rowFormat)); // This is actually lineToClear but FlexCel is 1-based

				var format = new CellFormat();
				format.BackgroundColor = Color.Yellow;
				format.FillPattern = FillPatternStyle.Solid;
				format.FontName = "Arial";
				format.FontSize = 24;
				format.FontStyle = FontStyle.Bold;
				format.Borders.Top.BorderStyle = CellBorderStyle.Double;
				format.Borders.Left.BorderStyle = CellBorderStyle.Dash_dot;
				format.Borders.Right.BorderStyle = CellBorderStyle.Hair;
				format.Borders.Bottom.BorderStyle = CellBorderStyle.Slanted_dash_dot;
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear - 1, 5, format);
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear - 1, 500, format);
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear, 5, format);
				excelInterface.WorkSheets[0].SetCellFormat(lineToClear, 500, format);
				excelInterface.WorkSheets[0][lineToClear - 1, 5] = "this is a test";
				excelInterface.WorkSheets[0][lineToClear - 1, 500] = "this is another test";
				excelInterface.WorkSheets[0][lineToClear, 5] = "this is yet another test";
				excelInterface.WorkSheets[0][lineToClear, 500] = "again????";
				excelInterface.WorkSheets[0][lineToClear + 1, 5] = "...";
				excelInterface.WorkSheets[0][lineToClear + 1, 500] = "!!!";

				excelInterface.WorkSheets[0].ClearBottomLine(lineToClear);

				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 5).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 500).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).FontBold);
				AssertEquals(true, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).FontBold);
				AssertEquals(false, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).FontBold);
				AssertEquals(false, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).FontBold);

				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 5).BackgroundColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 500).BackgroundColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).BackgroundColor.ToArgb());
				AssertEquals(Color.Yellow.ToArgb(), excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).BackgroundColor.ToArgb());
				AssertEquals(0, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).BackgroundColor.ToArgb());
				AssertEquals(0, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).BackgroundColor.ToArgb());

				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 500).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.Double, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Top.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Top.BorderStyle);

				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.Slanted_dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 500).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Bottom.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Bottom.BorderStyle);

				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 500).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.Dash_dot, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Left.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Left.BorderStyle);

				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 5).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear - 1, 500).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 5).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.Hair, excelInterface.WorkSheets[0].GetCellFormat(lineToClear, 500).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 5).Borders.Right.BorderStyle);
				AssertEquals(CellBorderStyle.None, excelInterface.WorkSheets[0].GetCellFormat(lineToClear + 1, 500).Borders.Right.BorderStyle);
			}
		}

		public void TestGetObjectNames()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);

				xlInterface.Xls.ActiveSheet = 1;
				string[] objectNames = xlInterface.WorkSheets[0].GetObjectNames();
				AssertEquals(8, objectNames.Length);
				AssertEquals(null, objectNames[0]);
				AssertEquals(null, objectNames[1]);
				AssertEquals(null, objectNames[2]);
				AssertEquals(null, objectNames[3]);
				AssertEquals(null, objectNames[4]);
				AssertEquals("PicToDelete", objectNames[5]);
				AssertEquals(null, objectNames[6]);
				AssertEquals("PicToDelete", objectNames[7]);
			}
		}

		public void TestRemoveObject()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);

				xlInterface.Xls.ActiveSheet = 1;
				AssertEquals(8, xlInterface.Xls.ObjectCount);
				xlInterface.WorkSheets[0].RemoveAllObjectsByName("PicToDelete");
				AssertEquals(6, xlInterface.Xls.ObjectCount);
				AssertEquals(false, xlInterface.WorkSheets[0].ObjectExists("PicToDelete"));
			}
		}

		[ExpectException(typeof(ExcelInterfaceException))]
		public void TestRemoveNonExistingObject()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);

				xlInterface.WorkSheets[0].RemoveAllObjectsByName("NonExisitingPicture");
			}
		}

		public void TestObjectExists()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);

				AssertEquals(true, xlInterface.WorkSheets[0].ObjectExists("PicToDelete"));
				AssertEquals(false, xlInterface.WorkSheets[0].ObjectExists("NonExisitingPicture"));
			}
		}

		public void TestInsertPicture()
		{
			Image insertedImage;
			using (var xlInterface = new ExcelInterface())
			using (var soapBubbles = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(false, xlInterface.WorkSheets[0].ObjectExists("InsertedImage"));
				var img = new ExcelImage(soapBubbles, 10, 10, "InsertedImage");
				xlInterface.WorkSheets[0].InsertPicture(2, 2, 1, 1, img);
				AssertEquals(true, xlInterface.WorkSheets[0].ObjectExists("InsertedImage"));

				AssertEquals(1, xlInterface.WorkSheets[0].InsertedImages.Count);
				insertedImage = xlInterface.WorkSheets[0].InsertedImages.First();
			}
			AssertEquals(true, insertedImage.IsDisposed());
		}

		public void TestInsertPictureWithDefaultWidth()
		{
			using (var xlInterface = new ExcelInterface())
			using (var soapBubbles = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(false, xlInterface.WorkSheets[0].ObjectExists("InsertedImage"));
				var img = new ExcelImage(soapBubbles, 10, 20, "InsertedImage");
				xlInterface.WorkSheets[0].InsertPicture(2, 2, 1, -1, img);
				AssertInsertedImage(xlInterface, "InsertedImage", false, img);

				img = new ExcelImage(soapBubbles, 10, 20, "InsertedImage2");
				xlInterface.WorkSheets[0].InsertPicture(2, 2, 6, -1, img);
				AssertInsertedImage(xlInterface, "InsertedImage2", false, img);
			}
		}

		public void TestInsertPictureWithDefaultHeight()
		{
			using (var xlInterface = new ExcelInterface())
			using (var soapBubbles = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(false, xlInterface.WorkSheets[0].ObjectExists("InsertedImage"));
				var img = new ExcelImage(soapBubbles, 12, 24, "InsertedImage");
				xlInterface.WorkSheets[0].InsertPicture(2, 2, -1, 1, img);
				AssertInsertedImage(xlInterface, "InsertedImage", false, img);

				img = new ExcelImage(soapBubbles, 10, 20, "InsertedImage2");
				xlInterface.WorkSheets[0].InsertPicture(2, 2, -1, 5, img);
				AssertInsertedImage(xlInterface, "InsertedImage2", false, img);
			}
		}

		public void TestInsertPictureWithDefaultRowAndHeight()
		{
			using (var xlInterface = new ExcelInterface())
			using (var soapBubbles = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(false, xlInterface.WorkSheets[0].ObjectExists("InsertedImage"));
				var img = new ExcelImage(soapBubbles, 10, 20, "InsertedImage");
				xlInterface.WorkSheets[0].InsertPicture(2, 1, -1, -1, img);
				AssertInsertedImage(xlInterface, "InsertedImage", true, img);
			}
		}

		public void TestInsertImageWithAspectRatioLock_LargeCell()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)Utilities.Round((decimal)(300 * ExcelMetrics.ColMult(xlInterface.Xls)), 0);

				activeSheet.InsertPicture(0, 0, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 300, 300);
			}
		}

		public void TestInsertImageWithAspectRatioLock_NarrowCell()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)ExcelMetrics.ColMult(xlInterface.Xls) * 100;

				activeSheet.InsertPicture(0, 1, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 100, 100);
			}
		}

		public void TestInsertImageWithAspectRatioLock_ShortCell()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)ExcelMetrics.ColMult(xlInterface.Xls) * 300;

				activeSheet.InsertPicture(1, 0, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 150, 150);
			}
		}

		public void TestInsertImageWithAspectRatioLock_Small()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)ExcelMetrics.ColMult(xlInterface.Xls) * 60;

				activeSheet.InsertPicture(10, 10, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 17, 17);
			}

			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)ExcelMetrics.ColMult(xlInterface.Xls) * 2;

				activeSheet.InsertPicture(10, 10, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 2, 2);
			}
		}

		public void TestInsertImageWithAspectRatioLock_MultipleRows_TallArea()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(RedJpgPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, image.Height, image.Width, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var restrictedWidth = 150;
				var width = (int)Utilities.Round((decimal)(restrictedWidth * ExcelMetrics.ColMult(xlInterface.Xls)), 0);

				activeSheet.InsertPicture(3, 0, 10, width, excelImage);

				//Shouldn't go beyond the restricted width of the cell.
				AssertImageDimensions(activeSheet, excelImage, 13, restrictedWidth);
			}
		}

		public void TestInsertImageWithAspectRatioLock_MultipleRows_WideArea()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(RedJpgPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, image.Height, image.Width, "AspectRatioTestImage", true);
				var activeSheet = xlInterface.WorkSheets[0];
				var restrictedWidth = 150;
				var width = (int)Utilities.Round((decimal)(restrictedWidth * ExcelMetrics.ColMult(xlInterface.Xls)), 0);

				activeSheet.InsertPicture(2, 0, 1, width, excelImage);

				//Shouldn't go beyond the restricted width of the cell.
				AssertImageDimensions(activeSheet, excelImage, 13, restrictedWidth);
			}
		}

		public void TestInsertImageWithNoAspectRatioLock_MultipleRows()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(RedJpgPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, image.Height, image.Width, "AspectRatioTestImage", false);
				var activeSheet = xlInterface.WorkSheets[0];
				var restrictedWidth = 150;
				var width = (int)Utilities.Round((decimal)(restrictedWidth * ExcelMetrics.ColMult(xlInterface.Xls)), 0);

				activeSheet.InsertPicture(3, 0, 10, width, excelImage);

				//Shouldn't go beyond the restricted width of the cell.
				AssertImageDimensions(activeSheet, excelImage, 236, restrictedWidth);
			}
		}

		public void TestInsertImageWithNoAspectRatioLock()
		{
			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage");
				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)ExcelMetrics.ColMult(xlInterface.Xls) * 60;

				activeSheet.InsertPicture(10, 10, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 17, 60);
			}

			using (var xlInterface = new ExcelInterface())
			using (var image = Image.FromFile(SoapBubblesPath))
			{
				xlInterface.LoadExcelFile(ImageDimensionsXlsxPath);
				var excelImage = new ExcelImage(image, 256, 256, "AspectRatioTestImage");

				var activeSheet = xlInterface.WorkSheets[0];
				var width = (int)ExcelMetrics.ColMult(xlInterface.Xls) * 300;
				activeSheet.InsertPicture(0, 0, 1, width, excelImage);

				AssertImageDimensions(activeSheet, excelImage, 300, 295);
			}
		}

		public void TestGetCellsLastColumn()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(0, xlInterface.WorkSheets[0].GetCellsLastColumn(2, 0));
				AssertEquals(5, xlInterface.WorkSheets[0].GetCellsLastColumn(2, 3));
			}
		}

		public void TestGetImageCount()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(ReportTestXlsFilePath);
				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("The worksheet should have only 2 images", 2, workSheet.GetImageCount());
			}
		}

		public void TestGetImageProperties()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(ReportTestXlsFilePath);
				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("The worksheet should have only 2 images", 2, workSheet.GetImageCount());
				AssertEquals(TFlxAnchorType.MoveAndDontResize, workSheet.GetImageProperties(0).Anchor.AnchorType);
				AssertEquals(TFlxAnchorType.MoveAndDontResize, workSheet.GetImageProperties(1).Anchor.AnchorType);
			}
		}

		public void TestGetImage()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(ReportTestXlsFilePath);
				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals("The worksheet should have only 2 images", 2, workSheet.GetImageCount());

				AssertEquals(184, workSheet.GetImage(0).Width);
				AssertEquals(48, workSheet.GetImage(0).Height);

				AssertEquals(184, workSheet.GetImage(1).Width);
				AssertEquals(48, workSheet.GetImage(1).Height);
			}
		}

		public void TestGetImageFailure_ReactsCorrectly()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				var foo = xlInterface.WorkSheets[0];
				foo.GetImage(0);
				var stream = (MemoryStream)foo.ImageStreams[0];
				//do corruption
				for (byte i = 0; i < 64; ++i)
				{
					stream.Position = i;
					stream.WriteByte(i);
				}
				foo.GetImage(0); //this produces ArgumentException instead of OutOfMemoryException, I suspect to get an OOM thrown you need an image that LOOKS well formatted to GDI+, yet it can't process it
				AssertEquals("GetImage returned red X bitmap", 8, foo.GetImage(0).Width);
				AssertEquals("GetImage returned red X bitmap", 8, foo.GetImage(0).Height);
				AssertEquals("red X bitmap is cached", 8, Image.FromStream(foo.ImageStreams[0]).Width);
				AssertEquals("red X bitmap is cached", 8, Image.FromStream(foo.ImageStreams[0]).Height);

				AssertEquals("Correct error dialog shown to user", true, ((UnitTestUserNotification)Globals.Message).PreviousMessages[0].ToString().StartsWith("Error An exception was thrown trying to display an image in this document. It may be in an unsupported format or corrupt. It has been saved to "));

				ErrorReporter.Clear();
				TempDirectory.DeleteDirectory(Temp.TempPath + Path.DirectorySeparatorChar + "InvalidImages");
			}
		}

		public void Test_CR_IsRemovedFromFormula()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				xlInterface.WorkSheets[0][1, 1] = new TFormula("=UPPER(\"test\rtest\")", "");
				var readFormula = xlInterface.WorkSheets[0][1, 1] as TFormula;

				AssertEquals("=UPPER(\"testtest\")", readFormula.Text);
			}
		}

		public void TestMoveColumns()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("Packages", xlInterface.WorkSheets[0][32, 2]);
				AssertEquals("Weight", xlInterface.WorkSheets[0][32, 3]);
				xlInterface.WorkSheets[0].MoveColumns(2, 3);
				AssertEquals("Packages", xlInterface.WorkSheets[0][32, 3]);
				AssertEquals("Weight", xlInterface.WorkSheets[0][32, 2]);
			}
		}

		public void TestMoveColumnsBackwards()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("Packages", xlInterface.WorkSheets[0][32, 2]);
				xlInterface.WorkSheets[0].MoveColumns(4, 2);
				AssertEquals("Volume", xlInterface.WorkSheets[0][32, 2]);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMoveColumnsAdjustsAdjacentCellReferencesCorrectly()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "AdjacentCellReferences.xls");
				var workSheet = xlInterface.WorkSheets[0];
				AssertCellValue(workSheet, 0, 0, 1d);
				AssertCellValue(workSheet, 0, 1, 2d);
				AssertCellValue(workSheet, 0, 2, 3d);
				AssertCellValue(workSheet, 0, 3, new TFormula("=+C1-A1", 2d));
				AssertCellValue(workSheet, 0, 4, new TFormula("=+D1+B1", 4d));
				AssertCellValue(workSheet, 0, 5, new TFormula("=+D1+E1", 6d));
				AssertEquals("workSheet.ColumnCount", 6, workSheet.ColumnCount);

				workSheet.MoveColumns(3, 4);
				AssertCellValue(workSheet, 0, 0, 1d);
				AssertCellValue(workSheet, 0, 1, 2d);
				AssertCellValue(workSheet, 0, 2, 3d);
				AssertCellValue(workSheet, 0, 3, new TFormula("=+E1+B1", 4d));
				AssertCellValue(workSheet, 0, 4, new TFormula("=+C1-A1", 2d));
				AssertCellValue(workSheet, 0, 5, new TFormula("=+E1+D1", 6d));
				AssertEquals("workSheet.ColumnCount", 6, workSheet.ColumnCount);

				workSheet.MoveColumns(3, 4);
				AssertCellValue(workSheet, 0, 0, 1d);
				AssertCellValue(workSheet, 0, 1, 2d);
				AssertCellValue(workSheet, 0, 2, 3d);
				AssertCellValue(workSheet, 0, 3, new TFormula("=+C1-A1", 2d));
				AssertCellValue(workSheet, 0, 4, new TFormula("=+D1+B1", 4d));
				AssertCellValue(workSheet, 0, 5, new TFormula("=+D1+E1", 6d));
				AssertEquals("workSheet.ColumnCount", 6, workSheet.ColumnCount);

				workSheet.MoveColumns(2, 4);
				AssertCellValue(workSheet, 0, 0, 1d);
				AssertCellValue(workSheet, 0, 1, 2d);
				AssertCellValue(workSheet, 0, 2, new TFormula("=+E1-A1", 2d));
				AssertCellValue(workSheet, 0, 3, new TFormula("=+C1+B1", 4d));
				AssertCellValue(workSheet, 0, 4, 3d);
				AssertCellValue(workSheet, 0, 5, new TFormula("=+C1+D1", 6d));
				AssertEquals("workSheet.ColumnCount", 6, workSheet.ColumnCount);
			}
		}

		#region TestDuplicateRows

		void AssertDuplicateRowsTooManyRows(string fileName, string type)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(fileName);
				var workSheet = excelInterface.WorkSheets[0];
				int destinationRow = FlxConsts.Max_Rows2007;
				if (!string.Equals(type, "xlsx", StringComparison.OrdinalIgnoreCase))
				{
					destinationRow = FlxConsts.Max_Rows97_2003;
				}

				AssertNoExceptionThrown(() => workSheet.DuplicateRows(32, 32, destinationRow, 1));
				using (var stream = new MemoryStream())
				{
					AssertNoExceptionThrown(() => excelInterface.SaveToStream(stream, type));
				}

				if (string.Equals(type, "xlsx", StringComparison.OrdinalIgnoreCase))
				{
					AssertExceptionThrown(typeof(DocumentEngineTooManyRowsException), () => workSheet.DuplicateRows(31, 32, destinationRow, 1));
					AssertExceptionThrown(typeof(DocumentEngineTooManyRowsException), () => workSheet.DuplicateRows(32, 32, destinationRow - 1, 3));
				}
				else
				{
					// the internal calculation is done assuming XLSX so it will always duplicate the rows for XLS, it will fail when writing
					AssertNoExceptionThrown(() => workSheet.DuplicateRows(32, 32, destinationRow - 1, 3));
					using (var stream = new MemoryStream())
					{
						AssertExceptionThrown(typeof(DocumentEngineTooManyRowsForThisFileFormatException), () => excelInterface.SaveToStream(stream, type));
					}
				}
			}
		}

		public void TestDuplicateRowsTooManyRows_XLS()
		{
			AssertDuplicateRowsTooManyRows(ReportTestXlsFilePath, "xls");
		}

		public void TestDuplicateRowsTooManyRows_XLSX()
		{
			AssertDuplicateRowsTooManyRows(ReportTestXlsxFilePath, "xlsx");
		}

		public void TestDuplicateRows()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals("Packages", excelInterface.WorkSheets[0][32, 2]);
				AssertEquals("Weight", excelInterface.WorkSheets[0][32, 3]);
				AssertEquals("", excelInterface.WorkSheets[0][33, 2]);
				AssertEquals("", excelInterface.WorkSheets[0][33, 3]);
				AssertEquals("SUM({JC_TareWeight})", excelInterface.WorkSheets[0][34, 2]);
				AssertEquals("MAX({JC_CargoWeight})", excelInterface.WorkSheets[0][34, 3]);

				excelInterface.WorkSheets[0].DuplicateRows(32, 32, 33, 1);

				AssertEquals("Packages", excelInterface.WorkSheets[0][32, 2]);
				AssertEquals("Weight", excelInterface.WorkSheets[0][32, 3]);
				AssertEquals("Packages", excelInterface.WorkSheets[0][33, 2]);
				AssertEquals("Weight", excelInterface.WorkSheets[0][33, 3]);
				AssertEquals("", excelInterface.WorkSheets[0][34, 2]);
				AssertEquals("", excelInterface.WorkSheets[0][34, 3]);
				AssertEquals("SUM({JC_TareWeight})", excelInterface.WorkSheets[0][35, 2]);
				AssertEquals("MAX({JC_CargoWeight})", excelInterface.WorkSheets[0][35, 3]);
			}
		}

		public void TestDuplicateRows_XLSX()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(ReportTestXlsxFilePath);
				AssertEquals("Packages", excelInterface.WorkSheets[0][32, 2]);
				AssertEquals("Weight", excelInterface.WorkSheets[0][32, 3]);
				AssertEquals("I'm so far away!", excelInterface.WorkSheets[0][32, 260]);
				AssertEquals("", excelInterface.WorkSheets[0][33, 2]);
				AssertEquals("", excelInterface.WorkSheets[0][33, 3]);
				AssertEquals("", excelInterface.WorkSheets[0][33, 260]);
				AssertEquals("SUM({JC_TareWeight})", excelInterface.WorkSheets[0][34, 2]);
				AssertEquals("MAX({JC_CargoWeight})", excelInterface.WorkSheets[0][34, 3]);
				AssertEquals("", excelInterface.WorkSheets[0][34, 260]);

				excelInterface.WorkSheets[0].DuplicateRows(32, 32, 33, 1);

				AssertEquals("Packages", excelInterface.WorkSheets[0][32, 2]);
				AssertEquals("Weight", excelInterface.WorkSheets[0][32, 3]);
				AssertEquals("I'm so far away!", excelInterface.WorkSheets[0][32, 260]);
				AssertEquals("Packages", excelInterface.WorkSheets[0][33, 2]);
				AssertEquals("Weight", excelInterface.WorkSheets[0][33, 3]);
				AssertEquals("I'm so far away!", excelInterface.WorkSheets[0][33, 260]);
				AssertEquals("", excelInterface.WorkSheets[0][34, 2]);
				AssertEquals("", excelInterface.WorkSheets[0][34, 3]);
				AssertEquals("", excelInterface.WorkSheets[0][34, 260]);
				AssertEquals("SUM({JC_TareWeight})", excelInterface.WorkSheets[0][35, 2]);
				AssertEquals("MAX({JC_CargoWeight})", excelInterface.WorkSheets[0][35, 3]);
				AssertEquals("", excelInterface.WorkSheets[0][35, 260]);
			}
		}

		#endregion

		public void TestMonetaryValueSetsFormatOnCell()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				xlInterface.WorkSheets[0][1, 1] = new FormattedCellValue(12, "0.00");
				var cellFormatCode = xlInterface.Xls.GetCellFormat(1 + 1, 1 + 1);
				var format = cellFormatCode <= 0 ? xlInterface.Xls.GetDefaultFormat : xlInterface.Xls.GetFormat(cellFormatCode);
				AssertEquals("0.00", format.Format);
			}
		}

		public void TestIsCellEmpty()
		{
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(ReportTestXlsFilePath);
				AssertEquals(true, xlInterface.WorkSheets[0].IsCellEmpty(37, 2));
				AssertEquals(false, xlInterface.WorkSheets[0].IsCellEmpty(40, 2));
				AssertEquals(false, xlInterface.WorkSheets[0].IsCellEmpty(41, 2));
				AssertEquals(false, xlInterface.WorkSheets[0].IsCellEmpty(41, 3));
				AssertEquals(false, xlInterface.WorkSheets[0].IsCellEmpty(30, 8));
				AssertEquals(false, xlInterface.WorkSheets[0].IsCellEmpty(32, 2));
			}
		}

		public void TestSetComment()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				string testComment = "this is the test comment";
				xlInterface.WorkSheets[0][1, 1] = "this is a test";
				xlInterface.WorkSheets[0].SetComment(1, 1, testComment);

				AssertEquals("Cell comment should be not empty", testComment, xlInterface.WorkSheets[0].GetComment(1, 0));
				AssertEquals("Cell should not be empty", false, xlInterface.WorkSheets[0].IsCellEmpty(1, 1));
				AssertEquals("Text in Cell should not be empty", "this is a test", xlInterface.WorkSheets[0][1, 1].ToString());
			}
		}

		public void TestInsertHyperlink()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.NewExcelFile(1);
				TFlxFormat originalCellFormat = xlInterface.Xls.GetCellVisibleFormatDef(1, 1);
				originalCellFormat.HAlignment = THFlxAlignment.right;
				xlInterface.Xls.SetCellFormat(1, 1, xlInterface.Xls.AddFormat(originalCellFormat));
				AssertEquals("Pre-condition", 0, xlInterface.Xls.HyperLinkCount);

				ExcelWorkSheet worksheet = xlInterface.WorkSheets[0];
				ExcelHyperlink hyperlink = new ExcelHyperlink(THyperLinkType.UNC, "LINK", "\\SOMESERVER", "AY1", "Blah", "Click here");
				worksheet.InsertHyperlink(0, 0, hyperlink);
				AssertEquals(1, xlInterface.Xls.HyperLinkCount);
				THyperLink generatedLink = xlInterface.Xls.GetHyperLink(1);
				AssertEquals(THyperLinkType.UNC, generatedLink.LinkType);
				AssertEquals("\\SOMESERVER", generatedLink.Text);
				AssertEquals("LINK", generatedLink.Description);
				AssertEquals("Click here", generatedLink.Hint);
				AssertEquals("AY1", generatedLink.TargetFrame);
				AssertEquals("Blah", generatedLink.TextMark);

				TXlsCellRange linkCellRange = xlInterface.Xls.GetHyperLinkCellRange(1);
				AssertEquals(1, linkCellRange.Top);
				AssertEquals(1, linkCellRange.Left);
				AssertEquals(1, linkCellRange.Right);
				AssertEquals(1, linkCellRange.Bottom);
				AssertEquals(1, linkCellRange.ColCount);
				AssertEquals(1, linkCellRange.RowCount);

				TFlxFormat linkCellFormat = xlInterface.Xls.GetCellVisibleFormatDef(1, 1);
				AssertEquals(TFlxUnderline.Single, linkCellFormat.Font.Underline);
				AssertEquals(TUIColor.FromArgb(255, 0, 0, 255), linkCellFormat.Font.Color.ToColor(xlInterface.Xls));
				AssertEquals(THFlxAlignment.right, linkCellFormat.HAlignment);
			}
		}

		public void TestDocumentIDBarcodeDocStripHeight()
		{
			var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilder.DocBuilderTemplateType.System);
			var portraitSection = template.TemplateSections.Find("Portrait Footers - Document Barcode");
			var landscapeSection = template.TemplateSections.Find("Landscape Footers - Document Barcode");
			AssertNotNull("Precondition - Section should exist.", portraitSection);
			AssertNotNull("Precondition - Section should exist.", landscapeSection);
			AssertEquals("Precondition - Portrait row count should be three.", 2, portraitSection.RowCount);
			AssertEquals("Precondition - Landscape row count should be three.", 2, landscapeSection.RowCount);

			var excelTemplate = template.GetExcelTemplate();
			using (var excelInterface = excelTemplate.GetNewExcelInterface())
			{
				const int numberOfPixelsForCellHeightUnit = 15;
				var worksheet = excelInterface.WorkSheets[0];
				var portraitSectionBarcodePixelCount = worksheet.GetCellHeight(portraitSection.StartingRowNumber, 0) / numberOfPixelsForCellHeightUnit;
				var landscapeSectionBarcodePixelCount = worksheet.GetCellHeight(landscapeSection.StartingRowNumber, 0) / numberOfPixelsForCellHeightUnit;
				var portraitSectionDocumentIDPixelCount = worksheet.GetCellHeight(portraitSection.StartingRowNumber + 1, 0) / numberOfPixelsForCellHeightUnit;
				var landscapeSectionDocumentIDPixelCount = worksheet.GetCellHeight(portraitSection.StartingRowNumber + 1, 0) / numberOfPixelsForCellHeightUnit;

				AssertEquals("Barcode pixel count should not be changed. If you want to change this, make sure number of rows skipped(RowSkipped) in BarcodeLocator.cs is sufficient to find the barcode during image processing and cartage advice in local transport fit into a one page.", 15, portraitSectionBarcodePixelCount);
				AssertEquals("Barcode pixel count should not be changed. If you want to change this, make sure number of rows skipped(RowSkipped) in BarcodeLocator.cs is sufficient to find the barcode during image processing and cartage advice in local transport fit into a one page..", 15, landscapeSectionBarcodePixelCount);
				AssertEquals("Document ID text should have 9 pixels.", 9, portraitSectionDocumentIDPixelCount);
				AssertEquals("Document ID text should have 9 pixels.", 9, landscapeSectionDocumentIDPixelCount);
			}
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

		string soapBubblesPath;
		string SoapBubblesPath
		{
			get
			{
				if (string.IsNullOrEmpty(soapBubblesPath))
				{
					soapBubblesPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.Soap Bubbles.bmp");
				}
				return soapBubblesPath;
			}
		}

		string imageDimensionsXlsxPath;
		string ImageDimensionsXlsxPath
		{
			get
			{
				if (string.IsNullOrEmpty(imageDimensionsXlsxPath))
				{
					imageDimensionsXlsxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.ImageDimensionsTest.xls");
				}
				return imageDimensionsXlsxPath;
			}
		}

		string redJpgPath;
		string RedJpgPath
		{
			get
			{
				if (string.IsNullOrEmpty(redJpgPath))
				{
					redJpgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.red.jpg");
				}
				return redJpgPath;
			}
		}

		StmTemplateBase CreateTemplate(string templateName, TestFilesSubFolder subFolderName)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(templateName, subFolderName);
			var result = Factory.New<StmTemplateBase>();
			result.SO_Template = excelTemplate.GetAsByteArray();
			result.SO_DataContext = nameof(Core.Constants.DataContext.UnitTest);

			return result;
		}

		void AssertInsertedImage(ExcelInterface xlInterface, string name, bool compareAbsoluteValues, ExcelImage img)
		{
			AssertEquals(true, xlInterface.WorkSheets[0].ObjectExists(name));

			int objIndex = xlInterface.Xls.FindObject(name);
			int imageIndex = xlInterface.Xls.ObjectIndexToImageIndex(objIndex);
			TImageProperties imgProps = xlInterface.Xls.GetImageProperties(imageIndex);
			double h = 0;
			double w = 0;
			imgProps.Anchor.CalcImageCoords(ref h, ref w, xlInterface.Xls);
			AssertEquals("Aspect ratio of inserted image is wrong", h * img.Image.Width / img.Image.Height, w, 1); //1 pixels max.
			if (compareAbsoluteValues)
			{
				AssertEquals("Height of inserted image should be the same", h, img.Image.Height, 2); //2 pixels max.
				AssertEquals("Width of inserted image should be the same", w, img.Image.Width, 2); //2 pixels max.
			}
		}

		static void AssertImageDimensions(ExcelWorkSheet sheet, ExcelImage excelImage, decimal expectedHeight, decimal expectedWidth)
		{
			Assert("Pre-condition: worksheet should contain image", sheet.ObjectExists(excelImage.ImageName));
			var anchor = sheet.GetObjectAnchor(excelImage.ImageName);
			double h = 0;
			double w = 0;
			anchor.CalcImageCoords(ref h, ref w, sheet.ParentExcelInterface.Xls);

			var resultHeight = Utilities.Round((decimal)h, 0);
			var resultWidth = Utilities.Round((decimal)w, 0);

			if (excelImage.IsAspectRatioLocked && excelImage.Width != 0)
			{
				AssertNotEquals("Image exists, should not be without dimensions", 0, resultWidth);

				int originalAspectRatio = (int)(excelImage.Height / (decimal)excelImage.Width);
				int resultAspectRatio = (int)(resultHeight / resultWidth);

				AssertEquals("Expected to keep the aspect ratio", originalAspectRatio, resultAspectRatio);
			}

			AssertEquals("height roughly rounded to the correct number", expectedHeight, resultHeight, 2);
			AssertEquals("width roughly rounded to the correct number", expectedWidth, resultWidth, 2);
		}

		void AssertCellValue(ExcelWorkSheet workSheet, int row, int col, double expectedValue)
		{
			AssertEquals("At [" + row + "," + col + "]", expectedValue, workSheet[row, col]);
		}

		void AssertCellValue(ExcelWorkSheet workSheet, int row, int col, TFormula expectedValue)
		{
			TFormula actualValue = workSheet[row, col] as TFormula;
			if (actualValue != null && expectedValue != null)
			{
				AssertEquals("At [" + row + "," + col + "]", FormatForComparison(expectedValue), FormatForComparison(actualValue));
			}
			else
			{
				AssertEquals("At [" + row + "," + col + "]", expectedValue, workSheet[row, col]);
			}
		}

		string FormatForComparison(TFormula formula)
		{
			return "Text:[" + formula.Text + "]   Result:[" + (formula.Result == null ? "null]" : formula.Result + "] Type:[" + formula.Result.GetType() + "]");
		}
	}
}
