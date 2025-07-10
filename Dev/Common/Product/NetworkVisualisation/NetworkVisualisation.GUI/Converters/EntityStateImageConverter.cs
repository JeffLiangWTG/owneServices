using System;
using System.Globalization;
using System.Windows.Data;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class EntityStateImageConverter : IValueConverter
	{
		public EntityStateImageConverter()
		{
			imageSourceProvider = new ResourcesImageSourceProvider();
		}

		readonly IImageSourceProvider imageSourceProvider;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var state = (EntityState)value;
			var bitmapName = GetBitmapNameForState(state);

			return bitmapName != null ? imageSourceProvider.GetImageSource(bitmapName) : null;
		}

		static string GetBitmapNameForState(EntityState state)
		{
			if (state.HasFlag(EntityState.HasErrors))
			{
				return nameof(Integration.Properties.Resources.Error);
			}
			else if (state.HasFlag(EntityState.HasWarnings))
			{
				return nameof(Integration.Properties.Resources.Warning);
			}
			else if (state.HasFlag(EntityState.HasMessages))
			{
				return nameof(Integration.Properties.Resources.Message);
			}
			else if (state.HasFlag(EntityState.Approved))
			{
				return nameof(Integration.Properties.Resources.ThumbUp);
			}
			else if (state.HasFlag(EntityState.NotApproved))
			{
				return nameof(Integration.Properties.Resources.ThumbUpQuestionMark);
			}
			else if (state.HasFlag(EntityState.Fixed))
			{
				return nameof(Integration.Properties.Resources.pin_in);
			}
			else
			{
				return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
