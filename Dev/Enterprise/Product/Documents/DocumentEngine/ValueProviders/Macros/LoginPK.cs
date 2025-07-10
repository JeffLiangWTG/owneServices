using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LoginPK : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginPK>",
				ResString.GetMultilingualString("f5ed6042-ebc2-45f1-8cf5-a8545596cb5e", "Gets the PK of the {0} record for the current user logged into {1}.", "GlbStaff", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginPK>", GlbStaff.CurrentUser.PK.ToString()) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GlbStaff.CurrentUser != null ? GlbStaff.CurrentUser.PK.ToString() : ZGuid.Empty.ToString();
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Login\s*PK\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
