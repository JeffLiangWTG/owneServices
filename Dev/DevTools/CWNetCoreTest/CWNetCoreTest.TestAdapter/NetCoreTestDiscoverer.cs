using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise;
using CWNetCoreTest.TestAdapter.Utilities;
using Dat.Integration;
using Enterprise.Dat.Implementation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using OldCWTestDiscoverer = CWNUnit.TestAdapter.TestDiscoverer;

namespace CWNetCoreTest.TestAdapter
{
	[FileExtension(".dll")]
	[DefaultExecutorUri(Nunit3ExecutorUri)]
	public class NetCoreTestDiscoverer : ITestDiscoverer
	{
		public const string Nunit3ExecutorUri = "executor://CW_NetCore_TestExecutor";
		static Uri executorUri { get => new(Nunit3ExecutorUri); }
		static readonly Lazy<HashSet<string>> explicitTests = new(LoadExplicitList, isThreadSafe: true);

		public static HashSet<string> LoadExplicitList() => new AssemblyResourceProvider().LoadExplicitTestsFromContentFiles("CWNetCoreTestAdapterResources", AssemblyResourceProvider.ShouldSkipLine);

		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
		{
			foreach (var testCase in DiscoverTests(sources, logger))
			{
				discoverySink.SendTestCase(testCase);
			}
		}

		public static IEnumerable<TestCase> DiscoverTests(IEnumerable<string>? sources, IMessageLogger? logger = null)
		{
			var options = CWNUnit.TestAdapter.TestOptionsManager.LoadOptions();
			if (!options.Enabled)
			{
				logger?.SendMessage(TestMessageLevel.Informational, $"{nameof(NetCoreTestDiscoverer)}.TestOption disabled");
				yield break;
			}

			if (sources is null)
			{
				logger?.SendMessage(TestMessageLevel.Informational, $"{nameof(sources)} is null");
				yield break;
			}

			var testDiscovery = CreateTestDiscovery(options, logger);
			foreach (var source in sources)
			{
				logger?.SendMessage(TestMessageLevel.Informational, $"{nameof(NetCoreTestDiscoverer)}.DiscoverTests {source}");
				foreach (var testCase in DiscoverTestsInternal(source, testDiscovery))
				{
					_ = AddExplicitAttribute(explicitTests.Value, testCase);
					yield return testCase;
				}
			}
		}

		public static bool AddExplicitAttribute(HashSet<string> explicitList, TestCase testCase)
		{
			if (explicitList.Count == 0)
			{
				return false;   // No sense running the method if the list is empty.
			}

			if (string.IsNullOrWhiteSpace(testCase.FullyQualifiedName))
			{
				return false;   // As this may happen from time to time, we just pass it along, rather than throwing an exception.
			}

			const string traitId = "Explicit";
			if (IsTraitOnTestCase(testCase, traitId))
			{
				return true;    // Already exists, so it has already been added.
			}

			if (!explicitList.Contains(testCase.FullyQualifiedName))
			{
				return false;   // Test was not found on the list.
			}

			testCase.Traits.Add(traitId, string.Empty);
			SetPropertyValue(testCase, traitId);

			return IsTraitOnTestCase(testCase, traitId);
		}

		public static bool IsTraitOnTestCase(TestCase testCase, string traitName)
		{
			return testCase.Traits.Any(trait =>
				string.Equals(trait.Name, traitName, StringComparison.OrdinalIgnoreCase));
		}

		static void SetPropertyValue(TestCase testCase, string id)
		{
			var testProperty = TestProperty.Find(id) ?? TestProperty.Register(id, id, typeof(string), typeof(TestCase));
			testCase.SetPropertyValue(testProperty, string.Empty);
		}

		static IEnumerable<TestCase> DiscoverTestsInternal(string source, TestDiscovery testDiscovery)
		{
			ArgumentNullException.ThrowIfNull(testDiscovery, nameof(testDiscovery));
			return OldCWTestDiscoverer.GetTestCases(source, testDiscovery, executorUri, AddDatCapabilityRequirements);
		}

		internal static void AddDatCapabilityRequirements(TestCase testCase, TestDescriptor testDescriptor)
		{
			if (testDescriptor.CapabilityRequirements is { Length: > 0 })
			{
				if (testDescriptor.DatTestFlags.HasFlag(DatTestFlags.GUITest))
				{
					testCase.Traits.Add("DAT:CapabilityRequirements", string.Join(",", testDescriptor.CapabilityRequirements.Append("GUI").Where(req => req != "NET48")));
				}
				else
				{
					testCase.Traits.Add("DAT:CapabilityRequirements", string.Join(",", testDescriptor.CapabilityRequirements.Where(req => req != "NET48")));
				}
			}
		}

		internal static TestDiscovery CreateTestDiscovery(CWNUnit.TestAdapter.ITestOptions testOptions, IMessageLogger? logger = null)
		{
			var options = new Dictionary<string, string>();
			if (testOptions.IncludeDeveloperOnlyTests)
			{
				options.Add("IncludeDeveloperOnlyTests", string.Empty);
			}
			logger?.SendMessage(TestMessageLevel.Informational, $"IncludeDeveloperOnlyTests: {testOptions.IncludeDeveloperOnlyTests}");
			var adapterContext = new TestAdapterContext(options);
			return new TestDiscovery(adapterContext);
		}

		static NetCoreTestDiscoverer()
		{
			NetCoreAssemblyResolver.Setup();
		}
	}
}
