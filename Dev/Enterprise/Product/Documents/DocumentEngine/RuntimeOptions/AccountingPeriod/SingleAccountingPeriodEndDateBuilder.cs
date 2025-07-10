using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class SingleAccountingPeriodEndDateBuilder : FilterBuilder
	{
		public SingleAccountingPeriodEndDateBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"[Accounting ]Period end date", Res.GetString("FilterDocumentation|F3D2D93A-17AE-481E-9CD6-95A418BDCF24", "Generates a single accounting period control which uses the last day of the typed period as the filter value."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^(Accounting )?Period end date", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new SingleAccountingPeriodEndDateField(fBusinessObjectFactory);
		}
	}
}
