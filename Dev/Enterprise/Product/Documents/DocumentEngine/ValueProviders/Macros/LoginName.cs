using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginName>",
				ResString.GetMultilingualString("dbd03427-2597-44c5-85d9-c6518cb71b78", "Gets the Login Name the current user typed in to log into {0}.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginName>", "john.doe") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_LoginName.ToString() : Core.Constants.ProductName;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Login\s*Name\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
