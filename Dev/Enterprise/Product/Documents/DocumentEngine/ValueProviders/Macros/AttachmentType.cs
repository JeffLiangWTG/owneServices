using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class AttachmentType : ValueProvider
	{
		public override Regex Regex => fRegex;
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Attachment(?:[\s]*)Type(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<AttachmentType>",
				ResString.GetMultilingualString("7d3a9331-151f-41ed-8439-905bfbdec04b", "This macro returns the 'Attachment Type' in 'Templates Used'."),
				new List<(string example, object expectedResult)> { ("<AttachmentType>", "PDF") });
		}

		protected override object GetReplacementCore(string macro, Report report) => report.AttachmentType;
	}
}
