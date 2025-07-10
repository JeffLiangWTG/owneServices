using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class MaximumDateFieldBuilder : MaxMinDateFieldBuilder
	{
		public MaximumDateFieldBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"on or before [this ]date", Res.GetString("FilterDocumentation|3ED0585E-1347-49F8-9984-BD362A71285E", "Generates a date filter for filtering data before a given date."), supportedProperties);
		}

		protected override string GetPattern()
		{
			return (NoResString)"on or before (this )?date";
		}

		protected override FilterField GetFilterField()
		{
			return new MaximumDateField(fBusinessObjectFactory);
		}

		protected override void SetDefaultValue(FilterField newField, ZDateTime defaultValue)
		{
			((MaximumDateField)(newField)).Value = defaultValue;
		}
	}
}
