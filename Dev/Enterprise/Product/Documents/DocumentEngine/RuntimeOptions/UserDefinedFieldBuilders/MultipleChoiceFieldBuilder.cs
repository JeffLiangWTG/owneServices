using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class MultipleChoiceFieldBuilder : UserDefinedFieldBuilder
	{
		public MultipleChoiceFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
			ExpectedProperties.AddRange((new MultipleChoiceBuilder(validators, factory, evaluatorForDefaultValues, ReportRunningType.Document)
				.ExpectedProperties));
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			var field = new MultipleChoice(Factory);
			if (isInRuntime)
			{
				var multipleChoiceBuilder = new MultipleChoiceBuilder(validators, Factory, evaluatorForDefaultValues, ReportRunningType.Document);
				multipleChoiceBuilder.DoCustomBuilding(fieldDef, field);
			}
			return field;
		}

		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^\s*multiplechoice\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter("MultipleChoice", Res.GetString("UDFFieldDocumentation|ECCAE54B-5C22-4051-9D7D-D50C5EBE7F49", "Generates a check box filter with a lookup list allowing multiple selections. Set the field value with the selected values."), supportedProperties);
		}
	}
}
