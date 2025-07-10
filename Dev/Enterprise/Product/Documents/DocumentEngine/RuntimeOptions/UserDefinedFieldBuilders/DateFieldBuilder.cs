using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class DateFieldBuilder : UserDefinedFieldBuilder
	{
		public DateFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
			ExpectedProperties.AddRange((new DateBuilder(validators, factory, evaluatorForDefaultValues, ReportRunningType.Document)
				.ExpectedProperties));
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			var field = new DateField(Factory);
			if (isInRuntime)
			{
				var dateBuilder = new DateBuilder(validators, Factory, evaluatorForDefaultValues, ReportRunningType.Document);
				dateBuilder.DoCustomBuilding(fieldDef, field);
			}
			return field;
		}

		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^\s*date\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Date", Res.GetString("UDFFieldDocumentation|C7DB9881-B91F-4569-9C4E-977155699D66", "Generates a date control. Set the field value with the date."), supportedProperties);
		}
	}
}
