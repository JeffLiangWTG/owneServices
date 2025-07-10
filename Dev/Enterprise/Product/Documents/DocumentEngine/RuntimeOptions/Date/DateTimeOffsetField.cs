using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DateTimeOffsetField : BaseDateField<DateTimeOffset, ZDateTimeOffset>
	{
		public DateTimeOffsetField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal DateTimeOffsetField(DateTimeOffsetFieldJsonData data)
			: base(data)
		{
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.DateTimeOffsetFieldUserControl; }
		}

		protected override void AddFilterDataToFilterCollection(ReportFilterData reportFilterData, BaseFilterData filterData)
		{
			if (filterData is DateTimeOffsetFilter valueAsDateTimeOffsetFilter)
			{
				reportFilterData.DateTimeOffsetFilterCollection.Add(valueAsDateTimeOffsetFilter);
			}
		}

		protected override BaseDateFilter<DateTimeOffset> GetNewFilter()
		{
			return new DateTimeOffsetFilter();
		}

		protected override BaseDateFilter<DateTimeOffset> GetFirstFilterDataFromCollection(ReportFilterData reportFilterData)
		{
			return reportFilterData.DateTimeOffsetFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
		}

		protected override BaseDateFieldJsonData<DateTimeOffset> CreateJsonDataCore() => new DateTimeOffsetFieldJsonData();

		protected override void SetLowerAndUpperValues(ZDateTimeOffset value, out ZDateTimeOffset lower, out ZDateTimeOffset upper)
		{
			if (value.IsEmpty)
			{
				lower = ZDateTimeOffset.Empty;
				upper = ZDateTimeOffset.Empty;
			}
			else
			{
				var lowerDate = (DateTimeOffset)((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
				var upperDate = lowerDate.AddDays(1);
				lower = lowerDate;
				upper = upperDate;
			}
		}
	}
}
