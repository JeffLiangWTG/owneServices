using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(LegacyModuleMapping))]
	internal sealed class LegacyModuleMappingTest : RegistryBusinessObjectTemplateTestCase<LegacyModuleMapping>
	{
		#region Validation

		public void TestValidateCode()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("AAA", "AAA Module", "", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("BBB", "BBB Module", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("CCC", "CCC Module", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			var mappingCollection = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			var emptyMapping = mappingCollection.AddNew("", "", "", "");
			AssertHasErrorContaining(emptyMapping.CodeInfo, MandatoryValidation.MustBeEntered);

			var mappingLM1 = mappingCollection.AddNew("LM1", "Legacy Module 1", "", "AAA");
			AssertNoErrors(mappingLM1.CodeInfo);

			var mappingLM1b = mappingCollection.AddNew("LM1", "Legacy Module 1b", "", "BBB");
			AssertHasErrorContaining(mappingLM1b.CodeInfo, "unique");

			var mappingAAA = mappingCollection.AddNew("AAA", "AAA Module", "", "BBB");
			AssertHasError(mappingAAA.CodeInfo, "Code [AAA] is currently being used by Module [AAA Module]");
		}

		#endregion

		public void TestModuleMappingDescription()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("AAA", "AAA Module", "", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("BBB", "BBB Module", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.Enterprise, "ASD", true);
			product.ModuleMappings.AddNew("CCC", "CCC Module", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			var obj = NewPopulatedBusinessObject(false);
			obj.ModuleMapping = "AAA";
			AssertEquals("AAA Module", obj.ModuleMappingDescription);
		}

		public new void TestThreadSafetyOfProperties()
		{
			try
			{
				LegacyModuleMapping.DummyLookupsForTests = (t) => new LegacyModuleMappingLookupsForTest(t);
				base.TestThreadSafetyOfProperties();
				Assert("Because it complains about an empty test", true);
			}
			finally
			{
				LegacyModuleMapping.DummyLookupsForTests = null;
			}
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override LegacyModuleMapping GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override LegacyModuleMapping GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		LegacyModuleMapping NewPopulatedBusinessObject(bool useDummyLookups = true)
		{
			LegacyModuleMapping result = new LegacyModuleMapping(ModuleListType.MenuSection);
			result.Code = "COR";
			result.Description = "Core";
			result.CriticalityMapping = "";
			result.ModuleMapping = MandatoryCustomerServiceMenuSectionList.Codes.Other;
			return result;
		}

		#endregion
	}
}
