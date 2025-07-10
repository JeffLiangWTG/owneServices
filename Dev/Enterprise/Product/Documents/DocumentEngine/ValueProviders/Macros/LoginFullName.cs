using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginFullName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginFullName>",
				ResString.GetMultilingualString("8eb64796-c48d-4899-a3c9-4d324ad370a5", "Gets the Full Name of the current user logged into {0}.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginFullName>", (NoResString)"John Doe") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			GlbStaff currentUser = GlbStaff.CurrentUser;
			return currentUser != null ? currentUser.GS_FullName.ToString() : Core.Constants.ProductName;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)login(?:[\s]*)full(?:[\s]*)name(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
