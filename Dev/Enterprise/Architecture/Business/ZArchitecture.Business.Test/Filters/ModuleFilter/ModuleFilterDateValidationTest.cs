using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleFilterDateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestProperty1Validation()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			var errorText = "You cant hug your children with atomic arms!";
			filter.Property1Validation = null;
			filter.Property1 = ZDateTime.Empty;

			filter.Validation.ValidateProperty1();
			AssertNoError(filter.Property1Info, errorText);

			filter.Property1Validation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			filter.Validation.ValidateProperty1();
			AssertHasError(filter.Property1Info, errorText);
		}

		public void TestProperty2Validation()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			var errorText = "You cant hug your children with atomic arms!";
			filter.Property2Validation = null;
			filter.Property2 = ZDateTime.Empty;

			filter.Validation.ValidateProperty2();
			AssertNoError(filter.Property2Info, errorText);

			filter.Property2Validation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			filter.Validation.ValidateProperty2();
			AssertHasError(filter.Property2Info, errorText);
		}

		public void TestPropertyDecimal1Validation()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			var errorText = "You cant hug your children with atomic arms!";
			filter.PropertyDecimal1Validation = null;
			filter.PropertyDecimal1 = ZDecimal.Zero;

			filter.Validation.ValidatePropertyDecimal1();
			AssertNoError(filter.PropertyDecimal1Info, errorText);

			filter.PropertyDecimal1Validation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			filter.Validation.ValidatePropertyDecimal1();
			AssertHasError(filter.PropertyDecimal1Info, errorText);
		}

		public void TestPropertyDecimal2Validation()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			var errorText = "You cant hug your children with atomic arms!";
			filter.PropertyDecimal2Validation = null;
			filter.PropertyDecimal2 = ZDecimal.Zero;

			filter.Validation.ValidatePropertyDecimal2();
			AssertNoError(filter.PropertyDecimal2Info, errorText);

			filter.PropertyDecimal2Validation = info =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			filter.Validation.ValidatePropertyDecimal2();
			AssertHasError(filter.PropertyDecimal2Info, errorText);
		}

		public void TestShowsErrorWhenFromDateIsGreaterThanToDate()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertShowsErrorWhenFromDateIsGreaterThanToDate(filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertShowsErrorWhenFromDateIsGreaterThanToDate(filter);
		}

		public void TestFilterIsDateRangeWithin3Months()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			filter.Property1 = new ZDateTime(2020, 08, 1);
			filter.Property2 = new ZDateTime(2020, 09, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertNoExceptionThrown("Invalid Date Exception", () =>
			{
				_ = filter.IsDateRangeWithin3Months;
			});
			AssertEquals("Date Range Correct", true, filter.IsDateRangeWithin3Months);

			filter.Property1 = new ZDateTime(2020, 08, 1);
			filter.Property2 = new ZDateTime(2020, 12, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertNoExceptionThrown("Invalid Date Exception", () =>
			{
				_ = filter.IsDateRangeWithin3Months;
			});
			AssertEquals("Date Range Correct", false, filter.IsDateRangeWithin3Months);

			filter.Property1 = new ZDateTime(2020, 08, 1);
			filter.Property2 = ZDateTime.Invalid;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertNoExceptionThrown("Invalid Date Exception", () =>
			{
				_ = filter.IsDateRangeWithin3Months;
			});
			AssertEquals("Date Range Correct", false, filter.IsDateRangeWithin3Months);

			filter.Property1 = ZDateTime.Invalid;
			filter.Property2 = new ZDateTime(2020, 09, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertNoExceptionThrown("Invalid Date Exception", () =>
			{
				_ = filter.IsDateRangeWithin3Months;
			});
			AssertEquals("Date Range Correct", false, filter.IsDateRangeWithin3Months);
		}

		void AssertShowsErrorWhenFromDateIsGreaterThanToDate(ModuleDateFilter filter)
		{
			filter.Property1 = new ZDateTime(2009, 09, 16);
			filter.Property2 = new ZDateTime(1977, 07, 19);

			filter.Validation.ValidateProperty2();
			AssertHasWarning(filter.Property2Info, "To Date and Time should be greater than the From date and Time.");

			filter.Property1 = ZDateTime.Empty;
			filter.Validation.ValidateProperty2();
			AssertNoWarning(filter.Property2Info, "To Date and Time should be greater than the From date and Time.");
		}

		[TestDate(2016, 11, 17)]
		public void TestShowsErrorWhenAtLeastIsNotValid_ForHourOffset()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			AssertShowsErrorWhenAtLeastIsNotValid(filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			AssertShowsErrorWhenAtLeastIsNotValid(filter);
		}

		void AssertShowsErrorWhenAtLeastIsNotValid(ModuleDateFilter filter)
		{
			filter.Property1 = ZDateTime.Now;
			AssertNoNotifications("Property1 should not have notifications when set correctly", filter.Property1Info);

			filter.Property1 = ZDateTime.Empty;
			AssertNoNotifications("Property1 should be optional", filter.Property1Info);

			filter.Property1 = ZDateTime.Invalid;
			AssertHasError("Property1 should have a correct value", filter.Property1Info, "Enter a valid inner offset.");
		}

		[TestDate(2016, 11, 17)]
		public void TestShowsErrorWhenAtMostIsNotValid_ForHourOffset()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			AssertShowsErrorWhenAtMostIsNotValid(filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			AssertShowsErrorWhenAtMostIsNotValid(filter);
		}

		void AssertShowsErrorWhenAtMostIsNotValid(ModuleDateFilter filter)
		{
			filter.Property2 = ZDateTime.Now;
			AssertNoNotifications("Property2 should not have notifications when set correctly", filter.Property2Info);

			filter.Property2 = ZDateTime.Empty;
			AssertNoNotifications("Property2 should be optional", filter.Property2Info);

			filter.Property2 = ZDateTime.Invalid;
			AssertHasError("Property2 should have a correct value", filter.Property2Info, "Enter a valid outer offset.");
		}

		[TestDate(2016, 11, 17)]
		public void TestShowsErrorWhenAtMostIsLessThanAtLeast()
		{
			var filter = new ModuleDateFilter("Filter", DummyBizoSchema.Z0_Date);

			filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			AssertShowsErrorWhenAtMostIsLessThanAtLeast(filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedWorkHourOffsetRange;
			AssertShowsErrorWhenAtMostIsLessThanAtLeast(filter);
		}

		void AssertShowsErrorWhenAtMostIsLessThanAtLeast(ModuleDateFilter filter)
		{
			filter.Property1 = ZDateTime.Now.AddMinutes(2);
			filter.Property2 = ZDateTime.Now.AddMinutes(1);
			filter.Validation.ValidateAll();
			var innerOffset = filter.Property1.GetMinutesFromDateTimeSpan();
			var outerOffset = filter.Property2.GetMinutesFromDateTimeSpan();
			Assert("We want to be sure that we are testing the right thing", innerOffset > outerOffset);
			AssertHasError("Property2 should be greater than Property1", filter.Property2Info, "The outer offset should be greater than the inner offset.");

			filter.Property1 = ZDateTime.Empty;
			filter.Validation.ValidateProperty2();
			AssertNoNotifications(filter.Property2Info);

			filter.Property1 = ZDateTime.Now.AddMinutes(1);
			filter.Property2 = ZDateTime.Now.AddMinutes(2);
			filter.Validation.ValidateAll();
			AssertNoNotifications(filter.Property2Info);
		}
	}
}
