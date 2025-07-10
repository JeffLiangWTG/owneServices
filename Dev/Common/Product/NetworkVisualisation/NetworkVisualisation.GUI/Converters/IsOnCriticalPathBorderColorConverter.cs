using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class IsOnCriticalPathBorderColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var booleanValue = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
			return booleanValue
				? new SolidColorBrush(Colors.Red)
				: new SolidColorBrush(Colors.Black);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
