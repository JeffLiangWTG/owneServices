using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreVisualiser
	{
		public MENTAgedScoreVisualiser(MENTAgedScoreVisualisation visualisation)
		{
			Argument.NotNull(visualisation, nameof(visualisation));

			this.visualisation = visualisation;
			categoryMapper = new CategoryIndexMapper(visualisation);
		}

		readonly MENTAgedScoreVisualisation visualisation;
		readonly CategoryIndexMapper categoryMapper;

		public PlotModel Visualise(MENTAgedScoreExtractorResult result)
		{
			Argument.NotNull(result, nameof(result));

			return CreateHistogramModel(result);
		}

		public PlotController HistogramPlotController { get; private set; }

		PlotModel CreateHistogramModel(MENTAgedScoreExtractorResult set)
		{
			Argument.NotNull(set, nameof(set));

			var plotModel = new PlotModel();

			var categoryIndexMap = categoryMapper.Map(set);

			var xAxis = CreateCategoryAxis(categoryIndexMap);
			plotModel.Axes.Add(xAxis);
			var yAxis = CreateYAxis();
			plotModel.Axes.Add(yAxis);

			AddSeriesToPlotModel(plotModel, set, categoryIndexMap);

			new PlotModelDecorator(visualisation, plotModel, xAxis, yAxis).Decorate();

			ApplyInteractionSettings(plotModel, xAxis, yAxis);

			return plotModel;
		}

		void ApplyInteractionSettings(PlotModel model, CategoryAxis xAxis, LinearAxis yAxis)
		{
			Argument.NotNull(model, nameof(model));

			model.Axes.ForEach(x => x.IsZoomEnabled = visualisation.AllowZoom);

			if (visualisation.AllowArrowAnnotations)
			{
				AddArrowAnnotationInteraction(model, xAxis, yAxis);
			}
		}

		void AddArrowAnnotationInteraction(PlotModel model, CategoryAxis xAxis, LinearAxis yAxis)
		{
			HistogramPlotController = new PlotController();

			Argument.NotNull(model, nameof(model));

			ArrowAnnotation arrowAnnotation = null;

			HistogramPlotController.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(
				(view, controller, args) =>
				{
					arrowAnnotation = new ArrowAnnotation();
					arrowAnnotation.StartPoint =
						arrowAnnotation.EndPoint = xAxis.InverseTransform(args.Position.X, args.Position.Y, yAxis);
					model.Annotations.Add(arrowAnnotation);
					args.Handled = true;
				}));
		}

		void AddSeriesToPlotModel(PlotModel plotModel, MENTAgedScoreExtractorResult set, IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
		{
			Argument.NotNull(set, nameof(set));
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));

			var generator = SeriesGeneratorProvider.GetSeriesGenerator(visualisation, set, categoryIndexMap);

			foreach (var series in generator.GenerateSeries())
			{
				plotModel.Series.Add(series);
			}
		}

		CategoryAxis CreateYAxis()
		{
			var yAxis = new CategoryAxis
			{
				AbsoluteMinimum = 0,
				MaximumPadding = 0.1,
				MinimumPadding = 0
			};
			AddTitleAndUnitsOnAxis(yAxis, visualisation.YAxisLabel, visualisation.YAxisUnits);

			if (visualisation.ShowHorizontalGridLines)
			{
				ShowGridLinesOnAxis(yAxis);
			}

			return yAxis;
		}

		CategoryAxis CreateCategoryAxis(IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
		{
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));
			var categoryAxis = new CategoryAxis
			{
				MinorStep = 1,
				GapWidth = 1
			};
			AddTitleAndUnitsOnAxis(categoryAxis, visualisation.XAxisLabel, visualisation.XAxisUnits);

			if (visualisation.ShowVerticalGridLines)
			{
				categoryAxis.Position = AxisPosition.Bottom;
				ShowGridLinesOnAxis(categoryAxis);
			}

			foreach (var category in categoryIndexMap)
			{
				categoryAxis.ActualLabels.Add(category.CategoryLabel);
			}

			return categoryAxis;
		}

		void ShowGridLinesOnAxis(LinearAxis axis)
		{
			Argument.NotNull(axis, nameof(axis));

			axis.MinorGridlineStyle = LineStyle.Solid;
			axis.MajorGridlineStyle = LineStyle.Solid;
		}

		void AddTitleAndUnitsOnAxis(LinearAxis axis, ZString title, ZString units)
		{
			Argument.NotNull(axis, nameof(axis));

			axis.Title = title;

			if (units != ZString.Empty)
			{
				axis.Unit = units;
			}
		}
	}
}
