using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DocumentClosingText : DocumentText
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DocumentClosing({fieldname1}[ {fieldname2}[ {fieldname3}]...] [, {modePrefix}])>",
				ResString.GetMultilingualString("67df1550-1d95-4f42-a9da-7f09f1c3515c", @"Reads the Document Closing Text from the {0} registry settings for Document Closing Texts. 
There are settings for each style of document in the registry under 'Documents', and a fallback closing text in 'Documents\Default Closing Text'. Unlike most function based macros, this function accepts field names only as parameters (separated by spaces with no surrounding angle brackets). 
This means you cannot pass in text literals at this point. 
The best way to work out how to use this function is to find a document that is showing the text you want to show on yours and implement your usage the same way.", Core.Constants.ProductName),
				new List<(string example, object expectedResult)> { ($"<DocumentClosing(LEVEL ReportName)>", (NoResString)"Some closing text"), ((NoResString)"<DocumentClosing(TransportStatus Note, TransportMode)>", (NoResString)"Some closing text") });
		}

		protected override DocumentTextType DocumentTextType
		{
			get { return DocumentTextType.Close; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Document\s*closing\s*" + regexParameters + @"\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
