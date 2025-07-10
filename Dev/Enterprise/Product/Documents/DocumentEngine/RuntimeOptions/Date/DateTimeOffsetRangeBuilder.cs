using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class DateTimeOffsetRangeBuilder : BaseDateRangeBuilder<DateTimeOffset, ZDateTimeOffset>
	{
		protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Date Time Offset Range", Res.GetString("FilterDocumentation|a87b1718-75c1-4c74-a618-79b01c8d4682", "Generates a date time offset filter with a From and To date fields. Filters the data in the selected date range."), supportedProperties);
		}

		public DateTimeOffsetRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DateFormat);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^date\s?time\s?offset\s?range$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new DateTimeOffsetRangeField(fBusinessObjectFactory);
		}

		protected override void BuildDateFormat(StringTreeNode fieldTree, FilterField newField)
		{
			((IDateFormatSupport)newField).PickerFormat = DocEngineDatePickerFormats.Long;
		}
	}
}
