using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class ReportFilterData
	{
		public List<AccountingPeriodFilter> AccountingPeriodFilterCollection { get; } = new List<AccountingPeriodFilter>();
		public List<AccountingPeriodsRangeFilter> AccountingPeriodsRangeFilterCollection { get; } = new List<AccountingPeriodsRangeFilter>();
		public List<CodeListMultipleChoiceFilter> CodeListMultipleChoiceFilterCollection { get; } = new List<CodeListMultipleChoiceFilter>();
		public List<CodeLookupFilter> CodeLookupFilterCollection { get; } = new List<CodeLookupFilter>();
		public List<DateFilter> DateFilterCollection { get; } = new List<DateFilter>();
		public List<DateRangeFilter> DateRangeFilterCollection { get; } = new List<DateRangeFilter>();
		public List<DateTimeOffsetFilter> DateTimeOffsetFilterCollection { get; } = new List<DateTimeOffsetFilter>();
		public List<DateTimeOffsetRangeFilter> DateTimeOffsetRangeFilterCollection { get; } = new List<DateTimeOffsetRangeFilter>();
		public List<LookupFilter> LookupFilterCollection { get; } = new List<LookupFilter>();
		public List<MultipleChoiceFilter> MultipleChoiceFilterCollection { get; } = new List<MultipleChoiceFilter>();
		public List<MultipleSelectionLookupFilter> MultipleSelectionLookupFilterCollection { get; } = new List<MultipleSelectionLookupFilter>();
		public List<NumberFilter> NumberFilterCollection { get; } = new List<NumberFilter>();
		public List<NumberNotInRangeFilter> NumberNotInRangeFilterCollection { get; } = new List<NumberNotInRangeFilter>();
		public List<NumberRangeFilter> NumberRangeFilterCollection { get; } = new List<NumberRangeFilter>();
		public List<AccountingNumberRangeFilter> AccountingNumberRangeFilterCollection { get; } = new List<AccountingNumberRangeFilter>();
		public List<OptionGroupFilter> OptionGroupFilterCollection { get; } = new List<OptionGroupFilter>();
		public List<SecurityFilter> SecurityFilterCollection { get; } = new List<SecurityFilter>();
		public List<SingleAccountingPeriodFilter> SingleAccountingPeriodFilterCollection { get; } = new List<SingleAccountingPeriodFilter>();
		public List<TextFilter> TextFilterCollection { get; } = new List<TextFilter>();
		public List<TextRangeFilter> TextRangeFilterCollection { get; } = new List<TextRangeFilter>();
		public List<ZMultiLineTextFilter> ZMultiLineTextFilterCollection { get; } = new List<ZMultiLineTextFilter>();
		public List<RegistrationCodedFilter> RegistrationCodedFilterCollection { get; } = new List<RegistrationCodedFilter>();
		public List<SalesTradeLaneChecklistFilter> SalesTradeLaneChecklistFilterCollection { get; } = new List<SalesTradeLaneChecklistFilter>();
		public List<PermitTypeChecklistFilter> PermitTypeChecklistFilterCollection { get; } = new List<PermitTypeChecklistFilter>();
		public List<MonthYearPeriodFilter> MonthYearPeriodFilterCollection { get; } = new List<MonthYearPeriodFilter>();
	}

	public class BaseFilterData
	{
		public string DisplayName { get; set; }
		public bool IsRequired { get; set; }
		public string Tab { get; set; }
		public string DependentFilter { get; set; }
		public string ReadOnlyIfFilter { get; set; }
	}

	public class BoolDescription
	{
		public string Description { get; set; }
		public bool Value { get; set; }
	}

	public class CodeDescriptionTree
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public bool Include { get; set; }
		public string Column { get; set; }
		public List<CodeDescriptionTree> SubItems { get; } = new List<CodeDescriptionTree>();
	}

	public class AccountingPeriodFilter : BaseFilterData
	{
		public bool UseSinglePeriod { get; set; }
		public bool UsePeriodRange { get; set; }
		public bool UseYearToPeriod { get; set; }
		public bool UseAllPeriods { get; set; }
		public int SinglePeriod { get; set; }
		public DateTime? ScheduleStorageSinglePeriod { get; set; }
		public int YearToPeriod { get; set; }
		public DateTime? ScheduleStorageYearTo { get; set; }
		public int ToPeriod { get; set; }
		public DateTime? ScheduleStorageTo { get; set; }
		public int FromPeriod { get; set; }
		public DateTime? ScheduleStorageFrom { get; set; }
	}

	public class AccountingPeriodsRangeFilter : BaseFilterData
	{
		public int PeriodTo { get; set; }
		public DateTime? ScheduleStorageTo { get; set; }
		public int PeriodFrom { get; set; }
		public DateTime? ScheduleStorageFrom { get; set; }
	}

	public class CodeListMultipleChoiceFilter : BaseFilterData
	{
		public string Value { get; set; }
		public List<CodeDescription> List { get; } = new List<CodeDescription>();
	}

	public class CodeLookupFilter : BaseFilterData
	{
		public string Value { get; set; }
		public string LookupType { get; set; }
	}

	public class DateFilter : BaseDateFilter<DateTime>
	{
	}

	public class DateRangeFilter : BaseDateRangeFilter<DateTime>
	{
	}

	public class DateTimeOffsetFilter : BaseDateFilter<DateTimeOffset>
	{
	}

	public class DateTimeOffsetRangeFilter : BaseDateRangeFilter<DateTimeOffset>
	{
	}

	public class BaseDateFilter<T> : BaseFilterData
	{
		public T Value { get; set; }
		public string DateFormat { get; set; }
	}

	public class BaseDateRangeFilter<T> : BaseFilterData
	{
		public T ValueLow { get; set; }
		public T ValueHigh { get; set; }
		public string DateFormat { get; set; }
	}

	public class LookupFilter : BaseFilterData
	{
		public Guid Value { get; set; }
		public string LookupType { get; set; }
	}

	public class MultipleChoiceFilter : BaseFilterData
	{
		public string Value { get; set; }
		public List<CodeDescription> List { get; } = new List<CodeDescription>();
	}

	public class MultipleSelectionLookupFilter : BaseFilterData
	{
		public string LookupType { get; set; }
		public List<Guid> SelectedValue { get; } = new List<Guid>();
	}

	public class NumberFilter : BaseFilterData
	{
		public decimal Value { get; set; }
	}

	public class NumberNotInRangeFilter : BaseFilterData
	{
		public decimal? From { get; set; }
		public decimal? To { get; set; }
	}

	public class NumberRangeFilter : BaseFilterData
	{
		public decimal? From { get; set; }
		public decimal? To { get; set; }
	}

	public class AccountingNumberRangeFilter : BaseFilterData
	{
		public string From { get; set; }
		public string To { get; set; }
	}

	public class OptionGroupFilter : BaseFilterData
	{
		public List<BoolDescription> Options { get; } = new List<BoolDescription>();
	}

	public class SecurityFilter : BaseFilterData
	{
		public string SelectedValue { get; set; }
	}

	public class SingleAccountingPeriodFilter : BaseFilterData
	{
		public int SinglePeriod { get; set; }
		public DateTime? ScheduleStorageValue { get; set; }
	}

	public class TextFilter : BaseFilterData
	{
		public string Value { get; set; }
	}

	public class TextRangeFilter : BaseFilterData
	{
		public string From { get; set; }
		public string To { get; set; }
	}

	public class ZMultiLineTextFilter : BaseFilterData
	{
		public string Value { get; set; }
	}

	public class RegistrationCodedFilter : BaseFilterData
	{
		public string CodeCountry { get; set; }
		public string CustomType { get; set; }
	}

	public class SalesTradeLaneChecklistFilter : BaseFilterData
	{
		public List<CodeDescriptionTree> RootItems { get; } = new List<CodeDescriptionTree>();
	}

	public class PermitTypeChecklistFilter : BaseFilterData
	{
		public List<CodeDescriptionTree> RootItems { get; } = new List<CodeDescriptionTree>();
	}

	public class MonthYearPeriodFilter : BaseFilterData
	{
		public int Month { get; set; }
		public int Year { get; set; }
	}
}
