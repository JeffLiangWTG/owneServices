using System;
using System.Windows.Data;

namespace CargoWise.Main.Navigation.Converters;

public class ShortcutToolTipConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value is int number)
		{
			var fKey = "F" + number;
			return (number <= 12) ? Res.GetString("02b0c932-db2f-40aa-a5a4-b5ec0b5b6de8", "Ctrl+{0} to open module or item.\r\nCtrl+Shift+{0} to open module in new window.", fKey) : string.Empty;
		}

		return null;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
