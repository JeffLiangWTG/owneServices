using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace CargoWise.Main.Navigation.Test;

sealed class NewsViewModelTest : TestCaseWithFactory
{
	// test for NewsSections property
	public void TestNewsSections()
	{
		// Arrange
		var newsViewModel = new NewsViewModel();
		// Act
		var newsSections = newsViewModel.NewsSections;
		// Assert
		AssertNotNull(newsSections);
		Assert("newsSections should not be empty", newsSections.Any());
	}

	// test for GetNews method
	public void TestGetNews()
	{
		// Arrange
		var newsViewModel = new NewsViewModel();
		// Act
		var newsSections = newsViewModel.NewsSections;
		foreach (var newsSection in newsSections)
		{
			var news = NewsViewModel.GetNews(Factory, newsSection.SectionID, false);
			// Assert
			AssertNotNull(news);
		}
	}

	// test loadnewsitems will respect HideReadItems flag
	public void TestNewsSectionsHideReadNews()
	{
		VerifyNewsSectionsHideReadNews(false);
		VerifyNewsSectionsHideReadNews(true);
	}

	void VerifyNewsSectionsHideReadNews(bool hideReadItems)
	{
		// Arrange
		var newsViewModel = new NewsViewModel();
		var newsItemSummary = Guid.NewGuid().ToString();
		var news = Factory.New<NewsAnnouncement>();
		var newsSections = SystemDataRegistry.Instance.NewsSectionLayouts.Value;
		var section = newsSections[0];
		var sectionHideReadItems = section.HideReadItems;
		section.HideReadItems = hideReadItems;
		SystemDataRegistry.Instance.NewsSectionLayouts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newsSections);

		news.GF_Section = section.SectionID;
		news.GF_Summary = newsItemSummary;
		news.GF_URL = "news.com";

		var readItem = Factory.New<GlbReleaseNoteRead>();
		readItem.GR_GS_Staff = GlbStaff.CurrentUser.PK;
		readItem.GR_ReleaseNoteID = news.PK;

		Factory.Save();

		// Act
		newsViewModel.LoadNewsItems();

		// Assert
		AssertEquals("when hideReadItems is true, the news item should not be in the filtered news items",
			newsViewModel.FilteredNewsItems.SingleOrDefault(i => i.Summary == newsItemSummary) == null, hideReadItems);
	}
}
