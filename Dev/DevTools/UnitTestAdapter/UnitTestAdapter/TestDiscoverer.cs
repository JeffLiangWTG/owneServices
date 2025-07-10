using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dat.Integration;
using Enterprise.Dat.Implementation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace CWNUnit.TestAdapter
{
	[FileExtension(".dll")]
	[FileExtension(".exe")]
	[DefaultExecutorUri(TestExecutor.ExecutorUriString)]
	public class TestDiscoverer : ITestDiscoverer
	{
		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
		{
			GetTestCases(sources, discoverySink);
		}

		public static ICollection<TestCase> GetTestCases(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink)
		{
			var testCases = new List<TestCase>();
			if (!TestOptionsManager.IsTestAdapterEnabled)
			{
				return testCases;
			}

			CecilExtensions.ClearCache();

			var testAdapterContext = new TestAdapterContext(new Dictionary<string, string>
			{
				{ TestDiscoveryOptions.IncludeDeveloperOnlyTests, string.Empty },
			});
			var testDiscovery = new TestDiscovery(testAdapterContext);

			foreach (var source in sources)
			{
				var testCasesForSource = GetTestCases(source, testDiscovery);
				testCases.AddRange(testCasesForSource);
			}

			if (discoverySink != null)
			{
				foreach (var testCase in testCases)
				{
					discoverySink.SendTestCase(testCase);
				}
			}

			CecilExtensions.ClearCache();

			return testCases;
		}

		static IEnumerable<TestCase> GetTestCases(string source, ITestDiscovery testDiscovery) => GetTestCases(source, testDiscovery, TestExecutor.ExecutorUri, (t, d) => { });

		public static IEnumerable<TestCase> GetTestCases(string source, ITestDiscovery testDiscovery, Uri executorUri, Action<TestCase, TestDescriptor> populateTraits)
		{
			var assemblyFileName = GetAssemblyFileName(source);
			foreach (var testDescriptor in testDiscovery.GetTests(string.Empty, Path.GetDirectoryName(source), Path.GetFileNameWithoutExtension(source)))
			{
				var testCase = TestUtilities.TestDescriptorToTestCase(testDescriptor, assemblyFileName, executorUri);
				AddSourceDetails(testCase, assemblyFileName, testDescriptor.Identifier.ElementName, testDescriptor.Identifier.TargetName);
				populateTraits(testCase, testDescriptor);
				yield return testCase;
			}
		}

		public static string GetAssemblyFileName(string source)
		{
			var assemblyFileName = source;
			if (!Path.IsPathRooted(assemblyFileName))
			{
				assemblyFileName = Path.Combine(Directory.GetCurrentDirectory(), assemblyFileName);
			}
			return assemblyFileName;
		}

		static void AddSourceDetails(TestCase testCase, string assemblyFileName, string typeName, string methodName)
		{
			var methodDefinition = CecilExtensions.GetMonoCecilMethodDefinition(assemblyFileName, typeName, methodName);
			if (methodDefinition?.DebugInformation?.SequencePoints != null)
			{
				var sequencePoint = methodDefinition.DebugInformation.SequencePoints.Where(s => !s.IsHidden).OrderBy(s => s.StartLine).FirstOrDefault();
				testCase.CodeFilePath = sequencePoint?.Document.Url;
				testCase.LineNumber = sequencePoint?.StartLine ?? -1;
			}
		}
	}
}
