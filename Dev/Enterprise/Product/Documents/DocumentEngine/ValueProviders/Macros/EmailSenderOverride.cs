using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class EmailSenderOverride : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<EmailSenderOverride>",
				ResString.GetMultilingualString("ad84b851-e0ae-4b24-b280-d0a960e6f028", "Returns the report's email sender override address, or an empty string if no override exists."),
				new List<(string example, object expectedResult)> { ("<EmailSenderOverride>", "mail@email.com") });
		}

		protected sealed override object GetReplacementCore(string macro, Report report)
		{
			return (report.MenuItem != null && !String.IsNullOrEmpty(report.MenuItem.SU_EmailSenderOverride)) ? report.MenuItem.SU_EmailSenderOverride : ZString.Empty;
		}

		public override Regex Regex
		{
			get { return _regex; }
		}
		static readonly Regex _regex = new Regex(@"^<EmailSenderOverride>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
