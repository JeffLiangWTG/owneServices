using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.Visualisation
{
	static class AreaVisualisationManagerFactory
	{
		internal static List<AreaVisualisationManager> NewList(List<Area> reportAnalyserAreas)
		{
			List<AreaVisualisationManager> result = new List<AreaVisualisationManager>();

			foreach (Area area in reportAnalyserAreas)
			{
				AreaVisualisationManager manager = New(area);
				if (manager != null)
				{
					result.Add(manager);
				}
			}

			SetAppropriateAreasToShowForVisualisation(result);

			return result;
		}

		internal static AreaVisualisationManager New(Area area)
		{
			GroupByArea groupByArea = area as GroupByArea;
			if (groupByArea != null)
			{
				return new AreaVisualisationManager(area, delegate
				{ return new RendererGroupByArea(groupByArea); });
			}

			SectionBodyArea sectionBodyArea = area as SectionBodyArea;
			if (sectionBodyArea != null && !(sectionBodyArea.DataRowSource is Enterprise.DocumentEngine.DataProviders.OneRowDataSource))
			{
				return new AreaVisualisationManager(area, delegate
				{ return new RendererSectionBody(sectionBodyArea); });
			}

			if (area is ConfigArea || area is EndOfReportArea)
			{
				return null;
			}

			return new AreaVisualisationManager(area, delegate
			{ return new RendererGeneral(area); });
		}

		static void SetAppropriateAreasToShowForVisualisation(List<AreaVisualisationManager> areaManagers)
		{
			areaManagers.Sort((x, y) => x.Area.StartingRow.CompareTo(y.Area.StartingRow));

			var footerRank = -1;
			var hasSectionHeader = false;

			AreaVisualisationManager preferredFooterSectionManager = null;
			foreach (var areaManager in areaManagers)
			{
				var footerArea = areaManager.Area as FooterArea;
				if (footerArea != null && footerArea.FooterRankForVisualisation > footerRank)
				{
					footerRank = footerArea.FooterRankForVisualisation;
					preferredFooterSectionManager = areaManager;
				}
				else if (areaManager.Area is SectionHeaderArea)
				{
					hasSectionHeader = true;
					areaManager.ShouldShowInVisualiser = true;
				}
				else if (areaManager.Area is SectionPageHeaderArea)
				{
					areaManager.ShouldShowInVisualiser = !hasSectionHeader;
				}
				else if (areaManager.Area is DataArea)
				{
					hasSectionHeader = false;
				}
			}

			if (preferredFooterSectionManager != null)
			{
				preferredFooterSectionManager.ShouldShowInVisualiser = true;
			}
		}
	}
}
