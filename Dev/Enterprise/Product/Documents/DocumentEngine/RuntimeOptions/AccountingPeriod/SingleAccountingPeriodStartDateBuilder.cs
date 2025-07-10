using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SingleAccountingPeriodStartDateBuilder : FilterBuilder
	{
		public SingleAccountingPeriodStartDateBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"[Accounting ]Period start date", Res.GetString("FilterDocumentation|BE4DAA30-BC53-4AA5-9758-5272A8364CDE", "Generates a single accounting period control which uses the first day of the typed period as the filter value."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^(Accounting )?Period start date", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new SingleAccountingPeriodStartDateField(fBusinessObjectFactory);
		}
	}
}
