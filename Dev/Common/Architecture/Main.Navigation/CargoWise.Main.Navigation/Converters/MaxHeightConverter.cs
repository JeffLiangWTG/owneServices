using System;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;
public class MaxHeightConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (values != null && values.Length == 2 && values[0] is int detailItems  && values[1] is double totalHeight && parameter != null && parameter is string)
		{
			_ = int.TryParse((string)parameter, out var itemHeight);

			var maxHeight = totalHeight - detailItems * itemHeight;
			return maxHeight > 0 ? maxHeight : 0;
		}

		return 0.0;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
