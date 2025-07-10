using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using DocumentEngineConstants = Enterprise.DocumentEngine.Constants;
using Res = ReportWriter.Res;

namespace Enterprise.ReportWriter
{
	public partial class ReportWriterForm : ZChildForm
	{
		public ReportWriterForm()
		{
		}

		public ReportWriterForm(ExcelTemplate template)
			: this(MainBizObj.New(template))
		{
			xlsInterface = new ExcelInterface();
			xlsInterface.LoadExcelFile(template.GetAsTemplateStream());
			PopulateMainBizObjFromTemplate();
		}

		public ReportWriterForm(MainBizObj bizObj)
			: base(bizObj)
		{
		}

		public static ReportWriterForm NewFromTemplate()
		{
			var localEnterprisePath = BuildConstants.GetLocalEnterprisePath(Directory.GetCurrentDirectory(), false);
			if (Directory.Exists(localEnterprisePath))
			{
				localEnterprisePath = Path.Combine(localEnterprisePath, @"Enterprise\Product\Documents\ExcelTemplates");
			}
			else
			{
				localEnterprisePath = "";
			}
			return new ReportWriterForm(new ExcelTemplateReadFromByteArray(Constants.DefaultTemplate.TemplateName, localEnterprisePath, typeof(ReportWriterForm).Assembly.GetManifestResourceStream(Constants.DefaultTemplate.ReportTemplateResources).CopyToByteArray()));
		}

		public new MainBizObj BusinessEntity
		{
			get { return (MainBizObj)base.BusinessEntity; }
		}

		void PopulateMainBizObjFromTemplate()
		{
			var mainBizObj = BusinessEntity;
			if (mainBizObj != null)
			{
				for (int sheetNumber = 0; sheetNumber < XlsInterface.WorkSheets.Count; sheetNumber++)
				{
					var workSheet = XlsInterface.WorkSheets[sheetNumber];
					var sheetName = workSheet.SheetName;
					if (Report.IsSortSheetName(sheetName))
					{
					}
					else if (Report.IsGroupBySheetName(sheetName))
					{
					}
					else if (Report.IsConstantsSheetName(sheetName))
					{
					}
					else if (Report.IsFilterSheetName(sheetName))
					{
					}
					else if (Report.IsTranslationsSheetName(sheetName))
					{
					}
					else if (Report.IsFieldSheetName(sheetName))
					{
					}
					else if (Report.IsFlexCelScaleSheetName(sheetName))
					{
					}
					else if (Report.IsOptionalTemplateSheetName(sheetName))
					{
					}
					else
					{
						ReportBizObj reportBizObj = null;
						if (WorksheepMapping.TryGetValue(workSheet, out reportBizObj))
						{
							if (!mainBizObj.ReportBizObjs.Contains(reportBizObj))
							{
								mainBizObj.ReportBizObjs.Add(reportBizObj);
							}
						}
						else
						{
							reportBizObj = mainBizObj.ReportBizObjs[sheetName];
							if (reportBizObj == null)
							{
								reportBizObj = mainBizObj.ReportBizObjs.AddNew();
							}
							WorksheepMapping.Add(workSheet, reportBizObj);
						}
						reportBizObj.PopulateFrom(workSheet);
					}
				}
			}
		}

		Dictionary<ExcelWorkSheet, ReportBizObj> WorksheepMapping
		{
			get { return worksheepMapping ?? (worksheepMapping = new Dictionary<ExcelWorkSheet, ReportBizObj>()); }
		}
		Dictionary<ExcelWorkSheet, ReportBizObj> worksheepMapping;

		ExcelInterface XlsInterface
		{
			get { return xlsInterface ?? (xlsInterface = new ExcelInterface()); }
		}
		ExcelInterface xlsInterface;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
					components = null;
				}
				if (xlsInterface != null)
				{
					xlsInterface.Dispose();
					xlsInterface = null;
				}
				if (currentArea != null)
				{
					UnHookAreaEvents(currentArea);
					currentArea = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ChangeAreaVisibility();
		}

