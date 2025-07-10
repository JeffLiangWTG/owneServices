using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using WTG.RtfConverter;
using WTG.RtfConverter.Dom;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.EU.NCTS.GUI;

public static class ColorizerHelper
{
	static readonly Regex XmlDeclarationRegex = new(
		pattern: @"<\?(.*?)\?>",
		RegexOptions.Compiled | RegexOptions.Singleline);

	[ThreadSafe]
	static readonly Lazy<XslCompiledTransform> XMLToHtmlTransform = new(GetXslt);

	static XslCompiledTransform GetXslt()
	{
		var result = new XslCompiledTransform();

		var filename = "XmlColorizer.xslt";
		var resourceName = "Enterprise.Customs.EU.NCTS.GUI.Common.ResourceFiles." + filename;
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

		using var streamReader = new StreamReader(stream);
		var xsltText = streamReader.ReadToEnd();

		using var reader = XmlReader.Create(new StringReader(xsltText));
		result.Load(reader);
		return result;
	}

	public static string GetXMLVersionString(string input)
		=> !string.IsNullOrEmpty(input)
			&& XmlDeclarationRegex.Match(input) is { Success: true } match
			? match.Value
			: string.Empty;

	public static string ColorizeHtmlText(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}

		var xmlDeclaration = GetXMLVersionString(input);
		var result = new StringBuilder();
		try
		{
			var readerSettings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Parse,
				IgnoreWhitespace = false,
				IgnoreComments = false,
				IgnoreProcessingInstructions = false,
			};

			var writerSettings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = false,
				ConformanceLevel = ConformanceLevel.Fragment,
			};
			using var inputReader = XmlReader.Create(new StringReader(input), readerSettings);
			using var outputWriter = new StringWriter();
			using var xmlWriter = XmlWriter.Create(outputWriter, writerSettings);
			XMLToHtmlTransform.Value.Transform(inputReader, xmlWriter);

			if (!string.IsNullOrEmpty(xmlDeclaration))
			{
				result.Append($"<span style='color:blue;'>{xmlDeclaration}</span>\n");
			}
			result.Append(outputWriter.ToString());
			return result.ToString();
		}
		catch (XmlException)
		{
			var newContent = SanitizeHtml(input);
			var content = newContent.Reduce(HtmlEncoder.CreateFactory()).Markup();
			return content;
		}
	}

	static IEnumerableTree<IWtgNode> SanitizeHtml(string html)
	{
		var result = HtmlParser.Parse(html).Decode();
		result = SetDefaultFont(result);
		result = result.DetectLineEndings(PlaintextParserConfiguration.Default);
		result = result.DetectLinks();
		result = result.PrefixWwwLinkDestinationIfProtocolMissing("./");
		result = result.SanitizeLinks();
		return result;
	}

	static IEnumerableTree<IWtgNode> SetDefaultFont(IEnumerableTree<IWtgNode> tree)
	{
		return tree.Reduce(b => b.AsSafeVisitor() with
		{
			OnEnter = parent =>
			{
				if (parent is Phrase phrase)
				{
					if (phrase is { Font: not null, Size: null })
					{
						b.Open(phrase with { Size = new Unit(12, UnitType.Point) });
					}
					else
					{
						var font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 10);
						var defaults = new Phrase()
						{
							Font = font.Name,
							Size = ConvertFontSize(font.Size, font.Unit),
						};
						b.Open(defaults.LayerWith(phrase));
					}
				}
				else
				{
					b.Open(parent);
				}
			}
		});
	}

	static Unit ConvertFontSize(float size, System.Drawing.GraphicsUnit unit) => unit switch
	{
		System.Drawing.GraphicsUnit.Point => new Unit(size, UnitType.Point),
		System.Drawing.GraphicsUnit.Inch => new Unit(size, UnitType.Inch),
		System.Drawing.GraphicsUnit.Document => new Unit(size / 300, UnitType.Inch),
		System.Drawing.GraphicsUnit.Millimeter => new Unit(size, UnitType.Millimeter),
		_ => new Unit(size, UnitType.Pixel),
	};
}
