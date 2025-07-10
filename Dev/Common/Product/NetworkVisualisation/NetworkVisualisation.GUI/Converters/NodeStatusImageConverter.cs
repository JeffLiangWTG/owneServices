using System;
using System.Globalization;
using System.Windows.Data;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NodeStatusImageConverter : IValueConverter
	{
		public NodeStatusImageConverter()
		{
			imageSourceProvider = new ResourcesImageSourceProvider();
		}

		readonly IImageSourceProvider imageSourceProvider;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var status = (WorkStatus)value;
			var bitmapName = GetBitmapNameForState(status);

			return bitmapName != null ? imageSourceProvider.GetImageSource(bitmapName) : null;
		}

		static string GetBitmapNameForState(WorkStatus status)
		{
			switch (status)
			{
				case WorkStatus.Startable:
					return nameof(Integration.Properties.Resources.asterisk);

				case WorkStatus.Suspended:
					return nameof(Integration.Properties.Resources.pause);

				case WorkStatus.Working:
					return nameof(Integration.Properties.Resources.play);

				case WorkStatus.Complete:
					return nameof(Integration.Properties.Resources.Complete_icon);

				case WorkStatus.Cancelled:
					return nameof(Integration.Properties.Resources.Cancel_icon);

				default:
					return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
