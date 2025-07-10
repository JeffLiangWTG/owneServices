using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Enterprise.ZArchitecture.Environment
{
	public class HtmlToTextUtility
	{
		public string GetPlainText(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return string.Empty;
			}
			var document = new HtmlDocument();
			document.LoadHtml(html);
			VisitDocumentNode(document.DocumentNode);
			return resultBuilder.ToString();
		}

		void AddText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			var isStartSpace = text.StartsWith(" ");
			var isEndSpace = text.EndsWith(" ");

			text = text.TrimStart(' ', '\t').TrimEnd(' ', '\t');

			if (string.IsNullOrEmpty(text))
			{
				canAddSpace = true;
				return;
			}

			if (canAddNewLine)
			{
				if (!hasNewLine)
				{
					resultBuilder.AppendLine();
				}
			}
			else if (canAddSpace || isStartSpace)
			{
				if (resultBuilder.Length > 0 && resultBuilder[resultBuilder.Length - 1] != '\n')
				{
					resultBuilder.Append(" ");
				}
			}

			resultBuilder.Append(text);

			hasNewLine = false;
			canAddNewLine = false;
			canAddSpace = isEndSpace;
		}

		string GetTextFromNode(HtmlTextNode htmlTextNode)
		{
			var text = htmlTextNode.Text;
			if (HtmlNode.IsOverlappedClosingElement(text))
			{
				return string.Empty;
			}

			return HtmlEntity.DeEntitize(Regex.Replace(text, @"\s{2,}", " "));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html nodes")]
		void VisitChildNodes(HtmlNode node)
		{
			if (node.HasChildNodes)
			{
				foreach (var childNode in node.ChildNodes)
				{
					var childNodeName = childNode.Name;
					var childNodeType = childNode.NodeType;

					if (childNodeName == "script" || childNodeName == "style" || childNodeName == "head" || childNodeType == HtmlNodeType.Comment)
					{
						continue;
					}
					else if (childNode is HtmlTextNode htmlTextNode)
					{
						VisitTextNode(htmlTextNode);
					}
					else if (childNodeName == "img")
					{
						VisitImgNode(childNode);
					}
					else if (childNodeName == "a")
					{
						VisitANode(childNode);
					}
					else if (childNodeName == "li")
					{
						VisitLiNode(childNode);
					}
					else
					{
						VisitCommonNode(childNode);
					}
				}
			}
		}

		void VisitTextNode(HtmlTextNode htmlTextNode)
		{
			AddText(GetTextFromNode(htmlTextNode));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		void VisitImgNode(HtmlNode node)
		{
			if (node.Attributes.Contains("alt"))
			{
				var alt = node.Attributes["alt"].Value.Trim();
				if (!string.IsNullOrEmpty(alt))
				{
					AddText(string.Format(" [{0}]", alt));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		void VisitANode(HtmlNode node)
		{
			VisitChildNodes(node);

			if (node.Attributes.Contains("href"))
			{
				var href = node.Attributes["href"].Value.Trim();
				if (!string.IsNullOrEmpty(href))
				{
					if (!(href.ToLowerInvariant().StartsWith("javascript:")))
					{
						AddText(string.Format(" [{0}]", href));
					}
				}
			}
		}

		void VisitDocumentNode(HtmlNode node)
		{
			VisitChildNodes(node);
		}

		void VisitLiNode(HtmlNode node)
		{
			WriteNodeTag(node);
			AddText("* ");
			VisitChildNodes(node);
			WriteNodeTag(node.EndNode);
		}

		void VisitCommonNode(HtmlNode node)
		{
			WriteNodeTag(node);
			VisitChildNodes(node);
			if (node.EndNode != null && node.EndNode != node)
			{
				WriteNodeTag(node.EndNode);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		void WriteNodeTag(HtmlNode node)
		{
			if (node == null)
			{
				return;
			}

			if (node.Name == "br")
			{
				resultBuilder.AppendLine();
			}

			if (blockTags.Contains(node.Name))
			{
				canAddNewLine = true;
			}

			if (addSpaceTags.Contains(node.Name))
			{
				canAddSpace = true;
			}
		}

		readonly StringBuilder resultBuilder = new StringBuilder();

		bool canAddNewLine;

		bool canAddSpace;

		bool hasNewLine = true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		readonly string[] blockTags = new string[] { "address", "blockquote", "div", "dl", "fieldset", "form", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "noscript", "ol", "p", "pre", "table", "ul", "dd", "dt", "li", "tbody", "#td", "tfoot", "#th", "thead", "tr" };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		readonly string[] addSpaceTags = new string[] { "td", "th" };
	}
}
