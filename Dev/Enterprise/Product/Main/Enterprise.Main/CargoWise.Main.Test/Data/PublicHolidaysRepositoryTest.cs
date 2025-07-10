using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Main.Test.Data;

public class PublicHolidaysRepositoryTest : TestCaseWithFactory
{
	public void TestGetPublicHolidays()
	{
		var repo = new PublicHolidaysRepository();
		var actual = repo.GetPublicHolidays(new DateTime(2015, 1, 1), new DateTime(2021, 12, 31));
		AssertEquals(7, actual.Count());

		actual = repo.GetPublicHolidays(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31));
		AssertEquals(0, actual.Count());

		// Boundary value test
		actual = new PublicHolidaysRepository()
			.GetPublicHolidays(new DateTime(2015, 6, 1), new DateTime(2019, 10, 5));
		AssertEquals(7, actual.Count());

		actual = new PublicHolidaysRepository()
			.GetPublicHolidays(new DateTime(2015, 6, 1), new DateTime(2019, 10, 4));
		AssertEquals(6, actual.Count());

		actual = new PublicHolidaysRepository()
			.GetPublicHolidays(new DateTime(2015, 6, 2), new DateTime(2019, 10, 5));
		AssertEquals(4, actual.Count());

		actual = new PublicHolidaysRepository()
			.GetPublicHolidays(new DateTime(2015, 6, 2), new DateTime(2019, 10, 4));
		AssertEquals(3, actual.Count());
	}

	public void TestGetPublicHolidays_ShouldOrderDateAscending()
	{
		var actual = new PublicHolidaysRepository()
			.GetPublicHolidays(new DateTime(2000, 1, 1), new DateTime(2025, 12, 31)).ToArray();

		AssertEquals(7, actual.Length);
		AssertEquals(new DateTime(2015, 6, 1), actual[0].Date);
		AssertEquals(new DateTime(2015, 6, 1), actual[1].Date);
		AssertEquals(new DateTime(2015, 6, 1), actual[2].Date);
		AssertEquals(new DateTime(2016, 7, 2), actual[3].Date);
		AssertEquals(new DateTime(2017, 8, 3), actual[4].Date);
		AssertEquals(new DateTime(2018, 9, 4), actual[5].Date);
		AssertEquals(new DateTime(2019, 10, 5), actual[6].Date);
	}

	public void TestGetPublicHolidays_WhenMoreThanOneRecordForSameHoliday_ShouldOrderByCountryName_ShouldBeDistinct()
	{
		var actual = new PublicHolidaysRepository()
			.GetPublicHolidays(new DateTime(2015, 6, 1), new DateTime(2015, 6, 1))
			.ToList();

		AssertEquals(3, actual.Count);
		AssertEquals("Australia", actual[0].RegionName);
		AssertEquals("China", actual[1].RegionName);
		AssertEquals("United States", actual[2].RegionName);
	}

	public void TestGetPublicHolidays_DateFrom_DateTo()
	{
		var date = new DateTime(2015, 6, 1);
		var actual = new PublicHolidaysRepository()
			.GetPublicHolidays(date, date.AddDays(1))
			.ToList();

		AssertEquals(3, actual.Count);
	}

	#region Create Test Data

	protected override void SetUp()
	{
		base.SetUp();

		Factory.Load<GlbHoliday>(new ZQuery()).DeleteAll();
		var china = GetCountry("CN");
		var australia = GetCountry("AU");
		var american = GetCountry("US");
		var americanState = CreateTestCountryState(american, "UU");

		// RN
		var holiday1 = CreateTestHoliday("Fake Holiday 1", new ZDateTime(2015, 6, 1), australia);
		var holiday2 = CreateTestHoliday("Fake Holiday 2", new ZDateTime(2015, 6, 1), american);
		var holiday3 = CreateTestHoliday("Fake Holiday 3", new ZDateTime(2015, 6, 1), china);
		var holiday4 = CreateTestHoliday("Fake Holiday 4", new ZDateTime(2016, 7, 2), china);
		var holiday5 = CreateTestHoliday("Fake Holiday 5", new ZDateTime(2017, 8, 3), american);

		// RW
		var holiday6 = CreateTestHoliday("Fake Holiday 6", new ZDateTime(2018, 9, 4), americanState);
		var holiday7 = CreateTestHoliday("Fake Holiday 7", new ZDateTime(2019, 10, 5), americanState);
		var holiday8 = CreateTestHoliday("Fake Holiday 8", new ZDateTime(2020, 11, 6), americanState);
		holiday8.GH_IsActive = false;

		// GB
		var holiday9 = CreateTestHoliday("Fake Holiday 9", new ZDateTime(2021, 12, 7), GlbBranch.CurrentBranch);
		Factory.Save();
	}

	RefCountry GetCountry(string code)
	{
		return Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, code));
	}

	RefCountryStates CreateTestCountryState(RefCountry country, string code)
	{
		var state = Factory.NewWithValidTestData<RefCountryStates>();
		state.RW_RN_NKCountryCode = country.RN_Code;
		state.RW_Code = code;

		return state;
	}

	GlbHoliday CreateTestHoliday(string name, ZDateTime date, RefCountry country)
	{
		var holiday = Factory.NewWithValidTestData<GlbHoliday>();
		holiday.GH_HolidayName = name;
		holiday.GH_Date = date;
		holiday.GH_ParentID = country.PK;
		holiday.GH_ParentTableCode = RefCountrySchema.Constants.Prefix;

		return holiday;
	}

	GlbHoliday CreateTestHoliday(string name, ZDateTime date, RefCountryStates country)
	{
		var holiday = Factory.NewWithValidTestData<GlbHoliday>();
		holiday.GH_HolidayName = name;
		holiday.GH_Date = date;
		holiday.GH_ParentID = country.PK;
		holiday.GH_ParentTableCode = RefCountryStatesSchema.Constants.Prefix;

		return holiday;
	}

	GlbHoliday CreateTestHoliday(string name, ZDateTime date, GlbBranch branch)
	{
		var holiday = Factory.NewWithValidTestData<GlbHoliday>();
		holiday.GH_HolidayName = name;
		holiday.GH_Date = date;

		branch.GlbHolidays.Add(holiday);
		return holiday;
	}

	#endregion
}
