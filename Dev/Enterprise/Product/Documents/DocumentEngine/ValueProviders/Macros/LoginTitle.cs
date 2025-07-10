using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginTitle : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginTitle>",
				ResString.GetMultilingualString("0e552ff0-8dd8-4c8a-86ab-621cc421624e", "Gets the Title of the current user logged into {0}.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginTitle>", (NoResString)"Developer") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_Title : ZString.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)login(?:[\s]*)title(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
