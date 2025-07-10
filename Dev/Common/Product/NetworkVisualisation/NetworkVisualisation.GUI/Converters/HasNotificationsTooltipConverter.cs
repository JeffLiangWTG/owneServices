using System;
using System.Globalization;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class HasNotificationsTooltipConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Length == 2)
			{
				var hasNotifications = System.Convert.ToBoolean(values[0], CultureInfo.InvariantCulture);
				if (hasNotifications)
				{
					var entity = values[1] as INetworkEntity;
					if (entity != null)
					{
						return entity.GetTooltipWhenHasNotification();
					}
				}
			}

			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
