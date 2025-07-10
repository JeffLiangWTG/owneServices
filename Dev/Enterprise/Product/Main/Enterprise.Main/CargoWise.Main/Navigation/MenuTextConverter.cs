#if !WINZOR
using System;
using System.Windows.Data;

namespace CargoWise.Main.Navigation;
internal class MenuTextConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if (value == null)
		{
			return string.Empty;
		}

		string text = value.ToString();
		if (text.Contains("&"))
		{
			text = text.Replace("&", string.Empty);
		}

		return text;
	}

	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
#endif
