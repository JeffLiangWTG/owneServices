using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class HasNotificationsBrushConverter : IMultiValueConverter
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
						if (entity.HasErrors())
						{
							return new SolidColorBrush(Colors.Red);
						}
						else if (entity.HasWarnings())
						{
							return new SolidColorBrush(Colors.Orange);
						}
						else if (entity.HasMessages())
						{
							return new SolidColorBrush(Colors.DodgerBlue);
						}
					}
				}
			}

			return new SolidColorBrush();
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
