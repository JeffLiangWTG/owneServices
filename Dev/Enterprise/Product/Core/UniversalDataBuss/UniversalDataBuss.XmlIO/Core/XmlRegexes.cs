using System.Text.RegularExpressions;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	static class XmlRegexes
	{
		internal static readonly Regex EncodingTag = new Regex(@"\<\?xml\sversion\s*=\s*""(?<version>[0-9]+\.[0-9]+)""\sencoding\s*=\s*""(?<encoding>.+)""\?\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		internal static readonly Regex RootElementTag = new Regex(@"\<" + validElementName + @"(?:" + version + xmlns + "|" + xmlns + version + @")+\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		internal static readonly Regex ValidOpeningTag = new Regex(@"\<" + validElementName + attributes + @"\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		internal static readonly Regex ValidClosingTag = new Regex(@"\</" + validElementName + @"\s*\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		internal static readonly Regex SelfEndingElement = new Regex(@"\<" + validElementName + attributes + @"\s?/\>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		internal static readonly Regex SelfEndingLineRegex = new Regex(@"\</" + validElementName + @"\>$|/\>$", RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.CultureInvariant);

		internal static readonly Regex FullOpeningTag = new Regex(@"\<(?<tag>[^\<\>]+)\>");
		internal static readonly Regex FullClosingTag = new Regex(@"\</(?<tag>[^\<\>]+)\>");

		internal static readonly Regex ElementContentEscapes = new Regex(@"&(?<content>[amplgt]+);");
		internal static readonly Regex AttributeContentEscapes = new Regex(@"&(?<content>[ampquolgt]+);");

		internal static readonly Regex SingleAttribute = new Regex(@"(" + validAttributeName + @"\s*=\s*""(?<attributeValue>[^""\<\>]*)"")", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		const string validAttributeName = @"(?<attributeName>" + validTagValues + ")";
		const string validElementName = @"((?:|" + validTagValues + @"\:)(?<elementName>" + validTagValues + "))";
		const string validTagValues = @"[\-_a-zA-Z0-9]+";
		const string attributes = @"(?:|(?<attributes>\s[a-zA-Z0-9]+\s*=\s*[^>]+""\s?))";
		const string version = @"(?:|\s(V|v)ersion\s*=\s*""(?<version>[0-9]+\.[0-9]+)"")";
		const string xmlns = @"(?:(?:|\sxmlns(?:\:" + validTagValues + @")\s*=\s*""(?:[^\s]+))|(?:|\sxmlns\s*=\s*""(?<xmlns>[^\s]+))"")";
	}
}
