using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Dat.Integration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace CWNetCoreTest.TestAdapter.Test
{
	public class TestDiscovererTests : TestCaseWithFactory
	{
		public void TestAddExplicitAttribute_NotOnList_ExplicitNotAdded()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);

			var explicistList = new HashSet<string>() { "MyNameSpace.Utilities.Benchmark" };

			var result = NetCoreTestDiscoverer.AddExplicitAttribute(explicistList, testCase);

			AssertEquals("Attribute should not be added if test case not found in the list.", expected: false, result);
		}

		public void TestAddExplicitAttribute_EmptyList_ExplicitNotAdded()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);

			var explicistList = new HashSet<string>();

			var result = NetCoreTestDiscoverer.AddExplicitAttribute(explicistList, testCase);

			AssertEquals("Attribute should not be added if the list is empty.", expected: false, result);
		}

		public void TestAddExplicitAttribute_TestOnList_ExplicitAdded()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);

			var explicistList = new HashSet<string>() { "MyNameSpace.GUI.TestInterfaceInteraction" };

			var result = NetCoreTestDiscoverer.AddExplicitAttribute(explicistList, testCase);

			AssertEquals("Fully qualified name exists on the explicit list, attribute should be added.", expected: true, result);
		}

		public void TestAddExplicitAttribute_TraitAlreadyExists_NoDuplicate()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);

			var explicitList = new HashSet<string>() { "MyNameSpace.GUI.TestInterfaceInteraction" };

			// Manually add the trait before calling the method
			testCase.Traits.Add("Explicit", string.Empty);

			var result = NetCoreTestDiscoverer.AddExplicitAttribute(explicitList, testCase);

			AssertEquals("Method should return true since the test is in the explicit list.", expected: true, result);
			AssertEquals("There should be only one instance of the 'Explicit' trait.", 1, testCase.Traits.Count(trait => trait.Name == "Explicit"));
		}

		public void TestAddDatCapabilityRequirements_TestDescriptorDatTestFlagsContainGUITest()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);
			var descriptor = new TestDescriptor(
				new TestIdentifier("testScope", "testElement", "testTarget"),
				0,
				DatTestFlags.GUITest,
				new[] { "A", "NET48" }
			);

			NetCoreTestDiscoverer.AddDatCapabilityRequirements(testCase, descriptor);

			var trait = testCase.Traits.FirstOrDefault(t => t.Name == "DAT:CapabilityRequirements");
			AssertNotNull(trait);

			AssertEquals(
				"Trait value should contain GUI and remove net48 requirements.",
				"A,GUI",
				trait?.Value
			);
		}

		public void TestAddDatCapabilityRequirements_TestDescriptorDatTestFlagsNotContainGUITest()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);
			var descriptor = new TestDescriptor(
				new TestIdentifier("testScope", "testElement", "testTarget"),
				0,
				DatTestFlags.Default,
				new[] { "A", "NET48" }
			);

			NetCoreTestDiscoverer.AddDatCapabilityRequirements(testCase, descriptor);

			var trait = testCase.Traits.FirstOrDefault(t => t.Name == "DAT:CapabilityRequirements");
			AssertNotNull(trait);

			AssertEquals(
				"Trait value should contain not GUI and remove net48 requirements.",
				"A",
				trait?.Value
			);
		}

		public void TestAddDatCapabilityRequirements_TestDescriptorCapabilityRequirementsIsNull()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);
			var descriptor = new TestDescriptor(
				new TestIdentifier("testScope", "testElement", "testTarget"),
				0,
				DatTestFlags.Default
			);

			NetCoreTestDiscoverer.AddDatCapabilityRequirements(testCase, descriptor);

			var trait = testCase.Traits.FirstOrDefault(t => t.Name == "DAT:CapabilityRequirements");
			AssertNull(trait);
		}

		public void TestAddDatCapabilityRequirements_TestDescriptorCapabilityRequirementsIsEmptyl()
		{
			var testCase = new TestCase(
				"MyNameSpace.GUI.TestInterfaceInteraction",
				new Uri("executor://mstestadapter/v2"),
				"C:\\Tests\\MyTestProject.dll"
			);
			var descriptor = new TestDescriptor(
				new TestIdentifier("testScope", "testElement", "testTarget"),
				0,
				DatTestFlags.Default,
				[]
			);

			NetCoreTestDiscoverer.AddDatCapabilityRequirements(testCase, descriptor);

			var trait = testCase.Traits.FirstOrDefault(t => t.Name == "DAT:CapabilityRequirements");
			AssertNull(trait);
		}
	}
}
