using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class LoginPhoneExtensionNumber : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LoginPhoneExtensionNumber>",
				ResString.GetMultilingualString("36f766d8-7c2c-41a4-9d16-2b317e90acc3", "Gets the published phone extension number of the current user logged into {0}.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ("<LoginPhoneExtensionNumber>", "123") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (GlbStaff.CurrentUser != null)
			{
				return GlbStaff.CurrentUser.GS_PublishWorkExtension ? GlbStaff.CurrentUser.GS_WorkExtension : ZString.Empty;
			}
			return string.Empty;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<(?:[\s]*)Login(?:[\s]*)Phone(?:[\s]*)Extension(?:[\s]*)Number(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
