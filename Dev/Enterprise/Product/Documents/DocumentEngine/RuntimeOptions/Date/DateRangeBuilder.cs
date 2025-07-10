using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class DateRangeBuilder : BaseDateRangeBuilder<DateTime, ZDateTime>
	{
		public DateRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DateFormat);
		}

		protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Date Range", Res.GetString("FilterDocumentation|1F066A03-6613-4612-ADDB-78B6C21E3495", "Generates a date filter with a From and To date fields. Filters the data in the selected date range."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^date\s?range$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new DateRangeField(fBusinessObjectFactory);
		}
	}
}
