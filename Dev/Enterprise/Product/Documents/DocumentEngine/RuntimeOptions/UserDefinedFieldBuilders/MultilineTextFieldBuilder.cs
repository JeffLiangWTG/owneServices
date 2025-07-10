using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class MultilineTextFieldBuilder : UserDefinedFieldBuilder
	{
		public MultilineTextFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			return new TextField(true, Factory);
		}

		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^\s*memo\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Memo", Res.GetString("UDFFieldDocumentation|46BAC09F-A007-4A43-AC81-99FE375A3AB8", "Generates a multi-line text box control for a text field."), supportedProperties);
		}
	}
}
