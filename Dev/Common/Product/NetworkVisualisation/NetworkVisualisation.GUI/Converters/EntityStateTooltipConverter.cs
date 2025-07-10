using System;
using System.Globalization;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class EntityStateTooltipConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length == 2)
			{
				var state = (EntityState)values[0];
				var entity = (INetworkEntity)values[1];

				return entity.GetTooltipForEntityState(state);
			}

			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
