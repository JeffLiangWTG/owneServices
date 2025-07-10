using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(SystemProductCollection))]
	internal sealed class SystemProductCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SystemProductCollection>
	{
		public void TestGetProductByCode()
		{
			var collection = new SystemProductCollection();
			var prod1 = collection.AddNew("AAA", "Module AAA", true);
			var prod2 = collection.AddNew("BBB", "Module BBB", false);

			AssertEquals(prod2, collection.GetProductByCode("BBB"));
		}

		public void TestGetProductByCode_DoesNotThrow()
		{
			var collection = new SystemProductCollection();
			var prod1 = collection.AddNew("AAA", "Module AAA", true);
			var prod2 = collection.AddNew("BBB", "Module BBB", false);

			AssertEquals(null, collection.GetProductByCode("CCC"));
		}

		public void TestGetMapping()
		{
			var collection = new SystemProductCollection();
			var prod1 = collection.AddNew("AAA", "Module AAA", true);
			var prod2 = collection.AddNew("BBB", "Module BBB", false);

			prod1.ModuleMappings.AddNew("C1", "Description prod 1 module 1", "", false);
			prod1.ModuleMappings.AddNew("C2", "Description prod 1 module 2", "", false);
			var mapping = prod2.ModuleMappings.AddNew("C1", "Description prod 2 module 1", "", false);

			AssertEquals(mapping, collection.GetMapping("BBB", "C1"));
		}

		public void TestGetModuleList()
		{
			var collection = new SystemProductCollection();
			var prod1 = collection.AddNew("AAA", "Module AAA", true);
			var prod2 = collection.AddNew("BBB", "Module BBB", false);
			var prod3 = collection.AddNew("CCC", "Module CCC", true);

			var mapping1 = prod1.ModuleMappings.AddNew("C1", "Prod1 Module 1 Enabled", "", false);
			var mapping2 = prod1.ModuleMappings.AddNew("C2", "Prod1 Module 2 Disabled", "", false, false, false);
			var mapping3 = prod2.ModuleMappings.AddNew("C3", "Prod2 Module 1 Enabled", "", false);
			var mapping4 = prod3.ModuleMappings.AddNew("C4", "Prod3 Module 1 Disabled", "", false, false, false);
			var mapping5 = prod3.ModuleMappings.AddNew("C5", "Prod3 Module 2 Disabled", "", false, false, false);

			AssertEquals("Precondition", mapping3.IsEnabled, true);
			var expectedModuleListAllModules = new CodeDescriptionPairList();
			expectedModuleListAllModules.AddPair("C3", "Prod2 Module 1 Enabled");
			AssertContainsExactElementsInAnyOrder(expectedModuleListAllModules, collection.GetModuleList("BBB"));
			expectedModuleListAllModules.Clear();

			AssertEquals("Precondition", mapping1.IsEnabled, true);
			AssertEquals("Precondition", mapping2.IsEnabled, false);
			expectedModuleListAllModules.AddPair("C1", "Prod1 Module 1 Enabled");
			expectedModuleListAllModules.AddPair("C2", "Prod1 Module 2 Disabled");
			AssertContainsExactElementsInAnyOrder(expectedModuleListAllModules, collection.GetModuleList("AAA"));
			expectedModuleListAllModules.Clear();

			AssertEquals("Precondition", mapping4.IsEnabled, false);
			AssertEquals("Precondition", mapping5.IsEnabled, false);
			expectedModuleListAllModules.AddPair("C4", "Prod3 Module 1 Disabled");
			expectedModuleListAllModules.AddPair("C5", "Prod3 Module 2 Disabled");
			AssertContainsExactElementsInAnyOrder(expectedModuleListAllModules, collection.GetModuleList("CCC"));
			expectedModuleListAllModules.Clear();

			AssertEquals("Precondition", mapping3.IsEnabled, true);
			var expectedModuleListEnabledModulesOnly = new CodeDescriptionPairList();
			expectedModuleListEnabledModulesOnly.AddPair("C3", "Prod2 Module 1 Enabled");
			AssertContainsExactElementsInAnyOrder(expectedModuleListEnabledModulesOnly, collection.GetModuleList("BBB", "", false, true));
			expectedModuleListEnabledModulesOnly.Clear();

			AssertEquals("Precondition", mapping1.IsEnabled, true);
			AssertEquals("Precondition", mapping2.IsEnabled, false);
			expectedModuleListEnabledModulesOnly.AddPair("C1", "Prod1 Module 1 Enabled");
			AssertContainsExactElementsInAnyOrder(expectedModuleListEnabledModulesOnly, collection.GetModuleList("AAA", "", false, true));
			expectedModuleListEnabledModulesOnly.Clear();

			AssertEquals("Precondition", mapping4.IsEnabled, false);
			AssertEquals("Precondition", mapping5.IsEnabled, false);
			AssertEquals(collection.GetModuleList("CCC", "", false, true).Count, 0);
		}

		public void TestGetDescriptionFromCode()
		{
			var collection = new SystemProductCollection();
			var prod1 = collection.AddNew("AAA", "Module AAA", true);
			var prod2 = collection.AddNew("BBB", "Module BBB", false);

			prod1.ModuleMappings.AddNew("C1", "Description prod 1 module 1", "", false);
			prod1.ModuleMappings.AddNew("C2", "Description prod 1 module 2", "", false);
			prod2.ModuleMappings.AddNew("C1", "Description prod 2 module 1", "", false);

			AssertEquals("Description prod 2 module 1", collection.GetDescriptionFromCode("BBB", "C1"));
		}

		public void TestGetClone()
		{
			var collection = new SystemProductCollection();
			collection.AddNew("AAA", "Module AAA", true);
			collection.AddNew("BBB", "Module BBB", false);

			SystemProductCollection clone = (SystemProductCollection)collection.Clone(collection.CurrentFallbackLevel, collection.Factory);
			AssertEquals(2, clone.Count);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override SystemProductCollection GetCollectionToTest()
		{
			return new SystemProductCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SystemProduct(NewFallbackLevel(), Factory);
		}
	}
}
