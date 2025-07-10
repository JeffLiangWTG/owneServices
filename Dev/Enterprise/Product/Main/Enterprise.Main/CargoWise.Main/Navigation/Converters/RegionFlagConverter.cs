#if !WINZOR
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CargoWise.Main.Navigation;

public class RegionFlagConverter : IValueConverter
{
	const int FlagWidth = 52;
	const int FlagHeight = 41;
	readonly BitmapSource _spriteSheet;

	public RegionFlagConverter() : this(LoadSpriteSheet()) { }

	public RegionFlagConverter(BitmapSource spriteSheet, bool isTestMode = false)
	{
		// In test mode, we can accept a null sprite sheet and create a dummy one
		if (spriteSheet == null && isTestMode)
		{
			// Create a minimal 1x1 dummy bitmap for tests
			var dummyBitmap = new WriteableBitmap(52, 41 * 100, 0, 0, System.Windows.Media.PixelFormats.Bgra32, null);
			_spriteSheet = dummyBitmap;
		}
		else
		{
			_spriteSheet = spriteSheet ?? LoadSpriteSheet();
		}
	}

	static BitmapSource LoadSpriteSheet()
	{
		try
		{
			var uri = new Uri(RegionFlags.FlagsImagePath, UriKind.Absolute);

			var image = new BitmapImage();
			image.BeginInit();
			image.UriSource = uri;
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.EndInit();
			image.Freeze(); // Make it thread-safe

			return image;
		}
		catch
		{
			return null;
		}
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (_spriteSheet is null || value is not string regionCode || string.IsNullOrEmpty(regionCode))
		{
			return null;
		}

		var index = RegionFlags.IndexOf(regionCode) + 1;
		if (index < 1)
		{
			return null;
		}

		try
		{
			var rect = new Int32Rect(x: 0, y: index * FlagHeight, width: FlagWidth, height: FlagHeight);
			var croppedBitmap = new CroppedBitmap(_spriteSheet, rect);
			croppedBitmap.Freeze(); // Make it thread-safe

			return croppedBitmap;
		}
		catch
		{
			return null;
		}
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
#endif
