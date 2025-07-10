using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class DynamicGridTest : TestCase
	{
		#region Populate Backgrounds

		public void TestPopulateBackgrounds_ShouldNotThrowNegativeWidthException()
		{
			const int desiredColumnWidth = 100;
			const int greaterThanFour = 5;
			const int startColumn = 20;
			const int endColumn = 21;

			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
			{
				grid.RenderSize = new Size(endColumn * desiredColumnWidth, greaterThanFour);
				AssertNoExceptionThrown("When this is called with ScaleSet and RenderSize set as above, PopulateBackgrounds should not throw a negative width exception and yet...", grid.RefreshScale);
			}, startColumn, endColumn);
		}

		public void TestPopulateBackgrounds_RectangleDimensions()
		{
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
			{
				CombineAssertions(() =>
				{
					AssertEquals(100d, point.X);
					AssertEquals(100d, rectangle.ActualWidth);

					AssertEquals("The background should start at the very top of the grid so we don't have a little gap of background colour. SAD!", -1d, point.Y);
					AssertGreaterThan("The background should go all the way to the bottom of the grid so we don't have a little gap of background colour. SAD!", rectangle.ActualHeight, grid.ActualHeight);
				});
			});
		}

		public void TestPopulateBackgrounds_RectangleDimensions_FirstColumn()
		{
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
			{
				CombineAssertions(() =>
				{
					AssertEquals("The first column should start a little to the right so that the background colour doesn't overlap the diagram borders. SAD!", 1d, point.X);
					AssertEquals(100d, rectangle.ActualWidth);

					AssertEquals(-1d, point.Y);
					AssertGreaterThan(rectangle.ActualHeight, grid.ActualHeight);
				});
			}, 0, 1);
		}

		public void TestPopulateBackgrounds_ForDiagramWithChannels_ShouldMoveBackgroundsToMakeRoomForChannelHeadings()
		{
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
			{
				AssertEquals("The background should have moved to the right to accommodate the channel headers. SAD!", 200d, point.X);
			}, channels: DummyChannel.Create("Fleeb", "Shlamie", "Non-channeled"));
		}

		static void PerformColumnBackgroundRelatedTest(Action<DynamicGrid, Rectangle, Point> testAction, int startColumn = 1, int endColumn = 2, IEnumerable<DummyChannel> channels = null, bool isNonScheduled = false)
		{
			var scaleDescriptor = new Mock<INetworkScaleDescriptor>();

			var networkScalePointInterface = new NetworkScalePoint(0, "TIME", string.Empty);
			var listOfNetworkScalePointInterfaces = new List<INetworkScalePoint>(1) { networkScalePointInterface };
			var scaleBackgroundInterface = new NetworkScaleBackground(System.Drawing.Color.Red, startColumn, endColumn);
			var listOfscaleBackgroundInterfaces = new List<INetworkScaleBackground>(1) { scaleBackgroundInterface };
			var scaleSet = new NetworkScaleSet(listOfNetworkScalePointInterfaces.AsReadOnly(), listOfscaleBackgroundInterfaces.AsReadOnly(), endColumn + 1);

			scaleDescriptor.Setup(m => m.GetScaleSetForColumns(It.IsAny<int>())).Returns(scaleSet);

			var diagram = new Entity();

			if (channels != null)
			{
				diagram.DiagramChannels = channels;
			}

			var network = (DummyNetwork)DummyNetwork.GetDummyNetwork(false);
			network.DiagramEntity = diagram;
			var viewModel = new NetworkUserControlViewModel(network);
			var diagramViewModel = new DiagramAreaUserControlViewModel(viewModel, isNonScheduled);

			var grid = new DynamicGrid { DataContext = diagramViewModel, ScaleDescriptor = scaleDescriptor.Object, Height = 800, Width = 1200 };
			var window = new Window { Content = grid, Height = 1000 };

			try
			{
				window.Show();

				var rectangle = grid.FindChildren<Rectangle>().SingleOrDefault(b => (b.Fill as SolidColorBrush)?.Color == Colors.Red);
				var upperLeftCorner = rectangle != null ? NetworkTestHelper.GetRelativePosition(rectangle, grid) : default(Point);

				testAction(grid, rectangle, upperLeftCorner);
			}
			finally
			{
				window.Close();
			}
		}

		#endregion

		#region Channels

		public void TestGrid_ForNonChanneledDiagram_ShouldNotDrawHorizontalLines()
		{
			AssertHorizontalLines("There are no channels, so no horizontal lines should be drawn. SAD!", Array.Empty<IDiagramChannel>(), Array.Empty<double>());
		}

		public void TestGrid_ForChanneledDiagram_ShouldDrawHorizontalLines()
		{
			AssertHorizontalLines("Two lines should have been drawn for the channels at the correct positions. SAD!", new[] { new DummyChannel("Shmlonathan", 420), new DummyChannel("Shmlangela", 931), new DummyChannel("Non-channeled") }, new[] { 420d, 1351d });
		}

		public void TestGrid_ForChanneledDiagram_WithShapeBelowFinalChannelHeight_ShouldNotDrawExtraHorizontalLine()
		{
			var diagram = new Entity { DiagramChannels = new[] { new DummyChannel("Shmlonathan", 420), new DummyChannel("Shmlangela", 931), new DummyChannel("Non-channeled") } };
			var network = (DummyNetwork)DummyNetwork.GetDummyNetwork(false);
			network.DiagramEntity = diagram;
			var entity = new Entity { Y = 4000 };
			entity.Parent = diagram;
			network.Entities.Add(entity);

			AssertHorizontalLines("Two lines should have been drawn for the channels at the correct positions, with no extra line for the final channel. SAD!", network, new[] { 420d, 1351d }, height: 4000);
		}

		public void TestGrid_ForChanneledDiagram_WithChannelHeightTooLow_ShouldUseMinimumHeightValue()
		{
			AssertHorizontalLines("The minimum channel height should be used. SAD!", new[] { new DummyChannel("Shmlonathan", 69), new DummyChannel("Non-channeled") }, new[] { 100d });
		}

		public void TestGrid_ForChanneledDiagram_WithChannelHeightTooHigh_ShouldUseMaximumHeightValue()
		{
			AssertHorizontalLines("The maximum channel height should be used. SAD!", new[] { new DummyChannel("Shmlonathan", 3000), new DummyChannel("Non-channeled") }, new[] { 2000d });
		}

		static void AssertHorizontalLines(string message, INetwork network, IEnumerable<double> expectedLinePositions, int? height = null)
		{
			AssertHorizontalLines(message, CreateAndShowGrid(network, height), expectedLinePositions);
		}

		static void AssertHorizontalLines(string message, IEnumerable<IDiagramChannel> channels, IEnumerable<double> expectedLinePositions)
		{
			AssertHorizontalLines(message, CreateAndShowGrid(channels), expectedLinePositions);
		}

		static void AssertHorizontalLines(string message, DynamicGrid grid, IEnumerable<double> expectedLinePositions)
		{
			var lines = NetworkTestHelper.GetChannelLines(grid);
			AssertContainsExactElementsInAnyOrder(message, expectedLinePositions, lines.Select(line => line.Y1));
		}

		public void TestChannelLines_Style()
		{
			var grid = CreateAndShowGrid(DummyChannel.Create("Squanch", "Non-channeled"));
			var line = NetworkTestHelper.GetChannelLines(grid).Single();

			AssertEquals(1d, line.StrokeThickness);
			AssertEquals(Brushes.Silver, line.Stroke);
			AssertContainsExactElementsInAnyOrder(Array.Empty<double>(), line.StrokeDashArray);
		}

		public void TestChannelHeaders_Style()
		{
			var grid = CreateAndShowGrid(DummyChannel.Create("Fleeb", "Shlamie", "Non-channeled"));

			foreach (var header in NetworkTestHelper.GetChannelHeaders(grid))
			{
				AssertEquals(TextAlignment.Center, header.TextAlignment);
				AssertEquals(VerticalAlignment.Center, header.VerticalAlignment);
				AssertEquals(TextWrapping.Wrap, header.TextWrapping);
				AssertEquals(24d, header.FontSize);
				AssertEquals(FontWeight.FromOpenTypeWeight(6), header.FontWeight);
				AssertEquals(Brushes.Black.ToString(), header.Foreground.ToString());

				var border = (Border)header.Parent;
				AssertEquals(270d, ((RotateTransform)border.LayoutTransform).Angle);
			}
		}

		public void TestChannels_ForSmallControl_ShouldNotThrowExceptions()
		{
			AssertNoExceptionThrown("Creating a small grid should not throw an exception in the drawing of the channels. SAD!", () => CreateAndShowGrid(DummyChannel.Create("Fleeb", "Shlamie", "Non-channeled"), 400));
		}

		#endregion

		#region NonScheduled Section

		public void TestScheduledGrid_WithChannels_ShouldHaveTimeScaleLinesAndLabels()
		{
			var channels = new[] { new DummyChannel("Lenny", 200), new DummyChannel("Non-Channeled", 200) };
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
				AssertScaleLinesAndLabels("A channeled, scheduled grid should have time scale lines and labels. SAD!", grid, shouldLinesAndLabelsBePresent: true), channels: channels, isNonScheduled: false);
		}

		public void TestScheduledGrid_WithoutChannels_ShouldHaveTimeScaleLinesAndLabels()
		{
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
				AssertScaleLinesAndLabels("A non-channeled, scheduled grid should have time scale lines and labels. SAD!", grid, shouldLinesAndLabelsBePresent: true), isNonScheduled: false);
		}

		public void TestNonScheduledGrid_WithChannels_ShouldNotHaveTimeScaleLinesOrLabels()
		{
			var channels = new[] { new DummyChannel("Lenny", 200), new DummyChannel("Non-Channeled", 200) };
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
				AssertScaleLinesAndLabels("A channeled, non-scheduled grid should not have time scale lines and labels. SAD!", grid, shouldLinesAndLabelsBePresent: false), channels: channels, isNonScheduled: true);
		}

		public void TestNonScheduledGrid_WithoutChannels_ShouldNotHaveTimeScaleLinesOrLabels()
		{
			PerformColumnBackgroundRelatedTest((grid, rectangle, point) =>
				AssertScaleLinesAndLabels("A non-channeled, non-scheduled grid should not have time scale lines and labels. SAD!", grid, shouldLinesAndLabelsBePresent: false), isNonScheduled: true);
		}

		static void AssertScaleLinesAndLabels(string message, DynamicGrid grid, bool shouldLinesAndLabelsBePresent)
		{
			var lines = grid.FindChildren<Line>().Where(x => x.Tag?.ToString() == "TimeScaleLine");
			AssertEquals(message, shouldLinesAndLabelsBePresent, lines.Any());

			var labels = grid.FindChildren<TextBlock>().Where(x => x.Tag?.ToString() == "TimeScaleLabel");
			AssertEquals(message, shouldLinesAndLabelsBePresent, labels.Any());
		}

		public void TestNonScheduledGrid_ForDiagramWithChannels_ShouldHaveChannelHeaders()
		{
			var grid = CreateAndShowGrid(DummyChannel.Create("Lenny", "Karl", "Non-channeled"), isNonScheduled: true);
			var headers = NetworkTestHelper.GetChannelHeaders(grid);

			AssertContainsExactElementsInAnyOrder("Channel headers should be shown on a non-scheduled grid. SAD!", new[] { "Lenny", "Karl", "Non-channeled" }, headers.Select(x => x.Text));
		}

		#endregion

		#region Implementation

		static DynamicGrid CreateAndShowGrid(IEnumerable<IDiagramChannel> channels, int? height = null, bool isNonScheduled = false)
		{
			var diagram = new Entity { DiagramChannels = channels, ForeColor = System.Drawing.Color.Black };
			var network = (DummyNetwork)DummyNetwork.GetDummyNetwork(false);
			network.DiagramEntity = diagram;

			return CreateAndShowGrid(network, height, isNonScheduled);
		}

		static DynamicGrid CreateAndShowGrid(INetwork network, int? height = null, bool isNonScheduled = false)
		{
			var viewModel = new NetworkUserControlViewModel(network);
			var areaViewModel = new DiagramAreaUserControlViewModel(viewModel, false);

			if (height.HasValue)
			{
				areaViewModel.ContentHeight = height.Value;
			}

			var grid = new DynamicGrid { DataContext = areaViewModel };
			_ = new Window { Content = grid };

			return grid;
		}

		#endregion
	}

	class NetworkScaleBackground : INetworkScaleBackground
	{
		internal NetworkScaleBackground(System.Drawing.Color color, int startColumn, int endColumn)
		{
			this.color = color;
			this.startColumn = startColumn;
			this.endColumn = endColumn;
		}
		readonly System.Drawing.Color color;
		readonly int startColumn;
		readonly int endColumn;

		#region INetworkScaleBackground

		public System.Drawing.Color Color => color;
		public int StartColumn => startColumn;
		public int EndColumn => endColumn;

		#endregion
	}

	class NetworkScalePoint : INetworkScalePoint
	{
		internal NetworkScalePoint(int column, string label, string toolTip)
		{
			this.column = column;
			this.label = label;
			this.toolTip = toolTip;
		}

		readonly int column;
		readonly string label;
		readonly string toolTip;

		#region INetworkScalePoint Implementation

		public int Column => column;
		public string Label => label;
		public string ToolTip => toolTip;

		#endregion
	}
}
