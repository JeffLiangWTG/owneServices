using System.Linq;
using Dat.Integration;
using TestCaseBaseClass = NUnit.Framework.TestCase;

namespace CWNUnit.TestAdapter.Tests
{
	public class TestDiscovererTestCase : TestCaseBaseClass
	{
		public void TestGetTestCases()
		{
			var testCases = TestDiscoverer.GetTestCases(new[] { GetType().Assembly.Location }, null).ToList();
			AssertNotEquals(0, testCases.Count);

			var thisTestCase = testCases.FirstOrDefault(t => t.FullyQualifiedName == GetType().FullName + "." + nameof(TestGetTestCases));
			AssertNotNull(thisTestCase);

			AssertEquals(GetType().Assembly.Location, thisTestCase.Source);
			Assert(thisTestCase.CodeFilePath.EndsWith("TestDiscovererTestCase.cs"));
			AssertNotEquals(0, thisTestCase.LineNumber);

			AssertNotNull(thisTestCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.ProjectDefinedCapabilityRequirements)));
			AssertNotNull(thisTestCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.DatTestFlags)));
			AssertNotNull(thisTestCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.CapabilityRequirements)));
		}

		public void TestOptions()
		{
			using (TestOptionsManager.OverrideTestOptions(new TestOptions { Enabled = false }))
			{
				var testCases = TestDiscoverer.GetTestCases(new[] { GetType().Assembly.Location }, null).ToList();
				AssertEquals("Should not discover any test", 0, testCases.Count);
			}

			using (TestOptionsManager.OverrideTestOptions(new TestOptions { Enabled = true }))
			{
				var testCases = TestDiscoverer.GetTestCases(new[] { GetType().Assembly.Location }, null).ToList();
				AssertNotEquals("Should discover tests", 0, testCases.Count);
			}
		}

		#region Nested class

		public void TestDiscoverNestedClassTestCase()
		{
			var testCases = TestDiscoverer.GetTestCases(new[] { GetType().Assembly.Location }, null).ToList();
			AssertNotEquals(0, testCases.Count);

			var testCase = testCases.FirstOrDefault(t => t.FullyQualifiedName.EndsWith("." + nameof(NestedClassTestClass.TestPass)));
			AssertNotNull(testCase);

			AssertEquals(typeof(NestedClassTestClass).FullName + "." + nameof(NestedClassTestClass.TestPass), testCase.FullyQualifiedName);

			AssertEquals(GetType().Assembly.Location, testCase.Source);
			Assert(testCase.CodeFilePath.EndsWith("TestDiscovererTestCase.cs"));
			Assert(testCase.LineNumber > 0);

			AssertNotNull(testCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.ProjectDefinedCapabilityRequirements)));
			AssertNotNull(testCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.DatTestFlags)));
			AssertNotNull(testCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.CapabilityRequirements)));
		}

		public class NestedClassTestClass : TestCaseBaseClass
		{
			public void TestPass()
			{
				Assert(true);
			}
		}

		#endregion
	}
}