		void SaveToFileButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZSaveFileDialog())
			{
				if (BusinessEntity.Filename.IsEmpty)
				{
					dialog.DefaultExt = AttachmentTypeList.Codes.Xls;
				}
				else
				{
					dialog.FileName = BusinessEntity.Filename;
				}
				dialog.FilterIndex = 2;
				dialog.Filter = ExcelFileFilter;
				dialog.RestoreDirectory = true;
				dialog.OverwritePrompt = true;

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					using (var excelStream = dialog.OpenFile())
					{
						var existingTemplateSheets = XlsInterface.WorkSheets.Where(x => Report.IsTemplateSheet(x.SheetName)).ToList();
						foreach (ReportBizObj reportBizObj in BusinessEntity.ReportBizObjs)
						{
							CreateOrUpdateWorksheet(reportBizObj, existingTemplateSheets);
						}
						existingTemplateSheets.ForEach(x =>
						{
							XlsInterface.WorkSheets.Remove(x);
							if (worksheepMapping != null)
							{
								worksheepMapping.Remove(x);
							}
							x.Dispose();
						});
						XlsInterface.SaveToStream(excelStream);
					}
					BusinessEntity.Filename = dialog.UnmappedFileName;
					Globals.Message.Show(Res.GetString("{37B94C08-125E-4AE1-ABFF-D8A435A50173}", "Data saved to:\r\n{0}", dialog.UnmappedFileName));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CreateOrUpdateWorksheet(ReportBizObj reportBizObj, List<ExcelWorkSheet> existingTemplateSheets)
		{
			try
			{
				currentReportBizObj = reportBizObj;
				var worksheet = worksheepMapping != null ? worksheepMapping.Where(x => x.Value == reportBizObj).Select(x => x.Key).FirstOrDefault() : null;
				if (worksheet == null)
				{
					worksheet = new ExcelWorkSheet(XlsInterface, WorksheetNameTextBox.Name);
					XlsInterface.WorkSheets.Insert(XlsInterface.WorkSheets.Count > 0 ? 1 : 0, worksheet);
					WorksheepMapping.Add(worksheet, reportBizObj);
				}
				XlsInterface.Xls.ActiveSheetByName = worksheet.SheetName;

				if (!worksheet[0, 0].ToString().Trim().Equals(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase)
					|| worksheet.GetLastRowOfTemplate() < 1)
				{
					worksheet.Clear();
					worksheet[0, 0] = Constants.AreaIdentifierTags.Config;
					worksheet[1, 0] = Constants.AreaIdentifierTags.EndOfReport;
				}

				existingTemplateSheets.Remove(worksheet);
				var lastRowOfTemplate = worksheet.GetLastRowOfTemplate();
				if (lastRowOfTemplate < XlsInterface.Xls.RowCount - 1)
				{
					worksheet.RemoveRows(lastRowOfTemplate + 1, XlsInterface.Xls.RowCount);
				}
				var newAreaRowNumber = CreateOrUpdateConfigArea(reportBizObj, worksheet);
				worksheet.RemoveRows(newAreaRowNumber, XlsInterface.Xls.RowCount);
				CreateDocumentHeader(reportBizObj, worksheet, XlsInterface.Xls.RowCount);
				CreateArea(reportBizObj, worksheet, XlsInterface.Xls.RowCount);
				worksheet[XlsInterface.Xls.RowCount, 0] = Constants.AreaIdentifierTags.EndOfReport;
			}
			finally
			{
				currentReportBizObj = null;
			}
		}

		void CreateArea(ReportBizObj reportBizObj, ExcelWorkSheet worksheet, int startRowNumber)
		{
			foreach (var area in reportBizObj.Areas.Cast<Area>().OrderBy(x => x.Order))
			{
				worksheet[startRowNumber++, 0] = area.GetConfigDetail();
				foreach (var row in area.Rows.Cast<RowData>().OrderBy(x => x.Order))
				{
					var rowNumber = startRowNumber++;
					worksheet[rowNumber, 0] = row.Expression;
					if (row.Height > 0)
					{
						worksheet.SetRowHeight(rowNumber, row.Height);
					}
					foreach (var column in row.Columns.Cast<ColumnData>().OrderBy(x => x.ColumnNumber))
					{
						worksheet[rowNumber, column.ColumnNumber] = column.Expression;
					}
				}
			}
		}

		void CreateDocumentHeader(ReportBizObj reportBizObj, ExcelWorkSheet worksheet, int startRowNumber)
		{
			worksheet[startRowNumber++, 0] = Constants.AreaIdentifierTags.DocumentHeader;
			foreach (var documentHeaderRow in reportBizObj.DocumentHeaderRows.Cast<DocumentHeaderRow>().OrderBy(x => x.Order))
			{
				var rowNumber = startRowNumber++;
				if (documentHeaderRow.Height > 0)
				{
					worksheet.SetRowHeight(rowNumber, documentHeaderRow.Height);
				}
				if (documentHeaderRow.IsCustomisedColumnRow)
				{
					var backgroundColor = System.Drawing.Color.FromArgb(documentHeaderRow.BackgroundColorInArgb);
					foreach (var columnHeading in reportBizObj.ColumnHeadings.Cast<ColumnHeading>().OrderBy(x => x.ColumnNumber))
					{
						var cell = worksheet.GetCell(rowNumber, columnHeading.ColumnNumber);
						cell.ValueSourceText = string.Format(Culture.Invariant, "<AutoHeight><CustomisedColumn({0})>", columnHeading.DisplayLabel);
						cell.Format.BackgroundColor = backgroundColor;
					}
				}
				else
				{
					worksheet[rowNumber, 0] = documentHeaderRow.Expression;
					foreach (var column in documentHeaderRow.Columns.Cast<ColumnData>().OrderBy(x => x.ColumnNumber))
					{
						worksheet[rowNumber, column.ColumnNumber] = column.Expression;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code")]
		int CreateOrUpdateConfigArea(ReportBizObj reportBizObj, ExcelWorkSheet worksheet)
		{
			worksheet[0, 0] = DocumentEngineConstants.AreaIdentifierTags.Config;
			var newAreaRowNumber = Math.Max(GetRowNumber(worksheet, worksheet.GetLastRowOfTemplate(), "#", 1), 1);
			int rowNumber;
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.AutoHeightModeSignature, out rowNumber, reportBizObj.IsExpandRow, () => DocumentEngineConstants.ConfigAreaParameters.AutoHeightModeSignature + Constants.ConfigArea.ExpandRow);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.DataContextSignature, out rowNumber, !reportBizObj.DataContext.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.DataContextSignature + reportBizObj.DataContext);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.DisableCSVExport, out rowNumber, reportBizObj.DisableCSVExport, () => DocumentEngineConstants.ConfigAreaParameters.DisableCSVExport);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.DisableXLSXExport, out rowNumber, reportBizObj.DisableXLSXExport, () => DocumentEngineConstants.ConfigAreaParameters.DisableXLSXExport);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.DocumentCurrency, out rowNumber, !reportBizObj.DocumentCurrency.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.DocumentCurrency + reportBizObj.DocumentCurrency);
			if (!reportBizObj.DocumentCurrency.IsEmpty && rowNumber > 0)
			{
				foreach (var columnNumber in reportBizObj.ApplyDocumentCurrencies.Cast<ColumnData>().Select(x => x.ColumnNumber).Where(x => x > ZInt.Zero).Distinct().OrderBy(x => x))
				{
					worksheet[rowNumber, columnNumber] = Constants.ConfigArea.ApplyDocumentCurrency;
				}
			}
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.EmailSubjectSignature, out rowNumber, !reportBizObj.EmailSubject.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.EmailSubjectSignature + reportBizObj.EmailSubject);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.ForceWebPublishSignature, out rowNumber, reportBizObj.ForceWebPublish, () => DocumentEngineConstants.ConfigAreaParameters.ForceWebPublishSignature);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.HideColumnIfSignature, out rowNumber, reportBizObj.HideColumns.Count > 0, () => DocumentEngineConstants.ConfigAreaParameters.HideColumnIfSignature);
			if (rowNumber > 0)
			{
				foreach (var hideColumn in reportBizObj.HideColumns.Cast<ColumnData>().Where(x => x.ColumnNumber > ZInt.Zero).OrderBy(x => x.ColumnNumber))
				{
					worksheet[rowNumber, hideColumn.ColumnNumber] = hideColumn.Expression;
				}
			}
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.HideSheetIfSignature, out rowNumber, !reportBizObj.HideSheetIf.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.HideSheetIfSignature + reportBizObj.HideSheetIf);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.PageStyleSignature, out rowNumber, !reportBizObj.PageStyle.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.PageStyleSignature + reportBizObj.PageStyle);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.SheetNameOverride, out rowNumber, !reportBizObj.SheetNameOverride.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.SheetNameOverride + reportBizObj.SheetNameOverride);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.SlowServers, out rowNumber, !reportBizObj.SlowServers.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.SlowServers + reportBizObj.SlowServers);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.SqlQueryHints, out rowNumber, !reportBizObj.SqlQueryHints.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.SqlQueryHints + reportBizObj.SqlQueryHints);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.SqlQueryHintsIgnoreRecompile, out rowNumber, reportBizObj.SqlQueryHintsIgnoreRecompile.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.SqlQueryHintsIgnoreRecompile);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.SqlTimeoutSignature, out rowNumber, !reportBizObj.SqlTimeout.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.SqlTimeoutSignature + reportBizObj.SqlTimeout.ToString());
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.SuppressDraftWatermark, out rowNumber, reportBizObj.SuppressDraftWatermark, () => DocumentEngineConstants.ConfigAreaParameters.SuppressDraftWatermark);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.TemplateNameSignature, out rowNumber, !reportBizObj.Name.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.TemplateNameSignature + reportBizObj.Name);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.Title, out rowNumber, !reportBizObj.Title.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.Title + reportBizObj.Title);
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.TrailingFormFeedSignature, out rowNumber, !reportBizObj.TrailingFormFeed.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.TrailingFormFeedSignature + reportBizObj.TrailingFormFeed.ToString());
			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.VersionSignature, out rowNumber, !reportBizObj.Version.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.VersionSignature + reportBizObj.Version.ToString());

			foreach (DataSource dataSource in reportBizObj.DataSources)
			{
				newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.DataSourceSignature + dataSource.Name + "=", out rowNumber, !dataSource.Name.IsEmpty || !dataSource.SQL.IsEmpty, () => DocumentEngineConstants.ConfigAreaParameters.DataSourceSignature + dataSource.Name + "=" + dataSource.SQL);
			}

			newAreaRowNumber = AddOrUpdateConfigDetail(newAreaRowNumber, worksheet, DocumentEngineConstants.ConfigAreaParameters.ColumnHeadingsSignature, out rowNumber, !reportBizObj.ColumnHeadingSavesTo.IsEmpty || reportBizObj.ColumnHeadings.Count > 0, () => DocumentEngineConstants.ConfigAreaParameters.ColumnHeadingsSignature + (reportBizObj.ColumnHeadingSavesTo.IsEmpty ? "" : ":" + Constants.ConfigArea.ColumnHeading.SavesTo + reportBizObj.ColumnHeadingSavesTo), true, PopulateColumnHeadings);
			return newAreaRowNumber;
		}

		ReportBizObj currentReportBizObj;

		void PopulateColumnHeadings(ExcelWorkSheet worksheet, int rowNumber)
		{
			if (currentReportBizObj != null)
			{
				foreach (var columnHeading in currentReportBizObj.ColumnHeadings.Cast<ColumnHeading>().Where(x => x.ColumnNumber > ZInt.Zero).OrderBy(x => x.ColumnNumber))
				{
					worksheet[rowNumber, columnHeading.ColumnNumber] = columnHeading.GetValue();
				}
			}
		}

		int AddOrUpdateConfigDetail(int newAreaRowNumber, ExcelWorkSheet worksheet, string cellDataStartWith, out int rowNumber, ZBool shouldPopulateData, Func<string> getData, bool clearExistingCell = true, Action<ExcelWorkSheet, int> additionalProcessing = null)
		{
			rowNumber = GetRowNumber(worksheet, newAreaRowNumber, cellDataStartWith);
			if (rowNumber == -1)
			{
				if (shouldPopulateData)
				{
					rowNumber = newAreaRowNumber++;
					worksheet.InsertRows(rowNumber, 1);
				}
			}
			else if (clearExistingCell)
			{
				var columnCount = worksheet.ColCountInRow(rowNumber);
				if (columnCount > 0)
				{
					for (int columnNumber = 0; columnNumber < columnCount; columnNumber++)
					{
						worksheet[rowNumber, columnNumber] = "";
						worksheet.SetCellFormat(rowNumber, columnNumber, new DocumentEngineIntegration.CellFormat());
					}
				}
			}

			if (rowNumber > 0)
			{
				if (shouldPopulateData)
				{
					worksheet[rowNumber, 0] = getData();
					if (additionalProcessing != null)
					{
						additionalProcessing(worksheet, rowNumber);
					}
				}
				else
				{
					worksheet.RemoveRow(rowNumber);
					newAreaRowNumber--;
				}
			}
			return newAreaRowNumber;
		}

		int GetRowNumber(ExcelWorkSheet workSheet, int maxRowCount, string cellDataStartWith, int startRowNumber = 1)
		{
			var result = -1;
			for (int rowNumber = startRowNumber; rowNumber <= maxRowCount; rowNumber++)
			{
				if (workSheet[rowNumber, 0].ToString().StartsWith(cellDataStartWith, StringComparison.OrdinalIgnoreCase))
				{
					result = rowNumber;
					break;
				}
			}
			return result;
		}

		static string ExcelFileFilter
		{
			get
			{
				return Res.GetString("855D14F2-BDB7-468B-A2D3-3906FF0DEC02", "Excel Templates (*.xls)|*.xls|All files (*.*)|*.*");
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void RegenerateButton_Click(object sender, EventArgs e)
		{
			foreach (ReportBizObj reportBizObj in BusinessEntity.ReportBizObjs)
			{
				reportBizObj.PopulateFromDataSource();
			}
		}

		void AreaGrid_AfterBind(object sender, EventArgs e)
		{
			AreaGrid.ListManager.PositionChanged += new EventHandler(AreaGridListManager_PositionChanged);
			AreaGridListManager_PositionChanged(null, null);
		}

		void AreaGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = AreaGrid.ListManager;
			var current = listManager != null ? (Area)listManager.GetCurrent() : null;
			if (current != null && current.IsDeleted)
			{
				current = null;
			}
			if (currentArea != current)
			{
				UnHookAreaEvents(currentArea);
				currentArea = current;
				HookAreaEvents(currentArea);
			}
		}
		Area currentArea;

		void HookAreaEvents(Area area)
		{
			if (area != null)
			{
				area.TypeInfo.ValueChanged += TypeInfo_ValueChanged;
			}
			TypeInfo_ValueChanged(null, null);
		}

		void UnHookAreaEvents(Area area)
		{
			if (area != null)
			{
				area.TypeInfo.ValueChanged -= TypeInfo_ValueChanged;
			}
		}

		void TypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ChangeAreaVisibility();
		}

		void ChangeAreaVisibility()
		{
			GroupByPanel.Visible = false;
			if (currentArea != null)
			{
				if (currentArea.Type == AreaTypeList.Codes.GroupBy)
				{
					GroupByPanel.Visible = true;
				}
				// TODO add other panel
			}
			AreaCommonPanel.Visible = !GroupByPanel.Visible;
		}

		void ReportBizObjsGrid_AfterBind(object sender, EventArgs e)
		{
			ReportBizObjsGrid.ListManager.PositionChanged += new EventHandler(ReportBizObjsGridListManager_PositionChanged);
			AreaGridListManager_PositionChanged(null, null);
		}

		void ReportBizObjsGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = ReportBizObjsGrid.ListManager;
			var current = listManager != null ? (ReportBizObj)listManager.GetCurrent() : null;
			if (current != null && current.IsDeleted)
			{
				current = null;
			}
			if (currentListManagerReportBizObj != current)
			{
				AreaGridListManager_PositionChanged(null, null);
				currentListManagerReportBizObj = current;
			}
		}
		ReportBizObj currentListManagerReportBizObj;

		void AreaGrid_VisibleChanged(object sender, EventArgs e)
		{
			currentListManagerReportBizObj?.RefreshRelatedDatasIfNeeded();
		}

		void HideColumnsGrid_VisibleChanged(object sender, EventArgs e)
		{
			currentListManagerReportBizObj?.RefreshRelatedDatasIfNeeded();
		}

		void HeaderRowsGrid_VisibleChanged(object sender, EventArgs e)
		{
			currentListManagerReportBizObj?.RefreshRelatedDatasIfNeeded();
		}
	}
}
