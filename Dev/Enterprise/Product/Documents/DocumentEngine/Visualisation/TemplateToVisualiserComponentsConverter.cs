using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class TemplateToVisualiserComponentsConverter
	{
		public TemplateToVisualiserComponentsConverter(Report report, VisualiserDataSet visualiserDataSet)
		{
			this.report = report;
			VisualiserDataSet = visualiserDataSet;
			report.ResetCachedExcelFile();
			report.ErrorManager.ClearErrors();
		}

		readonly Report report;
		readonly VisualiserDataSet VisualiserDataSet;
		readonly List<string> AddedSectionBodyNames = new List<string>();
		readonly List<Type> TypesOfAreasAlreadyProcessed = new List<Type>();
		int YOffsetInXL;

		List<VisualiserComponent> fComponents;
		public List<VisualiserComponent> Components
		{
			get
			{
				if (fComponents == null)
				{
					fComponents = new List<VisualiserComponent>();
					LoadComponentsFromReport();
				}

				return fComponents;
			}
		}

		void LoadComponentsFromReport()
		{
			using (Res.TemporarilySwitchLanguage(report.Language))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(report.Language) ?? Culture.Default))
			using (var temporaryRenderer = new TemporaryValueSetter<IReportRenderer>((renderer) => report.Renderer = renderer, report.Renderer))
			{
#if DEBUG
				languageWhenLoadingComponents = Res.CurrentLanguage;
#endif
				temporaryRenderer.Set(new VisualizerReportRenderer(report.Renderer));

				report.PrepareForRender();
				report.Renderer.SaveOriginalColumnWidths();

				foreach (var column in report.Analyser.Config.HideColumnExpressions.Keys)
				{
					if (report.Analyser.ShouldHideConditionalColumn(column))
					{
						report.WorkSheetCurrentlyBeingProcessed.HideColumn(column);
					}
				}

				var areaManagers = AreaVisualisationManagerFactory.NewList(report.Analyser.Areas);
				ProcessAreas(areaManagers);

				report.ResetDataProvider();
			}
		}

#if DEBUG
		internal string LanguageWhenLoadingComponents
		{
			get
			{
				return languageWhenLoadingComponents;
			}
		}
		string languageWhenLoadingComponents;
#endif

		void ProcessAreas(List<AreaVisualisationManager> areaManagers)
		{
			ProcessAreasOfType(typeof(SectionBodyArea), areaManagers);
			ProcessAreasOfType(typeof(GroupByArea), areaManagers);
			ProcessAreasOfAllOtherTypes(areaManagers);
			fComponents.AddRange(GetComponentsFromAreasInOrderAsAnalysed(areaManagers));
		}

		List<VisualiserComponent> GetComponentsFromAreasInOrderAsAnalysed(List<AreaVisualisationManager> areaManagers)
		{
			var result = new List<VisualiserComponent>();
			foreach (var manager in areaManagers)
			{
				if (manager.ShouldShowInVisualiser && manager.VisualisationRenderer != null)
				{
					foreach (var component in manager.Components)
					{
						component.AddToLocationY(ControlDpiScalingHelper.ScaleToCurrentDpiY((int)(YOffsetInXL / ExcelWorkSheet.XlsHeigthToFormHeightDivider)));
					}

					result.AddRange(manager.Components);
					YOffsetInXL += manager.VisualisationRenderer.GetHeightInXL();
				}
			}
			return result;
		}

		void ProcessAreasOfAllOtherTypes(List<AreaVisualisationManager> areaManagers)
		{
			foreach (AreaVisualisationManager areaManager in areaManagers)
			{
				if (!TypesOfAreasAlreadyProcessed.Contains(areaManager.Area.GetType()))
				{
					ProcessAreaIfShouldBeVisualised(areaManager);
				}
			}
		}

		void ProcessAreasOfType(Type areaType, List<AreaVisualisationManager> areaManagers)
		{
			foreach (AreaVisualisationManager areaManager in areaManagers)
			{
				if (areaManager.Area.GetType() == areaType)
				{
					ProcessAreaIfShouldBeVisualised(areaManager);
				}
			}

			TypesOfAreasAlreadyProcessed.Add(areaType);
		}

		void ProcessAreaIfShouldBeVisualised(AreaVisualisationManager areaVisualisationManager)
		{
			if (areaVisualisationManager.ShouldShowInVisualiser)
			{
				using (ReportRenderer.TrackCurrentAreaToProcess(report.Renderer, areaVisualisationManager.Area))
				{
					ProcessArea(areaVisualisationManager);
				}
			}
		}

		void ProcessArea(AreaVisualisationManager visualisationManager)
		{
			var components = visualisationManager.VisualisationRenderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, VisualiserDataSet, YOffsetInXL, AddedSectionBodyNames);
			visualisationManager.Components.AddRange(components);
		}
	}
}
