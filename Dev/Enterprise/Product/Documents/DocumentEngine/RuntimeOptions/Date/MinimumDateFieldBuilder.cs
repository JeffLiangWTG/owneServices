using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class MinimumDateFieldBuilder : MaxMinDateFieldBuilder
	{
		public MinimumDateFieldBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"on or after [this ]date", Res.GetString("FilterDocumentation|02180ECD-8CDC-4472-9EE8-928B7D790DC2", "Generates a date filter for filtering data on or after a given date."), supportedProperties);
		}

		protected override string GetPattern()
		{
			return (NoResString)"on or after (this )?date";
		}

		protected override FilterField GetFilterField()
		{
			return new MinimumDateField(fBusinessObjectFactory);
		}

		protected override void SetDefaultValue(FilterField newField, ZDateTime defaultValue)
		{
			((MinimumDateField)(newField)).Value = defaultValue;
		}
	}
}
