using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class IsFunctionalityValid : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<IsFunctionalityValid(Code)>",
				ResString.GetMultilingualString("94ab078e-d372-433d-a6ac-72cdc4d4ba41", "Returns Y if the functionality indicated by the 'Code' is valid or enabled for the current logged in company, and returns N otherwise."),
				new List<(string example, object expectedResult)> { ((NoResString)"<IsFunctionalityValid(Code)>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = regex.Match(macro);
			var code = match.Groups["Code"].Value;
			var matches = Regex.Matches(code);

			if (matches.Count != 1)
			{
				ReportMacroError(report, Res.GetString("77db8aee-3d83-43c3-819e-e58ba823817f", "Wrong number of arguments. Expected Macro: [<IsFunctionalityValid(code)>], where code is the only argument."));
			}
			var isValid = ObjectFactory.Get<IZZCustomsFunctionalityEffectiveDate>().IsFunctionalityValid(code, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today);
			return ((ZBool)isValid).ToYN();
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)IsFunctionalityValid(?:[\s]*)\((?:[\s]*)(?<Code>[^\s]+)(?:[\s]*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
