using System;
using System.Globalization;
using System.Windows.Data;

namespace CargoWise.NetworkVisualisation.GUI
{
	public abstract class TupleConverterBase<T1, T2> : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length == 2)
			{
				var value1 = values[0] is T1 ? (T1)values[0] : default(T1);
				var value2 = values[1] is T2 ? (T2)values[1] : default(T2);

				return Tuple.Create(value1, value2);
			}
			else
			{
				return null;
			}
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
