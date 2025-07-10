using System;
using System.Globalization;
using System.Windows.Data;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NodeSourceConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var isNonScheduled = (bool)values[2];

			return isNonScheduled ? values[1] : values[0];
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
