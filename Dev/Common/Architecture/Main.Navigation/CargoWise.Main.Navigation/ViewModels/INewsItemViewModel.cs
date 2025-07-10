using System;

namespace CargoWise.Main.Navigation.ViewModels;

public interface INewsItemViewModel
{
	string Summary { get; }
	string SectionName { get; }
	DateTime ReleaseDate { get; }
	string ReleaseDateAsText { get; }
	byte[] ThumbnailData { get; }
	string ThumbnailUrl { get; set; }
}

