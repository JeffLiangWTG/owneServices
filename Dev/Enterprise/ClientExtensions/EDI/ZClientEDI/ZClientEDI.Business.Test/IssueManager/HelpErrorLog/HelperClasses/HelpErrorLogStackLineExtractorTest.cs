using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.Client.EDI.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public class HelpErrorLogStackLineExtractorTest : TransactionedTestCase
	{
		public void TestExtractStackLines()
		{
			TestExtractStackLinesCore("SampleCallsWithAssembly01.xml", "SampleCallsWithAssembly01_StackLines.xml");
			TestExtractStackLinesCore("SampleCallsWithAssembly02.xml", "SampleCallsWithAssembly02_StackLines.xml");
			TestExtractStackLinesCore("SampleCallsWithoutParameters.xml", "SampleCallsWithoutParameters_StackLines.xml");
			TestExtractStackLinesCore("SampleCallsWithInnerExceptions01.xml", "SampleCallsWithInnerExceptions01_StackLines.xml");
			TestExtractStackLinesCore("SampleKorean.xml", "SampleKorean_StackLines.xml");
			TestExtractStackLinesCore("SampleCallsWithInvalidStackLine.xml", "SampleCallsWithInvalidStackLine_StackLines.xml");
		}

		public void TestExtractStackLinesWithExtraRegex()
		{
			var sampleLog = GetFileContent("SampleUnknown.xml");
			var extractor = new HelpErrorLogStackLineExtractor();
			var actualCalls = extractor.ReadStackLines(sampleLog).ToArray();

			var regexes = EDIDataRegistry.Instance.ErrorLogStackLineExtractorRegexes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			regexes.Add(new ExceptionKeyRegex() { Regex = @"^omenby:\s*(?<line>.*\(.*\)).*$", Description = "" });
			EDIDataRegistry.Instance.ErrorLogStackLineExtractorRegexes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regexes);

			AssertEquals("Expect there to be no calls returned", 0, actualCalls.Length);

			TestExtractStackLinesCore("SampleUnknown.xml", "SampleUnknown_StackLines.xml");
		}

		static void TestExtractStackLinesCore(string sampleLogFile, string sampleCallsFile)
		{
			var sampleLog = GetFileContent(sampleLogFile);
			var sampleCalls = GetFileContent(sampleCallsFile);

			var expectedCalls = GetStackCalls(sampleCalls).ToArray();

			var extractor = new HelpErrorLogStackLineExtractor();
			var actualCalls = extractor.ReadStackLines(sampleLog).ToArray();

			var comparer = new StackLineEqualityComparer();

			var areEqual = true;
			var errorBuilder = new StringBuilder();
			for (int i = 0; i < expectedCalls.Length; i++)
			{
				if (!comparer.Equals(expectedCalls[i], actualCalls[i]))
				{
					areEqual = false;
					errorBuilder.AppendLine($"[{i:n00}]: Expected '{expectedCalls[i].FullStackLine.ToUpperInvariant()}' but was '{actualCalls[i].FullStackLine.ToUpperInvariant()}'");
				}
			}
			var message = areEqual ? string.Empty : errorBuilder.ToString();
			AssertEquals(message, expected: true, actual: areEqual);
		}

		static IEnumerable<StackLine> GetStackCalls(string xml)
		{
			return XElement.Parse(xml).XPathSelectElements("/Call").Select(element =>
			{
				if (element.Attribute("Assembly") != null)
				{
					return new StackLine(
						assembly: element.Attribute("Assembly").Value,
						type: element.Attribute("Type").Value,
						method: element.Attribute("Method").Value,
						parameters: element.Attribute("Parameters").Value,
						fullStackLine: element.Value);
				}
				else
				{
					return new StackLine(element.Value);
				}
			});
		}

		static string GetFileContent(string fileName)
		{
			return SampleFileRetriever.GetFileContent(fileName);
		}
	}
}
