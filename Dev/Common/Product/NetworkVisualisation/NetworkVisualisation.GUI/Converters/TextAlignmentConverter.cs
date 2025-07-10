using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	[ValueConversion(typeof(NodeTextAlignment), typeof(TextAlignment))]
	public class TextAlignmentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var alignmentName = value.ToString();
			var textAlignment = Enum.Parse(typeof(TextAlignment), alignmentName);
			return (TextAlignment)textAlignment;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var alignmentName = value.ToString();
			var textAlignment = Enum.Parse(typeof(NodeTextAlignment), alignmentName);
			return (NodeTextAlignment)textAlignment;
		}
	}
}
