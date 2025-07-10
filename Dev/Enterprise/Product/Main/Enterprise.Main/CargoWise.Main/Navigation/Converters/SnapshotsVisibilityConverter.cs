#if !WINZOR
using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CargoWise.Main.Navigation
{
	public class SnapshotsVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null && value is IList collection)
			{
				return (collection.Count > 0 && collection.Count < SnapshotsViewModel.MaxSnapshots) ? Visibility.Visible : Visibility.Hidden;
			}

			return Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
#endif
