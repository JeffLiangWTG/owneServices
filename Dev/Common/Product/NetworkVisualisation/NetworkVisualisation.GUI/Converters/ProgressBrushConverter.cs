using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static CargoWise.NetworkVisualisation.Business.NodeViewModel;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class ProgressBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is null)
			{
				return null;
			}

			var progressBar = (ProgressBar)value;
			var c = progressBar.background;
			var backgroundColor = Color.FromArgb(c.A, c.R, c.G, c.B);
			var backgroundBrush = new SolidColorBrush(backgroundColor);
			var transitionSize = progressBar.percent > 0 ? 0.02 : 0.001;

			var brush = BrushHelper.AddProgressPercentageBar(backgroundBrush, progressBar.percent, transitionSize);
			brush.Opacity = progressBar.opacity;
			return brush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
