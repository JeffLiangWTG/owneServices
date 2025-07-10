using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorLogStackLineExtractor
	{
		public HelpErrorLogStackLineExtractor()
		{
			stackLineRegexes = EDIDataRegistry.Instance.ErrorLogStackLineExtractorRegexes
				.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				.Cast<ExceptionKeyRegex>()
				.Select(e => new Regex(e.Regex, RegexOptions.Compiled))
				.ToArray();
		}

		readonly Regex[] stackLineRegexes;

		public IEnumerable<StackLine> ReadStackLines(string logXml)
		{
			var lines = new List<StackLine>();
			foreach (var stack in XElement.Parse(logXml).XPathSelectElements("/ExceptionDetails"))
			{
				AppendStackLines(stack, lines);
			}
			return lines;
		}

		void AppendStackLines(XElement exception, List<StackLine> lines)
		{
			if (exception != null)
			{
				foreach (var inner in exception.XPathSelectElements("InnerException"))
				{
					AppendStackLines(inner, lines);
				}

				lines.AddRange(exception.XPathSelectElements("StackTrace/Call").Select(element => ExtractStackLineCount(element)).Where(call => call != null));
			}
		}

		StackLine ExtractStackLineCount(XElement element)
		{
			StackLine stackLine = null;
			if (element.Attribute("Assembly") != null)
			{
				stackLine = new StackLine(
						assembly: StackLineAssemblyLookupHelper.TruncateAssembly(element.Attribute("Assembly")?.Value),
						type: StackLineAssemblyLookupHelper.TruncateType(element.Attribute("Type")?.Value),
						method: StackLineAssemblyLookupHelper.TruncateMethod(element.Attribute("Method")?.Value),
						parameters: element.Attribute("Parameters") == null || string.Equals(element.Attribute("Parameters").Value, "NoParameters", StringComparison.OrdinalIgnoreCase) ? string.Empty : StackLineAssemblyLookupHelper.TruncateParameters(element.Attribute("Parameters").Value),
						fullStackLine: ParseStackLine(element)
				);
			}
			else
			{
				var stack = ParseStackLine(element);
				if (!string.IsNullOrEmpty(stack))
				{
					stackLine = new StackLine(stack);
				}
			}

			if (StackLineAssemblyLookupHelper.IsWeightCalculableStackLine(stackLine))
			{
				return stackLine;
			}

			return null;
		}

		string ParseStackLine(XElement element)
		{
			var line = element.Value.Trim();
			foreach (var stackLineRegex in stackLineRegexes)
			{
				var match = stackLineRegex.Match(line);
				if (match.Success)
				{
					return StackLineAssemblyLookupHelper.TruncateStackLine(match.Groups["line"].Value);
				}
			}
			return string.Empty;
		}
	}
}
