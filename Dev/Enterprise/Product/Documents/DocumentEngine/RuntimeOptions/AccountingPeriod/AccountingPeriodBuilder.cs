using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class AccountingPeriodBuilder : FilterBuilder
	{
		public AccountingPeriodBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"[Accounting ]Period", Res.GetString("FilterDocumentation|5814A23D-3075-406D-BBEE-6868B70E4AA4"
				, @"Generates an accounting period control with 4 options, Filters data from the selected accounting period.
1. Single period
2. A from and to range period
3. A first period of current year from and typed year to range period
4. All periods"), supportedProperties);
		}

		protected override FilterField GetFilterField()
		{
			return new AccountingPeriodField(fBusinessObjectFactory);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^(Accounting )?Period$", RegexOptions.IgnoreCase);
		}
	}
}
