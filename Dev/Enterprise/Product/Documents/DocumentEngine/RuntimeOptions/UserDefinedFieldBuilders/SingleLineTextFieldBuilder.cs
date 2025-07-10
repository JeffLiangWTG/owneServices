using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class SingleLineTextFieldBuilder : UserDefinedFieldBuilder
	{
		public SingleLineTextFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			return new TextField(Factory);
		}

		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^\s*text\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Text", Res.GetString("UDFFieldDocumentation|B4A41CA5-4238-4425-9F2B-37FE0586EE9D", "Generates a single-line text box control for a text field."), supportedProperties);
		}
	}
}
