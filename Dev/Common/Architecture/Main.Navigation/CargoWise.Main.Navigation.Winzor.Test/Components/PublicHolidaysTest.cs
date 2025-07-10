using System.Collections.ObjectModel;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Components;

sealed class PublicHolidaysTest : BunitTestContext
{
	Mock<IPublicHolidayService> _mockService;

	[SetUp]
	public void SetUp()
	{
		_mockService = new Mock<IPublicHolidayService>();
		_mockService.SetupGet(service => service.Title).Returns("Public Holidays Calendar");
		_mockService.SetupGet(service => service.PublicHolidays).Returns(() => new ObservableCollection<PublicHoliday>
	{
		new PublicHoliday
				{
					Date = new DateTime(2025, 1, 1),
					Weekday = "Wednesday",
					Month = "January",
					Description = "New Year's Day",
					RegionCode = "AU",
					RegionName = "Australia"
				},
				new PublicHoliday
				{
					Date = new DateTime(2025, 7, 4),
					Weekday = "Friday",
					Month = "July",
					Description = "Independence Day",
					RegionCode = "US",
					RegionName = "United States"
				},
				new PublicHoliday
				{
					Date = new DateTime(2025, 7, 15),
					Weekday = "Friday",
					Month = "July",
					Description = "test Day",
					RegionCode = "US",
					RegionName = "United States"
				},
				new PublicHoliday
				{
					Date = new DateTime(2025, 12, 25),
					Weekday = "Thursday",
					Month = "December",
					Description = "Christmas Day",
					RegionCode = "GB",
					RegionName = "United Kingdom"
				},
				new PublicHoliday
				{
					Date = new DateTime(2025, 10, 3),
					Weekday = "Friday",
					Month = "October",
					Description = "German Unity Day",
					RegionCode = "DE",
					RegionName = "Germany"
				},
				new PublicHoliday
				{
					Date = new DateTime(2025, 6, 12),
					Weekday = "Thursday",
					Month = "June",
					Description = "Independence Day",
					RegionCode = "PH",
					RegionName = "Philippines"
				}
	});
	}

	[Test]
	public async Task PublicHolidays_ShouldRenderAsync()
	{
		using var context = new EnterpriseTestContext();

		// Act
		var cut = await context.RenderControlOnFormAsync(() => new NextHomeUserControl());

		Assert.That(cut.FindAll(".cwn-public-holiday-calendar").Count, Is.EqualTo(1));
	}

	[Test]
	public void PublicHolidays_ShouldRender()
	{
		var cut = RenderComponent<PublicHolidayCalendar>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		var thumbnail = cut.Find(".cwn-public-holiday-calendar");
		Assert.That(thumbnail, Is.Not.Null);
	}

	[Test]
	public void PublicHolidays_ShouldRenderHolidays()
	{
		// Act
		var cut = RenderComponent<PublicHolidayCalendar>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		Assert.That(cut.FindAll(".cwn-public-holiday-calendar__item").Count, Is.EqualTo(6));

		Assert.That(cut.Markup, Does.Contain("New Year's Day"));
		Assert.That(cut.Markup, Does.Contain("Independence Day"));
		Assert.That(cut.Markup, Does.Contain("test Day"));
		Assert.That(cut.Markup, Does.Contain("Christmas Day"));
		Assert.That(cut.Markup, Does.Contain("German Unity Day"));

		Assert.That(cut.Markup, Does.Contain("Australia"));
		Assert.That(cut.Markup, Does.Contain("United States"));
		Assert.That(cut.Markup, Does.Contain("United Kingdom"));
		Assert.That(cut.Markup, Does.Contain("Germany"));
		Assert.That(cut.Markup, Does.Contain("Philippines"));
	}

	[Test]
	public void PublicHoliday_ShouldShowEmptyState()
	{
		// Arrange
		var mockService = new Mock<IPublicHolidayService>();
		mockService.SetupGet(service => service.NoPublicHolidaysLabel).Returns("No Public Holidays");
		mockService.SetupGet(service => service.PublicHolidays).Returns([]);

		// Act
		var cut = RenderComponent<PublicHolidayCalendar>(parameters => parameters.Add(p => p.Service, mockService.Object));

		// Assert
		Assert.That(cut.Markup, Does.Contain("No Public Holidays"));
	}

	[Test]
	public void PublicHoliday_ShouldRenderMonthHeader()
	{
		// Arrange
		var cut = RenderComponent<PublicHolidayCalendar>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		// Act
		var month = cut.FindAll(".cwn-public-holiday-calendar__month");

		// Assert
		Assert.That(month.Count, Is.EqualTo(5));
		Assert.That(month[0].TextContent, Is.EqualTo("January"));
		Assert.That(month[1].TextContent, Is.EqualTo("July"));
		Assert.That(month[2].TextContent, Is.EqualTo("December"));
		Assert.That(month[3].TextContent, Is.EqualTo("October"));
		Assert.That(month[4].TextContent, Is.EqualTo("June"));
	}

	[Test]
	public void PublicHoliday_ShouldRenderRegionFlag()
	{
		// Arrange
		var cut = RenderComponent<PublicHolidayCalendar>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		// Act
		var flag = cut.FindAll(".cwn-region-flag");

		// Assert
		Assert.That(flag.Count, Is.EqualTo(6));
		Assert.That(flag[0].ClassList, Contains.Item("cwn-region-flag--au"));
		Assert.That(flag[1].ClassList, Contains.Item("cwn-region-flag--us"));
		Assert.That(flag[2].ClassList, Contains.Item("cwn-region-flag--us"));
		Assert.That(flag[3].ClassList, Contains.Item("cwn-region-flag--gb"));
		Assert.That(flag[4].ClassList, Contains.Item("cwn-region-flag--de"));
		Assert.That(flag[5].ClassList, Contains.Item("cwn-region-flag--ph"));
	}

	[Test]
	public async Task TitleClick_ShouldExecuteOpenHolidayModuleAsync()
	{
		// Setup
		var mockService = new Mock<IPublicHolidayService>();
		mockService.Setup(s => s.OpenHolidayModuleAsync()).Returns(Task.CompletedTask);
		// Act
		var cut = RenderComponent<PublicHolidayCalendar>(parameters =>
			parameters.Add(p => p.Service, mockService.Object));

		mockService.Verify(s => s.OpenHolidayModuleAsync(), Times.Never);

		var titleElement = cut.Find(".cwn-public-holiday-calendar__title");
		await titleElement.ClickAsync(new WebMouseEventArgs());

		// Assert
		mockService.Verify(s => s.OpenHolidayModuleAsync(), Times.Once);
	}
}
