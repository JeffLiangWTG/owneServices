using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginEmail : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginEmail>",
				ResString.GetMultilingualString("f378a6a4-0bc9-414e-a2aa-5be0c17ca196", "Gets the Email Address of the current user logged into {0}.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginEmail>", "mail@mail.com") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.GS_EmailAddress : ZString.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)login(?:[\s]*)email(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
