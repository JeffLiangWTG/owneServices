using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class LookupFieldBuilder : UserDefinedFieldBuilder
	{
		public LookupFieldBuilder(ValidatorPack validators, BusinessObjectFactory factory,
			MatchEvaluator evaluatorForDefaultValues) : base(validators, factory, evaluatorForDefaultValues)
		{
			ExpectedProperties.AddRange((new LookupBuilder(validators, factory, evaluatorForDefaultValues, ReportRunningType.Document)
				.ExpectedProperties));
		}

		protected override FilterField GetFilterField(StringTreeNode fieldDef, bool isInRuntime)
		{
			var field = new LookupField(Factory);
			if (isInRuntime)
			{
				var lookupBuilder = new LookupBuilder(validators, Factory, evaluatorForDefaultValues, ReportRunningType.Document);
				lookupBuilder.DoCustomBuilding(fieldDef, field);
			}
			return field;
		}
		protected override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^.*\s+lookup\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Lookup", Res.GetString("UDFFieldDocumentation|4E11EC0D-E46C-4AED-8FDA-90F15DFCC403", "Generates a lookup filter whose value will be a GUID, with available selections from the lookup list. Set the field value with the data matching that GUID."), supportedProperties, true);
		}
	}
}
