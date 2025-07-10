using CargoWise.Common;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;

namespace Enterprise.PAVE.MENT.Business
{
	public class PlotModelDecorator
	{
		public PlotModelDecorator(MENTAgedScoreVisualisation visualisation, PlotModel model, LinearAxis xAxis, LinearAxis yAxis)
		{
			Argument.NotNull(visualisation, nameof(visualisation));
			Argument.NotNull(model, nameof(model));
			Argument.NotNull(xAxis, nameof(xAxis));
			Argument.NotNull(yAxis, nameof(yAxis));

			this.visualisation = visualisation;
			this.model = model;
		}

		readonly MENTAgedScoreVisualisation visualisation;
		readonly PlotModel model;

		public void Decorate()
		{
			model.Title = visualisation.GraphTitle;
			model.Subtitle = visualisation.Extraction.MEX_Name;
			model.IsLegendVisible = visualisation.ShowLegend;
			model.Legends.Add(new Legend
			{
				IsLegendVisible = visualisation.ShowLegend,
				LegendPlacement = LegendPlacement.Outside
			});

			AddAnnotations();
		}

		void AddAnnotations()
		{
			var band = visualisation.Extraction.RelatedQuery.RelatedAcceptabilityBand;

			if (band != null && visualisation.ShowAcceptabilityBands)
			{
				model.Annotations.Add(CreateAnnotation(band.BAB_CautionLowerBound, LineAnnotationType.Vertical, OxyColors.Red));
				model.Annotations.Add(CreateAnnotation(band.BAB_GoodLowerBound, LineAnnotationType.Vertical, OxyColors.Blue));
				model.Annotations.Add(CreateAnnotation(band.BAB_ExcellentLowerBound, LineAnnotationType.Vertical, OxyColors.Green));
				model.Annotations.Add(CreateAnnotation(band.BAB_ExcellentUpperBound, LineAnnotationType.Vertical, OxyColors.Green));
				model.Annotations.Add(CreateAnnotation(band.BAB_GoodUpperBound, LineAnnotationType.Vertical, OxyColors.Blue));
				model.Annotations.Add(CreateAnnotation(band.BAB_CautionUpperBound, LineAnnotationType.Vertical, OxyColors.Red));
			}
		}

		Annotation CreateAnnotation(decimal x, LineAnnotationType type, OxyColor color)
		{
			return new LineAnnotation
			{
				Type = type,
				Color = color,
				X = (double)x
			};
		}
	}
}
