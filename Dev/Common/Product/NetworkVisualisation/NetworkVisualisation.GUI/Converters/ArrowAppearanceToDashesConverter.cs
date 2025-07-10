using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class ArrowAppearanceToDashesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is ArrowAppearance)
			{
				var appearance = (ArrowAppearance)value;
				switch (appearance)
				{
					case ArrowAppearance.Dashed:
						return new DoubleCollection { 5, 2 };
					case ArrowAppearance.Dotted:
						return DashStyles.DashDot.Dashes;
				}
			}

			return DashStyles.Solid.Dashes;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
