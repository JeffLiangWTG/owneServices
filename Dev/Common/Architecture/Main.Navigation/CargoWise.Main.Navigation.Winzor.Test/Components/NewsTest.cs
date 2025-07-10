using System.Globalization;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using CargoWise.Main.Navigation.ViewModels;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test;

sealed class NewsTest : BunitTestContext
{
	[Test]
	public void TestNewsDateRender()
	{
		var mockNewsItemViewModel1 = new MockNewsItemViewModel()
		{
			Summary = "Summary 1",
			SectionName = "Section 1",
			ReleaseDate = new DateTime(2025, 1, 1),
		};

		var mockNewsItemViewModel2 = new MockNewsItemViewModel()
		{
			Summary = "Summary 2",
			SectionName = "Section 2",
			ReleaseDate = new DateTime(2025, 1, 1),
		};

		var cut = RenderComponent<NewsList>(parameters =>
			parameters.Add(p => p.Items, [mockNewsItemViewModel1, mockNewsItemViewModel2]));

		Assert.That(cut.FindAll(".cwn-list-item__info-time")[0].TextContent, Is.EqualTo(mockNewsItemViewModel1!.ReleaseDateAsText));
		Assert.That(cut.FindAll(".cwn-list-item__info-time")[1].TextContent, Is.EqualTo(mockNewsItemViewModel2!.ReleaseDateAsText));
	}

	[Test]
	public void ShouldShowThumbnailInNews()
	{
		var cut = RenderComponent<NewsList>(parameters =>
			parameters.Add(p => p.Items, new List<INewsItemViewModel>()
			{
				new MockNewsItemViewModel()
				{
					Summary = "Summary 1",
					SectionName = "Section 1",
					ReleaseDate = new DateTime(2025, 1, 1),
					ThumbnailData = new byte[100]
				}
			}));

		var thumbnail = cut.Find(".cwn-list-item__thumbnail");
		Assert.That(thumbnail, Is.Not.Null);
	}

	internal class MockNewsItemViewModel : INewsItemViewModel
	{
		public string Summary { get; set; } = string.Empty;
		public string SectionName { get; set; } = string.Empty;
		public DateTime ReleaseDate { get; set; }
		public string ReleaseDateAsText => ReleaseDate.ToString("dddd, MMMM dd, yyyy", CultureInfo.CurrentCulture);
		public byte[] ThumbnailData { get; set; } = Array.Empty<byte>();
		public string ThumbnailUrl { get; set; } = string.Empty;
	}
}

