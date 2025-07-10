using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CWNUnit.TestAdapter;
using Dat.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace CWNetCoreTest.TestAdapter.Test
{
	public class CWNetCoreTest : TestCaseWithFactory
	{
		public void TestWithEmptyAssertion()
		{
			AssertEquals("This is to test the custom Adapter", "This is to test the custom Adapter");
		}

		public void TestWithObjectFactory()
		{
			var group1 = Factory.LoadTop1<GlbGroup>(new ZQuery());
			AssertNotNull(group1);
		}

		public void TestWithDelayTimer()
		{
			Task.Delay(500).Wait();
			Assert(true);
		}

		//public void TestShouldFail()
		//{
		//	Assert(false);
		//}

		[DeveloperOnlyTest]
		public void TestShouldBeDeveloperOnly()
		{
			//TODO: WI00620929 - Developer test should be excluded from DAT
			Assert(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWithDatCapabilityRequirements()
		{
			const string expectedTraitName = "DAT:CapabilityRequirements";

			var testCases = NetCoreTestDiscoverer.DiscoverTests(sources: new[] { GetType().Assembly.Location }).ToList();
			AssertNotEquals(0, testCases.Count);

			var thisTestCase = testCases.FirstOrDefault(t => t.FullyQualifiedName == GetType().FullName + "." + nameof(TestWithDatCapabilityRequirements));

			AssertNotNull(thisTestCase);

			//Dat CapabilityRequirements should be filled in Traits.
			AssertNotEquals(0, thisTestCase?.Traits.Count());

			var trait = thisTestCase?.Traits.FirstOrDefault(t => t.Name.Equals(expectedTraitName, StringComparison.Ordinal));
			AssertNotNull(trait);
			AssertEquals(expectedTraitName, trait?.Name);
			AssertEquals("SOURCE_CODE,SQL2019", trait?.Value);

			var buildXmlPath = Path.Combine(BaseSourcePath, "Build.xml");
			AssertEquals(expected: true, File.Exists(buildXmlPath));
		}

		public void TestGetTestCases()
		{
			var testCases = NetCoreTestDiscoverer.DiscoverTests(sources: new[] { GetType().Assembly.Location }).ToList();
			AssertNotEquals(0, testCases.Count);

			var thisTestCase = testCases.FirstOrDefault(t => t.FullyQualifiedName == GetType().FullName + "." + nameof(TestGetTestCases));
			AssertNotNull(thisTestCase);

			AssertEquals(GetType().Assembly.Location, thisTestCase!.Source);
			AssertNotNull(thisTestCase.CodeFilePath);
			Assert(thisTestCase.CodeFilePath!.EndsWith("UnitTestAdapterNetCoreTest.cs"));
			AssertNotEquals(0, thisTestCase.LineNumber);

			AssertNotNull(thisTestCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.ProjectDefinedCapabilityRequirements)));
			AssertNotNull(thisTestCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.DatTestFlags)));
			AssertNotNull(thisTestCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.CapabilityRequirements)));
		}

		public void TestOptions()
		{
			using (TestOptionsManager.OverrideTestOptions(new TestOptions { Enabled = false }))
			{
				var testCases = NetCoreTestDiscoverer.DiscoverTests(new[] { GetType().Assembly.Location }).ToList();
				AssertEquals("Should not discover any test", 0, testCases.Count);
			}

			using (TestOptionsManager.OverrideTestOptions(new TestOptions { Enabled = true }))
			{
				var testCases = NetCoreTestDiscoverer.DiscoverTests(new[] { GetType().Assembly.Location }).ToList();
				AssertNotEquals("Should discover tests", 0, testCases.Count);
			}

			using (TestOptionsManager.OverrideTestOptions(new TestOptions { Enabled = true, IncludeDeveloperOnlyTests = false }))
			{
				var testCases = NetCoreTestDiscoverer.DiscoverTests(new[] { GetType().Assembly.Location }).ToList();
				AssertNull("Should exclude developer only test", testCases.FirstOrDefault(c => c.DisplayName == nameof(TestShouldBeDeveloperOnly)));
			}

			using (TestOptionsManager.OverrideTestOptions(new TestOptions { Enabled = true, IncludeDeveloperOnlyTests = true }))
			{
				var testCases = NetCoreTestDiscoverer.DiscoverTests(new[] { GetType().Assembly.Location }).ToList();
				AssertNotNull("Should include developer only test", testCases.FirstOrDefault(c => c.DisplayName == nameof(TestShouldBeDeveloperOnly)));
			}
		}
	}
}
