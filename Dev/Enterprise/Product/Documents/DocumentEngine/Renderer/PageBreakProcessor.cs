using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.Renderer
{
	class PageBreakProcessor
	{
		readonly Report report;
		readonly PageCollection pages;
		readonly List<Area> pageFooterAreasToKeep = new List<Area>();

		int lastPageAvailableHeight;
		Area lastAddedArea;

		public bool IsFooterFitOnPage { get; private set; } = true;
		public IEnumerable<Area> PageFooterAreasToKeep => pageFooterAreasToKeep;

		public PageBreakProcessor(Report report, PageCollection pages)
		{
			this.report = report;
			this.pages = pages;
		}

		public void OpenFirstPage()
		{
			pages.AddNew();
			lastPageAvailableHeight = pages.LastPage.AvailableHeight;

			AddAreaToLastPage(report.Analyser.DocumentHeader);
			if (report.Analyser.PageHeader != null && !report.Analyser.PageHeader.StartFromSecondPage)
			{
				AddAreaToLastPage(report.Analyser.PageHeader);
			}
		}

		public void OpenPage(Area areaToInsertInPage)
		{
			var lastAreaBeforeNewPage = pages.LastPage.LastArea;
			pages.AddNew();
			lastPageAvailableHeight = pages.LastPage.AvailableHeight;

			foreach (HeaderArea header in GetHeaderAreas(areaToInsertInPage))
			{
				lastAreaBeforeNewPage = InsertHeaderFooter(header, lastAreaBeforeNewPage);
			}
		}

		public void CloseLastPage()
		{
			Area lastPageFooter = report.Analyser.LastPageFooter;
			Area secondPagePageFooter = null;

			AddAreaToLastPage(report.Analyser.DocumentFooter);

			if (pages.Count == 1 && report.Analyser.OnlyOnePageFooter != null)
			{
				if (lastPageFooter == null || CheckIfFooterFitsOnPage(report.Analyser.OnlyOnePageFooter))
				{
					lastPageFooter = report.Analyser.OnlyOnePageFooter;
				}
				else if (lastPageFooter != null)
				{
					secondPagePageFooter = lastPageFooter;
					lastPageFooter = null;
					if (CheckIfFooterFitsOnPage(report.Analyser.FirstPageFooter))
					{
						lastPageFooter = report.Analyser.FirstPageFooter;
					}

					if (lastPageFooter == null && CheckIfFooterFitsOnPage(report.Analyser.PageFooter))
					{
						lastPageFooter = report.Analyser.PageFooter;
					}
				}
			}

			if (lastPageFooter == null)
			{
				lastPageFooter = report.Analyser.PageFooter;
			}

			if (lastPageFooter == null)
			{
				lastPageFooter = report.Analyser.FirstPageFooter;
			}

			using (TrackAddingPageFooterDuringCloseLastPage())
			{
				if (lastPageFooter != null)
				{
					var insertedFooter = InsertHeaderFooter(lastPageFooter, pages.LastPage.LastArea);
					pageFooterAreasToKeep.Add(insertedFooter);
					PadBeginningOfAreaToFillRestOfPage(insertedFooter);
				}

				if (secondPagePageFooter != null)
				{
					Area insertedFooter = InsertHeaderFooter(secondPagePageFooter, pages.LastPage.LastArea);
					pageFooterAreasToKeep.Add(insertedFooter);
					PadBeginningOfAreaToFillRestOfPage(insertedFooter);
				}
			}
		}

		bool isAddingPageFooterDuringCloseLastPage;

		DisposableAction TrackAddingPageFooterDuringCloseLastPage()
		{
			isAddingPageFooterDuringCloseLastPage = true;
			return new DisposableAction(() => { isAddingPageFooterDuringCloseLastPage = false; });
		}

		public void ClosePage()
		{
			Page lastPage = pages.LastPage;
			Area firstFooter = null;

			foreach (FooterArea footer in GetFooterAreas(report, pages, lastPage.LastArea))
			{
				Area insertedFooter = InsertHeaderFooter(footer, lastPage.LastArea);

				if (firstFooter == null)
				{
					pageFooterAreasToKeep.Add(insertedFooter);
					firstFooter = insertedFooter;
				}
			}

			if (firstFooter != null)
			{
				PadBeginningOfAreaToFillRestOfPage(firstFooter);
			}
		}

		bool needToOpenPageWhenAddingArea;

		public void AddAreaToLastPage(Area area)
		{
			var needToAddPageForSplitArea = true;
			while (area != null && !area.ShouldDelete && needToAddPageForSplitArea)
			{
				if (needToOpenPageWhenAddingArea)
				{
					needToOpenPageWhenAddingArea = false;
					OpenPage(area);
				}

				var lastPage = pages.LastPage;

				area.RenderedToPageNumber = pages.Count;

				ExpandLastPageForAreaIfFooterIsTooBigAndAddWarning(area);

				int heightAvailableInPage = GetAvailableHeightForArea(area, lastPageAvailableHeight);
				if (area.FitsInPage(heightAvailableInPage, lastPage))
				{
					lastPage.Areas.Add(area);
					lastPageAvailableHeight -= area.HeightInXls;
					needToAddPageForSplitArea = false;
				}
				else
				{
					var secondPartOfSplitArea = SplitArea(area, heightAvailableInPage);

					if (secondPartOfSplitArea == area) // Means area is not splittable in next page
					{
						lastPage.Height = lastPage.Areas.Height + area.HeightInXls + GetFooterSizeForArea(area);
						lastPage.Areas.Add(area);
						lastPageAvailableHeight = GetFooterSizeForArea(area);
						ClosePage();
						needToOpenPageWhenAddingArea = true;
						break;
					}

					if (secondPartOfSplitArea == null)
					{
						secondPartOfSplitArea = area;
					}
					else
					{
						lastPage.Areas.Add(area);
						lastPage.PageBroken = area.PageBroken;
						lastPageAvailableHeight -= area.HeightInXls;
					}

					if (lastAddedArea == area && !area.SplitIfNotFitInAPage)
					{
						throw new DocumentAreaUnbreakableException("Area " + area + " does not fit in a page and is unbreakable!");
					}

					lastAddedArea = secondPartOfSplitArea;

					if (!lastPage.PageBroken)
					{
						ClosePage();
					}
					OpenPage(secondPartOfSplitArea);
					area = secondPartOfSplitArea;
				}
			}
		}

		bool CheckIfFooterFitsOnPage(FooterArea area)
		{
			var result = false;

			if (area != null)
			{
				var page = pages[0];
				int heightAvailable = GetAvailableHeightForArea(area, page);
				result = area.FitsInPage(heightAvailable, page);
			}

			return result;
		}

		internal int GetRowsToKeepInEmptyPage(Area area)
		{
			var headerAreasHeight = GetHeaderAreas(area).Height;
			var footerAreasHeight = GetFooterAreas(report, pages, area, true).Height;

			return area.GetRowsToKeep(pages.DefaultPageHeight - headerAreasHeight - footerAreasHeight, pages.LastPage);
		}

		internal bool DoesFitInEmptyPage(Area area)
		{
			var headerAreas = GetHeaderAreas(area);
			var footerAreas = GetFooterAreas(report, pages, area, true);

			var dummyEmptyPage = new Page { Height = pages.DefaultPageHeight };
			dummyEmptyPage.Areas.AddRange(headerAreas);
			dummyEmptyPage.Areas.AddRange(footerAreas);

			return area.FitsInPage(pages.DefaultPageHeight - headerAreas.Height - footerAreas.Height, dummyEmptyPage);
		}

		internal bool IsSplittableInEmptyPage(Area area)
		{
			return area.GetRealRowsToKeepConcerningMergedCell(GetRowsToKeepInEmptyPage(area)) > 0;
		}

		Area InsertHeaderFooter(Area areaToInsert, Area areaToInsertAfter)
		{
			Area areaInserted = areaToInsert;

			if (areaToInsert != null)
			{
				if (areaToInsertAfter != null && areaToInsert.StartingRow != areaToInsertAfter.End + 1)
				{
					areaInserted = areaToInsert.Clone(areaToInsertAfter.End + 1);
					report.WorkSheetCurrentlyBeingProcessed.DuplicateRows(areaToInsert.StartingRow, areaToInsert.End, areaInserted.StartingRow, 1);

					for (int i = report.Analyser.Areas.IndexOf(areaToInsertAfter) + 1; i < report.Analyser.Areas.Count; i++)
					{
						report.Analyser.Areas[i].Shift(areaInserted.HeightInRowsIncludingStartingRow);
					}

					report.Analyser.Areas.Insert(report.Analyser.Areas.IndexOf(areaToInsertAfter) + 1, areaInserted);
					report.Analyser.UpdateSectionBodyAreaRowRangesInReport(areaInserted.StartingRow - 1, areaInserted.HeightInRowsIncludingStartingRow);
				}
				else
				{
					areaInserted.ShouldDelete = false;
				}

				AddAreaToLastPage(areaInserted);
			}

			return areaInserted;
		}

		AreaCollection GetHeaderAreas(Area area)
		{
			var result = new AreaCollection();

			var pageHeader = report.Analyser.PageHeader;
			if (pageHeader != null && pageHeader != area)
			{
				result.Add(pageHeader);
			}

			if (area.IsDataArea)
			{
				var sectionPageHeader = area.OwnerSection.SectionPageHeader;
				if (sectionPageHeader != null && sectionPageHeader != area)
				{
					result.Add(sectionPageHeader);
				}
			}

			return result;
		}

		internal AreaCollection GetFooterAreas(Report report, PageCollection pages, Area area, bool isPreCalculation = false)
		{
			return GetAvailableFooterAreas(report, pages, area, isPreCalculation, isAddingPageFooterDuringCloseLastPage);
		}

		internal static AreaCollection GetAvailableFooterAreas(Report report, PageCollection pages, Area area, bool isPreCalculation = false, bool isAddingPageFooterDuringCloseLastPage = false)
		{
			var result = new AreaCollection();

			//	if (area.IsDataArea && area.OwnerSection.SectionPageFooter != null)
			if (!(area is FooterArea))
			{
				if (report == null)
				{
					throw new InvalidOperationException("Report cannot be null in GetFooterAreas.");
				}
				if (report.Analyser == null)
				{
					throw new InvalidOperationException("Report cannot be null in GetFooterAreas.");
				}

				if (area != null && area.IsDataArea && area.OwnerSection != null && area.OwnerSection.SectionPageFooter != null)
				{
					result.Add(area.OwnerSection.SectionPageFooter);
				}

				if (area is DocumentFooterArea && report.Analyser.LastPageFooter != null)
				{
					result.Add(report.Analyser.LastPageFooter);
				}
				else if (pages.Count == 1 && report.Analyser.FirstPageFooter != null && !isPreCalculation)
				{
					result.Add(report.Analyser.FirstPageFooter);
				}
				else if (report.Analyser.PageFooter != null && !isAddingPageFooterDuringCloseLastPage)
				{
					result.Add(report.Analyser.PageFooter);
				}
			}

			return result;
		}

		Area SplitArea(Area area, int maxHeightAvailable)
		{
			Area splittedArea = null;
			var page = pages.LastPage;

			if (area.TryHardPutInOnePage && DoesFitInEmptyPage(area))
			{
				return null;
			}

			if (area is GroupByArea groupByArea && groupByArea.ShouldForcePutInNextPage(maxHeightAvailable, page))
			{
				return null;
			}

			if (area.HeightInRows > 1 && area.SplitIfNotFitInAPage)
			{
				try
				{
					splittedArea = area.SplitAndReturnNewArea(maxHeightAvailable, page, this);
				}
				catch (CloneAreaException)
				{
					// it is not cloneable, ignore
				}
			}
			else if (area.HeightInRows == 1 && !DoesFitInEmptyPage(area))
			{
				splittedArea = area; // Means this area is not splittable and cannot fit in the next new page, it should be in current last page
			}

			return splittedArea;
		}

		int GetAvailableHeightForArea(Area area, Page page)
			=> GetAvailableHeightForArea(area, page.AvailableHeight);

		int GetAvailableHeightForArea(Area area, int pageAvailableHeight)
			=> pageAvailableHeight - GetFooterSizeForArea(area);

		void PadBeginningOfAreaToFillRestOfPage(Area areaToPad)
		{
			if (report.Analyser.Config.PageStyle != PageStyles.Continuous)
			{
				int addedRowCount = report.WorkSheetCurrentlyBeingProcessed.SetRowHeightInsertingExtraRowsIfRequiredToGetTheRightTotalHeightReturningNumberOfAdditionalRows(areaToPad.StartingRow, pages.LastPage.AvailableHeight);
				UpdateFollowingAreasWithNewStartAndEndRowIndex(addedRowCount, report.Analyser.Areas, report.Analyser.Areas.IndexOf(areaToPad));
			}
		}

		[SuppressMessage("CargoWise", "CW1017", Justification = "These values are not used as screen pixels")]
		void ExpandLastPageForAreaIfFooterIsTooBigAndAddWarning(Area area)
		{
			var lastPage = pages.LastPage;

			var maximumFooterSizePageRatio = 0.9;
			var marginMultiplier = 1450;
			var footerSizeForArea = GetFooterSizeForArea(area);
			if (footerSizeForArea > maximumFooterSizePageRatio * report.Analyser.PageHeight)
			{
				int pagesRequired = (int)((report.Analyser.PageHeight / maximumFooterSizePageRatio + footerSizeForArea) / report.Analyser.PageHeight);
				var newHeight = Math.Max(lastPage.Height, (report.Analyser.PageHeight * pagesRequired -
					(int)((report.WorkSheetCurrentlyBeingProcessed.GetBottomMargin() + report.WorkSheetCurrentlyBeingProcessed.GetTopMargin() * marginMultiplier))));
				lastPage.Height = newHeight;
				IsFooterFitOnPage = false;
				lastPageAvailableHeight = lastPage.AvailableHeight;
			}
		}

		int GetFooterSizeForArea(Area area)
		{
			return GetFooterAreas(report, pages, area).Height;
		}

		void UpdateFollowingAreasWithNewStartAndEndRowIndex(int addedRowCount, List<Area> areas, int indexOfCurrentArea)
		{
			if (addedRowCount > 0)
			{
				for (int areaNumber = areas.Count - 1; areaNumber > indexOfCurrentArea; areaNumber--)
				{
					areas[areaNumber].Shift(addedRowCount);
				}

				var currentArea = areas[indexOfCurrentArea];
				currentArea.UpdateEndPosition(currentArea.End + addedRowCount);

				report.Analyser.UpdateSectionBodyAreaRowRangesInReport(currentArea.StartingRow, addedRowCount);
			}
		}
	}
}

// Tests are found in ReportRenderer. This was extracted from there and placed into its own class for organisations sake.
