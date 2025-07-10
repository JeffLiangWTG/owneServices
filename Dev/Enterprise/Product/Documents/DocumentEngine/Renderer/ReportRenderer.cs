using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine
{
	class ReportRenderer : IReportRenderer
	{
		public ReportRenderer(Report report)
		{
			this.report = report;
		}

		readonly Report report;
		readonly List<Area> backPageAreasToKeep = new List<Area>();
		readonly PageCollection pages = new PageCollection();
		readonly List<int> originalColumnWidths = new List<int>();
		readonly List<int> hiddenColumns = new List<int>();

		readonly List<string> templatesWithNoImagesToRemove = new List<string>() { (NoResString)"TLU Bill Of Lading TLU" };

		public PageCollection Pages { get { return pages; } }

		public bool ExpandRowsForAutoHeight { get { return report.ExpandRowsForAutoHeight; } }

		public Area CurrentAreaToProcess { get; set; }
		public Passes CurrentPass { get; set; }
		public int CurrentRow { get; private set; }
		public int CurrentColumn { get; set; }

		public int CurrentDataRow => CurrentAreaToProcess is SectionBodyArea bodyArea ? bodyArea.GetRowIndex(CurrentRow) : 0;

		public IEnumerable<int> OriginalColumnWidths { get { return originalColumnWidths; } }
		public IEnumerable<int> HiddenColumns { get { return hiddenColumns; } }

		public bool IsProcessingMacros { get; private set; }
		public bool IsProcessingPageBreaks { get; private set; }
		public bool IsProcessingAutoShapes { get; private set; }

		public bool IsCurrentSectionBodyAreaWithMoreThanOneRowData => CurrentAreaToProcess is SectionBodyArea bodyArea && !(bodyArea.DataRowSource is DataProviders.OneRowDataSource);

		public bool ReplaceTotalMacroInFormulaCell => true;

		public void Render()
		{
			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			using (Culture.SetTemporarily(GetTemporaryRenderCulture()))
			{
				report.RenderedLanguage = report.Language;
				BeginRender();
			}
		}

		void BeginRender()
		{
			report.PrepareForRender();

			using (Env.Licence.UseLanguageLicense(report.Language, LanguageUsageType.DocBuilder))
			using (Res.TemporarilySwitchLanguage(report.Language))
			{
				try
				{
#if DEBUG
					if (!report.FilterValidationCheckingSuspendedForTesting)
#endif
					{
						report.ValidateFilters();
					}

					if (report.ErrorManager.HasErrors && !report.ErrorManager.HasWarningsOnly)
					{
						return;
					}

					try
					{
						if (!report.IsDisabledOptionalTemplateSheet(report.WorkSheetCurrentlyBeingProcessed.SheetName))
						{
							RenderCore();
						}
					}
					catch (FlexCelXlsAdapterException exception)
					{
						if (exception.ErrorCode == XlsErr.ErrTooManyRows || exception.ErrorCode == XlsErr.ErrTooManyEntries)
						{
							throw new DocumentEngineTooManyRowsException();
						}
						else if (exception.ErrorCode == XlsErr.ErrTooManyColumns)
						{
							throw new DocumentEngineTooManyColumnsException();
						}

						throw;
					}
				}
				finally
				{
					report.ResetIsPreparedForRender();
				}
			}
		}

		internal void RenderCore()
		{
			try
			{
				var pageBreaker = new PageBreakProcessor(report, Pages);

				backPageAreasToKeep.Clear();
				Pages.Clear();
				Pages.DefaultPageHeight = report.Analyser.PageHeight;

				if ((report.Analyser.Config.PageStyle == PageStyles.Continuous) && (report.GroupByCollection.Count != 0))
				{
					UpdatePageBreaksOnGroupByAreas();
				}

				ProcessGroupBys();
				HideConditionalColumns();
				SaveOriginalColumnWidths();
				RearrangeColumns();
				TranslateAreas();
				ProcessMergedCells();
				PopulateRows();
				report.TranslateColumnHeadingMacrosIfNeeded();
				report.MacroTranslator.ResetProviders();
				report.SetStartTime();
				report.SetParametersOnDocDataProvider();
				ProcessMacros(Passes.FirstPass);
				EvaluateHideRowIfMacrosForSectionPageHeaders();

				if (AreaSizesAreValid())
				{
					using (TrackProcessingPageBreaks())
					{
						ProcessPageBreaks(pageBreaker);
					}
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("817867ba-f114-4e3c-a7bb-6717c3b7cbdd", "Some header/footers are too big."), Res.GetString("976b2f7c-1b34-49e2-8da2-8bf2c2ab8462", "Could not process page breaks."));
				}

				ProcessMacros(Passes.SecondPass);
				using (TrackProcessingAutoShapes())
				{
					ProcessMacrosInAutoShapes(report.WorkSheetCurrentlyBeingProcessed);
				}

				SetPageOrientation();
				UpdateSheetNameOverride();

				RemoveSpecifiedImages();
				CleanUp(pageBreaker);

				ProcessExcessivePageBreaks();

				if (!pageBreaker.IsFooterFitOnPage)
				{
					report.ErrorManager.Add(
						new ReportProcessingError(
							Res.GetString("7f6c7e44-d7e1-4e83-8739-c21bb3e9dfde", "The footers of this report do not fit. Some pages may not be rendered correctly."),
							ReportProcessingErrorSeverity.Warning));
				}

				if (report.OverflowNoteDocument.OverflowNotes.Count > 0)
				{
					TransferFollowPages(report);
				}

				report.WorkSheetCurrentlyBeingProcessed.IsRendered = true;

				SetPageLayoutIfNeeded();
			}
			catch (NullReferenceException)
			{
				if (report.IsDisposed && report.Analyser == null)
				{
					return;
				}

				throw;
			}
		}

		void ProcessMergedCells()
		{
			var xls = report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.Xls;
			var mergedCells = new List<TXlsCellRange>();

			for (int i = 1; i <= xls.CellMergedListCount; i++)
			{
				mergedCells.Add(xls.CellMergedList(i));
			}

			report.Analyser.Areas.OfType<SectionBodyArea>().ForEach(body =>
			{
				var mergedCellsInCurrentArea = mergedCells.Distinct().Where(m => (m.Top - 1 >= body.StartOfBody && m.Top - 1 <= body.End) || (m.Bottom - 1 >= body.StartOfBody && m.Bottom - 1 <= body.End)).ToList();
				mergedCellsInCurrentArea.ForEach(m =>
				{
					var keyIndex = m.Top - body.StartOfBody - 1;
					var keyColumn = m.Left - 1;
					var value = 0;
					for (int i = m.Left; i <= m.Right; i++)
					{
						value += report.WorkSheetCurrentlyBeingProcessed.GetColWidth(i - 1);
					}

					body.AutoHeightColumnsWidthCache.Add((keyIndex, keyColumn), value);

					if (m.Top != m.Bottom)
					{
						body.ContainsMergedCellWithNextRow = true;
					}
				});
			});
		}

		void SetPageLayoutIfNeeded()
		{
			var customPageWidthInTenthsOfMillimeters = report.Analyser.Config.CustomPageWidthInMillimeters * 10;
			var customPageHeightInTenthsOfMillimeters = report.Analyser.Config.CustomPageHeightInMillimeters * 10;

			if (report.Analyser.Config.PageStyle == PageStyles.Custom && customPageWidthInTenthsOfMillimeters > 0 && customPageHeightInTenthsOfMillimeters > 0)
			{
				report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.SetPageLayout(customPageWidthInTenthsOfMillimeters, customPageHeightInTenthsOfMillimeters);
			}
			else if (report.Analyser.Config.PageStyle == PageStyles.Continuous && customPageWidthInTenthsOfMillimeters > 0 && report.Analyser.Config.AutoPageHeight)
			{
				var heightInAllPages = Pages.Sum(p => p.Areas.Height) / FlxConsts.RowMult;
				var realHeightInTenthsOfMm = (heightInAllPages * 2.54 / 96) * 100; // 96 is based on Flexcel Image DPI.
				report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.SetPageLayout(customPageWidthInTenthsOfMillimeters, (int)Utilities.Round(Convert.ToDecimal(realHeightInTenthsOfMm) + 100, 0));
			}
		}

		#region Methods to handle excessive HPageBreak

		void ProcessExcessivePageBreaks()
		{
			if (report.WorkSheetCurrentlyBeingProcessed.HasTooManyHPageBreak)
			{
				var currentWorksheet = report.WorkSheetCurrentlyBeingProcessed;
				var excelInterface = report.XlInterface;
				var initialSheetName = excelInterface.Xls.ActiveSheetByName;

				var originalName = new ZString(currentWorksheet.SheetName).Left(30); //This name can be up to 31 characters long. Need to reserve 1 char for nameIndex
				var originalWorkSheetDisabled = report.IsDisabledOptionalTemplateSheet(originalName);

				int pageBreakOverflowRow = currentWorksheet.GetFirstPageBreakOverflowRow();
				int nameIndex = 2;

				while (pageBreakOverflowRow != -1)
				{
					var newName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", originalName, nameIndex);

					while (excelInterface.WorkSheets[newName] != null)
					{
						nameIndex++;
						originalName = originalName.Left(31 - GetNameIndexLength(nameIndex));
						newName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", originalName, nameIndex);
					}

					var newSheet = excelInterface.CopySheet(currentWorksheet, newName);
					report.SetSheetVisibility(newName, originalWorkSheetDisabled);

					newSheet.RemoveRows(0, pageBreakOverflowRow + 1);
					ReplicatePrintTitles(currentWorksheet, newSheet);
					currentWorksheet.RemoveRows(pageBreakOverflowRow, currentWorksheet.RowCount - 1);

					newSheet.IsRendered = true;
					currentWorksheet = newSheet;

					pageBreakOverflowRow = currentWorksheet.GetFirstPageBreakOverflowRow();

					nameIndex++;
					originalName = originalName.Left(31 - GetNameIndexLength(nameIndex));
				}

				//We've inserted new sheets in between other sheets, so we need to reset calculated worksheet numbers
				report.XlInterface.WorkSheets.ForEach(worksheet => worksheet.ResetWorksheetNumber());

				excelInterface.Xls.ActiveSheetByName = initialSheetName;
			}
		}

		void ReplicatePrintTitles(ExcelWorkSheet source, ExcelWorkSheet destination)
		{
			var sourcePrintTitlesRange = report.XlInterface.GetPrintTitlesRange(source.WorkSheetNumber - 1);
			if (sourcePrintTitlesRange != null)
			{
				var rangeFormula = ExcelInterface.GetRangeFormulaWithNewSheetName(sourcePrintTitlesRange.RangeFormula, source.SheetName, destination.SheetName);
				var repeatingRange = ExcelInterface.GetFirstAndLastRowsToRepeat(rangeFormula);
				if (0 < repeatingRange.Item1 && repeatingRange.Item1 <= repeatingRange.Item2)
				{
					destination.CopyAndInsertRows(source, repeatingRange.Item1 - 1, repeatingRange.Item2 - repeatingRange.Item1 + 1, 0);
				}
				var destinationPrintTitlesRange = new TXlsNamedRange(TXlsNamedRange.GetInternalName(InternalNameRange.Print_Titles), destination.WorkSheetNumber, 0, rangeFormula);
				report.XlInterface.SetPrintTitlesRange(destinationPrintTitlesRange);
				destination.DeleteHPageBreak(sourcePrintTitlesRange.RowCount - 1);
			}
		}

		int GetNameIndexLength(int i)
		{
			return i < 10 ? 1 : (int)Math.Floor(Math.Log10(i) + 1);
		}

		#endregion

		void EvaluateHideRowIfMacrosForSectionPageHeaders()
		{
			foreach (var area in report.Analyser.Areas)
			{
				var sectionPageHeader = area as SectionPageHeaderArea;
				if (sectionPageHeader != null)
				{
					var workSheet = report.WorkSheetCurrentlyBeingProcessed;

					for (var row = sectionPageHeader.StartOfBody; row <= sectionPageHeader.End; row++)
					{
						for (var column = 0; column < workSheet.ColumnCount; column++)
						{
							var cell = workSheet.GetCell(row, column);

							if (HideRowIf.RegexToFindMacroAnyWhereInString.IsMatch(cell.ValueSourceText) || HideRowIfCellIsEmpty.RegexToFindMacroAnyWhereInString.IsMatch(cell.ValueSourceText))
							{
								var replacer = new CellContentReplacer(report, row, column);
								replacer.ReplaceMacros();

								var value = replacer.Content;
								if (value is RowHider)
								{
									cell.Value = value;
								}
							}
						}
					}
				}
			}
		}

		void SetPageOrientation()
		{
			if (report.Analyser.Config.PageStyle == PageStyles.Continuous && !report.Orientation.IsEmpty)
			{
				if (report.Orientation == ReportOrientationTypeList.Codes.Portrait)
				{
					report.XlInterface.SetOrientation(Orientation.Portrait);
				}
				else if (report.Orientation == ReportOrientationTypeList.Codes.Landscape)
				{
					report.XlInterface.SetOrientation(Orientation.Landscape);
				}
			}
			else if (report.Analyser.Config.HasPageStyleSignature && report.StTemplate != null && !report.StTemplate.IsDocBuilderStyle)
			{
				if (report.Analyser.Config.PageStyle == PageStyles.Portrait)
				{
					report.XlInterface.SetOrientation(Orientation.Portrait);
				}
				else if (report.Analyser.Config.PageStyle == PageStyles.Landscape)
				{
					report.XlInterface.SetOrientation(Orientation.Landscape);
				}
			}
		}

		internal void UpdateSheetNameOverride()
		{
			if (!string.IsNullOrEmpty(report.Analyser.Config.SheetNameOverride))
			{
				string sheetNameOverride = report.Analyser.Config.ReplaceMacros(report.Analyser.Config.SheetNameOverride).ToString();
				report.WorkSheetCurrentlyBeingProcessed.SheetNameOverride = sheetNameOverride;
			}
			else if (report.StTemplate != null && report.StTemplate.IsDocBuilderStyle)
			{
				report.WorkSheetCurrentlyBeingProcessed.SheetNameOverride = report.Name;
			}
		}

		bool TemplateHasNoImages
		{
			get
			{
				return templatesWithNoImagesToRemove.Contains(report.Template.TemplateName);
			}
		}

		void RemoveSpecifiedImages()
		{
			if (TemplateHasNoImages)
			{
				return;
			}

			if (report.BODocDataProvider != null)
			{
				foreach (string imageName in report.BODocDataProvider.ImageNamesToRemove)
				{
					try
					{
						report.WorkSheetCurrentlyBeingProcessed.RemoveAllObjectsByName(imageName);
					}
					catch (ExcelInterfaceException ex)
					{
						if (ex.Type == ExcelInterfaceExceptionType.ErrorRemovingObject)
						{
							string error = Res.GetString("b9df684d-94cf-4160-ace6-613920d4458b", "Couldn't remove image [{0}] as directed by the [{1}] property on {2}: [{3}]. {4}"
								, imageName
								, "ImageNamesToRemove"
								, "DataSource"
								, report.BODocDataProvider.GetType().Name
								, ex.Message);

							ReportProcessingErrorSeverity errorSeverity = report.StTemplate != null && !report.StTemplate.SO_IsClientSpecific && report.StTemplate.SO_IsSystemDefined
								? ReportProcessingErrorSeverity.WarningWithoutErrorReport
								: ReportProcessingErrorSeverity.Warning;

							report.ErrorManager.Add(new ReportProcessingError(error, errorSeverity, ex));
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		void CleanUp(PageBreakProcessor pageBreaker)
		{
			PutPageBreaks();
			CleanUpExtraLines(pageBreaker);
			report.XlInterface.Xls.DocumentProperties.RemoveAllProperties();
		}

		void CleanUpExtraLines(PageBreakProcessor pageBreaker)
		{
			if (report.Analyser.BackPage != null)
			{
				report.DocumentRendererLogger.Log(Res.GetString("e2b594db-7725-4c50-b649-b88779598ba4", "Inserting horizontal page break."));
				report.WorkSheetCurrentlyBeingProcessed.InsertHPageBreak(report.Analyser.BackPage.StartOfBody - 1);
			}

			for (var areaIndex = report.Analyser.Areas.Count - 1; areaIndex > 0; areaIndex--)
			{
				var area = report.Analyser.Areas[areaIndex];

				if (NeedToRemoveArea(area) || (area is BackPageArea && !backPageAreasToKeep.Contains(area)))
				{
					RemoveRows(area.StartingRow, area.End + 1);
				}
				else
				{
					if (pageBreaker.PageFooterAreasToKeep.Contains(area))
					{
						report.WorkSheetCurrentlyBeingProcessed[area.StartingRow, 0] = "";
					}
					else
					{
						if (area is SectionBodyArea)
						{
							for (var i = area.End; i > area.StartingRow; i--)
							{
								var identifier = report.WorkSheetCurrentlyBeingProcessed[i, 0].ToString().Trim();
								if (SectionForeachArea.IsSectionForeachAreaBeginStart(identifier) || SectionForeachArea.IsSectionForeachAreaEndStart(identifier))
								{
									RemoveRows(i, i + 1);
								}
							}
						}

						RemoveRows(area.StartingRow, area.StartOfBody);
					}
				}
			}

			report.WorkSheetCurrentlyBeingProcessed.RemoveRows(report.Analyser.Areas[0].StartingRow, report.Analyser.Areas[0].End + 1);
			report.WorkSheetCurrentlyBeingProcessed.HideColumn(0);
		}

		void PutPageBreaks()
		{
			for (int pageNumber = 0; pageNumber < Pages.Count; pageNumber++)
			{
				Page page = Pages[pageNumber];
				Page nextPage = null;

				if (pageNumber < Pages.Count - 1)
				{
					nextPage = Pages[pageNumber + 1];
				}

				if (page.Areas.Count > 0)
				{
					if (nextPage == null || (nextPage.Areas.Count > 0 && !(nextPage.Areas[0] is FooterArea)))
					{
						report.DocumentRendererLogger.Log(Res.GetString("e2b594db-7725-4c50-b649-b88779598ba4", "Inserting horizontal page break."));
						report.WorkSheetCurrentlyBeingProcessed.InsertHPageBreak(page.LastArea.End + 1);
					}
				}
			}
		}

		bool AreaSizesAreValid()
		{
			var result = true;

			if (report.Analyser.PageHeader != null && report.Analyser.PageHeader.HeightInXls > report.Analyser.PageHeight)
			{
				result = false;
			}

			return result;
		}

		int GetFooterSizeForArea(Area area)
		{
			return PageBreakProcessor.GetAvailableFooterAreas(report, Pages, area).Height;
		}

		internal int GetMaxAvailableHeightForArea(Area area, Page page)
		{
			return page.Height - GetFooterSizeForArea(area);
		}

		public int CurrentPageNumber
		{
			get { return CurrentAreaToProcess.RenderedToPageNumber; }
		}

		public int TotalNumberOfPages
		{
			get { return Pages.Count; }
		}

		void InsertBackPages()
		{
			var backPage = report.Analyser.BackPage;

			if (backPage != null)
			{
				for (int i = 0; i < Pages.Count; i++)
				{
					if (i == 0 || !backPage.FirstPageOnly)
					{
						var page = Pages[i];
						var lastAreaInPage = page.LastArea;
						if (lastAreaInPage == null)
						{
							backPage.RenderedToPageNumber = i + 1;
							backPageAreasToKeep.Add(backPage);
							continue;
						}
						if (lastAreaInPage.End + 1 != backPage.StartingRow)
						{
							var backPageInserted = backPage.Clone(lastAreaInPage.End + 1);
							backPageInserted.RenderedToPageNumber = lastAreaInPage.RenderedToPageNumber;
							report.WorkSheetCurrentlyBeingProcessed.DuplicateRows(backPage.StartingRow, backPage.End, backPageInserted.StartingRow, 1);

							var indexOfLastAreaInPage = report.Analyser.Areas.IndexOf(lastAreaInPage);
							for (int index = indexOfLastAreaInPage + 1; index < report.Analyser.Areas.Count; index++)
							{
								report.Analyser.Areas[index].Shift(backPageInserted.HeightInRowsIncludingStartingRow);
							}

							report.Analyser.Areas.Insert(indexOfLastAreaInPage + 1, backPageInserted);
							report.Analyser.UpdateSectionBodyAreaRowRangesInReport(backPageInserted.StartingRow - 1, backPageInserted.HeightInRowsIncludingStartingRow);
							report.DocumentRendererLogger.Log(Res.GetString("e2b594db-7725-4c50-b649-b88779598ba4", "Inserting horizontal page break."));
							report.WorkSheetCurrentlyBeingProcessed.InsertHPageBreak(backPageInserted.End + 1);

							backPageAreasToKeep.Add(backPageInserted);
						}
						else
						{
							report.Analyser.BackPage.RenderedToPageNumber = lastAreaInPage.RenderedToPageNumber;
							backPageAreasToKeep.Add(backPage);
						}
					}
				}

				report.DocumentRendererLogger.Log(Res.GetString("e2b594db-7725-4c50-b649-b88779598ba4", "Inserting horizontal page break."));
				report.WorkSheetCurrentlyBeingProcessed.InsertHPageBreak(report.Analyser.BackPage.End);
			}
		}

		void TransferFollowPages(Report report)
		{
			var overflowTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.OverflowNotesTemplate);

			var overflowReportName = Res.GetString("702dc036-7764-4460-a0f5-2653b9b4ae64", "Follow Page");
			using (var pack = new DocumentPack())
			using (var overflowReport = new Report(pack, overflowTemplate, report.OverflowNoteDocument, overflowReportName, null, Enterprise.DocumentEngineCore.DocumentSupport.DocumentDirection.ANY, false))
			using (var overflowReportStream = new MemoryStream())
			{
				overflowReport.Save(overflowReportStream);

				using (var overflowExcel = new ExcelInterface())
				{
					overflowExcel.LoadExcelFile(overflowReportStream);

					var overflowWorksheet = overflowExcel.WorkSheets[0];
					var copyRowCount = overflowWorksheet.RowCount;
					int insertWorksheetRow = report.WorkSheetCurrentlyBeingProcessed.RowCount;

					report.WorkSheetCurrentlyBeingProcessed.CopyAndInsertRows(overflowWorksheet, 0, copyRowCount, insertWorksheetRow);

					var followPages = overflowReport.Renderer.Pages;
					foreach (var page in followPages)
					{
						foreach (var area in page.Areas)
						{
							area.OverrideParentReportForNotesOverflowPage(report);
							area.Shift(insertWorksheetRow);
							area.RenderedToPageNumber = Pages.Count + 1;

							report.Analyser.Areas.Add(area);
						}

						Pages.Add(page);
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ProcessPageBreaks(PageBreakProcessor pageBreaker)
		{
			var lastAreaBrokeThePage = false;
			var firstBreakingGroupTitle = true;

			pageBreaker.OpenFirstPage();

			var isFirstPageFilled = false;
			var removeFirstPageIfNoData = report.Analyser.Config.RemoveFirstPageIfNoData;

			foreach (var section in report.Analyser.Sections)
			{
				if (section.SectionHeader != null)
				{
					if (section.SectionHeader.ShouldDelete)
					{
						continue;
					}

					if (section.SectionHeader.BreakPage)
					{
						if (removeFirstPageIfNoData && isFirstPageFilled || !removeFirstPageIfNoData)
						{
							pageBreaker.ClosePage();
							pageBreaker.OpenPage(section.SectionHeader);
						}
					}
				}

				pageBreaker.AddAreaToLastPage(section.SectionHeader);

				if (section.SectionHeader == null || report.Analyser.Config.PageStyle == PageStyles.Continuous)
				{
					pageBreaker.AddAreaToLastPage(section.SectionPageHeader);
				}
				else if (section.SectionPageHeader != null)
				{
					section.SectionPageHeader.ShouldDelete = true;
				}

				foreach (var areaToRender in section.ProcessedSectionBodyAndGroupByAreas)
				{
					var groupByArea = areaToRender as GroupByArea;

					if (groupByArea != null && groupByArea.GroupByPosition == GroupByArea.Position.Top)
					{
						if (groupByArea.BreakPage && !firstBreakingGroupTitle)
						{
							var ownerSection = groupByArea.OwnerSection;
							if (ownerSection != null)
							{
								var sectionBody = ownerSection.SectionBody;
								if (sectionBody != null)
								{
									lastAreaBrokeThePage = sectionBody.RowCount > 0 && Pages.LastPage.LastArea != null && Pages.LastPage.LastArea.End > Pages.LastPage.LastArea.StartingRow;
								}
							}
						}

						firstBreakingGroupTitle &= (section.SectionBody.DataRowSource.RowCount == 0);
					}

					if (lastAreaBrokeThePage)
					{
						pageBreaker.ClosePage();
						pageBreaker.OpenPage(areaToRender);
						lastAreaBrokeThePage = false;
					}

					pageBreaker.AddAreaToLastPage(areaToRender);

					if (groupByArea != null && groupByArea.GroupByPosition == GroupByArea.Position.Bottom && groupByArea.BreakPage)
					{
						lastAreaBrokeThePage = true;
					}

					areaToRender.ContainsMergedCellWithNextRow = null;
				}

				pageBreaker.AddAreaToLastPage(section.SectionFooter);

				if (section.SectionBody.RowCount > 0 && !isFirstPageFilled)
				{
					isFirstPageFilled = true;
				}
			}

			pageBreaker.CloseLastPage();

			InsertBackPages();
		}

		void PopulateRows()
		{
			using (ReportMutex reportMutex = new ReportMutex())
			{
				if (report.DataContextValue.DataContext == Enterprise.Core.Constants.DataContext.None && ObjectFactory.Get<ISystemDataRegistry>().ReportMaxConnections > 0 && !Globals.IsWeb)
				{
					try
					{
						if (reportMutex.Lock(report))
						{
							PopulateRowForAllSections();
						}
						else
						{
							throw new MaxConcurrentReportConnectionsExceeded(reportMutex.GetFormattedCurrentLocks(), report.ToString());
						}
					}
					finally
					{
						reportMutex.Unlock(report);
					}
				}
				else
				{
					PopulateRowForAllSections();
				}
			}
		}

		void PopulateRowForAllSections()
		{
			foreach (Section section in report.Analyser.Sections)
			{
				section.PopulateRows(report);
			}
		}

		bool AreaNeedsProcessing(Area area, Passes pass)
		{
			if (area is ConfigArea)
			{
				return false;
			}

			if ((area is SectionPageHeaderArea) && pass == Passes.FirstPass)
			{
				return false;
			}

			var pageHeaderArea = area as PageHeaderArea;
			if (pageHeaderArea != null && pageHeaderArea.StartFromSecondPage && pass == Passes.FirstPass)
			{
				return true;
			}

			if (pass == Passes.FirstPass && area is FooterArea)
			{
				return true;
			}

			if (NeedToRemoveArea(area))
			{
				return false;
			}

			return true;
		}

		bool NeedToRemoveArea(Area area)
		{
			return area.ShouldDelete;
		}

		void RemoveRows(int startingRow, int endingRow)
		{
			bool containesHPageBreak = false;
			for (int i = startingRow; i < endingRow; i++)
			{
				if (report.WorkSheetCurrentlyBeingProcessed.HasHPageBreak(i - 1))
				{
					containesHPageBreak = true;
					break;
				}
			}

			if (report.Analyser.Config.HideRowsInsteadOfRemove && report.WorkSheetCurrentlyBeingProcessed.RowCount < Excel.MaxRowCountSupported2007)
			{
				for (int i = startingRow; i < endingRow; i++)
				{
					report.WorkSheetCurrentlyBeingProcessed.SetRowHeight(i, 0);
				}
			}
			else
			{
				report.WorkSheetCurrentlyBeingProcessed.RemoveRows(startingRow, endingRow);
			}

			if (containesHPageBreak)
			{
				report.DocumentRendererLogger.Log(Res.GetString("e2b594db-7725-4c50-b649-b88779598ba4", "Inserting horizontal page break."));
				report.WorkSheetCurrentlyBeingProcessed.InsertHPageBreak(startingRow - 1);
			}
		}

		public void SaveOriginalColumnWidths()
		{
			originalColumnWidths.Clear();

			for (var column = 0; column < 256; column++)
			{
				originalColumnWidths.Add(report.WorkSheetCurrentlyBeingProcessed.GetColWidth(column));
			}
		}

		void HideConditionalColumns()
		{
			foreach (int columnIndex in report.Analyser.Config.HideColumnExpressions.Keys)
			{
				if (report.Analyser.ShouldHideConditionalColumn(columnIndex))
				{
					report.WorkSheetCurrentlyBeingProcessed.HideColumn(columnIndex);
					hiddenColumns.Add(columnIndex);
				}
			}
		}

		static void InsertionSort<T>(IList<T> list, Comparison<T> comparison)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			if (comparison == null)
			{
				throw new ArgumentNullException(nameof(comparison));
			}

			int count = list.Count;
			for (int j = 1; j < count; j++)
			{
				T key = list[j];

				int i = j - 1;
				for (; i >= 0 && comparison(list[i], key) > 0; i--)
				{
					list[i + 1] = list[i];
				}
				list[i + 1] = key;
			}
		}

		ColumnHeading[] GetSortedCopyOfHeadings()
		{
			ColumnHeading[] result = new ColumnHeading[report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.Count];
			result = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.Clone().ToArray();
			InsertionSort<ColumnHeading>(result, (x, y) => x.CurrentPosition.CompareTo(y.CurrentPosition));
			return result;
		}

		void RearrangeColumns()
		{
			if (!report.ColumnHeadingManager.IsEmpty)
			{
				if (report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Contains(report.WorkSheetCurrentlyBeingProcessed.SheetName)
					&& report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings.Count > 0)
				{
					var startingColumn = report.ColumnHeadingManager.CurrentConfiguration.Worksheets[report.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings[0].OriginalColumnNumber;
					var headings = GetSortedCopyOfHeadings();
					var multiplier = (float)ExcelMetrics.ColMult(report.XlInterface.Xls);

					var savedOriginalColumnNumbers = new int[headings.Length];
					for (var index = 0; index < headings.Length; index++)
					{
						savedOriginalColumnNumbers[index] = headings[index].OriginalColumnNumber;
					}

					for (var i = 0; i < report.Analyser.Config.ColumnsWithCurrencySetup.Count; i++)
					{
						var originalColumnNumberOfColumnWithCurrencySetup = report.Analyser.Config.ColumnsWithCurrencySetup[i];
						report.Analyser.Config.ColumnsWithCurrencySetup[i] = startingColumn + Array.IndexOf(savedOriginalColumnNumbers, originalColumnNumberOfColumnWithCurrencySetup);
					}

					var moveHeadings = new List<ColumnHeading>();
					foreach (var heading in headings)
					{
						if (heading.Hidden || ShouldHideHeading(heading))
						{
							if (heading.IsSafeToRemove)
							{
								report.WorkSheetCurrentlyBeingProcessed.DeleteColumn(heading.OriginalColumnNumber);

								foreach (var innerHeading in headings)
								{
									if (innerHeading.OriginalColumnNumber > heading.OriginalColumnNumber)
									{
										innerHeading.OriginalColumnNumber--;
									}
								}

								report.Analyser.Config.ColumnsWithCurrencySetup.Remove(heading.OriginalColumnNumber);
								for (var i = 0; i < report.Analyser.Config.ColumnsWithCurrencySetup.Count; i++)
								{
									if (report.Analyser.Config.ColumnsWithCurrencySetup[i] > heading.OriginalColumnNumber)
									{
										report.Analyser.Config.ColumnsWithCurrencySetup[i]--;
									}
								}
							}
							else
							{
								report.WorkSheetCurrentlyBeingProcessed.HideColumn(heading.OriginalColumnNumber);
							}
						}
						else
						{
							moveHeadings.Add(heading);
						}
					}

					var xls = report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.Xls;
					var mergedCells = new List<TXlsCellRange>();
					for (var i = 1; i <= xls.CellMergedListCount; i++)
					{
						mergedCells.Add(xls.CellMergedList(i));
					}

					var shouldRestoreMergedCells = false;
					foreach (var heading in moveHeadings)
					{
						var originalColumnNumber = heading.OriginalColumnNumber;
						var destinationColumnNumber = startingColumn + heading.CurrentPosition;
						if (!shouldRestoreMergedCells && originalColumnNumber != destinationColumnNumber)
						{
							shouldRestoreMergedCells = true;
						}

						report.WorkSheetCurrentlyBeingProcessed.MoveColumns(originalColumnNumber, destinationColumnNumber);

						UpdateOriginalColumnNumberForMovedColumns(headings, originalColumnNumber, destinationColumnNumber);

						var colWidth = heading.WidthInPixels * multiplier;
						var colWidthInt = colWidth >= int.MaxValue ? int.MaxValue : (int)colWidth;
						report.WorkSheetCurrentlyBeingProcessed.SetColWidth(destinationColumnNumber, colWidthInt);
					}

					if (shouldRestoreMergedCells)
					{
						mergedCells.ForEach(cellRange =>
						{
							xls.MergeCells(cellRange.Top, cellRange.Left, cellRange.Bottom, cellRange.Right);
						});
					}

					RecalculateOptionalNonExistantColumns(headings, startingColumn, savedOriginalColumnNumbers);
				}
			}
		}

		bool ShouldHideHeading(ColumnHeading heading)
		{
			return heading.Description.IsEmpty && heading.HideIfDescriptionEmpty;
		}

		void RecalculateOptionalNonExistantColumns(ColumnHeading[] headings, int startingColumn, int[] originalColumnNumbers)
		{
			if (headings != null && headings.Length > 0)
			{
				int hiddenCount = 0;
				var reCalculatedNonExistantOptionalColumns = new List<int>();
				var nonExistantOptionalColumns = report.Analyser.NonExistantOptionalColumns;

				for (int index = 0; index < headings.Length; index++)
				{
					if (headings[index].Hidden)
					{
						hiddenCount++;
					}

					if (nonExistantOptionalColumns.Contains(originalColumnNumbers[index]))
					{
						int columnPosition = index + startingColumn - hiddenCount;
						if (!reCalculatedNonExistantOptionalColumns.Contains(columnPosition))
						{
							reCalculatedNonExistantOptionalColumns.Add(columnPosition);
						}
					}
				}
				nonExistantOptionalColumns.Clear();
				nonExistantOptionalColumns.AddRange(reCalculatedNonExistantOptionalColumns);
			}
		}

		void UpdateOriginalColumnNumberForMovedColumns(ColumnHeading[] headings, int startOfMove, int endOfMove)
		{
			foreach (var heading in headings)
			{
				if ((heading.OriginalColumnNumber < startOfMove) && (heading.OriginalColumnNumber >= endOfMove))
				{
					heading.OriginalColumnNumber++;
				}
				else if (heading.OriginalColumnNumber > startOfMove && heading.OriginalColumnNumber <= endOfMove)
				{
					heading.OriginalColumnNumber--;
				}
			}
		}

		void ProcessGroupBys()
		{
			if (report.GroupByCollection.SelectedGroupBy.FieldList != null)
			{
				ArrayList selectedGroupBys = new ArrayList(report.GroupByCollection.SelectedGroupBy.FieldList.Split(new char[] { ',' }));
				if (selectedGroupBys.Count > 0)
				{
					foreach (Section section in report.Analyser.Sections)
					{
						RemoveNonUsedGroupBysInSection(selectedGroupBys, section);
						RearrangeGroupbysInSection(selectedGroupBys, section);
					}
				}
			}
		}

		void RearrangeGroupbysInSection(ArrayList selectedGroupBys, Section section)
		{
			int indexOfLastProcessedGroupBy = 1;
			for (int groupByRank = 0; groupByRank < selectedGroupBys.Count; groupByRank++)
			{
				var groupByFields = selectedGroupBys[groupByRank];
				bool groupByOfThisRankFound;
				do
				{
					groupByOfThisRankFound = false;
					for (int groupByNumber = indexOfLastProcessedGroupBy; groupByNumber < section.SectionBodyAndGroupByAreas.Count; groupByNumber++)
					{
						GroupByArea groupBy = section.SectionBodyAndGroupByAreas[groupByNumber] as GroupByArea;
						if (string.Join("+", groupBy.GroupByColumns) == Convert.ToString(groupByFields))
						{
							if (groupByNumber != indexOfLastProcessedGroupBy)
							{
								MoveGroupByArea(section, groupByNumber, indexOfLastProcessedGroupBy);
							}
							indexOfLastProcessedGroupBy++;
							groupByOfThisRankFound = true;
							break;
						}
					}
				} while (groupByOfThisRankFound);
			}
		}

		void MoveGroupByArea(Section section, int fromIndex, int toIndex)
		{
			GroupByArea groupByToMove = section.SectionBodyAndGroupByAreas[fromIndex] as GroupByArea;
			GroupByArea groupByToInsertBefore = section.SectionBodyAndGroupByAreas[toIndex] as GroupByArea;
			int indexOfGroupByToInsertBeforeInAllAreas = report.Analyser.Areas.IndexOf(groupByToInsertBefore);
			int indexOfGroupByInAllAreas = report.Analyser.Areas.IndexOf(groupByToMove);

			report.WorkSheetCurrentlyBeingProcessed.DuplicateRows(groupByToMove.StartingRow, groupByToMove.End, groupByToInsertBefore.StartingRow, 1);
			int shiftForGroupByToMove = 0;
			for (int i = indexOfGroupByToInsertBeforeInAllAreas; i <= indexOfGroupByInAllAreas; i++)
			{
				report.Analyser.Areas[i].Shift(groupByToMove.End - groupByToMove.StartingRow + 1);
				shiftForGroupByToMove -= report.Analyser.Areas[i].HeightInRowsIncludingStartingRow;
			}
			report.WorkSheetCurrentlyBeingProcessed.RemoveRows(groupByToMove.StartingRow, groupByToMove.End + 1);

			groupByToMove.Shift(shiftForGroupByToMove);

			report.Analyser.Areas.Remove(groupByToMove);
			report.Analyser.Areas.Insert(indexOfGroupByToInsertBeforeInAllAreas, groupByToMove);

			section.SectionBodyAndGroupByAreas.Remove(groupByToMove);
			section.SectionBodyAndGroupByAreas.Insert(toIndex, groupByToMove);
		}

		void RemoveNonUsedGroupBysInSection(ArrayList selectedGroupBys, Section section)
		{
			for (int i = section.SectionBodyAndGroupByAreas.Count - 1; i >= 0; i--)
			{
				GroupByArea groupBy = section.SectionBodyAndGroupByAreas[i] as GroupByArea;
				if (groupBy != null)
				{
					if (selectedGroupBys.IndexOf(string.Join("+", groupBy.GroupByColumns)) == -1)
					{
						RemoveGroupBy(groupBy);
					}
				}
			}
		}

		void RemoveGroupBy(GroupByArea groupBy)
		{
			groupBy.ShouldDelete = true;
			int indexOfGroupByInAllAreas = report.Analyser.Areas.IndexOf(groupBy);
			int heightOfGroupByInRows = groupBy.End - groupBy.StartingRow + 1;
			for (int i = indexOfGroupByInAllAreas + 1; i < report.Analyser.Areas.Count; i++)
			{
				report.Analyser.Areas[i].Shift(-heightOfGroupByInRows);
			}
			report.WorkSheetCurrentlyBeingProcessed.RemoveRows(groupBy.StartingRow, groupBy.End + 1);
			groupBy.OwnerSection.SectionBodyAndGroupByAreas.Remove(groupBy);
			report.Analyser.Areas.Remove(groupBy);
		}

		void UpdatePageBreaksOnGroupByAreas()
		{
			foreach (var area in report.Analyser.Areas)
			{
				if (area is GroupByArea groupByArea)
				{
					var fieldList = string.Join("+", groupByArea.GroupByColumns);

					if (report.GroupByCollection.SelectedGroupBy.FieldList.EndsWith(fieldList, StringComparison.OrdinalIgnoreCase))
					{
						groupByArea.BreakPage = report.GroupByCollection.BreakPageOverride;
					}
					else
					{
						groupByArea.BreakPage = false;
					}
				}
			}
		}

		void ProcessMacros(Passes pass)
		{
			using (TrackProcessingMacros())
			{
				foreach (var areaToReplace in report.Analyser.Areas)
				{
					if (AreaNeedsProcessing(areaToReplace, pass))
					{
						using (ReportRenderer.TrackCurrentAreaToProcess(this, areaToReplace))
						{
							CurrentPass = pass;
							ProcessArea(areaToReplace);
						}
					}
				}
			}

			void ProcessArea(Area area)
			{
				if (area is SectionBodyArea bodyArea)
				{
					if (CurrentPass == Passes.SecondPass && !bodyArea.IsDataRowIndexWithRowRangesMatchedWithStartEndRow())
					{
						bodyArea.ReportRowIndexIsMinusOneOrUnmatchedWithStartEndRow();
					}
				}

				for (var row = area.StartOfBody; row <= area.End; row++)
				{
					ReplaceTemplateRow(area, row);
				}

				area.ClearColumnsWidthCache();
			}
		}

		public IDisposable TrackProcessingMacros()
		{
			IsProcessingMacros = true;
			return new DisposableAction(() => IsProcessingMacros = false);
		}

		internal IDisposable TrackProcessingPageBreaks()
		{
			IsProcessingPageBreaks = true;
			return new DisposableAction(() => IsProcessingPageBreaks = false);
		}

		public IDisposable TrackProcessingAutoShapes()
		{
			IsProcessingAutoShapes = true;
			return new DisposableAction(() => IsProcessingAutoShapes = false);
		}

		internal static IDisposable TrackCurrentAreaToProcess(IReportRenderer renderer, Area currentArea)
		{
			var originalArea = renderer.CurrentAreaToProcess;
			renderer.CurrentAreaToProcess = currentArea;
			return new DisposableAction(() => renderer.CurrentAreaToProcess = originalArea);
		}

		void TranslateAreas()
		{
			if ((report.StTemplate != null && report.StTemplate.SO_IsSystemDefined && (report.Style == Report.Styles.Report || report.TranslateLegacyDocument)) || report.IsCoverSheet)
			{
				foreach (Area areaToReplace in report.Analyser.Areas)
				{
					for (var row = areaToReplace.StartOfBody; row <= areaToReplace.End; row++)
					{
						for (var column = 1; column < report.MaxCol; column++)
						{
							var cell = report.WorkSheetCurrentlyBeingProcessed.GetCell(row, column);
							var analyzer = new ExcelCellAnalyzer(cell);
							if (!cell.IsEmpty)
							{
								var allTranslatable = analyzer.GetTextToBeTranslated();
								if (allTranslatable.Length > 0)
								{
									foreach (var translatable in allTranslatable)
									{
										if (report.Style == Report.Styles.Report)
										{
											translatable.TranslatableText = DocBuilderResourceStrings.GetReportString(Path.GetFileNameWithoutExtension(report.StTemplate.SO_ExcelTemplatePath), translatable.TranslatableText);
										}
										else if (report.TranslateLegacyDocument)
										{
											var legacyDocumentTemplateTranslationHelper = new LegacyDocumentTemplateTranslationHelper();
											var templateName = legacyDocumentTemplateTranslationHelper.GetFileNameByPath(report.StTemplate.SO_ExcelTemplatePath);
											translatable.TranslatableText = DocBuilderResourceStrings.GetLegacyDocumentString(templateName, translatable.TranslatableText);
										}
										else if (report.IsCoverSheet)
										{
											translatable.TranslatableText = DocBuilderResourceStrings.GetCoverSheetString(report.Template.TemplateName, translatable.TranslatableText);
										}
									}
									analyzer.SetTranslation();
								}
							}
						}
					}
				}
			}
		}

		void ReplaceTemplateRow(Area area, int rowIndex)
		{
			var linesNeededForThisRow = 1;
			var multiLineRowColumnContents = new Dictionary<int, MultiLineCellInfo>();
			var contentsShouldCopyToNewLines = new Dictionary<int, string>();

			CurrentRow = rowIndex;

			for (int columnIndex = 0; columnIndex < report.MaxCol; columnIndex++)
			{
				CurrentColumn = columnIndex;
				linesNeededForThisRow = area.ReplaceTemplateCell(rowIndex, columnIndex, linesNeededForThisRow, multiLineRowColumnContents, contentsShouldCopyToNewLines, HiddenColumns);
			}

			if (multiLineRowColumnContents.Count > 0)
			{
				area.AddLinesToFitMultiLineRowColumnContents(rowIndex, linesNeededForThisRow, multiLineRowColumnContents, contentsShouldCopyToNewLines, this);
			}
		}

		void ProcessMacrosInAutoShapes(ExcelWorkSheet excelWorkSheet)
		{
			Area originalAreaToProcess = CurrentAreaToProcess;
			var originalRow = CurrentRow;
			try
			{
				ExcelFile excelFile = excelWorkSheet.ParentExcelInterface.Xls;
				var objectCount = excelFile.ObjectCount;

				// This is an issue of Flexcel, the comments' anchor won't be changed accordingly after deleting rows.
				// Below needSaveForComments related code is just a workaround for the anchor issue in DocEngine.
				// We have emailed Flexcel's author about this, see eDocs in WI00194491 for details.
				bool needSaveForComments = Enumerable.Range(1, objectCount).Any(i => excelFile.GetObjectProperties(i, false).ObjectType == TObjectType.Comment);
				if (needSaveForComments)
				{
					using (var stream = new MemoryStream())
					{
						excelWorkSheet.ParentExcelInterface.SaveToStream(stream);

						// Above code (excelWorkSheet.ParentExcelInterface.SaveToStream(stream)) will change ActiveSheet to first visible sheet of excel.
						// That will cause issue in WI00566496 - Report Returning Unknown Error
						// We need to set ActiveSheet back to current work sheet.
						if (excelFile.ActiveSheet != excelWorkSheet.WorkSheetNumber)
						{
							excelFile.ActiveSheet = excelWorkSheet.WorkSheetNumber;
						}
					}
				}

				for (int i = 1; i <= objectCount; i++)
				{
					var shapeProperties = excelFile.GetObjectProperties(i, false);
					if (shapeProperties.Text != null)
					{
						int currentShapeRow;

						if (shapeProperties.ObjectType == TObjectType.Comment)
						{
							currentShapeRow = shapeProperties.NestedAnchor.Row1;
						}
						else
						{
							currentShapeRow = shapeProperties.NestedAnchor.Row1 - 1;
						}

						var containerArea = report.Analyser.Areas.Find((a) => a.StartingRow <= currentShapeRow && a.End >= currentShapeRow);
						CurrentAreaToProcess = containerArea;

						CurrentRow = currentShapeRow;
						var shapePropertiesText = shapeProperties.TextAsRichString(excelFile);
						using (report.ErrorManager.EvaluatingCell(new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, shapeProperties.NestedAnchor.Row1, shapeProperties.NestedAnchor.Col1)))
						using (report.ErrorManager.EvaluatingOuterContent(shapePropertiesText))
						{
							var replacer = new CellContentReplacer(report, shapePropertiesText.Value);
							replacer.ReplaceMacros();
							if (shapePropertiesText.Value != replacer.ContentAsString)
							{
								TRichString newString = shapePropertiesText.Replace(shapePropertiesText.Value, replacer.ContentAsString);
								excelFile.SetObjectText(i, shapeProperties.ObjectPath, newString);
							}
						}
					}
				}
			}
			finally
			{
				CurrentAreaToProcess = originalAreaToProcess;
				CurrentRow = originalRow;
			}
		}

		CultureInfo GetTemporaryRenderCulture()
		{
			// Language and other styles are based on the language of the report
			// If is LocalDocuemnt, only Separators are changed and based on the registry
			var temporaryRenderCulture = Culture.GetCultureForLanguage(report.Language) ?? Culture.Default;
			if (report.MenuItem?.SU_IsLocalDocument == true)
			{
				var currentCompanyNumberFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;
				temporaryRenderCulture.NumberFormat.NumberGroupSeparator = currentCompanyNumberFormat.NumberGroupSeparator;
				temporaryRenderCulture.NumberFormat.NumberDecimalSeparator = currentCompanyNumberFormat.NumberDecimalSeparator;
				temporaryRenderCulture.NumberFormat.NumberGroupSizes = currentCompanyNumberFormat.NumberGroupSizes;
			}
			return temporaryRenderCulture;
		}

#if DEBUG
		internal IDisposable TemporarilySwitchCurrentPassForTest(Passes pass)
		{
			var originalPass = CurrentPass;
			CurrentPass = pass;
			return new DisposableAction(() => CurrentPass = originalPass);
		}
#endif
	}
}
