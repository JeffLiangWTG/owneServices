using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine
{
	sealed class Section
	{
		public Section()
		{
		}

		public SectionHeaderArea SectionHeader;
		public Area SectionPageHeader;
		public List<Area> SectionBodyAndGroupByAreas = new List<Area>();
		public List<Area> ProcessedSectionBodyAndGroupByAreas = new List<Area>();
		public List<Area> OriginalSectionBodyAndGroupByAreas;
		public Area SectionPageFooter;
		public SectionFooterArea SectionFooter;
		public string TableName = "";

		public List<SectionBodyArea> SplitSectionBodyAreaInstances = new List<SectionBodyArea>();

		public List<Area> GetAreasInLevel(int level)
		{
			List<Area> result = new List<Area>();
			if (level < OriginalSectionBodyAndGroupByAreas.Count)
			{
				if (OriginalSectionBodyAndGroupByAreas[level] is SectionBodyArea)
				{
					foreach (Area area in ((SectionBodyArea)OriginalSectionBodyAndGroupByAreas[level]).AreasSplittedAcrossPages)
					{
						if (!area.ShouldDelete)
						{
							result.Add(area);
						}
					}
				}
				else
				{
					foreach (Area area in ((GroupByArea)OriginalSectionBodyAndGroupByAreas[level]).ClonedAreas)
					{
						result.Add(area);
					}
				}
			}
			return result;
		}

		public List<Area> AreasToRemoveIfThereIsNoData
		{
			get
			{
				List<Area> result = new List<Area>();
				if (SectionHeader != null && !SectionHeader.ShowEvenWithNoData)
				{
					result.Add(SectionHeader);
				}
				result.AddRange(DataAreas);
				if (SectionFooter != null && !SectionFooter.ShowEvenWithNoData)
				{
					result.Add(SectionFooter);
				}

				return result;
			}
		}

		public List<Area> DataAreas
		{
			get
			{
				List<Area> result = new List<Area>();
				AddAreaIfNotNull(result, SectionPageHeader);

				foreach (Area area in SectionBodyAndGroupByAreas)
				{
					result.Add(area);
				}

				AddAreaIfNotNull(result, SectionPageFooter);

				return result;
			}
		}

		public int StartingRowOfSection
		{
			get
			{
				return AllAreas.Count > 0 ? AllAreas[0].StartingRow : -1;
			}
		}

		public SectionBodyArea SectionBody
		{
			get
			{
				SectionBodyArea result = null;

				foreach (Area area in SectionBodyAndGroupByAreas)
				{
					if (area is SectionBodyArea)
					{
						result = (SectionBodyArea)area;
						break;
					}
				}

				if (result == null)
				{
					throw new DocumentEngineException("There is no section body area in section!");
				}
				else
				{
					return result;
				}
			}
		}

		public void PopulateRows(Report report)
		{
			MovePositionOfGroupByWithGroupTitle();
			SetParentshipForAreas();
			if (SectionBody.RowCount > 0)
			{
				SetupColumnsForFormulaProvider(report);
				CreateDuplicatedAreas(SectionBodyAndGroupByAreas, report);
			}
			else
			{
				foreach (Area areaToDelete in AreasToRemoveIfThereIsNoData)
				{
					areaToDelete.ShouldDelete = true;
				}
			}

			foreach (Area area in report.Analyser.Areas)
			{
				if (area != null && area.IsDataArea && area.OwnerSection == this)
				{
					ProcessedSectionBodyAndGroupByAreas.Add(area);
				}
			}
		}

		void MovePositionOfGroupByWithGroupTitle()
		{
			for (int i = 0; i < SectionBodyAndGroupByAreas.Count; i++)
			{
				if (SectionBodyAndGroupByAreas[i] is GroupByArea groupBy)
				{
					if (groupBy.GroupByPosition == GroupByArea.Position.Top && i < SectionBodyAndGroupByAreas.Count - 1
						&& SectionBodyAndGroupByAreas[i + 1] is GroupByArea nextGroupBy && groupBy.GroupByColumns.SequenceEqual(nextGroupBy.GroupByColumns))
					{
						groupBy.MoveAreaAfter(nextGroupBy);
						SectionBodyAndGroupByAreas.Remove(nextGroupBy);
						SectionBodyAndGroupByAreas.Insert(i, nextGroupBy);
						i++;
					}
				}
			}
		}

		void SetParentshipForAreas()
		{
			SplitSectionBodyAreaInstances.Add(SectionBody);
			for (int i = 1; i < SectionBodyAndGroupByAreas.Count; i++)
			{
				SectionBodyAndGroupByAreas[i].DataParent = SectionBodyAndGroupByAreas[i - 1];
			}

			if (SectionHeader != null)
			{
				SectionHeader.OwnerSection = this;
			}
			if (SectionPageHeader != null)
			{
				SectionPageHeader.OwnerSection = this;
			}
			if (SectionPageFooter != null)
			{
				SectionPageFooter.OwnerSection = this;
			}
			if (SectionFooter != null)
			{
				SectionFooter.OwnerSection = this;
			}

			OriginalSectionBodyAndGroupByAreas = new List<Area>();
			OriginalSectionBodyAndGroupByAreas.AddRange(SectionBodyAndGroupByAreas.ToArray());
		}

		List<Area> AllAreas
		{
			get
			{
				List<Area> result = new List<Area>();
				AddAreaIfNotNull(result, SectionHeader);
				result.AddRange(DataAreas);
				AddAreaIfNotNull(result, SectionFooter);

				return result;
			}
		}

		void CreateDuplicatedAreas(List<Area> originalAreas, Report report)
		{
			SectionBodyArea originalBodyArea = ((SectionBodyArea)originalAreas[0]);
			originalBodyArea.SortForBusinessObjectDataSourceIfNeeded();

			if (originalAreas.Count > 1) // HasGroupByAreas
			{
				GroupByArea lastGroupByArea = ((GroupByArea)originalAreas[originalAreas.Count - 1]);
				IDataRowSource[] groupDataSources = originalBodyArea.GetGroups(lastGroupByArea.GroupByColumns);
				List<Area> currentLevelGroupByAreas = new List<Area>();
				List<Area>[] newSectionsAreas = DuplicateSectionForGroupBy(originalAreas, groupDataSources.Length, report);
				for (int groupIndex = 0; groupIndex < groupDataSources.Length; groupIndex++)
				{
					List<Area> newSectionAreas = newSectionsAreas[groupIndex];
					IDataRowSource groupDataSource = groupDataSources[groupIndex];

					GroupByArea currentLevelGroupByArea;
					if (newSectionAreas[0] is GroupByArea) // Has GroupBy(s) before the body
					{
						currentLevelGroupByArea = (GroupByArea)newSectionAreas[0]; // Top GroupBy Area
						currentLevelGroupByArea.DataParent = newSectionAreas[newSectionAreas.Count - 1]; // Last Area
					}
					else // Has GroupBy(s) after the body.
					{
						currentLevelGroupByArea = (GroupByArea)newSectionAreas[newSectionAreas.Count - 1]; // Bottom GroupBy Area
						currentLevelGroupByArea.DataParent = newSectionAreas[newSectionAreas.Count - 2]; // Next Area Up
					}
					currentLevelGroupByArea.RowCount = groupDataSource.RowCount;
					currentLevelGroupByAreas.Add(currentLevelGroupByArea);
					newSectionAreas.Remove(currentLevelGroupByArea);

					SectionBodyArea bodyArea = (SectionBodyArea)newSectionAreas[0];
					bodyArea.SetDataSource(groupDataSource);

					CreateDuplicatedAreas(newSectionAreas, report);
				}

				foreach (GroupByArea currentLevelGroupByArea in currentLevelGroupByAreas)
				{
					currentLevelGroupByArea.ClonedAreas = currentLevelGroupByAreas;
				}
			}
			else
			{
				int expansionSize = originalBodyArea.RowCount - 1;
				if (originalBodyArea.StartingRow != originalBodyArea.End)
				{
					originalBodyArea.ExpandForDataRows(expansionSize);
				}
			}
		}

		FormulaProvider PageFooterFormulaProvider;

		public void GetFormularProviderWithAllFields(Report report, Area area, FormulaProvider formularProvider)
		{
			if (area != null)
			{
				var ignoreTableName = area is SectionFooterArea;
				AddAreaColumnsToFormulaProvider(report, area, formularProvider, ignoreTableName);
			}
		}

		void SetupColumnsForFormulaProvider(Report report)
		{
			var workSheet = report.WorkSheetCurrentlyBeingProcessed;

			foreach (Area area in SectionBodyAndGroupByAreas)
			{
				area.SetWorksheetForFormulaProvider(workSheet);
				AddAreaColumnsToFormulaProvider(report, area, area.FormulaProvider, false);
			}

			if (SectionFooter != null)
			{
				SectionFooter.SetWorksheetForFormulaProvider(workSheet);
				AddAreaColumnsToFormulaProvider(report, SectionFooter, SectionFooter.FormulaProvider, true);
			}

			PageFooterFormulaProvider = new FormulaProvider(workSheet);
			if (SectionBodyAndGroupByAreas.Count > 0)
			{
				AddAreaColumnsToFormulaProvider(report, SectionBodyAndGroupByAreas[0], PageFooterFormulaProvider, false);
			}
		}

		internal void AddAreaColumnsToFormulaProvider(Report report, Area area, FormulaProvider formulaProvider, bool ignoreTableName)
		{
			var workSheet = report.WorkSheetCurrentlyBeingProcessed;

			for (int row = area.StartOfBody; row <= area.End; row++)
			{
				for (int col = 0; col < workSheet.ColumnCount; col++)//255 -> LastColumnInTemplate
				{
					if (string.IsNullOrEmpty(workSheet.GetCellFormula(row, col)))
					{
						string cellString = workSheet[row, col].ToString();
						if (!string.IsNullOrEmpty(cellString))
						{
							var cellContentWithoutFormattingOnlyMacros = NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly(cellString);

							if (!string.IsNullOrEmpty(cellContentWithoutFormattingOnlyMacros))
							{
								Match singleMacroMatch = RegexProvider.OutermostSingleMacroRegex.Match(cellContentWithoutFormattingOnlyMacros);
								if (singleMacroMatch.Success)
								{
									Match match = RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex.Match(cellContentWithoutFormattingOnlyMacros);
									string fieldNameToAdd = "";
									if (match.Success)
									{
										string tableName = match.Groups[1].Value;
										string columnName = match.Groups[2].Value;
										fieldNameToAdd = (ignoreTableName ? "" : tableName + ".") + columnName;
									}
									else
									{
										fieldNameToAdd = singleMacroMatch.Groups[0].Value.Substring(1, singleMacroMatch.Groups[0].Value.Length - 2);
									}
									formulaProvider.AddColumn(col, fieldNameToAdd, row - area.StartOfBody);
								}
							}
						}
					}
				}
			}
		}

		void AddAreaIfNotNull(List<Area> list, Area area)
		{
			if (area != null)
			{
				list.Add(area);
			}
		}

		#region DuplicateSectionForGroupBy and associated methods
		List<Area>[] DuplicateSectionForGroupBy(List<Area> areasForSection, int sectionCount, Report report)
		{
			List<Area>[] result = new List<Area>[sectionCount];
			GroupByArea lastGroupByArea = null;
			result[0] = areasForSection;
			lastGroupByArea = (GroupByArea)areasForSection[areasForSection.Count - 1];
			int nextAnalyserAreaIndex = report.Analyser.Areas.IndexOf(lastGroupByArea) + 1;
			int nextSectionStartPosition = lastGroupByArea.End + 1;

			if (lastGroupByArea.GroupByPosition == GroupByArea.Position.Top)
			{
				MoveTopGroupByAreaFromLastToFirst(areasForSection, lastGroupByArea);
			}

			PopulateDuplicatedAreas(result, nextSectionStartPosition, nextAnalyserAreaIndex, areasForSection, report);

			AddCopiedAreasToFormulaRelatedAreasOfAreasAfterCopiedAreas(areasForSection, result, report);

			SetSectionBodyAreaForAllGroupBys(result);

			if (lastGroupByArea.Sticky)
			{
				SetStickyAttributeForDuplicatedBodySections(result);
			}

			return result;
		}

		int GetSectionHeight(List<Area> areasForSection)
		{
			int sectionHeight = 0;
			foreach (Area area in areasForSection)
			{
				sectionHeight += area.HeightInRowsIncludingStartingRow;
			}
			return sectionHeight;
		}

		void MoveTopGroupByAreaFromLastToFirst(List<Area> areasForSection, Area topGroupByArea)
		{
			Area bodyArea = areasForSection[0];

			if (areasForSection.Count < 2)
			{
				throw new DocumentEngineException("MoveGroupByAreaToTheBegginingOfAreaList: should have more than 2 areas.");
			}

			topGroupByArea.MoveAreaBefore(bodyArea); // Does the Report.Worksheet and Report.Analyser.

			areasForSection.Remove(topGroupByArea);
			areasForSection.Insert(0, topGroupByArea);
		}

		void PopulateDuplicatedAreas(List<Area>[] sectionsAreas, int nextSectionStartPosition, int nextAnalyserAreaIndex, List<Area> areasForSection, Report report)
		{
			for (int sectionIndex = 1; sectionIndex < sectionsAreas.Length; sectionIndex++)
			{
				List<Area> previousSectionAreas = sectionsAreas[sectionIndex - 1];
				List<Area> newSectionAreas = new List<Area>();
				foreach (Area previousSectionArea in previousSectionAreas)
				{
					Area newSectionArea = previousSectionArea.Clone(nextSectionStartPosition);
					report.Analyser.Areas.Insert(nextAnalyserAreaIndex++, newSectionArea);
					newSectionAreas.Add(newSectionArea);
					nextSectionStartPosition += newSectionArea.HeightInRowsIncludingStartingRow;
				}

				Area parentDataArea = null;
				foreach (Area newSectionArea in newSectionAreas)
				{
					if (parentDataArea != null)
					{
						newSectionArea.FormulaRelatedAreas.Add(parentDataArea);
					}
					bool areaIsTopGroupByArea = newSectionArea is GroupByArea && ((GroupByArea)newSectionArea).GroupByPosition == GroupByArea.Position.Top;
					if (!areaIsTopGroupByArea)
					{
						parentDataArea = newSectionArea;
					}
				}

				sectionsAreas[sectionIndex] = newSectionAreas;
			}

			Area firstAreaInSection = areasForSection[0];
			Area lastAreaInSection = areasForSection[areasForSection.Count - 1];
			int numberOfTimesToDuplicateSection = sectionsAreas.Length - 1;
			report.WorkSheetCurrentlyBeingProcessed.DuplicateRows(firstAreaInSection.StartingRow, lastAreaInSection.End, lastAreaInSection.End + 1, numberOfTimesToDuplicateSection);

			int heightOfAllDuplicateSections = numberOfTimesToDuplicateSection * GetSectionHeight(areasForSection);
			for (int analyserAreaIndex = nextAnalyserAreaIndex; analyserAreaIndex < report.Analyser.Areas.Count; analyserAreaIndex++)
			{
				report.Analyser.Areas[analyserAreaIndex].Shift(heightOfAllDuplicateSections);
			}
		}

		void AddCopiedAreasToFormulaRelatedAreasOfAreasAfterCopiedAreas(List<Area> originalSectionAreas, List<Area>[] sectionsAreas, Report report)
		{
			if (sectionsAreas.Length > 1)
			{
				List<Area> lastSectionAreas = sectionsAreas[sectionsAreas.Length - 1];
				Area lastArea = lastSectionAreas[lastSectionAreas.Count - 1];
				for (int analyserAreaIndex = report.Analyser.Areas.IndexOf(lastArea) + 1;
					analyserAreaIndex < report.Analyser.Areas.Count;
					analyserAreaIndex++)
				{
					Area analyserArea = report.Analyser.Areas[analyserAreaIndex];
					if (analyserArea.FormulaRelatedAreas.IndexOf(originalSectionAreas[originalSectionAreas.Count - 1]) > -1)
					{
						for (int i = 1; i < sectionsAreas.Length; i++)
						{
							List<Area> newSectionAreas = sectionsAreas[i];
							analyserArea.FormulaRelatedAreas.Add(newSectionAreas[newSectionAreas.Count - 1]);
						}
					}
				}
			}
		}

		void SetStickyAttributeForDuplicatedBodySections(List<Area>[] sectionsAreas)
		{
			foreach (List<Area> sectionAreas in sectionsAreas)
			{
				foreach (Area sectionArea in sectionAreas)
				{
					if (sectionArea is SectionBodyArea)
					{
						((SectionBodyArea)sectionArea).Sticky = true;
						break;
					}
				}
			}
		}

		void SetSectionBodyAreaForAllGroupBys(List<Area>[] sectionsAreas)
		{
			foreach (List<Area> sectionAreas in sectionsAreas)
			{
				SectionBodyArea sectionBody = null;
				foreach (Area sectionArea in sectionAreas)
				{
					if (sectionArea is SectionBodyArea)
					{
						sectionBody = (SectionBodyArea)sectionArea;
						break;
					}
				}
				foreach (Area sectionArea in sectionAreas)
				{
					if (sectionArea is GroupByArea)
					{
						((GroupByArea)sectionArea).SectionBody = sectionBody;
					}
				}
			}
		}
		#endregion
	}
}
