using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class SecurityFilterBuilder : FilterBuilder
	{
		const string SecurityRightFilterType = "SecurityRight";

		public SecurityFilterBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, factory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"securityright", Res.GetString("FilterDocumentation|A0EA693C-791C-4B22-AF64-428675E5F5E9", "Generates a filter that allow the selection of a staff security item. Filters the data matching the selected right. This filter is only available in reports using the {0} data source.", "StaffSecurity"), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return string.Equals(filterType, SecurityRightFilterType, StringComparison.OrdinalIgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new SecurityFilterField(Factory);
		}
	}
}
