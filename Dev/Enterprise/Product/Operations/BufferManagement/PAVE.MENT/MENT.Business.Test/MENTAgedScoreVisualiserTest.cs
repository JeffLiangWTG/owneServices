using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using NUnit.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Enterprise.PAVE.MENT.Business.Test
{
	class MENTAgedScoreVisualiserTest : TestCaseWithFactory
	{
		public void TestPlotLabels_ColumnGraph()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "EOP");
			visualisation.MVI_GraphType = GraphTypes.Codes.Column;

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var barSeries = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals("{0}\n{1}: {2}", barSeries.TrackerFormatString);
		}

		public void TestPlotLabels_LineGraph()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "NEP");
			visualisation.MVI_GraphType = GraphTypes.Codes.Line;

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var barSeries = plotModel.Series.OfType<LineSeries>().Single();

			AssertEquals("{0}\n{1}: {2:0.#}\n{3}: {4:0.#}", barSeries.TrackerFormatString);
		}

		public void TestZeroValuesShouldStillVisualise()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "GREGLE");
			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 0));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var points = plotModel.Series.OfType<BarSeries>().First().Items;

			AssertEquals(0d, points.Single().Value);
		}

		public void TestZeroValuesShouldStillVisualise_ZeroValuesAndNonZero()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "GREGLE");
			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 0));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 10));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var points = plotModel.Series.OfType<BarSeries>().First().Items;

			AssertEquals(0d, points.First().Value);
			AssertEquals(10d, points.Last().Value);
			AssertEquals(2, points.Count);
		}

		public void TestEmptySet()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "GREGLE");
			var items = new Collection<MENTDataRow>();

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(string.Empty, plotModel.Title);
		}

		public void TestSingleSeries_SingleCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());
			AssertEquals(10d, plotModel.Series.OfType<BarSeries>().First().Items.First().Value);
		}

		public void TestSingleSeries_TwoCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 20));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());
			var seriesItems = plotModel.Series.OfType<BarSeries>().First().Items.ToArray();
			AssertEquals(2, seriesItems.Length);

			AssertColumnItem(seriesItems[0], 0, 10d);
			AssertColumnItem(seriesItems[1], 1, 20d);
		}

		public void TestTwoSeries_SingleCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(2, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;
			var series2Items = series[1].Items;

			AssertEquals(1, series1Items.Count);
			AssertEquals(1, series2Items.Count);

			AssertColumnItem(series1Items[0], 0, 10d);
			AssertColumnItem(series2Items[0], 0, 20d);
		}

		public void TestTwoSeries_TwoCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(2, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;
			var series2Items = series[1].Items;

			AssertEquals(2, series1Items.Count);
			AssertEquals(2, series2Items.Count);

			AssertColumnItem(series1Items[0], 0, 10d);
			AssertColumnItem(series1Items[1], 1, 30d);

			AssertColumnItem(series2Items[0], 0, 20d);
			AssertColumnItem(series2Items[1], 1, 40d);
		}

		#region LineSeries

		public void TestTwoLineSeries_TwoCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "NARVIN");
			visualisation.MVI_GraphType = GraphTypes.Codes.Line;

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(2, plotModel.Series.OfType<LineSeries>().Count());

			var series = plotModel.Series.OfType<LineSeries>().ToArray();

			var series1Items = series[0].Points;
			var series2Items = series[1].Points;

			AssertEquals(2, series1Items.Count);
			AssertEquals(2, series2Items.Count);

			AssertDataPointItem(series1Items[0], 0d, 10d);
			AssertDataPointItem(series1Items[1], 1d, 30d);

			AssertDataPointItem(series2Items[0], 0d, 20d);
			AssertDataPointItem(series2Items[1], 1d, 40d);
		}

		public void TestTwoLineSeries_TwoCategory_Smooth()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "MARVIN");
			visualisation.MVI_GraphType = GraphTypes.Codes.Line;
			visualisation.SmoothCurve = true;

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(2, plotModel.Series.OfType<LineSeries>().Count());

			var series = plotModel.Series.OfType<LineSeries>().ToArray();
			AssertEquals(true, series.All(s => s.InterpolationAlgorithm == InterpolationAlgorithms.CanonicalSpline));
		}

		#endregion

		#region Normalisation

		public void TestNormalisation_SingleSeries()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "CHAD", isNormalised: true);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10), MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 20), MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 30), MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 40) };

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());
			var series = plotModel.Series.OfType<BarSeries>().First();
			var seriesItems = series.Items;

			AssertEquals(4, seriesItems.Count);

			AssertEquals(0d, seriesItems[0].Value);
			AssertEquals(0, seriesItems[0].CategoryIndex);
			AssertEquals(100 / 3d, seriesItems[1].Value);
			AssertEquals(1, seriesItems[1].CategoryIndex);
			AssertEquals(200 / 3d, seriesItems[2].Value);
			AssertEquals(2, seriesItems[2].CategoryIndex);
			AssertEquals(100d, seriesItems[3].Value);
			AssertEquals(3, seriesItems[3].CategoryIndex);
		}

		public void TestNormalisation_TwoSeries()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TODD", isNormalised: true);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10), MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20), MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30), MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40) };

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Series.OfType<BarSeries>().Count());
			var series = plotModel.Series.OfType<BarSeries>().ToArray();
			var series1Items = series[0].Items;
			var series2Items = series[1].Items;

			AssertEquals(2, series1Items.Count);
			AssertEquals(2, series2Items.Count);

			AssertEquals(0d, series1Items[0].Value);
			AssertEquals(0, series1Items[0].CategoryIndex);
			AssertEquals(200 / 3d, series1Items[1].Value);
			AssertEquals(1, series1Items[1].CategoryIndex);

			AssertEquals(100 / 3d, series2Items[0].Value);
			AssertEquals(0, series2Items[0].CategoryIndex);
			AssertEquals(100d, series2Items[1].Value);
			AssertEquals(1, series2Items[1].CategoryIndex);
		}

		public void TestNormalisation_OverriddenCategories()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "DAHN", isNormalised: true);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10), MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 20), MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 30), MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 40) };

			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride = new VisualisationColumnSpecification(visualisation) { Column = "Category1", Sequence = 0, Selected = true };
			categoryOverride.ColumnDisplay = "Not another display";
			visualisation.CategorySequenceCollection.Add(categoryOverride);

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());
			var series = plotModel.Series.OfType<BarSeries>().First();
			var seriesItems = series.Items;

			AssertEquals(1, seriesItems.Count);

			AssertEquals("Value is 100%", 100d, seriesItems[0].Value);
			AssertEquals(0, seriesItems[0].CategoryIndex);
		}

		#endregion

		public void TestShowGridLines()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "NED");
			var items = new Collection<MENTDataRow>();
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(2, plotModel.Axes.OfType<LinearAxis>().Count());

			visualisation.ShowHorizontalGridLines = true;

			plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);
			AssertEquals(2, plotModel.Axes.OfType<LinearAxis>().Count());
			AssertEquals(1, plotModel.Axes.OfType<LinearAxis>().Count(a => a.MajorGridlineStyle == OxyPlot.LineStyle.Solid && a.MinorGridlineStyle == OxyPlot.LineStyle.Solid));

			visualisation.ShowHorizontalGridLines = false;
			visualisation.ShowVerticalGridLines = true;

			plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);
			AssertEquals(2, plotModel.Axes.OfType<LinearAxis>().Count());
			AssertEquals(1, plotModel.Axes.OfType<LinearAxis>().Count(a => a.MajorGridlineStyle == OxyPlot.LineStyle.Solid && a.MinorGridlineStyle == OxyPlot.LineStyle.Solid && a.Position == AxisPosition.Bottom));

			visualisation.ShowHorizontalGridLines = true;
			visualisation.ShowVerticalGridLines = true;

			plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);
			AssertEquals(2, plotModel.Axes.OfType<LinearAxis>().Count());
			AssertEquals(2, plotModel.Axes.OfType<LinearAxis>().Count(a => a.MajorGridlineStyle == OxyPlot.LineStyle.Solid && a.MinorGridlineStyle == OxyPlot.LineStyle.Solid));
			AssertEquals(2, plotModel.Axes.OfType<CategoryAxis>().Count(a => a.MajorGridlineStyle == OxyPlot.LineStyle.Solid && a.MinorGridlineStyle == OxyPlot.LineStyle.Solid && a.Position == AxisPosition.Bottom));
		}

		public void TestAxesLabels()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TAD");
			var items = new Collection<MENTDataRow>();

			var xLabel = "Xaxis, sounds like craxis";
			var xUnits = "Left turns per airplane";
			var yLabel = "Yaxis, you pronounce it like that Yeeeaxis";
			var yUnits = "Fingernails sneezed per sandwich";

			visualisation.YAxisLabel = yLabel;
			visualisation.YAxisUnits = yUnits;
			visualisation.XAxisLabel = xLabel;
			visualisation.XAxisUnits = xUnits;

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var yAxis = plotModel.Axes.Single(a => a.Title == yLabel);
			AssertEquals(yUnits, yAxis.Unit);

			var xAxis = plotModel.Axes.Single(a => a.Title == xLabel);
			AssertEquals(xUnits, xAxis.Unit);

			visualisation.YAxisUnits = ZString.Empty;
			visualisation.XAxisUnits = ZString.Empty;

			plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(true, plotModel.Axes.All(a => a.Unit == null));
		}

		public void TestDisallowZoom()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TAD");
			var items = new Collection<MENTDataRow>();

			visualisation.AllowZoom = false;

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(true, plotModel.Axes.All(a => !a.IsZoomEnabled));

			visualisation.AllowZoom = true;

			plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			AssertEquals(true, plotModel.Axes.All(a => a.IsZoomEnabled));
		}

		#region CategoryOverride

		public void TestCategoryIsOverridden_DifferentTitles_Single()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Peter");
			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride = new VisualisationColumnSpecification(visualisation) { Column = "A", Sequence = 0, Selected = true };
			categoryOverride.ColumnDisplay = "Not another display";
			visualisation.CategorySequenceCollection.Add(categoryOverride);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(1, categoryAxis.ActualLabels.Count);

			AssertEquals("Not another display", categoryAxis.ActualLabels.Single());
		}

		public void TestCategoryIsOverridden_OnlyIncludeCategorysThatExistInCollection()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Nial");
			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride = new VisualisationColumnSpecification(visualisation) { Column = "B", Sequence = 0, Selected = true };
			categoryOverride.ColumnDisplay = "A single lone person";
			visualisation.CategorySequenceCollection.Add(categoryOverride);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 10), MENTTestHelper.CreateMENTDataRow("S1", "B", 11), MENTTestHelper.CreateMENTDataRow("S1", "C", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(1, categoryAxis.ActualLabels.Count);

			AssertEquals("A single lone person", categoryAxis.ActualLabels.Single());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;

			AssertEquals(11d, series1Items[0].Value);
		}

		public void TestCategoryIsOverridden_ValuesDontExistForTitle()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Steve");
			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride = new VisualisationColumnSpecification(visualisation) { Column = "B", Sequence = 0, Selected = true };
			categoryOverride.ColumnDisplay = "A single lone person";
			visualisation.CategorySequenceCollection.Add(categoryOverride);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 10), MENTTestHelper.CreateMENTDataRow("S1", "C", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(1, categoryAxis.ActualLabels.Count);

			AssertEquals("A single lone person", categoryAxis.ActualLabels.Single());

			AssertEquals(0, plotModel.Series.OfType<BarSeries>().Count());
		}

		public void TestCategoryIsOverridden_SequencingObeysOrder()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Nathan");
			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride1 = new VisualisationColumnSpecification(visualisation) { Column = "B", Sequence = 0, Selected = true };
			categoryOverride1.ColumnDisplay = "A single lone person";
			var categoryOverride2 = new VisualisationColumnSpecification(visualisation) { Column = "A", Sequence = 1, Selected = true };
			categoryOverride2.ColumnDisplay = "A hippo";
			var categoryOverride3 = new VisualisationColumnSpecification(visualisation) { Column = "C", Sequence = 2, Selected = true };
			categoryOverride3.ColumnDisplay = "A grandmother";
			var categoryOverride4 = new VisualisationColumnSpecification(visualisation) { Column = "D", Sequence = 3, Selected = true };
			categoryOverride4.ColumnDisplay = "On a bus";
			visualisation.CategorySequenceCollection.Add(categoryOverride1);
			visualisation.CategorySequenceCollection.Add(categoryOverride2);
			visualisation.CategorySequenceCollection.Add(categoryOverride3);
			visualisation.CategorySequenceCollection.Add(categoryOverride4);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 20), MENTTestHelper.CreateMENTDataRow("S1", "D", 11), MENTTestHelper.CreateMENTDataRow("S1", "B", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(4, categoryAxis.ActualLabels.Count);

			AssertEquals("A single lone person", categoryAxis.ActualLabels[0]);
			AssertEquals("A hippo", categoryAxis.ActualLabels[1]);
			AssertEquals("A grandmother", categoryAxis.ActualLabels[2]);
			AssertEquals("On a bus", categoryAxis.ActualLabels[3]);

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;

			AssertEquals(3, series1Items.Count);

			AssertColumnItem(series1Items[0], 0, 10d);
			AssertColumnItem(series1Items[1], 1, 20d);
			AssertColumnItem(series1Items[2], 3, 11d);
		}

		public void TestCategoryIsOverridden_SequencingObeysOrder_SameSequence()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Ned");
			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride1 = new VisualisationColumnSpecification(visualisation) { Column = "B", Sequence = 0, Selected = true };
			categoryOverride1.ColumnDisplay = "A single lone person";
			var categoryOverride2 = new VisualisationColumnSpecification(visualisation) { Column = "A", Sequence = 0, Selected = true };
			categoryOverride2.ColumnDisplay = "A hippo";
			visualisation.CategorySequenceCollection.Add(categoryOverride1);
			visualisation.CategorySequenceCollection.Add(categoryOverride2);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 10), MENTTestHelper.CreateMENTDataRow("S1", "C", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(2, categoryAxis.ActualLabels.Count);

			AssertEquals("A hippo comes before a single lone person alphabetically", "A hippo", categoryAxis.ActualLabels[0]);
			AssertEquals("A single lone person", categoryAxis.ActualLabels[1]);
		}

		public void TestCategoryIsOverridden_OnlyIncludesSelected()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Charmander");
			visualisation.UseOverriddenCategorySequence = true;
			var categoryOverride1 = new VisualisationColumnSpecification(visualisation) { Column = "B", Sequence = 0, Selected = false };
			categoryOverride1.ColumnDisplay = "A single lone person";
			var categoryOverride2 = new VisualisationColumnSpecification(visualisation) { Column = "A", Sequence = 1, Selected = true };
			categoryOverride2.ColumnDisplay = "A hippo";
			visualisation.CategorySequenceCollection.Add(categoryOverride1);
			visualisation.CategorySequenceCollection.Add(categoryOverride2);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 10), MENTTestHelper.CreateMENTDataRow("S1", "C", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(1, categoryAxis.ActualLabels.Count);

			AssertEquals("A hippo", categoryAxis.ActualLabels.Single());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;

			AssertEquals(10d, series1Items[0].Value);
		}

		public void TestCategoryOverridden_SetToNotOverride()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Geoff");
			visualisation.UseOverriddenCategorySequence = false;
			var categoryOverride = new VisualisationColumnSpecification(visualisation) { Column = "A", Sequence = 0, Selected = true };
			categoryOverride.ColumnDisplay = "Not another display";
			visualisation.CategorySequenceCollection.Add(categoryOverride);

			var items = new Collection<MENTDataRow> { MENTTestHelper.CreateMENTDataRow("S1", "A", 10) };
			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(1, categoryAxis.ActualLabels.Count);

			AssertEquals("A", categoryAxis.ActualLabels.Single());
		}

		#endregion

		#region CategoryAggregation

		public void TestAggregateLowerBoundContiguousColumn()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TEX");

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 3;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", true, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(2, categoryAxis.ActualLabels.Count);
			AssertEquals("Category4", categoryAxis.ActualLabels[0]);
			AssertEquals("Category5", categoryAxis.ActualLabels[1]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(2, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 114d);
			AssertColumnItem(series.Items[1], 1, 56d);
		}

		public void TestAggregateLowerBoundNotEnabled()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "DARYL");

			visualisation.PerformLowerBoundAggregation = false;
			visualisation.LowerBoundAggregationSequence = 1;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(3, categoryAxis.ActualLabels.Count);
			AssertEquals("Category1", categoryAxis.ActualLabels[0]);
			AssertEquals("Category2", categoryAxis.ActualLabels[1]);
			AssertEquals("Category3", categoryAxis.ActualLabels[2]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(3, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 12d);
			AssertColumnItem(series.Items[1], 1, 23d);
			AssertColumnItem(series.Items[2], 2, 34d);
		}

		public void TestAggregateLowerBoundSkipColumns()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "JAX");

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 3;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", false, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", false, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", true, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(2, categoryAxis.ActualLabels.Count);
			AssertEquals("Category4", categoryAxis.ActualLabels[0]);
			AssertEquals("Category5", categoryAxis.ActualLabels[1]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(2, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 57d);
			AssertColumnItem(series.Items[1], 1, 56d);
		}

		public void TestAggregateUpperBoundContiguousColumns()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "MEX");

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 1;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", true, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(2, categoryAxis.ActualLabels.Count);
			AssertEquals("Category1", categoryAxis.ActualLabels[0]);
			AssertEquals("Category2", categoryAxis.ActualLabels[1]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(2, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 12d);
			AssertColumnItem(series.Items[1], 1, 158d);
		}

		public void TestAggregateUpperBoundNotEnabled()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "DARYL");

			visualisation.PerformUpperBoundAggregation = false;
			visualisation.UpperBoundAggregationSequence = 1;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(3, categoryAxis.ActualLabels.Count);
			AssertEquals("Category1", categoryAxis.ActualLabels[0]);
			AssertEquals("Category2", categoryAxis.ActualLabels[1]);
			AssertEquals("Category3", categoryAxis.ActualLabels[2]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(3, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 12d);
			AssertColumnItem(series.Items[1], 1, 23d);
			AssertColumnItem(series.Items[2], 2, 34d);
		}

		public void TestAggregateUpperBoundSkipsColumns()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "KFC");

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 1;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", false, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", false, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(2, categoryAxis.ActualLabels.Count);
			AssertEquals("Category1", categoryAxis.ActualLabels[0]);
			AssertEquals("Category2", categoryAxis.ActualLabels[1]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(2, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 12d);
			AssertColumnItem(series.Items[1], 1, 79d);
		}

		public void TestAggregateUpperBound_ShouldNotThowExceptionIfUpperBoundColumnsAreNotSelected()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "KFC");

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 3;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", false, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", false, 4);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", false, 5);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var message = "When trying to visualise an upper bound with no selected column, System.InvalidOperationException should not be thrown";
			AssertNoExceptionThrown(message, () => MENTTestHelper.CreateAndRunVisualiser(visualisation, items));
		}

		public void TestAggregateBothBounds()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "MACCAS");

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 1;

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 5;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", true, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category6", "Category6", true, 5);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category7", "Category7", true, 6);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(5, categoryAxis.ActualLabels.Count);
			AssertEquals("Category2", categoryAxis.ActualLabels[0]);
			AssertEquals("Category3", categoryAxis.ActualLabels[1]);
			AssertEquals("Category4", categoryAxis.ActualLabels[2]);
			AssertEquals("Category5", categoryAxis.ActualLabels[3]);
			AssertEquals("Category6", categoryAxis.ActualLabels[4]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(5, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 35d);
			AssertColumnItem(series.Items[1], 1, 34d);
			AssertColumnItem(series.Items[2], 2, 45d);
			AssertColumnItem(series.Items[3], 3, 56d);
			AssertColumnItem(series.Items[4], 4, 145d);
		}

		public void TestAggregateWithNoOverriddenColumns()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Daytona 500");

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 2;

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 4;
			visualisation.UseOverriddenCategorySequence = false;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", true, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category6", "Category6", true, 5);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category7", "Category7", true, 6);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryA", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryB", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryC", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryD", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryE", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryF", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "CategoryG", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(7, categoryAxis.ActualLabels.Count);
			AssertEquals("CategoryA", categoryAxis.ActualLabels[0]);
			AssertEquals("CategoryB", categoryAxis.ActualLabels[1]);
			AssertEquals("CategoryC", categoryAxis.ActualLabels[2]);
			AssertEquals("CategoryD", categoryAxis.ActualLabels[3]);
			AssertEquals("CategoryE", categoryAxis.ActualLabels[4]);
			AssertEquals("CategoryF", categoryAxis.ActualLabels[5]);
			AssertEquals("CategoryG", categoryAxis.ActualLabels[6]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(7, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 12d);
			AssertColumnItem(series.Items[1], 1, 23d);
			AssertColumnItem(series.Items[2], 2, 34d);
			AssertColumnItem(series.Items[3], 3, 45d);
			AssertColumnItem(series.Items[4], 4, 56d);
			AssertColumnItem(series.Items[5], 5, 67d);
			AssertColumnItem(series.Items[6], 6, 78d);
		}

		public void TestAggregateWithNoGapBetweenAggregationPoints()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Ford Millenium Falcon XR6");

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 2;

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 3;
			visualisation.UseOverriddenCategorySequence = true;

			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category1", "Category1", true, 0);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category2", "Category2", true, 1);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category3", "Category3", true, 2);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category4", "Category4", true, 3);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category5", "Category5", true, 4);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category6", "Category6", true, 5);
			MENTTestHelper.CreateAndAddVisualisationColumn(visualisation, visualisation.CategorySequenceCollection, "Category7", "Category7", true, 6);

			var items = new Collection<MENTDataRow>();
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 12));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 23));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 34));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 45));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category5", 56));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category6", 67));
			items.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category7", 78));

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, items);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			AssertEquals(2, categoryAxis.ActualLabels.Count);
			AssertEquals("Category3", categoryAxis.ActualLabels[0]);
			AssertEquals("Category4", categoryAxis.ActualLabels[1]);

			AssertEquals(1, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().Single();

			AssertEquals(2, series.Items.Count);

			AssertColumnItem(series.Items[0], 0, 69d);
			AssertColumnItem(series.Items[1], 1, 246d);
		}

		#endregion

		public void TestXCategoryOrdering()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TAD");

			var items1 = new Collection<MENTDataRow>();
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDecimal(40m) }, 10));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDecimal(30m) }, 20));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDecimal(10m) }, 30));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDecimal(20m) }, 40));

			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "Probincrux");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1 });

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, extractorResult);

			AssertEquals(2, plotModel.Axes.Count);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			var labels = categoryAxis.ActualLabels.ToArray();

			AssertEquals("10", labels[0]);
			AssertEquals("20", labels[1]);
			AssertEquals("30", labels[2]);
			AssertEquals("40", labels[3]);
		}

		public void TestXCategoryOrdering_Date()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "TAD");

			var items1 = new Collection<MENTDataRow>();
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDateTime(2015, 08, 31) }, 10));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDateTime(2015, 06, 2) }, 20));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDateTime(2015, 12, 10) }, 30));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", new IZType[] { new ZDateTime(2015, 01, 1) }, 40));

			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "Probincrux");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1 });

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, extractorResult);

			AssertEquals(2, plotModel.Axes.Count);

			var categoryAxis = plotModel.Axes.OfType<CategoryAxis>().First();

			var labels = categoryAxis.ActualLabels.ToArray();

			AssertEquals("01-01-2015", labels[0]);
			AssertEquals("02-06-2015", labels[1]);
			AssertEquals("31-08-2015", labels[2]);
			AssertEquals("10-12-2015", labels[3]);
		}

		public void TestMultipleExtractionResultSet_SameCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items1 = new Collection<MENTDataRow>();
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40));

			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "Probincrux");

			var items2 = new Collection<MENTDataRow>();
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series3", "Category1", 10));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series4", "Category1", 20));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series3", "Category2", 30));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series4", "Category2", 40));

			var extractionResult2 = new MENTAgedScoreExtractionResult(items2, "LgoodingSpaltt");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1, extractionResult2 });

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, extractorResult);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(4, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;
			var series2Items = series[1].Items;
			var series3Items = series[2].Items;
			var series4Items = series[3].Items;

			AssertEquals(2, series1Items.Count);
			AssertEquals(2, series2Items.Count);
			AssertEquals(2, series3Items.Count);
			AssertEquals(2, series4Items.Count);
		}

		public void TestMultipleExtractionResultSet_DifferentCategory()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items1 = new Collection<MENTDataRow>();
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40));

			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "Probincrux");

			var items2 = new Collection<MENTDataRow>();
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series3", "Category3", 10));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series4", "Category3", 20));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series3", "Category4", 30));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series4", "Category4", 40));

			var extractionResult2 = new MENTAgedScoreExtractionResult(items2, "LgoodingSpaltt");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1, extractionResult2 });

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, extractorResult);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(4, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			var series1Items = series[0].Items;
			var series2Items = series[1].Items;
			var series3Items = series[2].Items;
			var series4Items = series[3].Items;

			AssertEquals(2, series1Items.Count);
			AssertEquals(2, series2Items.Count);
			AssertEquals(2, series3Items.Count);
			AssertEquals(2, series4Items.Count);
		}

		public void TestMultipleExtractionResultSet_SameSeries()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "ERVIN");

			var items1 = new Collection<MENTDataRow>();
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category1", 10));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category1", 20));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category2", 30));
			items1.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category2", 40));

			var extractionResult1 = new MENTAgedScoreExtractionResult(items1, "Probincrux");

			var items2 = new Collection<MENTDataRow>();
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category3", 10));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category3", 20));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series1", "Category4", 30));
			items2.Add(MENTTestHelper.CreateMENTDataRow("Series2", "Category4", 40));

			var extractionResult2 = new MENTAgedScoreExtractionResult(items2, "LgoodingSpaltt");

			var extractorResult = new MENTAgedScoreExtractorResult(new[] { extractionResult1, extractionResult2 });

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, extractorResult);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(4, plotModel.Series.OfType<BarSeries>().Count());

			var series = plotModel.Series.OfType<BarSeries>().ToArray();

			AssertEquals("Probincrux - Series1", series[0].Title);
			AssertEquals("Probincrux - Series2", series[1].Title);
			AssertEquals("LgoodingSpaltt - Series1", series[2].Title);
			AssertEquals("LgoodingSpaltt - Series2", series[3].Title);
		}

		#region Implementation

		public void AssertColumnItem(BarItem actual, int expectedColumnIndex, double expectedColumnValue)
		{
			AssertNotNull(actual);
			AssertEquals(expectedColumnIndex, actual.CategoryIndex);
			AssertEquals(expectedColumnValue, actual.Value);
		}

		public void AssertDataPointItem(DataPoint actual, double expectedX, double expectedY)
		{
			AssertNotNull(actual);
			AssertEquals(expectedX, actual.X);
			AssertEquals(expectedY, actual.Y);
		}

		#endregion
	}

	[UseSnapshotProtection]
	class MENTAgedScoreVisualiserNonTransactionedTest : TestCase
	{
		public void TestExtractor_NoResults_VisualisesCorrectly()
		{
			var factory = new BusinessObjectFactory();
			var query = MENTTestHelper.CreateQuery(factory, "Quattro");
			var extraction = MENTTestHelper.CreateExtraction(factory, "DanSmith", query, aggregationType: ExtractionTypes.Codes.Sum, collectionColumn: MENTColumns.Codes.Score);
			var visualisation = MENTTestHelper.CreateVisualisation(factory, "ERVIN", extraction);

			factory.Save();

			var extractor = new MENTAgedScoreExtractor(extraction, new MENTTestHelper.TestExtractorFactoryProvider());

			var extractorResult = extractor.Extract();
			var results = extractorResult.ExtractionResults.ToArray(); // Trigger Enumeration

			var plotModel = MENTTestHelper.CreateAndRunVisualiser(visualisation, extractorResult);

			AssertEquals(2, plotModel.Axes.Count);
			AssertEquals(0, plotModel.Series.OfType<BarSeries>().Count());
		}

		protected override void SetUp()
		{
			MENTTestHelper.ClearMENTTables();
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
