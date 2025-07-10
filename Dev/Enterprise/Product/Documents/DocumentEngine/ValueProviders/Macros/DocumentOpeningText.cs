using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DocumentOpeningText : DocumentText
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DocumentOpening({fieldname1}[ {fieldname2}[ {fieldname3}]...] [, {modePrefix}])>",
				ResString.GetMultilingualString("3ed60e4e-149d-4c14-91b1-2ac9820bda1c", @"Reads the Document Opening Text from the {0} registry settings for Document Opening Texts. 
There are settings for each style of document in the registry under 'Documents', and a fallback opening text in 'Documents\\Default Opening Text'. Unlike most function based macros, this function accepts field names only as parameters (separated by spaces with no surrounding angle brackets). 
This means you cannot pass in text literals at this point.
The best way to work out how to use this function is to find a document that is showing the text you want to show on yours and implement your usage the same way.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ($"<DocumentOpening(LEVEL ReportName)>", (NoResString)"Some opening text"), ((NoResString)"<DocumentOpening(TransportStatus Note, TransportMode)>", (NoResString)"Some opening text") });
		}

		protected override DocumentTextType DocumentTextType
		{
			get { return DocumentTextType.Open; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Document\s*opening\s*" + regexParameters + @"\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
