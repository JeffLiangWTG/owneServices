using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class DateBasedAccountingPeriodBuilder : FilterBuilder
	{
		public DateBasedAccountingPeriodBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"[Accounting ]Period Date", Res.GetString("FilterDocumentation|0C87710E-7FC8-44A4-9740-6AF4586D2913"
				, @"Generates a date type based accounting period control with 4 options, use date ranges as the filter of the selected accounting period.
1. The from and to date of the single period.
2. A from and to range period.
3. A first period of current year from and typed year to range period.
4. All periods."), supportedProperties);
		}

		protected override FilterField GetFilterField()
		{
			return new DateBasedAccountingPeriodField(fBusinessObjectFactory);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^(Accounting )?Period +Date$", RegexOptions.IgnoreCase);
		}
	}
}
