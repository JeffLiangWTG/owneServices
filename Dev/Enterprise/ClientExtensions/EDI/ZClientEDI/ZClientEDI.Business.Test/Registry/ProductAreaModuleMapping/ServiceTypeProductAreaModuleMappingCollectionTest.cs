using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ServiceTypeProductAreaModuleMappingCollection))]
	internal sealed class ServiceTypeProductAreaModuleMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ServiceTypeProductAreaModuleMappingCollection>
	{
		public void TestAddIfNotExists()
		{
			var collection = new ServiceTypeProductAreaModuleMappingCollection();
			collection.AddNew("XXX", "D1", "P1", false);
			collection.AddIfNotExists("XXX", "D2", "P2");

			AssertEquals(1, collection.Count);
			AssertEquals("D1", collection[0].ModuleDescription);
			AssertEquals("P1", collection[0].ProductArea);
			AssertEquals(false, collection[0].IsModuleReadOnly);
		}

		public void TestAddNewWithValue()
		{
			var collection = new ServiceTypeProductAreaModuleMappingCollection();

			var mapping1 = collection.AddNew("AAA", "Module AAA", "ARC", false);
			AssertEquals("AAA", mapping1.ModuleCode);
			AssertEquals("Module AAA", mapping1.ModuleDescription);
			AssertEquals("ARC", mapping1.ProductArea);
			AssertEquals(false, mapping1.IsModuleReadOnly);

			var mapping2 = collection.AddNew("BBB", "Module BBB", "", false);
			AssertEquals("BBB", mapping2.ModuleCode);
			AssertEquals("Module BBB", mapping2.ModuleDescription);
			AssertEquals("", mapping2.ProductArea);
			AssertEquals(false, mapping2.IsModuleReadOnly);
		}

		public void TestGetModuleListDefaultSettings()
		{
			var collection1 = new ServiceTypeProductAreaModuleMappingCollection();
			var mapping1 = collection1.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			var mapping2 = collection1.AddNew("BBB", "Module BBB Enabled", "ARC", false);
			var mapping3 = collection1.AddNew("CCC", "Module CCC Enabled", "", false);

			CodeDescriptionPairList moduleList = collection1.GetModuleList("");
			AssertEquals(3, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
			AssertEquals(true, moduleList.ContainsCode("CCC"));

			moduleList = collection1.GetModuleList("ARC");
			AssertEquals(2, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));

			var collection2 = new ServiceTypeProductAreaModuleMappingCollection();
			mapping1 = collection2.AddNew("AAA", "Module AAA Disabled", "ARC", false, false);
			mapping2 = collection2.AddNew("BBB", "Module BBB Disabled", "ARC", false, false);
			mapping3 = collection2.AddNew("CCC", "Module CCC Disabled", "", false, false);

			moduleList = collection2.GetModuleList("");
			AssertEquals(3, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
			AssertEquals(true, moduleList.ContainsCode("CCC"));

			moduleList = collection2.GetModuleList("ARC");
			AssertEquals(2, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));

			var collection3 = new ServiceTypeProductAreaModuleMappingCollection();
			mapping1 = collection3.AddNew("AAA", "Module AAA Enabled", "ARC");
			mapping2 = collection3.AddNew("BBB", "Module BBB Disabled", "ARC", false, false);
			mapping3 = collection3.AddNew("CCC", "Module CCC Enabled", "", false);

			moduleList = collection3.GetModuleList("");
			AssertEquals(3, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
			AssertEquals(true, moduleList.ContainsCode("CCC"));

			moduleList = collection3.GetModuleList("ARC");
			AssertEquals(2, moduleList.Count);
			AssertEquals(true, moduleList.ContainsCode("AAA"));
			AssertEquals(true, moduleList.ContainsCode("BBB"));
		}

		public void TestGetModuleListExcludeDisabledArg()
		{
			var collection1 = new ServiceTypeProductAreaModuleMappingCollection();
			var mapping1 = collection1.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			var mapping2 = collection1.AddNew("BBB", "Module BBB Enabled", "ARC", false);
			var mapping3 = collection1.AddNew("CCC", "Module CCC Enabled", "", false);

			CodeDescriptionPairList moduleListIncludeAllEnabledDisabled = collection1.GetModuleList("");
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			CodeDescriptionPairList moduleListExcludeDisabled = collection1.GetModuleList("", false, true);
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			var collection2 = new ServiceTypeProductAreaModuleMappingCollection();
			mapping1 = collection2.AddNew("AAA", "Module AAA Disabled", "ARC", false, false);
			mapping2 = collection2.AddNew("BBB", "Module BBB Disabled", "ARC", false, false);
			mapping3 = collection2.AddNew("CCC", "Module CCC Disabled", "", false, false);

			moduleListIncludeAllEnabledDisabled = collection2.GetModuleList("");
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			moduleListExcludeDisabled = collection2.GetModuleList("", false, true);
			AssertEquals(0, moduleListExcludeDisabled.Count);

			var collection3 = new ServiceTypeProductAreaModuleMappingCollection();
			mapping1 = collection3.AddNew("AAA", "Module AAA Enabled", "ARC", true);
			mapping2 = collection3.AddNew("BBB", "Module BBB Disabled", "ARC", false, false);
			mapping3 = collection3.AddNew("CCC", "Module CCC Enabled", "", false);

			moduleListIncludeAllEnabledDisabled = collection3.GetModuleList("");
			AssertEquals(3, moduleListIncludeAllEnabledDisabled.Count);
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("BBB"));
			AssertEquals(true, moduleListIncludeAllEnabledDisabled.ContainsCode("CCC"));

			moduleListExcludeDisabled = collection3.GetModuleList("", false, true);
			AssertEquals(2, moduleListExcludeDisabled.Count);
			AssertEquals(true, moduleListExcludeDisabled.ContainsCode("AAA"));
			AssertEquals(true, moduleListExcludeDisabled.ContainsCode("CCC"));

			moduleListExcludeDisabled = collection3.GetModuleList("ARC", false, true);
			AssertEquals(1, moduleListExcludeDisabled.Count);
			AssertEquals(true, moduleListExcludeDisabled.ContainsCode("AAA"));
		}

		public void TestGetClone()
		{
			var collection = new ServiceTypeProductAreaModuleMappingCollection();
			collection.AddNew("AAA", "Module AAA", "ARC", true);
			collection.AddNew("BBB", "Module BBB", "ARC", false);

			var clone = (ServiceTypeProductAreaModuleMappingCollection)collection.Clone(collection.CurrentFallbackLevel, collection.Factory);
			AssertEquals(2, clone.Count);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ServiceTypeProductAreaModuleMappingCollection GetCollectionToTest()
		{
			return new ServiceTypeProductAreaModuleMappingCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ServiceTypeProductAreaModuleMapping(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
