using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class DynamicGrid : Canvas
	{
		#region Dependency Properties

		public static readonly DependencyProperty ScaleDescriptorProperty = DependencyProperty.Register("ScaleDescriptor", typeof(INetworkScaleDescriptor), typeof(DynamicGrid));

		public INetworkScaleDescriptor ScaleDescriptor
		{
			get
			{
				if (DataContext is DiagramAreaUserControlViewModel viewModel && viewModel.IsNonScheduled)
				{
					return null;
				}

				return (INetworkScaleDescriptor)GetValue(ScaleDescriptorProperty);
			}
			set => SetValue(ScaleDescriptorProperty, value);
		}

		#endregion

		public int PresentColumnIndex => ScaleSet.IndexOfColumnInPresent;

		public INetworkScaleSet ScaleSet => scaleSet ?? NetworkScaleSet.Empty;

		void InitScaleSet(int columns)
		{
			scaleSet = ScaleDescriptor?.GetScaleSetForColumns(columns);
		}

		INetworkScaleSet scaleSet;

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			RecreateGrid();
			SizeChanged += DynamicGrid_SizeChanged;
		}

		#region Grid Lines

		void DynamicGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			RecreateGrid();
		}

		public void RefreshScale()
		{
			RecreateGrid();
		}

		void RecreateGrid()
		{
			if (ScaleDescriptor != null && ScaleDescriptor.IsScaleDescriptorInvalidated)
			{
				return;
			}

			var viewModel = DataContext as DiagramAreaUserControlViewModel;

			if (viewModel?.NetworkControlViewModel?.DiagramEntity != null)
			{
				RecreateGrid(viewModel);
			}
		}

		double GridHeight => ActualHeight - (GetHeaderHeight?.Invoke() ?? 0);

		public Func<double> GetHeaderHeight { get; set; }

		bool hasChannels;
		int columnWidth, numberOfColumnsRequired, numberOfMergedColumns;
		double lastColumnWidth;

		void RecreateGrid(DiagramAreaUserControlViewModel viewModel)
		{
			Children.Clear();
			var scaleRatio = viewModel.NetworkControlViewModel.ContentScale;
			var diagramEntity = viewModel.NetworkControlViewModel.DiagramEntity;
			var scaleType = GetScaleType(scaleRatio);
			numberOfMergedColumns = GetNumberOfMergedColumns(scaleType);

			var channelAndRangeTuples = viewModel.NetworkControlViewModel.NetworkViewModel.GetChannelYAxisRanges().ToArray();
			hasChannels = channelAndRangeTuples.Any();
			var currentWidth = ActualWidth; // Eliminate round corner

			columnWidth = diagramEntity.ScaleUnitPixelSize;
			var channelHeadersWidth = hasChannels ? columnWidth : 0;
			var currentWidthExcludingChannelHeaders = currentWidth - channelHeadersWidth;

			numberOfColumnsRequired = (int)(currentWidthExcludingChannelHeaders / columnWidth);
			lastColumnWidth = currentWidthExcludingChannelHeaders % columnWidth;

			if (lastColumnWidth > 0 && !diagramEntity.IsDiagramSurfaceFixed)
			{
				numberOfColumnsRequired++;
			}

			if (hasChannels)
			{
				numberOfColumnsRequired++;
			}

			PopulateBackgroundsIfNeeded();

			if (hasChannels)
			{
				AddChannels(viewModel, channelHeadersWidth, channelAndRangeTuples);
			}

			AddColumns(numberOfColumnsRequired, numberOfMergedColumns, hasChannels, columnWidth, scaleType, scaleRatio, viewModel);
		}

		internal void PopulateBackgroundsIfNeeded()
		{
			var diagramEntity = (DataContext as DiagramAreaUserControlViewModel)?.NetworkControlViewModel?.DiagramEntity;
			if (diagramEntity == null)
			{
				return;
			}

			if (ScaleDescriptor != null)
			{
				ScaleDescriptor.ClearCachedData();
			}

			DepopulateBackgrounds();

			if (numberOfColumnsRequired > 0)
			{
				InitScaleSet(Ceiling(numberOfColumnsRequired, numberOfMergedColumns));
			}

			if (GridHeight > 4)
			{
				var leftMarginForBackgrounds = hasChannels ? columnWidth : 0;
				PopulateBackgrounds(columnWidth, lastColumnWidth, numberOfColumnsRequired, diagramEntity, leftMarginForBackgrounds);
			}
		}

		void DepopulateBackgrounds()
		{
			for (int i = Children.Count - 1; i >= 0; i--)
			{
				if (Children[i] is Rectangle)
				{
					Children.RemoveAt(i);
				}
			}
		}

		const int BottomMarginOffset = Constants.DynamicGridBottomMargin * 2;

		void AddColumns(int numberOfColumnsRequired, int numberOfMergedColumns, bool hasChannels, int columnWidth, ScaleType scaleType, double scaleRatio, DiagramAreaUserControlViewModel viewModel)
		{
			var isNonScheduled = viewModel.IsNonScheduled;
			for (var i = 0; i < numberOfColumnsRequired; i += numberOfMergedColumns)
			{
				var isChannelHeaderColumn = hasChannels && i == 0;

				if (!isChannelHeaderColumn && isNonScheduled)
				{
					break;
				}

				var xOffset = columnWidth * i;
				var line = isChannelHeaderColumn ? CreateBoldBlackLine() : CreateColumnLine(scaleType);

				if (line != null)
				{
					line.X1 = xOffset + columnWidth;
					line.X2 = xOffset + columnWidth;
					line.Y1 = 0;
					line.Y2 = GridHeight + Constants.EntityDetailsBottomMargin + BottomMarginOffset;

					Children.Add(line);
				}

				var currentColumnIndex = hasChannels ? i - 1 : i;

				if (!isChannelHeaderColumn && currentColumnIndex < ScaleSet.ScalePoints.Count)
				{
					var foregroundColor = viewModel.NetworkControlViewModel.NetworkViewModel.DiagramNodeViewModel.ForegroundColor;
					var foregroundColorBrush = (SolidColorBrush)new ColorToBrushConverter().Convert(foregroundColor, typeof(SolidColorBrush), null, CultureInfo.CurrentCulture);
					var textBlock = CreateTimeScaleLabel(columnWidth, scaleType, scaleRatio, foregroundColorBrush);
					var column = ScaleSet.ScalePoints[currentColumnIndex + numberOfMergedColumns - 1]; // Weird offset in scale label calculation makes this necessary. :(
					textBlock.Text = column.Label;
					textBlock.FontWeight = column.Column == ScaleSet.IndexOfColumnInPresent ? FontWeights.Bold : FontWeights.Normal;
					SetTop(textBlock, GridHeight + BottomMarginOffset - textBlock.FontSize);
					SetLeft(textBlock, xOffset + 4);

					Children.Add(textBlock);
				}
			}
		}

		void AddChannels(DiagramAreaUserControlViewModel viewModel, int channelHeadersWidth, Tuple<IDiagramChannel, Range<int>>[] channelAndRangeTuples)
		{
			for (var i = 0; i < channelAndRangeTuples.Length; i++)
			{
				var channel = channelAndRangeTuples[i].Item1;
				var range = channelAndRangeTuples[i].Item2;
				var isLastChannel = i == channelAndRangeTuples.Length - 1;
				var height = (double)(range.Maximum - range.Minimum);

				if (isLastChannel)
				{
					height = Math.Max(GridHeight - range.Minimum, height);
				}

				var foregroundColor = viewModel.NetworkControlViewModel.NetworkViewModel.DiagramNodeViewModel.ForegroundColor;
				var foregroundColorBrush = (SolidColorBrush)new ColorToBrushConverter().Convert(foregroundColor, typeof(SolidColorBrush), null, CultureInfo.CurrentCulture);
				var headerLabel = GetChannelHeaderLabel(channel.Name, height, channelHeadersWidth, foregroundColorBrush);
				SetTop(headerLabel, range.Minimum);
				SetLeft(headerLabel, 0);
				Children.Add(headerLabel);

				if (!isLastChannel)
				{
					AddHorizontalLine(range.Maximum, isBold: false);
				}
			}

			viewModel.ContentLeftMargin = channelHeadersWidth;
		}

		void AddHorizontalLine(double position, bool isBold)
		{
			var line = isBold ? CreateBoldBlackLine() : CreateThinSilverLine();
			line.X1 = 0;
			line.X2 = ActualWidth;
			line.Y1 = position;
			line.Y2 = position;
			line.Tag = "ChannelLine";

			Children.Add(line);
		}

		static Line CreateBoldBlackLine()
		{
			return new Line
			{
				StrokeThickness = 2,
				Stroke = Brushes.Black,
			};
		}

		static Line CreateThinSilverLine()
		{
			return new Line
			{
				StrokeThickness = 1,
				Stroke = Brushes.Silver,
			};
		}

		void PopulateBackgrounds(int columnWidth, double lastColumnWidth, int lastColumn, IDiagramEntity diagramEntity, int leftMarginForBackgrounds)
		{
			// WARNING! Magic numbers ahead!!!

			var backgrounds = ScaleSet.BackgroundPoints;
			foreach (var background in backgrounds)
			{
				var firstColumnOffset = background.StartColumn == 0 ? 1 : 0;
				var rectangle = new Rectangle();

				if (diagramEntity.IsDiagramSurfaceFixed)
				{
					rectangle.Width = columnWidth * (background.EndColumn - background.StartColumn) - 6 + lastColumnWidth - firstColumnOffset;
				}
				else
				{
					if (background.EndColumn == lastColumn)
					{
						rectangle.Width = Math.Max(columnWidth * (background.EndColumn - background.StartColumn - 1) - 3 + lastColumnWidth, 0);
					}
					else
					{
						rectangle.Width = columnWidth * (background.EndColumn - background.StartColumn);
					}
				}

				rectangle.Height = GridHeight + BottomMarginOffset + 3;
				rectangle.Fill = (SolidColorBrush)new ColorToBrushConverter().Convert(background.Color, typeof(SolidColorBrush), null, CultureInfo.CurrentCulture);

				rectangle.RadiusX = 20;
				rectangle.RadiusY = 20;

				SetLeft(rectangle, (columnWidth * background.StartColumn) + leftMarginForBackgrounds + firstColumnOffset);
				SetTop(rectangle, -1);
				SetZIndex(rectangle, -1);

				Children.Add(rectangle);
			}
		}

		static Line CreateColumnLine(ScaleType scale)
		{
			var line = new Line
			{
				StrokeThickness = GetStrokeThicknessForTimeScaleLines(scale),
				Stroke = Brushes.Black,
				StrokeDashArray = new DoubleCollection(new double[] { 1, 2 }),
				Tag = "TimeScaleLine",
			};
			return line;
		}

		static double GetStrokeThicknessForTimeScaleLines(ScaleType scale)
		{
			switch (scale)
			{
				case ScaleType.Eighths:
					return 2;
				default:
					return 1;
			}
		}

		static TextBlock CreateTimeScaleLabel(double width, ScaleType scale, double scaleRatio, Brush foregroundBrush)
		{
			var label = new TextBlock
			{
				Width = (width * GetNumberOfMergedColumns(scale)) - 6,
				TextAlignment = TextAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0, 0, Constants.DynamicGridRightMargin, Constants.DynamicGridBottomMargin),
				Tag = "TimeScaleLabel",
				Foreground = foregroundBrush,
			};

			label.FontSize = GetFontSize(scaleRatio, label.FontSize);
			label.Height = label.FontSize * 1.2; // Fonts are a little taller than their size.

			return label;
		}

		static UIElement GetChannelHeaderLabel(string text, double height, double width, Brush foregroundBrush)
		{
			var border = new Border
			{
				BorderBrush = null,
				Width = height, // these appear backwards because we're rotating it.
				Height = width,
				LayoutTransform = new RotateTransform { Angle = 270d },
			};

			var label = new TextBlock
			{
				TextAlignment = TextAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextWrapping = TextWrapping.Wrap,
				Text = text,
				FontSize = Constants.ChannelHeaderFontSize,
				FontWeight = FontWeight.FromOpenTypeWeight(6),
				Foreground = foregroundBrush,
				Tag = "ChannelHeaderTag",
			};

			border.Child = label;

			return border;
		}

		public const string ChannelHeaderTag = "ChannelHeaderTag";

		static int Ceiling(int num, int multiple)
		{
			var mod = num % multiple;
			if (mod == 0)
			{
				return num;
			}
			else
			{
				return num - mod + multiple;
			}
		}
		static double GetFontSize(double scaleRatio, double font)
		{
			return font * 1 / scaleRatio;
		}

		static int GetNumberOfMergedColumns(ScaleType scaleType)
		{
			switch (scaleType)
			{
				case ScaleType.Normal:
					return 1;
				case ScaleType.Half:
					return 2;
				case ScaleType.Quarters:
					return 4;
				case ScaleType.Eighths:
					return 8;
				default:
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognised [{0}].", scaleType));
			}
		}

		static ScaleType GetScaleType(double scale)
		{
			if (scale <= 0.25)
			{
				return ScaleType.Eighths;
			}
			else if (scale <= 0.5)
			{
				return ScaleType.Quarters;
			}
			else if (scale <= 0.75)
			{
				return ScaleType.Half;
			}
			else
			{
				return ScaleType.Normal;
			}
		}

		enum ScaleType
		{
			Normal = 0,
			Half,
			Quarters,
			Eighths,
		}

		#endregion
	}
}
