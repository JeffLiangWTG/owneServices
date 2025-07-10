using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DateField : BaseDateField<DateTime, ZDateTime>
	{
		public DateField(BusinessObjectFactory factory) : base(factory)
		{
		}

		public DateField(BaseDateFieldJsonData<DateTime> data)
			: base(data)
		{
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.DateFieldUserControl; }
		}

		protected override void AddFilterDataToFilterCollection(ReportFilterData reportFilterData, BaseFilterData filterData)
		{
			if (filterData is DateFilter valueAsDateFilter)
			{
				reportFilterData.DateFilterCollection.Add(valueAsDateFilter);
			}
		}

		protected override BaseDateFilter<DateTime> GetNewFilter()
		{
			return new DateFilter();
		}

		protected override BaseDateFilter<DateTime> GetFirstFilterDataFromCollection(ReportFilterData reportFilterData)
		{
			return reportFilterData.DateFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
		}

		protected override BaseDateFieldJsonData<DateTime> CreateJsonDataCore() => new DateFieldJsonData();

		protected override void SetLowerAndUpperValues(ZDateTime value, out ZDateTime lower, out ZDateTime upper)
		{
			if (value.IsEmpty)
			{
				lower = ZDateTime.Empty;
				upper = ZDateTime.Empty;
			}
			else
			{
				DateTime lowerDate = ((DateTime)((IZTypeInternals)value).GetValueForLogicalDataLayer(true));
				DateTime upperDate = lowerDate.AddDays(1);
				if (PickerFormat == DocEngineDatePickerFormats.Short)
				{
					lowerDate = lowerDate.Date;
					upperDate = upperDate.Date;
				}
				else if (PickerFormat == DocEngineDatePickerFormats.YearAndMonth)
				{
					lowerDate = new DateTime(lowerDate.Year, lowerDate.Month, 1);
					upperDate = lowerDate.AddMonths(1);
				}
				lower = lowerDate;
				upper = upperDate;
			}
		}
	}
}
