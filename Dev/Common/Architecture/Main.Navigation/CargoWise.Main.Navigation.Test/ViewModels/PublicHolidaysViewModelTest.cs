using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test.ViewModels;

public class PublicHolidaysViewModelTest : TestCase
{
	Mock<IPublicHolidaysRepository> mockRepository;

	protected override void SetUp()
	{
		base.SetUp();

		mockRepository = new Mock<IPublicHolidaysRepository>();
		mockRepository.Setup(x => x.GetPublicHolidays(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
			.Returns(new List<PublicHoliday>
			{
				new PublicHoliday { Date = new DateTime(2026, 1, 1), Description = "New Year", RegionCode = "AU" },
				new PublicHoliday { Date = new DateTime(2025, 12, 25), Description = "Christmas Day", RegionCode = "AU" },
				new PublicHoliday { Date = new DateTime(2025, 12, 26), Description = "Boxing Day", RegionCode = "AU" },
				new PublicHoliday { Date = new DateTime(2026, 10, 6), Description = "Labour Day", RegionCode = "AU" },
			});
	}

	public void TestPublicHolidaysViewModel_WhenInitializing()
	{
		var uut = new PublicHolidaysViewModel(mockRepository.Object, DateTime.Today);

		AssertEquals("Repository", mockRepository.Object, uut.Repository);
		AssertEquals("Date", DateTime.Today, uut.Date);
	}

	public void TestPublicHolidaysViewModel_WhenProperties()
	{
		var uut = new PublicHolidaysViewModel(mockRepository.Object, DateTime.Today);

		AssertEquals("Title", "Public Holiday Calendar", uut.Title);
		AssertEquals("Empty Label", "No Public Holidays", uut.EmptyLabel);
	}

	public void TestPublicHolidaysViewModel_WhenRefresh()
	{
		var changes = new List<string>();
		var uut = new PublicHolidaysViewModel(mockRepository.Object, DateTime.Today);
		uut.PropertyChanged += (sender, args) => changes.Add(args.PropertyName);

		AssertEquals("Public Holidays should count 0", 0, uut.PublicHolidays.Count);
		AssertEquals("Property changed should count 0", 0, changes.Count);

		uut.Refresh();

		AssertEquals("Public Holidays should count 4", 4, uut.PublicHolidays.Count);
		AssertEquals("Property changed should count 1", 1, changes.Count);
		AssertCollectionContains("Property changed args", "PublicHolidays", changes);
	}
}
