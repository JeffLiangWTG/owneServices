using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class NumberFieldBuilder : UserDefinedFieldBuilder
	{
		public NumberFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
			ExpectedProperties.AddRange((new NumberBuilder(validators, factory, evaluatorForDefaultValues, ReportRunningType.Document)
				.ExpectedProperties));
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			var field = new NumberField(Factory);
			if (isInRuntime)
			{
				var numberBuilder = new NumberBuilder(validators, Factory, evaluatorForDefaultValues, ReportRunningType.Document);
				numberBuilder.DoCustomBuilding(fieldDef, field);
			}
			return field;
		}

		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^\s*number\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Number", Res.GetString("UDFFieldDocumentation|054BE273-C192-4D4D-AE77-72EF3C8DF2F6", "Generates a number control whose value is a decimal. Set the field value with the number."), supportedProperties);
		}
	}
}
