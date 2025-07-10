using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginCode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginCode>",
				ResString.GetMultilingualString("a1caabe6-01de-4831-a837-f70f85f6091f", "Gets the Short Code (usually initials) of the current user logged into {0}.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginCode>", "WTG") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)login(?:[\s]*)code(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
