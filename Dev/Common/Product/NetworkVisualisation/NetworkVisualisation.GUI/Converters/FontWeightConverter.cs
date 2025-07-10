using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	[ValueConversion(typeof(NodeFontWeight), typeof(FontWeight))]
	public class FontWeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var weightName = value.ToString();
			var fontWeight = typeof(FontWeights).GetProperty(weightName).GetValue(null);
			return (FontWeight)fontWeight;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var weightName = value.ToString();
			var fontWeight = Enum.Parse(typeof(NodeFontWeight), weightName);
			return (NodeFontWeight)fontWeight;
		}
	}
}
