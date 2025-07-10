using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ModuleDescriptionFromCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ModuleDescriptionFromCode({licencingmodulecode})>",
				ResString.GetMultilingualString("ce3faff4-6640-43ac-9a24-345f1b231500", @"Gets a description for the specified licensing checkpoint based on the 3 character code passed in as a parameter."),
				new List<(string example, object expectedResult)> { ("<ModuleDescriptionFromCode(COR)>", (NoResString)"Core") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			string moduleCode = fRegex.Match(macro).Groups[1].Value;
			LicenceCheckpoint checkPoint = Env.Licence.GetCheckpointFromCode(moduleCode);
			return checkPoint != null ? checkPoint.DisplayName : "";
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)ModuleDescriptionFromCode(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
