using System;
using System.Globalization;
using System.IO;
using System.Windows;

#if !WINZOR
using System.Windows.Media.Imaging;
#else
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Main.Navigation;

public class NewsItemViewModel : ViewModelBase
#if WINZOR
	, ViewModels.INewsItemViewModel
#endif
{
	internal readonly GlbReleaseNoteCombined NewsItem;

	public NewsItemViewModel(GlbReleaseNoteCombined newsItem, NewsSection newsSection)
	{
		SectionName = newsSection.SectionName;
		Summary = (!newsItem.CategoryDisplayName.IsEmpty ? newsItem.CategoryDisplayName + ": " : string.Empty) + newsItem.Title;
		ReleaseDate = newsItem.GF_ReleaseNoteDate.ToDateTime();
		ReleaseDateAsText = ConvertDateTimeToReleaseText(Env.Time.GetLocalTimeFromUtc(ReleaseDate));
		NewsItem = newsItem;
#if WINZOR
		ThumbnailData = newsItem.GF_Thumbnail;
#else
		ThumbnailBitmap = GetBitmapImage(newsItem.GF_Thumbnail);
#endif
	}

	public NewsItemViewModel(string summary, DateTime releaseDate)
	{
		Summary = summary;
		ReleaseDate = releaseDate;
		ReleaseDateAsText = ConvertDateTimeToReleaseText(releaseDate);
	}

	string ConvertDateTimeToReleaseText(DateTime dateTime)
	{
		var dateTimeText = dateTime.ToString(DateTimeFormatStrings.LongDateFormatIncludingWeek, CurrentUserCultureInfo);
		return CurrentUserCultureInfo.TextInfo.ToTitleCase(dateTimeText);
	}

	public string Summary { get; }

	public string SectionName { get; }

	public DateTime ReleaseDate { get; }

	public string ReleaseDateAsText { get; }

#if WINZOR
	public byte[] ThumbnailData { get; set; }
	public string ThumbnailUrl { get; set; }
#else
	public BitmapImage ThumbnailBitmap { get; set; }
	public Visibility ThumbnailVisibility =>
	ThumbnailBitmap == null ? Visibility.Collapsed : Visibility.Visible;

	public BitmapImage GetBitmapImage(byte[] imageData)
	{
		if (imageData == null || imageData.Length == 0)
		{
			return null;
		}

		var bitmapImage = new BitmapImage();

		using var ms = new MemoryStream(imageData);
		bitmapImage.BeginInit();
		bitmapImage.StreamSource = ms;
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();

		return bitmapImage;
	}
#endif

	CultureInfo CurrentUserCultureInfo
	{
		get
		{
			if (Env.CurrentUser != null && Env.CurrentUser.Language != null)
			{
				return CultureInfo.GetCultureInfo(Env.CurrentUser.Language);
			}

			return CultureInfo.DefaultThreadCurrentCulture;
		}
	}
}
