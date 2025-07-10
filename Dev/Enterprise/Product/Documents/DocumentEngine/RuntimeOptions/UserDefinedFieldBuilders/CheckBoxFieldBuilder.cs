using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class CheckBoxFieldBuilder : UserDefinedFieldBuilder
	{
		public CheckBoxFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
			ExpectedProperties.AddRange((new OptionGroupBuilder(validators, factory, evaluatorForDefaultValues, ReportRunningType.Document)
				.ExpectedProperties));
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			var field = new OptionGroup(Factory);
			if (isInRuntime)
			{
				var optionGroupBuilder = new OptionGroupBuilder(validators, Factory, evaluatorForDefaultValues, ReportRunningType.Document);
				optionGroupBuilder.DoCustomBuilding(fieldDef, field);
			}
			return field;
		}

		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^\s*checkbox\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Checkbox", Res.GetString("UDFFieldDocumentation|5DD5C380-AABF-4FDC-AEEC-140CFE67468F", "Generates a check box control from the provided options allowing multiple selections. Set the value of this field with the selected values."), supportedProperties);
		}
	}
}
