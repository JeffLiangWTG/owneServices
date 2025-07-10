using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace CargoWise.Main.Navigation.Test;

public class NewsItemViewModelTest : TestCaseWithFactory
{
	public void TestNewsItemViewModel_WithCategory()
	{
		note.GF_Category = "FOR";
		var newsItemViewModel = new NewsItemViewModel(note, new NewsSection { SectionID = NewsSectionTypeList.Codes.ProductUpdates });

		AssertEquals("View Model summary is category plus title", note.CategoryDisplayName + ": " + note.Title, newsItemViewModel.Summary);
	}

	public void TestNewsItemViewModel_WithoutCategory()
	{
		note.GF_Category = "";
		var newsItemViewModel = new NewsItemViewModel(note, new NewsSection() { SectionID = NewsSectionTypeList.Codes.ProductUpdates });

		AssertEquals("View Model summary is the same as title", note.Title, newsItemViewModel.Summary);
	}

	public void TestDateTextMultiLanguage()
	{
		var mockUser = new Mock<IUser>();
		mockUser.SetupGet(user => user.Language).Returns("en-us");
		Env.SetUserContext(new UserContext(mockUser.Object, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
		var newsItemViewModel = new NewsItemViewModel(note, new NewsSection() { SectionID = NewsSectionTypeList.Codes.ProductUpdates });
		AssertEquals("Release date is in English format", "Wednesday, May 31, 1995", newsItemViewModel.ReleaseDateAsText);

		mockUser = new Mock<IUser>();
		mockUser.SetupGet(user => user.Language).Returns("zh-cn");
		Env.SetUserContext(new UserContext(mockUser.Object, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
		newsItemViewModel = new NewsItemViewModel(note, new NewsSection() { SectionID = NewsSectionTypeList.Codes.ProductUpdates });
		AssertEquals("Release date is in Japanese format", "1995年5月31日 星期三", newsItemViewModel.ReleaseDateAsText);

		mockUser = new Mock<IUser>();
		mockUser.SetupGet(user => user.Language).Returns("ja-jp");
		Env.SetUserContext(new UserContext(mockUser.Object, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
		newsItemViewModel = new NewsItemViewModel(note, new NewsSection() { SectionID = NewsSectionTypeList.Codes.ProductUpdates });
		AssertEquals("Release date is in Japanese format", "1995年5月31日水曜日", newsItemViewModel.ReleaseDateAsText);

		mockUser = new Mock<IUser>();
		mockUser.SetupGet(user => user.Language).Returns("es-es");
		Env.SetUserContext(new UserContext(mockUser.Object, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
		newsItemViewModel = new NewsItemViewModel(note, new NewsSection() { SectionID = NewsSectionTypeList.Codes.ProductUpdates });
		AssertEquals("Release date is in Spanish format", "Miércoles, 31 Mayo 1995", newsItemViewModel.ReleaseDateAsText);
	}

	protected override void SetUp()
	{
		base.SetUp();

		// As long as Factory.Save() is not called, no need to change this.
		// Otherwise, use RefGlbReleaseNoteTestDataHelper to prepare testing data located in Reference database.
		// Or, update ZGF_Section to a value under section category News & Announcements.
		note = Factory.NewWithValidTestData<GlbReleaseNoteCombined>();
		note.GF_Section = "C1U";
		note.GF_ReleaseNoteDate = new ZDateTime(1995, 5, 31);
		note.GF_Summary = "Summary: Long Description";
	}

	GlbReleaseNoteCombined note;
}
