using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GroupIndex : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GroupIndex>",
				ResString.GetMultilingualString("ecc45766-e6aa-4393-9e7c-988a71b4c05b", "Gives the Index of the current group."),
				new List<(string example, object expectedResult)> { ("<GroupIndex>", 1) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			int result = 0;

			var currentArea = report.Renderer.CurrentAreaToProcess;

			if (currentArea is GroupByArea)
			{
				var section = report.Analyser.Sections.Find(new Predicate<Section>((Section a) => { return a.SplitSectionBodyAreaInstances.Contains(((GroupByArea)currentArea).SectionBody); }));
				if (section != null)
				{
					result += section.SplitSectionBodyAreaInstances.IndexOf((currentArea as GroupByArea).SectionBody) + 1;
				}
			}
			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^(?:[\s]*)<(?:[\s]*)GroupIndex(?:[\s]*)>(?:[\s]*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
