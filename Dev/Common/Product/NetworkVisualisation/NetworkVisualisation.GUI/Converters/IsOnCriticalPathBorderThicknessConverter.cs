using System;
using System.Globalization;
using System.Windows.Data;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class IsOnCriticalPathBorderThicknessConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var booleanValue = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
			return booleanValue ? 2.0 : 1.3;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
