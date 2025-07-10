using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.Testing.ExcelExporterGuiNotificationsTest;
using Env = Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Excel.Testing
{
	class ExcelTest : TestCaseWithDummy
	{
		#region Testing the Export

		/// we do not want to spawn Excel.exe during testing, thus PreviewInXL() creates the excel file but does not start Excel if Globals.IsTest.
		/// the next best thing is to manually load the Excel file and check the data it contains.

		public void TestExportIntoAndOpenExcelWithFormattedColumns()
		{
			exporter = new ExcelExporter(dummyChildCollection, excelExportColumnCollection, notifications);
			exporter.ExportIntoAndOpenExcel();

			AssertLastExportedExcelFileContent();
		}

		#region Test Excel column limit

		[ExpectNoExceptions]
		public void TestPopulateHeadings_HitsExcel97_2003ColumnLimit_ThrowsNoException_ChangeToXLSX()
		{
			SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
			SetupMaxColCountForAssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException();
			AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel97_2003ColumnLimit, Env.Excel.MaxColCountSupported97_2003, DialogResult.Yes);
		}

		[ExpectNoExceptions]
		public void TestPopulateHeadings_HitsExcel97_2003ColumnLimit_ThrowsNoException_DontChangeToXLSX()
		{
			SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
			SetupMaxColCountForAssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException();
			AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel97_2003ColumnLimit, Env.Excel.MaxColCountSupported97_2003, DialogResult.No);
		}

		[ExpectNoExceptions]
		public void TestPopulateHeadings_HitsExcel2007ColumnLimit_ThrowsNoException_ChangeToXLSX()
		{
			SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
			SetupMaxColCountForAssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException();
			AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel2007ColumnLimit, Env.Excel.MaxColCountSupported2007, DialogResult.Yes);
		}

		[ExpectNoExceptions]
		public void TestPopulateHeadings_HitsExcel2007ColumnLimit_ThrowsNoException_DontChangeToXLSX()
		{
			SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
			SetupMaxColCountForAssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException();
			AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel2007ColumnLimit, Env.Excel.MaxColCountSupported2007, DialogResult.No);
		}

		public void TestPopulateHeadings_HitsExcel97_2003ColumnLimit_ThrowsNoException_XLSX()
		{
			AssertNoExceptionThrown(() =>
			{
				SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xlsx);
				SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
				AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel97_2003ColumnLimit, Env.Excel.MaxRowCountSupported97_2003);
			});
		}

		public void TestPopulateHeadings_HitsExcel2007ColumnLimit_ThrowsNoException_XLSX()
		{
			AssertNoExceptionThrown(() =>
			{
				SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xlsx);
				SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
				AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel2007ColumnLimit, Env.Excel.MaxColCountSupported2007);
			});
		}

		public void TestPopulateHeadings_HitsExcel97_2003ColumnLimit_ThrowsNoException_XLS()
		{
			AssertNoExceptionThrown(() =>
			{
				SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
				SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
				AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(ExcelExportColumnsThatHitsExcel97_2003ColumnLimit, Env.Excel.MaxRowCountSupported97_2003, DialogResult.None, Times.Once());
			});
		}

		void SetupMaxColCountForAssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException()
		{
			Env.Excel.MaxColCountSupported97_2003 = 5;
			Env.Excel.MaxColCountSupported2007 = 10;
		}

		void AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(List<ExcelExportColumnBase> excelExportColumns, int minColumnCount)
		{
			AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(excelExportColumns, minColumnCount, DialogResult.None, Times.Never());
		}

		void AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(List<ExcelExportColumnBase> excelExportColumns, int minColumnCount, DialogResult answer)
		{
			AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(excelExportColumns, minColumnCount, answer, Times.Once());
		}

		void AssertPopulateHeadings_HitsExcelColumnLimit_ThrowsNoException(List<ExcelExportColumnBase> excelExportColumns, int minColumnCount, DialogResult answer, Times times)
		{
			try
			{
				notificationsMock.Setup(o => o.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(It.IsAny<string>())).Returns(answer).Verifiable(times);
				exporter = new ExcelExporter(dummyChildCollection, excelExportColumns, notifications);
				Assert(string.Format("There should be more than {0} columns to export", minColumnCount), excelExportColumns.Count > minColumnCount);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify();
			}
			finally
			{
				Env.Excel.Reset();
			}
		}

		#endregion

		#region Test Excel row limit

		[ExpectNoExceptions]
		public void TestPopulateHeadings_HitsExcel97_2003RowLimit_ThrowsNoException_ChangeToXLSX()
		{
			SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
			notificationsMock.Setup(o => o.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(It.IsAny<string>())).Returns(DialogResult.Yes);
			AssertPopulateRows_HitsExcelRowLimit_ThrowsNoException(DummyChildCollectionThatHitsExcel97_2003RowLimit, Env.Excel.MaxRowCountSupported97_2003);
		}

		[ExpectNoExceptions]
		public void TestPopulateRows_HitsExcel97_2003RowLimit_ThrowsNoException_DontChangeToXLSX()
		{
			SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
			notificationsMock.Setup(o => o.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(It.IsAny<string>())).Returns(DialogResult.No);
			AssertPopulateRows_HitsExcelRowLimit_ThrowsNoException(DummyChildCollectionThatHitsExcel97_2003RowLimit, Env.Excel.MaxRowCountSupported97_2003);
		}

		[ExpectNoExceptions]
		public void TestPopulateRows_HitsExcel2007RowLimit_ThrowsNoException()
		{
			SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
			AssertPopulateRows_HitsExcelRowLimit_ThrowsNoException(DummyChildCollectionThatHitsExcel2007RowLimit, Env.Excel.MaxRowCountSupported2007);
			notificationsMock.Verify(o => o.ShowMaxEntriesSupportedByExcelExceededError(It.IsAny<string>()), Times.Once());
		}

		public void TestPopulateHeadings_HitsExcel97_2003RowLimit_ThrowsNoException_XLSX()
		{
			AssertNoExceptionThrown(() =>
			{
				SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xlsx);
				SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
				AssertPopulateRows_HitsExcelRowLimit_ThrowsNoException(DummyChildCollectionThatHitsExcel97_2003RowLimit, Env.Excel.MaxRowCountSupported97_2003);
				notificationsMock.Verify(o => o.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(It.IsAny<string>()), Times.Never());
			});
		}

		public void TestPopulateHeadings_HitsExcel97_2003RowLimit_ThrowsNoException_XLS()
		{
			AssertNoExceptionThrown(() =>
			{
				SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
				SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException();
				AssertPopulateRows_HitsExcelRowLimit_ThrowsNoException(DummyChildCollectionThatHitsExcel97_2003RowLimit, Env.Excel.MaxRowCountSupported97_2003);
				notificationsMock.Verify(o => o.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(It.IsAny<string>()), Times.Once());
			});
		}

		void SetupMaxRowCountForAssertPopulateRows_HitsExcelRowLimit_ThrowsNoException()
		{
			Env.Excel.MaxRowCountSupported97_2003 = 5;
			Env.Excel.MaxRowCountSupported2007 = 10;
		}

		void AssertPopulateRows_HitsExcelRowLimit_ThrowsNoException(DummyChildBusinessObjectCollection collection, int minRowCount)
		{
			try
			{
				exporter = new ExcelExporter(collection, excelExportColumnCollection, notifications);
				Assert(string.Format("There should be more than {0} rows to export", minRowCount), collection.Count > minRowCount);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify();
			}
			finally
			{
				Env.Excel.Reset();
			}
		}

		void SetupDefaultFormat(string format)
		{
			SystemDataRegistry.Instance.ExportToExcelFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, format);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestExportIntoAndOpenExcel_WhenCellContentExceedsExcelCapabilities_ShouldNotThrowException()
		{
			var stringTooLongToFitInACell = new ZString('*', ExcelExporter.MaxStringLengthInCellSupportedByExcel + 1);
			var expectedCellContent = new ZString('*', ExcelExporter.MaxStringLengthInCellSupportedByExcel);

			dummyChildCollection.RemoveAndDeleteAll();

			var bigDummy = DummyChildBusinessObject.New(Dummy.Factory);
			bigDummy.Z0_NVarCharMax = stringTooLongToFitInACell;
			dummyChildCollection.Add(bigDummy);

			exporter = new ExcelExporter(dummyChildCollection, excelExportColumnCollection, notifications);
			exporter.ExportIntoAndOpenExcel();

			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				excelInterface.LoadExcelFile(exporter.LastExportedFileNameForTest);
				using (var workSheet = excelInterface.WorkSheets[0])
				{
					AssertEquals(string.Format("Cell content should have been truncated to the first {0} characters", ExcelExporter.MaxStringLengthInCellSupportedByExcel),
						expectedCellContent, workSheet[1, nvarcharmaxIndex]);
				}
			}
		}

		class FilteredBusinessObjectReaderWithSqlException : FilteredBusinessObjectReader
		{
			public FilteredBusinessObjectReaderWithSqlException(ZQuery objectFilter, Type businessObjectType)
				: base(objectFilter, businessObjectType)
			{
			}

			protected override List<ZGuid> GetObjectPKsCore()
			{
				throw SqlExceptionBuilder.CreateSqlException(8623, "The query processor ran out of internal resources and could not produce a query plan.");
			}
		}

		public void TestExportIntoAndOpenExcelWithSqlException()
		{
			Env.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.UnitTestUserNotification.Instance.AddAnswer(Env.ZDialogResult.OK);

			var initialValue = Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids;
			try
			{
				Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids = 2;

				using (var parentForm = new ZForm())
				{
					var progressFormMock = new Mock<IProgressForm>();
					var exporterNotifications = new ExcelExporterGuiNotificationsForTest(parentForm, progressFormMock.Object);

					DummyChildBusinessObject.New(Dummy.Factory);
					DummyChildBusinessObject.New(Dummy.Factory);
					Factory.Save();

					var reader = new FilteredBusinessObjectReaderWithSqlException(new ZQuery(), typeof(DummyBusinessObject));
					exporter = new ExcelExporter(reader, exporterNotifications);

					AssertNoExceptionThrown(() => exporter.ExportIntoAndOpenExcel());
					AssertEquals("Your query is too complicated, please simplify your search conditions.", Env.UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("ProgressForm is cleaned up", exporterNotifications.ProgressFormExposed);
				}
			}
			finally
			{
				Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids = initialValue;
			}
		}

		public void TestExportIntoAndOpenExcelWithFilteredBusinessObjectReader()
		{
			Factory.Save(); // save the dummies created in Setup()

			var reader = new FilteredBusinessObjectReader(new ZQuery(DummyBizoSchema.Z0_Code, "smurf"), typeof(DummyBusinessObject));
			exporter = new ExcelExporter(reader, notifications);
			exporter.ExportIntoAndOpenExcel();

			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				excelInterface.LoadExcelFile(exporter.LastExportedFileNameForTest);
				using (var workSheet = excelInterface.WorkSheets[0])
				{
					AssertEquals(1 + 1, workSheet.RowCount); // header + 1 bizO

					AssertEquals("DummyBizo|Z0_AnotherDate", workSheet[0, anotherDateIndex]);
					AssertEquals("DummyBizo|Z0_DateOnly", workSheet[0, dateOnlyIndex]);
					AssertEquals("Code", workSheet[0, codeIndex]);
					AssertEquals("Another Decimal", workSheet[0, anotherDecimalIndex]);
					AssertEquals("Date (With Offset)", workSheet[0, dateTimeOffsetIndex]);

					AssertEquals(new DateTime(1979, 5, 6), DateTime.FromOADate((double)workSheet[1, anotherDateIndex]));
					AssertEquals(ZDateTime.LongTimeFormat, workSheet.GetCellFormat(1, anotherDateIndex).FormatPattern);
					AssertEquals(new DateTime(1979, 5, 6), DateTime.FromOADate((double)workSheet[1, dateOnlyIndex]));
					AssertEquals(ZDateTime.ShortDateFormat, workSheet.GetCellFormat(1, dateOnlyIndex).FormatPattern);
					AssertEquals(new DateTime(1979, 5, 7, 1, 2, 3), DateTime.FromOADate((double)workSheet[1, dateTimeOffsetIndex]));
					AssertEquals(ZDateTime.LongTimeFormat, workSheet.GetCellFormat(1, dateTimeOffsetIndex).FormatPattern);

					AssertEquals("smurf", workSheet[1, codeIndex]);

					AssertEquals(10.10, workSheet[1, anotherDecimalIndex]);
					AssertEquals("#,##0.00", workSheet.GetCellFormat(1, anotherDecimalIndex).FormatPattern);
				}
			}
		}

		void AssertLastExportedExcelFileContent()
		{
			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				excelInterface.LoadExcelFile(exporter.LastExportedFileNameForTest);
				using (var workSheet = excelInterface.WorkSheets[0])
				{
					AssertExportedExcelFileContent(workSheet);
				}
			}
		}

		void AssertExportedExcelFileContent(IExcelWorkSheet workSheet)
		{
			AssertEquals(1 + 3, workSheet.RowCount); // header + 3 bizOs

			for (var i = 0; i < excelExportColumnCollection.Count; i++)
			{
				var column = excelExportColumnCollection[i];
				AssertEquals(column.Description, workSheet[0, i]);
			}

			AssertEquals("DummyBizo|Z0_AnotherDate", workSheet[0, anotherDateIndex]);
			AssertEquals("Code", workSheet[0, codeIndex]);
			AssertEquals("Another Decimal", workSheet[0, anotherDecimalIndex]);

			AssertEquals(new DateTime(1979, 5, 6), DateTime.FromOADate((double)workSheet[1, anotherDateIndex]));
			AssertEquals(new DateTime(1978, 2, 10), DateTime.FromOADate((double)workSheet[3, anotherDateIndex]));
			AssertEquals(ZDateTime.LongTimeFormat, workSheet.GetCellFormat(1, anotherDateIndex).FormatPattern);
			AssertEquals(ZDateTime.LongTimeFormat, workSheet.GetCellFormat(3, anotherDateIndex).FormatPattern);

			AssertEquals(new DateTime(1979, 5, 7, 1, 2, 3), DateTime.FromOADate((double)workSheet[1, dateTimeOffsetIndex]));
			AssertEquals(new DateTime(1978, 2, 11, 4, 5, 6), DateTime.FromOADate((double)workSheet[3, dateTimeOffsetIndex]));
			AssertEquals(ZDateTime.LongTimeFormat, workSheet.GetCellFormat(1, dateTimeOffsetIndex).FormatPattern);
			AssertEquals(ZDateTime.LongTimeFormat, workSheet.GetCellFormat(3, dateTimeOffsetIndex).FormatPattern);

			AssertEquals("smurf", workSheet[1, codeIndex]);
			AssertEquals("jokey", workSheet[3, codeIndex]);

			AssertEquals(10.10, workSheet[1, anotherDecimalIndex]);
			AssertEquals(10.30, workSheet[3, anotherDecimalIndex]);
			AssertEquals("#,##0.00", workSheet.GetCellFormat(1, anotherDecimalIndex).FormatPattern);
			AssertEquals("#,##0.00", workSheet.GetCellFormat(2, anotherDecimalIndex).FormatPattern);
		}

		int IndexOfSchemaColumn(List<ExcelExportColumnBase> collection, SchemaColumn columnToFind)
		{
			for (var i = 0; i < collection.Count; i++)
			{
				var column = collection[i] as ExcelExportColumn;
				if (column != null && column.SchemaColumn == columnToFind)
				{
					return i;
				}
			}

			throw new ArgumentException("SchemaColumn not found in collection", nameof(columnToFind));
		}

		public void TestExporterWithCommentAndColor()
		{
			TestExporterWithCommentAndColor(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
		}

		public void TestExporterWithCommentAndColor_Xlsx()
		{
			TestExporterWithCommentAndColor(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xlsx);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestExporterWithCommentAndColor(string format)
		{
			SetupDefaultFormat(format);
			var collection = new DummyBusinessObjectCollection(Factory);

			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_Date = new ZDateTime(1983, 7, 10);
			bizObj1.Z0_Description = ZString.Empty;
			collection.Add(bizObj1);

			var bizObj2 = Factory.New<DummyBusinessObject>();
			bizObj2.Z0_Description = new ZString("Description");
			bizObj2.Z0_Date = ZDateTime.Empty;
			collection.Add(bizObj2);

			BusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);
			var dummyGridColumn = new DummyGridColumnWithCustomValueCommentAndColor();

			var exportColumns = new List<ExcelExportColumnBase>();
			var testColumn = ExcelExportCustomValueColumn.New(dummyGridColumn, dummyGridColumn);
			exportColumns.Add(testColumn);
			var testColumn2 = ExcelExportColumn.New(DummyBizoSchema.Z0_Description, "TestColumn", dummyGridColumn);
			exportColumns.Add(testColumn2);

			Factory.Save(); // save the dummies created in Setup()

			exporterWithComment = new ExcelExporter(reader, exportColumns, null);
			AssertNotNull("Exporter should not be null", exporterWithComment);

			try
			{
				exporterWithComment.ExportIntoAndOpenExcel();

				using (var excelInterface = ExcelInterfaceFactory.New())
				{
					excelInterface.LoadExcelFile(exporterWithComment.LastExportedFileNameForTest);
					using (var workSheet = excelInterface.WorkSheets[0])
					{
						CombineAssertions(delegate
						{
							AssertEquals("Should be three rows", 3, workSheet.RowCount); // header + 2 bizO
							AssertEquals("Should be two columns", 2, workSheet.ColumnCount); // 2 Column
							AssertEquals("First Column Name", "Test Description", workSheet[0, 0]);
							AssertEquals("Second Column Name", "TestColumn", workSheet[0, 1]);

							AssertEquals("First Column First Row expected value", new ZDateTime(1983, 7, 10), DateTime.FromOADate((double)workSheet[1, 0]));
							AssertEquals("Second Column First Row expected value", "", workSheet[1, 1]);
							AssertEquals("First Column Second Row expected value", "Description", workSheet[2, 0]);
							AssertEquals("Second Column Second Row expected value", "Description", workSheet[2, 1]);

							AssertEquals("First Column First Row Format", "dd-MMM-yy", workSheet.GetCellFormat(1, 0).FormatPattern);
							AssertEquals("Second Column First Row Format", "", workSheet.GetCellFormat(1, 1).FormatPattern);
							AssertEquals("First Column Second Row Format", "", workSheet.GetCellFormat(2, 0).FormatPattern);
							AssertEquals("Second Column Second Row Format", "", workSheet.GetCellFormat(2, 1).FormatPattern);

							AssertEquals("First Column First Row Color", System.Drawing.Color.Blue.ToArgb(), workSheet.GetCellFormat(1, 0).BackgroundColor.ToArgb());
							AssertEquals("Second Column First Row Color", System.Drawing.Color.Blue.ToArgb(), workSheet.GetCellFormat(1, 1).BackgroundColor.ToArgb());
							AssertEquals("First Column Second Row Color", System.Drawing.Color.Green.ToArgb(), workSheet.GetCellFormat(2, 0).BackgroundColor.ToArgb());
							AssertEquals("Second Column Second Row Color", System.Drawing.Color.Green.ToArgb(), workSheet.GetCellFormat(2, 1).BackgroundColor.ToArgb());

							AssertEquals("First Column First Row Expected Comment", new ZDateTime(1983, 7, 10).ToString(), workSheet.GetComment(1, 0));
							AssertEquals("Second Column First Row Expected Comment", new ZDateTime(1983, 7, 10).ToString(), workSheet.GetComment(1, 1));
							AssertEquals("First Column Second Row Expected Comment", "Description", workSheet.GetComment(2, 0));
							AssertEquals("Second Column Second Row Expected Comment", "Description", workSheet.GetComment(2, 1));
						});
					}
				}
			}
			finally
			{
				if (!exporterWithComment.LastExportedFileNameForTest.IsEmpty && File.Exists(exporterWithComment.LastExportedFileNameForTest))
				{
					File.Delete(exporterWithComment.LastExportedFileNameForTest);
				}
			}
		}

		public void TestExportInAnExternallyIntroducedFileAndSheet()
		{
			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				excelInterface.NewExcelFile(1);

				exporter = new ExcelExporter(dummyChildCollection, excelExportColumnCollection, notifications);

				using (var workSheet = excelInterface.WorkSheets[0])
				{
					exporter.ExportIntoExcel(excelInterface, workSheet);
					AssertExportedExcelFileContent(workSheet);
				}
			}
		}

		#endregion

		#region Export with non-visible columns
		SchemaColumn GetSchemaColumn(BusinessObject bizo, string name)
		{
			return new SchemaStringColumn(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizo.TableName), name, 99, System.Data.SqlDbType.VarChar, ZString.Empty, true, 100);
		}

		public void TestImportsColumnnsThatOnlyVisibleOnTheGrid()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var bizObj = Factory.New<DummyBusinessObject>();
			collection.Add(bizObj);

			BusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);

			using (var zGrid = new ZGrid())
			{
				var gridColumns = new ZGridColumns(zGrid);
				gridColumns.AddTextColumn("Visible", 100, true, true);
				gridColumns.AddTextColumn("Visible1", 100, true, false);
				gridColumns.AddTextColumn("NotVisible", 100, false, true);
				gridColumns.AddTextColumn("NotVisible1", 100, false, false);

				var exportColumns = new List<ExcelExportColumnBase>();
				exportColumns.AddRange(excelExportColumnCollection);
				exporter = new ExcelExporter(reader, exportColumns, null);
				AssertNotNull("Exporter should not be null", exporter);
				exporter.ExportVisibleIntoAndOpenExcel(null, gridColumns);
				exporter.CancelExport();
				gridColumns.DisposeAllColumns();
			}
		}

		public void TestExporterWithNonExportableColumnsExportsCorrectDateColumnFormats()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var bizObj = Factory.New<DummyBusinessObject>();
			collection.Add(bizObj);

			BusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);

			using (var zGrid = new ZGrid())
			{
				var gridColumns = new ZGridColumns(zGrid);
				gridColumns.AddTextColumn("NotVisible", 100, false, false);
				for (var i = 1; i <= DummyBizoSchema.All.Count; i++)
				{
					gridColumns.AddTextColumn("VisibleBeforeDateColumn" + i, 100, true, true);
				}
				gridColumns.AddDateColumn("Bad Constant Value instead of Good Date", 100, true, true, ZDateTimePickerFormat.Long);
				gridColumns.AddTextColumn("LastColumn", 100, true, true);

				var exportColumn = Excel.ExcelExportColumn.New(GetSchemaColumn(bizObj, "blah"));
				exportColumn.PropertyDescriptor = new ConstantValuePropertyDescriptor(null, "blah", typeof(DummyBusinessObject), new ZString("this is not a date"));

				var exportColumns = new List<ExcelExportColumnBase>();
				exportColumns.AddRange(excelExportColumnCollection);
				exportColumns.Add(exportColumn);
				exportColumns.Add(Excel.ExcelExportColumn.New(GetSchemaColumn(bizObj, "blah2")));
				exportColumns.Add(Excel.ExcelExportColumn.New(GetSchemaColumn(bizObj, "blah3")));
				exportColumns.Add(Excel.ExcelExportColumn.New(GetSchemaColumn(bizObj, "blah4")));
				exportColumns.Add(Excel.ExcelExportColumn.New(GetSchemaColumn(bizObj, "blah5")));
				exporter = new ExcelExporter(reader, exportColumns, null);
				AssertNotNull("Exporter should not be null", exporter);
				exporter.ExportIntoAndOpenExcel(null, gridColumns);
				exporter.CancelExport();
				gridColumns.DisposeAllColumns();
			}
		}
		#endregion

		#region Saving to stream

		public void TestSaveToStream()
		{
			TestSaveToStream(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
		}

		public void TestSaveToStream_Xlsx()
		{
			TestSaveToStream(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xlsx);
		}

		public void TestSaveToStream(string format)
		{
			SetupDefaultFormat(format);
			Factory.Save(); // save the dummies created in Setup()

			var reader = new FilteredBusinessObjectReader(new ZQuery(DummyBizoSchema.Z0_Code, "smurf"), typeof(DummyBusinessObject));
			exporter = new ExcelExporter(reader, notifications);
			exporter.ExportIntoAndOpenExcel();

			using (var fs = new FileStream(exporter.LastExportedFileNameForTest, FileMode.Open))
			{
				var ms = new MemoryStream();
				exporter.SaveToStream(ms);
				if (format == Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls)
				{
					AssertEquals("Length should be the same", ms.Length, fs.Length);
				}
				else
				{
					AssertGreaterThan("Length should be smaller using xlsx", ms.Length, fs.Length);
				}
			}
		}

		public void TestSaveToStreamWithCouldNotGetValueForExportException()
		{
			TestSaveToStreamWithCouldNotGetValueForExportException(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
		}

		public void TestSaveToStreamWithCouldNotGetValueForExportException_Xlsx()
		{
			TestSaveToStreamWithCouldNotGetValueForExportException(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xlsx);
		}

		void TestSaveToStreamWithCouldNotGetValueForExportException(string format)
		{
			SetupDefaultFormat(format);
			Factory.Save(); // save the dummies created in Setup()

			var bizObj = Factory.New<DummyBusinessObject>();

			var exportColumns = new List<ExcelExportColumnBase>();
			exportColumns.AddRange(excelExportColumnCollection);

			var exportColumn = ExcelExportColumn.New(GetSchemaColumn(bizObj, "I_Dont_Exist"));
			exportColumns.Add(exportColumn);

			var collection = new DummyBusinessObjectCollection(Factory) { bizObj };
			var reader = new CollectionWrapperBusinessObjectReader(collection);

			exporter = new ExcelExporter(reader, exportColumns, null);
			var ms2 = new MemoryStream();
			AssertExceptionThrown(typeof(CouldNotGetValueForExportException), () => exporter.SaveToStream(ms2));
			AssertEquals("The respons should be Empty", ms2.Length, 0);

			exporter = new ExcelExporter(reader, exportColumns, notifications) { AllowMissingColumns = true };
			exporter.ExportIntoAndOpenExcel();

			using (var fs = new FileStream(exporter.LastExportedFileNameForTest, FileMode.Open))
			{
				var ms = new MemoryStream();
				AssertNoExceptionThrown(() => exporter.SaveToStream(ms));
				if (format == Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls)
				{
					AssertEquals("Length should be the same", ms.Length, fs.Length);
				}
				else
				{
					AssertGreaterThan("Length should be smaller using xlsx", ms.Length, fs.Length);
				}
			}
		}

		#endregion

		#region Testing Export Events

		public void TestExportEventsAreFired()
		{
			exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
			exporter.ExportStarting += new ExcelExporter.ExportStartingEventHandler(Exporter_ExportStarting);
			exporter.RecordExported += new ExcelExporter.RecordExportedEventHandler(Exporter_RecordExported);
			exporter.ExportFinished += new ExcelExporter.ExportFinishedEventHandler(Exporter_ExportFinished);

			exporter.ExportIntoAndOpenExcel();

			AssertEquals("Exporter.ExportStarting(3) event should have fired.", 3, exporter_ExportStarting_RecordsToExport);
			AssertEquals("Exporter.RecordExported(int RecordNumber) event should have fired 3 times.", 3, exporter_ExportFinishedFiredCount);
			AssertEquals("Exporter.ExportFinished() event should have fired.", true, exporter_ExportFinishedFired);
		}

		void Exporter_ExportStarting(int recordsToExport)
		{
			exporter_ExportStarting_RecordsToExport = recordsToExport;
		}

		void Exporter_RecordExported(int recordNumber)
		{
			exporter_ExportFinishedFiredCount++;
			AssertEquals("Exporter.RecordExported(int RecordNumber) event fired but the Record number is incorrect", exporter_ExportFinishedFiredCount, recordNumber);
		}

		void Exporter_ExportFinished()
		{
			exporter_ExportFinishedFired = true;
		}

		int exporter_ExportStarting_RecordsToExport;
		int exporter_ExportFinishedFiredCount;
		bool exporter_ExportFinishedFired;

		#endregion

		#region Testing Notifications

		[ExpectNoExceptions]
		public void TestShowNoRecordsToExportError_WhenNoRecords()
		{
			dummyChildCollection.RemoveAndDeleteAll();
			exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
			exporter.ExportIntoAndOpenExcel();
			notificationsMock.Verify(o => o.ShowNoRecordsToExportError(It.IsAny<string>()), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestShowNoRecordsToExportError_WhenRecordsExist()
		{
			exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
			exporter.ExportIntoAndOpenExcel();
			notificationsMock.Verify(o => o.ShowNoRecordsToExportError(It.IsAny<string>()), Times.Never());
		}

		[ExpectNoExceptions]
		public void TestShowTruncatedCellsMessage_WithoutTruncation()
		{
			exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
			exporter.ExportIntoAndOpenExcel();
			notificationsMock.Verify(o => o.ShowTruncatedCellsMessage(It.IsAny<string>()), Times.Never());
		}

		[ExpectNoExceptions]
		public void TestShowTruncatedCellsMessage_WithTruncation()
		{
			var bigDummy = DummyChildBusinessObject.New(Dummy.Factory);
			bigDummy.Z0_NVarCharMax = new ZString('*', ExcelExporter.MaxStringLengthInCellSupportedByExcel + 1);
			dummyChildCollection.Add(bigDummy);

			exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
			exporter.ExportIntoAndOpenExcel();
			notificationsMock.Verify(o => o.ShowTruncatedCellsMessage(It.IsAny<string>()), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestShowMaxRecordsSupportedByExcel97_2003ExceededError_WhenRecordMaxExceeded()
		{
			try
			{
				Env.Excel.MaxRowCountSupported97_2003 = 2;
				SetupDefaultFormat(Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls);
				notificationsMock.Setup(o => o.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(It.IsAny<string>())).Returns(DialogResult.Yes).Verifiable();
				exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify();
			}
			finally
			{
				Env.Excel.Reset();
			}
		}

		[ExpectNoExceptions]
		public void TestShowMaxRecordsSupportedByExcel2007ExceededError_WhenRecordMaxExceeded()
		{
			try
			{
				Env.Excel.MaxRowCountSupported2007 = 2;
				exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify(o => o.ShowMaxEntriesSupportedByExcelExceededError(It.IsAny<string>()), Times.Once());
			}
			finally
			{
				Env.Excel.Reset();
			}
		}

		[ExpectNoExceptions]
		public void TestShowMaxRecordsSupportedByExcel97_2003ExceededError_WhenRecordMaxNotExceeded()
		{
			try
			{
				Env.Excel.MaxRowCountSupported97_2003 = 10;
				exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify(o => o.ShowMaxEntriesSupportedByExcelExceededError(It.IsAny<string>()), Times.Never());
			}
			finally
			{
				Env.Excel.Reset();
			}
		}

		[ExpectNoExceptions]
		public void TestShowMaxRecordsSupportedByExcel2007ExceededError_WhenRecordMaxNotExceeded()
		{
			try
			{
				Env.Excel.MaxRowCountSupported2007 = 10;
				exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify(o => o.ShowMaxEntriesSupportedByExcelExceededError(It.IsAny<string>()), Times.Never());
			}
			finally
			{
				Env.Excel.Reset();
			}
		}

		[ExpectNoExceptions]
		public void TestNotifyExportingLotsOfRecords_WhenLotsOfRecords()
		{
			var initialValue = Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids;

			try
			{
				Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids = 2;
				exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify(o => o.NotifyExportingLotsOfRecords(It.IsAny<ExcelExporter>()), Times.Once());
			}
			finally
			{
				Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids = initialValue;
			}
		}

		[ExpectNoExceptions]
		public void TestNotifyExportingLotsOfRecords_WhenFewRecords()
		{
			var initialValue = Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids;

			try
			{
				Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids = 10;
				exporter = new ExcelExporter(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);
				exporter.ExportIntoAndOpenExcel();
				notificationsMock.Verify(o => o.NotifyExportingLotsOfRecords(It.IsAny<ExcelExporter>()), Times.Never());
			}
			finally
			{
				Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids = initialValue;
			}
		}

		#endregion

		#region Test Cancel Export

		public void TestCancelExport()
		{
			var cancelledExporter = new ExcelExporterWithCancelledExport(dummyChildCollection, ExcelExportColumnsForDummyBizObj, notifications);

			var onRecordExportedCalled = false;
			var onExportFinishedCalled = false;

			cancelledExporter.RecordExported += delegate
			{
				onRecordExportedCalled = true;
			};

			cancelledExporter.ExportFinished += delegate
			{
				onExportFinishedCalled = true;
			};

			AssertEquals("OnRecordExported should not be called.", false, onRecordExportedCalled);
			AssertEquals("OnExportFinished should not be called.", false, onExportFinishedCalled);
			AssertEquals("No file should be created because the Export was cancelled.", true, cancelledExporter.LastExportedFileNameForTest.IsEmpty);
		}

		public class ExcelExporterWithCancelledExport : ExcelExporter
		{
			public ExcelExporterWithCancelledExport(IBusinessObjectCollection collectionToExport, List<ExcelExportColumnBase> excelColumns, IExcelExporterNotifications notifications)
				: base(collectionToExport, excelColumns, notifications)
			{
			}

			protected override void PopulateExcelWorkSheet(IExcelInterface excelInterface, IExcelWorkSheet sheet, CurrencyManager source, ZGridColumns columns)
			{
				CancelExport();
				base.PopulateExcelWorkSheet(excelInterface, sheet, source, columns);
			}
		}

		#endregion

		public void TestFilteredBatchSizeIsSetTo100()
		{
			var reader = new FilteredBusinessObjectReader(new ZQuery(), typeof(DummyBusinessObject));
			reader.BatchSize = 10;
			AssertEquals("Precondition - Reader.BatchSize is 10.", 10, reader.BatchSize);

			exporter = new ExcelExporter(reader, notifications);
			AssertEquals("Reader.BatchSize should be set to 100.", 100, reader.BatchSize);
		}

		#region TestFetchHints

		public void TestFetchHints()
		{
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var collection = new DummyWithRefCollection(Factory);
			for (var i = 0; i < 5; i++)
			{
				var d2 = factory2.New<DummyBusinessObject>();
				d2.Z0_Code = "D2" + i;

				var d1 = collection.AddNew();
				d1.Z0_Guid = d2.PK;
				d1.Z0_Code = "D1" + i;
			}

			factory2.Save();

			using (var testForm = new ZForm())
			{
				var grid = new ZGrid();
				testForm.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Guid });

				grid.SetDataBinding(collection, "");

				Factory.ClearLoadedFetchHintCountForTable(DummyBizoSchema.Constants.TableName);

				using (Db.Connection.TrackExecutedCommands())
				{
					grid.ExportIntoAndOpenExcel();
					if (!string.IsNullOrEmpty(ExcelExporter.LastExportedFileNameStaticForTest) && File.Exists(ExcelExporter.LastExportedFileNameStaticForTest))
					{
						File.Delete(ExcelExporter.LastExportedFileNameStaticForTest);
					}

					var commands = Db.Connection.ExecutedCommands.Where(c => c.Contains(DummyBizoSchema.Constants.TableName));

					AssertEquals(1, commands.Count());
				}
			}
		}

		class DummyWithRef : DummyBusinessObject
		{
			public DummyWithRef(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("Refs")]
			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set { base.Z0_Guid = value; }
			}
			public DummyBusinessObjectCollection Refs
			{
				get
				{
					if (refs == null)
					{
						refs = new DummyBusinessObjectCollection(Factory);
						RegisterEditableChildObject(refs);
					}
					return refs;
				}
			}
			DummyBusinessObjectCollection refs;
		}

		class DummyWithRefCollection : BusinessObjectCollection<DummyWithRef>
		{
			public DummyWithRefCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			dummyChildCollection = Dummy.Collection;

			var dummy1 = DummyChildBusinessObject.New(Dummy.Factory);
			dummy1.Z0_AnotherDate = new ZDateTime(1979, 5, 6);
			dummy1.Z0_DateOnly = new ZDate(1979, 5, 6);
			dummy1.Z0_DateTimeOffset = new ZDateTimeOffset(1979, 5, 7, 1, 2, 3, TimeSpan.FromHours(11));
			dummy1.Z0_Code = "smurf";
			dummy1.Z0_AnotherDecimal = 10.1m;

			var dummy2 = DummyChildBusinessObject.New(Dummy.Factory);
			dummy2.Z0_AnotherDate = ZDateTime.Empty;
			dummy2.Z0_DateOnly = ZDate.Empty;
			dummy2.Z0_DateTimeOffset = ZDateTimeOffset.Empty;
			dummy2.Z0_Code = "";
			dummy2.Z0_AnotherDecimal = 10.2m;

			var dummy3 = DummyChildBusinessObject.New(Dummy.Factory);
			dummy3.Z0_AnotherDate = new ZDateTime(1978, 2, 10);
			dummy3.Z0_DateOnly = new ZDate(1978, 2, 10);
			dummy3.Z0_DateTimeOffset = new ZDateTimeOffset(1978, 2, 11, 4, 5, 6, TimeSpan.FromHours(-11));
			dummy3.Z0_Code = "jokey";
			dummy3.Z0_AnotherDecimal = 10.3m;

			dummyChildCollection.Add(dummy1);
			dummyChildCollection.Add(dummy2);
			dummyChildCollection.Add(dummy3);

			notificationsMock = new Mock<IExcelExporterNotifications>();
			notifications = notificationsMock.Object;

			excelExportColumnCollection = new List<ExcelExportColumnBase>();
			foreach (var schemaColumn in DummyBizoSchema.All)
			{
				if (!schemaColumn.IsPKColumn && !schemaColumn.LightValidationIsValidColumn)
				{
					excelExportColumnCollection.Add(ExcelExportColumn.New(schemaColumn));
				}
			}
			anotherDateIndex = IndexOfSchemaColumn(excelExportColumnCollection, DummyBizoSchema.Z0_AnotherDate);
			dateOnlyIndex = IndexOfSchemaColumn(excelExportColumnCollection, DummyBizoSchema.Z0_DateOnly);
			codeIndex = IndexOfSchemaColumn(excelExportColumnCollection, DummyBizoSchema.Z0_Code);
			anotherDecimalIndex = IndexOfSchemaColumn(excelExportColumnCollection, DummyBizoSchema.Z0_AnotherDecimal);
			nvarcharmaxIndex = IndexOfSchemaColumn(excelExportColumnCollection, DummyBizoSchema.Z0_NVarCharMax);
			dateTimeOffsetIndex = IndexOfSchemaColumn(excelExportColumnCollection, DummyBizoSchema.Z0_DateTimeOffset);
		}

		List<ExcelExportColumnBase> excelExportColumnCollection;
		int anotherDateIndex;
		int dateOnlyIndex;
		int codeIndex;
		int anotherDecimalIndex;
		int nvarcharmaxIndex;
		int dateTimeOffsetIndex;

		protected override void TearDown()
		{
			if (exporter != null && !exporter.LastExportedFileNameForTest.IsEmpty && File.Exists(exporter.LastExportedFileNameForTest))
			{
				File.Delete(exporter.LastExportedFileNameForTest);
			}

			base.TearDown();
		}

		List<ExcelExportColumnBase> ExcelExportColumnsForDummyBizObj
		{
			get
			{
				if (excelExportColumnsForDummyBizObj == null)
				{
					excelExportColumnsForDummyBizObj = new List<ExcelExportColumnBase>();
					foreach (var schemaColumn in DummyBizoSchema.All)
					{
						if (!schemaColumn.IsPKColumn && schemaColumn != DummyBizoSchema.Z0_IsValid)
						{
							excelExportColumnsForDummyBizObj.Add(ExcelExportColumn.New(schemaColumn));
						}
					}
				}
				return excelExportColumnsForDummyBizObj;
			}
		}
		List<ExcelExportColumnBase> excelExportColumnsForDummyBizObj;

		#region Test Excel column limit

		List<ExcelExportColumnBase> ExcelExportColumnsThatHitsExcel97_2003ColumnLimit
		{
			get { return excelExportColumnsThatHitsExcel97_2003ColumnLimit ?? (excelExportColumnsThatHitsExcel97_2003ColumnLimit = GetExcelExportColumnsList(Env.Excel.MaxColCountSupported97_2003 + 1)); }
		}
		List<ExcelExportColumnBase> excelExportColumnsThatHitsExcel97_2003ColumnLimit;

		List<ExcelExportColumnBase> ExcelExportColumnsThatHitsExcel2007ColumnLimit
		{
			get { return excelExportColumnsThatHitsExcel2007ColumnLimit ?? (excelExportColumnsThatHitsExcel2007ColumnLimit = GetExcelExportColumnsList(Env.Excel.MaxColCountSupported2007 + 1)); }
		}
		List<ExcelExportColumnBase> excelExportColumnsThatHitsExcel2007ColumnLimit;

		List<ExcelExportColumnBase> GetExcelExportColumnsList(int nbColumns)
		{
			var list = new List<ExcelExportColumnBase>();
			for (var i = 0; i < nbColumns; i++)
			{
				list.Add(ExcelExportColumn.New(DummyBizoSchema.Z0_Code));
			}
			return list;
		}

		#endregion

		#region Test Excel row limit

		DummyChildBusinessObjectCollection DummyChildCollectionThatHitsExcel97_2003RowLimit
		{
			get { return dummyChildCollectionThatHitsExcel97_2003RowLimit ?? (dummyChildCollectionThatHitsExcel97_2003RowLimit = GetBigDummyChildCollection(Env.Excel.MaxRowCountSupported97_2003 + 1)); }
		}
		DummyChildBusinessObjectCollection dummyChildCollectionThatHitsExcel97_2003RowLimit;

		DummyChildBusinessObjectCollection DummyChildCollectionThatHitsExcel2007RowLimit
		{
			get { return dummyChildCollectionThatHitsExcel2007RowLimit ?? (dummyChildCollectionThatHitsExcel2007RowLimit = GetBigDummyChildCollection(Env.Excel.MaxRowCountSupported2007 + 1)); }
		}
		DummyChildBusinessObjectCollection dummyChildCollectionThatHitsExcel2007RowLimit;

		DummyChildBusinessObjectCollection GetBigDummyChildCollection(int nbEntries)
		{
			var collection = Dummy.Collection;
			for (var i = 0; i < nbEntries; i++)
			{
				var dummy = DummyChildBusinessObject.New(Dummy.Factory);
				collection.Add(dummy);
			}
			return collection;
		}

		#endregion

		Mock<IExcelExporterNotifications> notificationsMock;
		IExcelExporterNotifications notifications;
		ExcelExporter exporter;
		ExcelExporter exporterWithComment;

		DummyChildBusinessObjectCollection dummyChildCollection;

		internal class DummyGridColumnWithCustomValueCommentAndColor : IExcelExportCustomValue, IExcelExportCellColor, IExcelExportCellComment
		{
			#region IExcelExportCustomValue Members

			public IZType GetCustomValue(BusinessObject bizObj)
			{
				IZType result;
				var dummyBizObj = bizObj as DummyBusinessObject;
				if (dummyBizObj != null)
				{
					if (!dummyBizObj.Z0_Date.IsEmpty)
					{
						result = dummyBizObj.Z0_Date;
					}
					else
					{
						result = dummyBizObj.Z0_Description;
					}
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}

			public ZString GetValueFormat(IZType value)
			{
				return value is ZDateTime ? (ZString)ZDateTime.ShortDateFormat : ZString.Empty;
			}

			public ZString GetDescription()
			{
				return "Test Description";
			}

			#endregion

			#region IExcelExportCellComment Members

			public ZString GetComment(BusinessObject bizObj)
			{
				ZString result;
				var dummyBizObj = bizObj as DummyBusinessObject;
				if (dummyBizObj != null)
				{
					if (!dummyBizObj.Z0_Date.IsEmpty)
					{
						result = dummyBizObj.Z0_Date.ToString();
					}
					else
					{
						result = dummyBizObj.Z0_Description.ToString();
					}
				}
				else
				{
					result = ZString.Empty;
				}

				return result;
			}

			#endregion

			#region IExcelExportCellColor Members

			public System.Drawing.Color? GetCustomColor(BusinessObject bizObj)
			{
				var result = System.Drawing.Color.Red;
				var dummyBizObj = bizObj as DummyBusinessObject;
				if (dummyBizObj != null)
				{
					result = dummyBizObj.Z0_Date.IsEmpty ? System.Drawing.Color.Green : System.Drawing.Color.Blue;
				}
				return result;
			}

			#endregion
		}

		#endregion
	}
}
