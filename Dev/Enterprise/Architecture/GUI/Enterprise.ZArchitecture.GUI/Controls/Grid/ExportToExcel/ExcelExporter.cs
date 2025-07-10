using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Env = Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Excel
{
	public class ExcelExporter
	{
		public ExcelExporter(IBusinessObjectCollection collectionToExport, List<ExcelExportColumnBase> formattedColumnsToExport, IExcelExporterNotifications notifications)
			: this(new CollectionWrapperBusinessObjectReader(collectionToExport), formattedColumnsToExport, notifications)
		{
		}

		public ExcelExporter(BusinessObject[] arrayToExport, Type businessObjectType, List<ExcelExportColumnBase> formattedColumnsToExport, IExcelExporterNotifications notifications)
			: this(new ArrayBusinessObjectReader(arrayToExport, businessObjectType), formattedColumnsToExport, notifications)
		{
		}

		public ExcelExporter(BusinessObjectReader reader, IExcelExporterNotifications notifications)
		{
			this.Reader = reader;
			this.Notifications = notifications;

			extension = SystemDataRegistry.Instance.ExportToExcelFormat.Value == Enterprise.Core.Constants.ExportToExcelFormats.Code.Xls ? xlsExtension : xlsxExtension;

			SetFilteredReaderBatchSizeTo100();
		}

		public ExcelExporter(BusinessObjectReader reader, List<ExcelExportColumnBase> formattedColumnsToExport, IExcelExporterNotifications notifications)
			: this(reader, notifications)
		{
			excelExportColumns = formattedColumnsToExport;
		}

		public const int MaxStringLengthInCellSupportedByExcel = 32767;

		public bool AllowMissingColumns { get; set; }

		void SetFilteredReaderBatchSizeTo100()
		{
			var filteredReader = Reader as FilteredBusinessObjectReader;
			if (filteredReader != null)
			{
				filteredReader.BatchSize = 100;
			}
		}

		readonly BusinessObjectReader Reader;

		#region Export

		public void ExportVisibleIntoAndOpenExcel(CurrencyManager source = null, ZGridColumns columns = null)
		{
			var visibleCol = columns.Clone();
			try
			{
				foreach (var column in columns)
				{
					if (!column.IsVisible)
					{
						visibleCol.Remove(column.ColumnName);
					}
				}
			}
			finally
			{
				ExportIntoAndOpenExcel(source, visibleCol);
			}
		}

		public void ExportIntoExcel(IExcelInterface excelInterface, IExcelWorkSheet workSheet, CurrencyManager source = null, ZGridColumns columns = null)
		{
			ExportToExcelCore(() => Export(excelInterface, workSheet, source, columns, false));
		}

		public void ExportIntoAndOpenExcel(CurrencyManager source = null, ZGridColumns columns = null)
		{
			ExportToExcelCore(
				() =>
				{
					using (var excelInterface = ExcelInterfaceFactory.New())
					{
						excelInterface.NewExcelFile(SingleSheet);

						using (var workSheet = excelInterface.WorkSheets[0])
						{
							Export(excelInterface, workSheet, source, columns, true);
						}
					}
				}
			);
		}

		void ExportToExcelCore(Action exportAction)
		{
			IsExportCancelled = false;
			Reader.Factory.RefreshEnabled = false;
			if (!Reader.HasRecords)
			{
				ShowNoRecordsToExportError();
			}
			else if (Reader.ApproximateCount >= Env.Excel.MaxRowCountSupported2007)
			{
				ShowMaxRecordsSupportedByExcel2007ExceededError();
			}
			else if (extension == xlsExtension && Reader.ApproximateCount >= Env.Excel.MaxRowCountSupported97_2003)
			{
				var answer = ShowMaxRecordsSupportedByExcel97_2003ExceededErrorAndAskForFormatChange();
				if (answer == DialogResult.Yes)
				{
					extension = xlsxExtension;
					exportAction();
				}
			}
			else
			{
				exportAction();
			}
		}

#if DEBUG
		virtual
#endif
		public void CancelExport()
		{
			IsExportCancelled = true;
		}

		public bool IsExportCancelled { get; private set; }

		void Export(IExcelInterface excelInterface, IExcelWorkSheet workSheet, CurrencyManager source, ZGridColumns columns, bool showPreview)
		{
			if (Reader.ApproximateCount > MaxRecommendedNumberOfRecordsToExport)
			{
				NotifyExportingLotsOfRecords();
			}

			OnExportStarting();

			try
			{
				PopulateExcelWorkSheet(excelInterface, workSheet, source, columns);
			}
			catch (CouldNotGetValueForExportException ex)
			{
				CancelExport();

				Env.Globals.Message.Show(Res.GetString("E8448867-B250-4C0A-8189-219E081E51A5", "There was an error exporting the column for '{0}'. If you would like to export anyway please remove the column and try again", ex.ColumnName));
			}
			catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 8623)
			{
				CancelExport();

				Env.Globals.Message.ShowWarning(ZGUIConstants.GetQueryTooComplicatedError());
			}

			OnExportFinished();

			if (!IsExportCancelled)
			{
				if (truncatedCellsListMessage.Length > 0)
				{
					ShowTruncatedCellsMessage();
				}

				if (showPreview)
				{
					var fileName = GetNewRandomFileName();
#if DEBUG
					#region for Testing
					if (Env.Globals.IsTest)
					{
						LastExportedFileNameForTest = fileName;
						excelInterface.PreviewInXlWithoutDeletingFile(fileName);
						LastExportedColumnsForTest = columns;
					}
					else
					#endregion
#endif
					{
						excelInterface.PreviewInXl(fileName);
					}
				}
			}
		}

		const int SingleSheet = 1;
		const string xlsExtension = ".xls";
		const string xlsxExtension = ".xlsx";

		string extension = xlsExtension;
		int maxColumnsToExport;

		public void SaveToStream(Stream saveStream)
		{
			using (var excelInterface = ExcelInterfaceFactory.New())
			{
				excelInterface.NewExcelFile(SingleSheet);

				using (var workSheet = excelInterface.WorkSheets[0])
				{
					PopulateExcelWorkSheet(excelInterface, workSheet, null, null);
					excelInterface.SaveToStream(saveStream);
				}
			}
		}

		protected void PopulateHeadings(IExcelWorkSheet workSheet)
		{
			if (ExcelExportColumns.Count <= maxColumnsToExport)
			{
				maxColumnsToExport = ExcelExportColumns.Count;
			}
			for (var columnIndex = 0; columnIndex < maxColumnsToExport; columnIndex++)
			{
				var excelColumn = ExcelExportColumns[columnIndex];

				workSheet[0, columnIndex] = excelColumn.Description;
				workSheet.SetColWidth(columnIndex, excelColumn.Width);
				workSheet.SetCellFormat(0, columnIndex, HeadingCellFormat);
			}
		}

#if DEBUG
		virtual
#endif
		protected void PopulateExcelWorkSheet(IExcelInterface excelInterface, IExcelWorkSheet workSheet, CurrencyManager source, ZGridColumns columns)
		{
			if (columns != null)
			{
				if (extension == xlsExtension && columns.Count > Env.Excel.MaxColCountSupported97_2003)
				{
					var answer = ShowMaxColumnsSupportedByExcel97_2003ExceededErrorAndAskForFormatChange();
					if (answer == DialogResult.Yes)
					{
						extension = xlsxExtension;
					}
				}

				maxColumnsToExport = Math.Min(columns.Count, extension == xlsExtension ? Env.Excel.MaxColCountSupported97_2003 : Env.Excel.MaxColCountSupported2007);
			}
			else
			{
				if (extension == xlsExtension && ExcelExportColumns.Count > Env.Excel.MaxColCountSupported97_2003)
				{
					var answer = ShowMaxColumnsSupportedByExcel97_2003ExceededErrorAndAskForFormatChange();
					if (answer == DialogResult.Yes)
					{
						extension = xlsxExtension;
					}
				}

				maxColumnsToExport = Math.Min(ExcelExportColumns.Count, extension == xlsExtension ? Env.Excel.MaxColCountSupported97_2003 : Env.Excel.MaxColCountSupported2007);
			}

			PopulateHeadings(workSheet);

			ZGridColumns exportableColumns = null;
			if (columns != null)
			{
				exportableColumns = new ZGridColumns(columns.Grid);
				foreach (var column in columns)
				{
					exportableColumns.AddColumn(column);
				}
			}
			TableColumn[] tableColumns = null;
			var bizoIndex = 0;

			const int BatchSize = 100;

			var cache = new List<BusinessObject>(BatchSize);

			foreach (var bizObjInReader in Reader)
			{
				tableColumns = (tableColumns == null && bizObjInReader != null && exportableColumns != null) ?
					new TableColumnCalculator().GetTableColumnsOnThisObject(bizObjInReader, exportableColumns, true) : tableColumns;

				cache.Add(bizObjInReader);
				if (cache.Count >= BatchSize)
				{
					PopulateCellsBatchWithFetchHints(cache, excelInterface, workSheet, source, exportableColumns, ref bizoIndex, tableColumns);
				}
			}
			PopulateCellsBatchWithFetchHints(cache, excelInterface, workSheet, source, exportableColumns, ref bizoIndex, tableColumns);
		}

		void PopulateCellsBatchWithFetchHints(List<BusinessObject> cache, IExcelInterface excelInterface, IExcelWorkSheet workSheet, CurrencyManager source, ZGridColumns exportableColumns, ref int bizoIndex, TableColumn[] tableColumns)
		{
			if (tableColumns != null)
			{
				foreach (var bizObj in cache)
				{
					bizObj.FetchStrategy.FetchForView(tableColumns);
				}
			}
			foreach (var bizObj in cache)
			{
				if (IsExportCancelled)
				{
					break;
				}

				if (bizObj == null)
				{
					continue;
				}

				bizoIndex++;
				for (var columnIndex = 0; columnIndex < maxColumnsToExport; columnIndex++)
				{
					SetCellValueAndFormat(bizObj, excelInterface, workSheet, bizoIndex, columnIndex, source, exportableColumns);
				}
				OnRecordExported(bizoIndex);
			}
			cache.Clear();
		}

		void SetCellValueAndFormat(BusinessObject bizObj, IExcelInterface excelInterface, IExcelWorkSheet workSheet, int cellRow, int cellColumn, CurrencyManager source = null, ZGridColumns columns = null)
		{
			var excelColumn = ExcelExportColumns[cellColumn];

			ZGridColumnStyle style = null;

			if (columns != null)
			{
				style = columns[cellColumn].ColumnStyle as ZGridColumnStyle;
			}

			IZType cellValue = null;
			try
			{
				cellValue = excelColumn.GetValueForExport(bizObj, style);
			}
			catch (CouldNotGetValueForExportException)
			{
				if (!AllowMissingColumns)
				{
					throw;
				}

				cellValue = ZString.Empty;
			}

			if (cellValue is ZString)
			{
				var valueAsZString = (ZString)cellValue;
				if (valueAsZString.Length > ExcelExporter.MaxStringLengthInCellSupportedByExcel)
				{
					cellValue = valueAsZString.SubstringSafe(0, ExcelExporter.MaxStringLengthInCellSupportedByExcel);
					truncatedCellsListMessage.AppendLine(Res.GetString("928e5b06-6a9c-42d8-a889-88eee326ebc4", "- Cell {0}, starting with '{1}'", excelInterface.GetCellReference(cellRow, cellColumn), valueAsZString.Left(50) + "..."));
				}
			}

			workSheet[cellRow, cellColumn] = cellValue;

			if (!(cellValue is ZString) || (cellValue is ZString && !((ZString)cellValue).StartsWith(Enterprise.URLHandler.EdiUrlPrefix.Value, StringComparison.OrdinalIgnoreCase)))
			{
				var cellFormat = excelColumn.GetFormat(cellValue);

				var backGroundColor = excelColumn.GetColor(bizObj);
				if (backGroundColor.HasValue)
				{
					cellFormat.BackgroundColor = backGroundColor.Value;
					cellFormat.FillPattern = extension == xlsExtension ? FillPatternStyle.Automatic : FillPatternStyle.Solid;
				}
				else
				{
					cellFormat.FillPattern = FillPatternStyle.None;
				}

				workSheet.SetCellFormat(cellRow, cellColumn, cellFormat);
				workSheet.SetComment(cellRow, cellColumn + 1, excelColumn.GetComment(bizObj));
			}
		}

		readonly StringBuilder truncatedCellsListMessage = new StringBuilder();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File name cannot be translated")]
		public string GetNewRandomFileName()
		{
			return Path.ChangeExtension(Path.Combine(Env.EnvProxy.Instance.TempPath, "CargoWise Export - " + ZDateTime.Now.ToString("yyyyMMddHHmmss") + " - " + new Random().Next(0, 1000)), extension);
		}

		int MaxRecommendedNumberOfRecordsToExport
		{
			get { return Env.EnvProxy.Instance.Registry.MaxRecommendedNumberOfRecordsToShowInDisplayGrids; }
		}

		CellFormat HeadingCellFormat
		{
			get
			{
				if (headingCellFormat == null)
				{
					headingCellFormat = new CellFormat();
					headingCellFormat.SetFontParametersFrom(Enterprise.ZArchitecture.Core.OFont.GetFontBold());
				}

				return headingCellFormat;
			}
		}
		CellFormat headingCellFormat;

		#region Export Events

		public delegate void ExportStartingEventHandler(int recordsToExport);
		public delegate void ExportFinishedEventHandler();
		public delegate void RecordExportedEventHandler(int recordNumber);

		public event ExportStartingEventHandler ExportStarting;
		public event ExportFinishedEventHandler ExportFinished;
		public event RecordExportedEventHandler RecordExported;

		protected void OnExportStarting()
		{
			if (ExportStarting != null)
			{
				ExportStarting(Reader.ApproximateCount);
			}
		}

		protected void OnExportFinished()
		{
			if (ExportFinished != null)
			{
				ExportFinished();
			}
		}

		protected void OnRecordExported(int recordNumber)
		{
			if (RecordExported != null)
			{
				RecordExported(recordNumber);
			}
		}

		#endregion

		#region for Testing
#if DEBUG
		public static ZString LastExportedFileNameStaticForTest { get; private set; }
		internal ZString LastExportedFileNameForTest
		{
			get
			{
				return lastExportedFileNameForTest;
			}
			set
			{
				lastExportedFileNameForTest = value;
				LastExportedFileNameStaticForTest = value;
			}
		}
		ZString lastExportedFileNameForTest;

		public ZGridColumns LastExportedColumnsForTest { get; private set; }
#endif
		#endregion

		#endregion

		#region Notifications

		public static class ExportMessages
		{
			public static string NoRecordsToExport
			{
				get { return Res.GetString("b0145298-b194-4eb1-9b08-7132610dad03", "There are no records to export."); }
			}

			public static string TooManyRecordsToExport97_2003
			{
				get
				{
					return Res.GetString("8209a85c-c973-46ea-957d-1ada746bd663", @"Exporting more than 65,536 records is not supported by Microsoft Excel 97-2003.
Would you like to export using Excel 2007 XLSX format, which will raise the row limit to 1,048,575 ? If not, the export will be canceled.");
				}
			}

			public static string TooManyColumnsToExport97_2003
			{
				get
				{
					return Res.GetString("86018dc1-50fa-4db3-8e8f-4c462d544012", @"Exporting more than 256 columns is not supported by Microsoft Excel 97-2003.
Would you like to export using Excel 2007 XLSX format, which will raise the column limit to 16,384 ? If not, the export will only display the first 256 columns.");
				}
			}

			public static string TooManyRecordsToExport2007
			{
				get { return Res.GetString("985aac35-7a58-4774-99e7-78b8c5361c22", "Exporting more than 1,048,576 records is not supported by Microsoft Excel 2007."); }
			}

			public static string TooManyColumnsToExport2007
			{
				get { return Res.GetString("476980b7-2c95-49ca-bde2-884899a6daf8", "Exporting more than 16,384 columns is not supported by Microsoft Excel 2007."); }
			}

			public static string TruncatedCellsBaseMessage
			{
				get
				{
					return Res.GetString("09aab42b-736c-439e-b5f7-13d503c69c5c",
						@"The following cells have been truncated because they exceed the maximum cell length of {0} characters supported by Excel:", ExcelExporter.MaxStringLengthInCellSupportedByExcel);
				}
			}
		}

		void ShowNoRecordsToExportError()
		{
			Notifications.ShowNoRecordsToExportError(ExportMessages.NoRecordsToExport);
		}

		void ShowTruncatedCellsMessage()
		{
			Notifications.ShowTruncatedCellsMessage(ExportMessages.TruncatedCellsBaseMessage + System.Environment.NewLine + this.truncatedCellsListMessage.ToString());
		}

		#region Excel 97-2003

		DialogResult ShowMaxRecordsSupportedByExcel97_2003ExceededErrorAndAskForFormatChange()
		{
			return Notifications.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(ExportMessages.TooManyRecordsToExport97_2003);
		}

		DialogResult ShowMaxColumnsSupportedByExcel97_2003ExceededErrorAndAskForFormatChange()
		{
			return Notifications.ShowMaxEntriesSupportedByCurrentExcelExceededErrorAndAskForFormatChange(ExportMessages.TooManyColumnsToExport97_2003);
		}

		#endregion

		#region Excel 2007

		void ShowMaxRecordsSupportedByExcel2007ExceededError()
		{
			Notifications.ShowMaxEntriesSupportedByExcelExceededError(ExportMessages.TooManyRecordsToExport2007);
		}

		#endregion

		void NotifyExportingLotsOfRecords()
		{
			Notifications.NotifyExportingLotsOfRecords(this);
		}

		readonly IExcelExporterNotifications Notifications;

		#endregion

		#region Export Columns

		List<ExcelExportColumnBase> ExcelExportColumns
		{
			get
			{
				if (excelExportColumns == null)
				{
					excelExportColumns = GetSchemaColumns()
						.Cast<SchemaColumn>()
						.Where(schemaColumn => !schemaColumn.IsPKColumn && !schemaColumn.LightValidationIsValidColumn)
						.Select(schemaColumn => ExcelExportColumn.New(schemaColumn))
						.ToList<ExcelExportColumnBase>();
				}

				return excelExportColumns;
			}
		}
		List<ExcelExportColumnBase> excelExportColumns;

		SchemaColumnCollection GetSchemaColumns()
		{
			var tableName = BusinessObjectFactory.GetTableNameFromType(Reader.BusinessObjectType);
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(tableName);
		}

		#endregion
	}
}
