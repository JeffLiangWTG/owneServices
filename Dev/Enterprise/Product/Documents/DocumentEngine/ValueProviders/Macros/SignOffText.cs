using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class SignOffText : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SignOffText>", ResString.GetMultilingualString("443eb942-eb29-4e59-b981-9b25e368fedf", "Returns the sign off text from the Registry."),
				new List<(string example, object expectedResult)> { ("<SignOffText>", (NoResString)"Best Regards,") });
		}

		static readonly Regex regex = new Regex(@"^<\s*SignOffText\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public SignOffText()
		{
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return Env.Registry.SignOffText;
		}
	}
}
